using CommonTypes;
using System.Text;
using System.Text.Json;

namespace ApiIntegrations.LLM
{
	public class MistralApiClientLibrary : ILangModelClientLibrary
    {
        public async Task<string> MakeApiRequest(List<Message> messages, string model)
        {
            string response = null;

            Helpers.RetryDelegate retry = Helpers.Retry;
            retry(async () =>
            {
                response = await MakeApiRequestRetryAttempt(messages, model);
            }, 3, "MakeGptApiRequest");

            return response;
        }

        public async Task<string> MakeFunctionCallApiRequest(List<Message> messages, string functionDefinitions, string functionToInvoke, string model)
        {
            string response = null;

            Helpers.RetryDelegate retry = Helpers.Retry;
            retry(async () =>
            {
                response = await MakeFunctionCallApiRequestRetryAttempt(messages, functionDefinitions, functionToInvoke, model);
            }, 3, "MakeGptFunctionCallApiRequest");

            return response;
        }

        private void UpdateSystemMessages(List<Message> messages)
        {
            foreach (var message in messages)
            {
                if (message.role == "system")
                    message.role = "user";
            }
        }


        private async Task<string> MakeApiRequestRetryAttempt(List<Message> messages, string model)
        {
            switch (model)
            {
				case "large":
					model = "mistral-large-latest";
					break;

				case "medium":
                    model = "mistral-medium-latest";
                    break;

                case "small":
                    model = "mistral-small-latest";
                    break;

                default:
                    model = "mistral-large-latest";
                    break;
            }

            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(300);
            httpClient.DefaultRequestHeaders.Add("Authorization", Environment.GetEnvironmentVariable("MistralApiKey"));

            UpdateSystemMessages(messages);
            var requestBodyObj = new
            {
                model,
                messages,
                temperature = 0.7,
            };

            var requestBodyJson = JsonSerializer.Serialize(requestBodyObj);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mistral.ai/v1/chat/completions")
            {
                Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json")
            };

            var response = httpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Got non-success status code from Mustral API");
            }
            var responseBody = response.Content.ReadAsStringAsync().Result;

            using var doc = JsonDocument.Parse(responseBody);
            var contentString = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return contentString;
        }
        private async Task<string> MakeFunctionCallApiRequestRetryAttempt(List<Message> messages, string functionDefinitions, string functionToInvoke, string model)
        {
			switch (model)
			{
				case "large":
					model = "mistral-large-latest";
					break;

				case "medium":
					model = "mistral-medium-latest";
					break;

				case "small":
					model = "mistral-small-latest";
					break;

				default:
					model = "mistral-large-latest";
					break;
			}

			var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(300);
            httpClient.DefaultRequestHeaders.Add("Authorization", Environment.GetEnvironmentVariable("MistralApiKey"));

            UpdateSystemMessages(messages);
            var requestBodyObj = new
            {
                model,
                messages,
                temperature = 0.7,
                functions = "##"
            };

            var requestBodyJson = JsonSerializer.Serialize(requestBodyObj);
            requestBodyJson = requestBodyJson.Replace("\"##\"", functionDefinitions + ", \"function_call\": { \"name\": \"" + functionToInvoke + "\" }");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mistral.ai/v1/chat/completions")
            {
				// Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json")
				Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json")
			};

            var response = httpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Got non-success status code from Mistral API");
            }
            var responseBody = response.Content.ReadAsStringAsync().Result;

            using var doc = JsonDocument.Parse(responseBody);
            var contentString = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("function_call")
                .GetProperty("arguments")
                .GetString();

            return contentString;
        }

        public async Task<string> MakeStreamingApiRequest(List<Message> messages, ProcessReceivedSentenceStream processSentence, string model, string avatarName)
        {
            DataAccess.Logger.LogError($@"GptApi {Guid.NewGuid()} Start : {((dynamic)messages.Last()).content}");

            try
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", Environment.GetEnvironmentVariable("MistralApiKey"));

				switch (model)
				{
					case "large":
						model = "mistral-large-latest";
						break;

					case "medium":
						model = "mistral-medium-latest";
						break;

					case "small":
						model = "mistral-small-latest";
						break;

					default:
						model = "mistral-large-latest";
						break;
				}

                UpdateSystemMessages(messages);
                var requestBodyObj = new
                {
                    model,
                    messages,
                    temperature = 0.7,
                    stream = true
                };

                var requestBodyJson = JsonSerializer.Serialize(requestBodyObj);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mistral.ai/v1/chat/completions")
                {
                    Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json")
                };

                StringBuilder fullResponseText = new StringBuilder();
                StringBuilder unprocessedText = new StringBuilder();
                int sequenceNumber = 0;

                using (var response = await httpClient.SendAsync(request,
                            HttpCompletionOption.ResponseHeadersRead))
                {
                    // Get stream of content
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        // Wrap stream with stream reader
                        using (var reader = new StreamReader(contentStream))
                        {
                            // Read stream asynchronously 
                            while (!reader.EndOfStream)
                            {
                                var json = await reader.ReadLineAsync();
                                if (string.IsNullOrEmpty(json) || json == "data: [DONE]")
                                    continue;

                                json = json.Replace("data: ", "");

                                var data = JsonDocument.Parse(json);

                                var delta = data.RootElement
                                  .GetProperty("choices")[0]
                                  .GetProperty("delta");

                                JsonElement content;
                                if (delta.TryGetProperty("content", out content))
                                {
                                    var text = content.GetString();
                                   
                                    if (!String.IsNullOrEmpty(text))
                                    {
                                        fullResponseText.Append(text);
                                        unprocessedText.Append(text);

                                        // Dont send too short a text. If eleven labs finds no
                                        // speakable words, it will end the stream.
                                        if (unprocessedText.Length >= 20)
                                        {
                                            processSentence(Helpers.SanitizeText(unprocessedText.ToString(), avatarName), sequenceNumber++);
                                            unprocessedText.Clear();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (unprocessedText.ToString().Length > 0)
                    {
                        processSentence(unprocessedText.ToString(), sequenceNumber++);
                    }

                    try
                    {
                        processSentence(string.Empty, sequenceNumber++);
                    }
                    catch (Exception) { }

                    var assistantResponse = fullResponseText.ToString();
                    DataAccess.Logger.LogInfo(assistantResponse);

                    return assistantResponse;
                }
            }
            catch (Exception e)
            {
                DataAccess.Logger.LogError(e.Message);
                throw;
            }
        }
    }
}
