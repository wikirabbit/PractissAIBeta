using CommonTypes;
using System.Text;
using System.Text.Json;

namespace ApiIntegrations.LLM
{
	public class GptApiClientLibrary : ILangModelClientLibrary
    {
        public async Task<string> MakeApiRequest(List<Message> messages, string model)
        {
            string response = null;

            Helpers.RetryDelegate retry = Helpers.Retry;
            retry(() =>
            {
                response = MakeApiRequestRetryAttempt(messages, model).Result;
            }, 3, "MakeGptApiRequest");

            return response;
        }

        public async Task<string> MakeFunctionCallApiRequest(List<Message> messages, string functionDefinitions, string functionToInvoke, string model)
        {
            string response = null;

            Helpers.RetryDelegate retry = Helpers.Retry;
            retry(() =>
            {
                response = MakeFunctionCallApiRequestRetryAttempt(messages, functionDefinitions, functionToInvoke, model).Result;
            }, 3, "MakeGptFunctionCallApiRequest");

            return response;
        }

        private async Task<string> MakeApiRequestRetryAttempt(List<Message> messages, string model)
        {
            switch (model)
            {
				case "m4turbo":
					model = "gpt-4-turbo-preview";
					break;

				case "m4":
                    model = "gpt-4";
                    break;

                case "m3":
                    model = "gpt-3.5-turbo-0125";
                    break;

                default:
                    model = "gpt-4-0125-preview";
                    break;
            }

            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(300);
            httpClient.DefaultRequestHeaders.Add("Authorization", Environment.GetEnvironmentVariable("OpenAIApiKey"));

            var requestBodyObj = new
            {
                model,
                messages,
                temperature = 0.7,
            };

            var requestBodyJson = JsonSerializer.Serialize(requestBodyObj);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json")
            };

            var response = httpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Got non-success status code from GPT API");
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
				case "m4turbo":
					model = "gpt-4-turbo-preview";
					break;

				case "m4":
                    model = "gpt-4";
                    break;

                case "m3":
                    model = "gpt-3.5-turbo-0125";
                    break;

                default:
                    model = "gpt-4-0125-preview";
                    break;
            }

            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(300);
            httpClient.DefaultRequestHeaders.Add("Authorization", Environment.GetEnvironmentVariable("OpenAIApiKey"));

            var requestBodyObj = new
            {
                model,
                messages,
                temperature = 0.7,
                functions = "##"
            };

            var requestBodyJson = JsonSerializer.Serialize(requestBodyObj);
            requestBodyJson = requestBodyJson.Replace("\"##\"", functionDefinitions + ", \"function_call\": { \"name\": \"" + functionToInvoke + "\" }");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
				// Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json")
				Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json")
			};

            var response = httpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Got non-success status code from GPT API");
            }
            var responseBody = await response.Content.ReadAsStringAsync();

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
                httpClient.DefaultRequestHeaders.Add("Authorization", Environment.GetEnvironmentVariable("OpenAIApiKey"));

                switch (model)
                {
                    case "m4turbo":
                        model = "gpt-4-turbo-preview";
                        break;

					case "m4":
                        model = "gpt-4";
                        break;

                    case "m3":
                        model = "gpt-3.5-turbo-0125";
                        break;

                    default:
                        model = "gpt-4-0125-preview";
                        break;
                }

                var requestBodyObj = new
                {
                    model,
                    messages,
                    temperature = 0.7,
                    stream = true
                };

                var requestBodyJson = JsonSerializer.Serialize(requestBodyObj);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
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

                    if(unprocessedText.ToString().Length > 0)
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
