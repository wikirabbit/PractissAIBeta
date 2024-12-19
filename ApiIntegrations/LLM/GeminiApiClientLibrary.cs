using CommonTypes;
using System.Text;
using System.Text.Json;

namespace ApiIntegrations.LLM
{
	public class GeminiApiClientLibrary : ILangModelClientLibrary
    {
        public GeminiApiClientLibrary()
        {
        }

        public async Task<string> MakeApiRequest(List<Message> messages, string model)
        {
            string response = null;

            Helpers.RetryDelegate retry = Helpers.Retry;
            retry(() =>
            {
                response = MakeApiRequestRetryAttempt(messages, model).Result;
            }, 3, "MakeGeminiApiRequest");

            return response;
        }

        public async Task<string> MakeFunctionCallApiRequest(List<Message> messages, string functionDefinitions, string functionToInvoke, string model)
        {
            // Implementation needed
            throw new NotImplementedException();
        }

        public async Task<string> MakeStreamingApiRequest(List<Message> messages, ProcessReceivedSentenceStream processSentence, string model, string avatarName)
        {
            switch (model)
            {
                case "m4":
                    model = "gemini-1.0-pro";
                    break;

                case "m3":
                    model = "gemini-1.0-pro";
                    break;

                default:
                    model = "gemini-1.0-pro";
                    break;
            }

            string apiKey = Environment.GetEnvironmentVariable("GeminiApiKey");
            string requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:streamGenerateContent?key={apiKey}";

            dynamic payload = GeneratePayload(messages);
            var jsonPayload = JsonSerializer.Serialize(payload);
            var stringPayload = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpClient httpClient = new HttpClient();
            var response = await httpClient.PostAsync(requestUri, stringPayload);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            var jsonDocumentOptions = new JsonDocumentOptions { AllowTrailingCommas = true };

            StringBuilder accumulatedText = new StringBuilder();
            int sequenceNumber = 0;

            // Read and process the stream.
            try
            {
                while (true) // Continue until the stream ends or break on condition.
                {
                    if (stream.Position == stream.Length) break; // End of stream check.

                    var jsonDocument = await JsonDocument.ParseAsync(stream, jsonDocumentOptions);
                    var rootElement = jsonDocument.RootElement;

					foreach (var item in rootElement.EnumerateArray())
					{
						// Now, directly access "candidates" within each array item
						if (item.TryGetProperty("candidates", out var candidates))
						{
							foreach (var candidate in candidates.EnumerateArray())
							{
								var content = candidate.GetProperty("content");
								foreach (var part in content.GetProperty("parts").EnumerateArray())
								{
									var text = part.GetProperty("text").GetString();									
									accumulatedText.Append(text);

                                    if(!String.IsNullOrEmpty(text))
									    processSentence(Helpers.SanitizeText(accumulatedText.ToString(), avatarName), sequenceNumber++);
								}
							}
						}
					}
				}

                // Indicates End of Input Stream to Eleven Labs
                try
                {
                    processSentence(string.Empty, sequenceNumber++);
                }
                catch (Exception) { }

                var assistantResponse = accumulatedText.ToString();
                DataAccess.Logger.LogInfo(assistantResponse);
                return assistantResponse;
            }
            catch (JsonException ex)
            {
                DataAccess.Logger.LogError($"JSON Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                DataAccess.Logger.LogError($"Error: {ex.Message}");
                throw;
            }
        }

        private dynamic GeneratePayload(List<Message> messages)
        {
            var contents = new List<dynamic>();
            foreach (dynamic message in messages)
            {
                contents.Add(new
                {
                    role = message.role == "assistant" ? "model" : "user",
                    parts = new { text = message.content }
                });
            }

            var generationConfig = new { temperature = 0.7 };

            var payload = new
            {
                contents,
                generationConfig
            };

            return payload;
        }

        private async Task<string> MakeApiRequestRetryAttempt(List<Message> messages, string model)
        {
            switch (model)
            {
                case "m4":
                    model = "gemini-pro";
                    break;

                case "m3":
                    model = "gemini-pro";
                    break;

                default:
                    model = "gemini-pro";
                    break;
            }

            string apiKey = Environment.GetEnvironmentVariable("GeminiApiKey");
            string requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            dynamic payload = GeneratePayload(messages);
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpClient httpClient = new HttpClient();
            var response = await httpClient.PostAsync(requestUri, content);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);
                var contentString = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                return contentString;
            }
            else
            {
                throw new HttpRequestException($"HTTP request failed with status code {response.StatusCode}");
            }
        }
    }
}
