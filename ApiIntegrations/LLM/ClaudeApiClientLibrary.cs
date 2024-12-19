using CommonTypes;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ApiIntegrations.LLM
{
	public class ClaudeApiClientLibrary : ILangModelClientLibrary
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
            throw new NotImplementedException();
        }

        public async Task<string> MakeStreamingApiRequest(List<Message> messages, ProcessReceivedSentenceStream processSentence, string model, string avatarName)
        {
            throw new NotImplementedException();
        }

        private async Task<string> MakeApiRequestRetryAttempt(List<Message> messages, string model)
        {
            switch (model)
            {
				case "large":
					model = "claude-3-opus-20240229";
					break;

				case "medium":
                    model = "claude-3-sonnet-20240229";
                    break;

                case "small":
                    model = "claude-3-haiku-20240307";
                    break;

                default:
                    model = "claude-3-opus-20240229";
                    break;
            }

            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(300);
            httpClient.DefaultRequestHeaders.Add("Authorization", Environment.GetEnvironmentVariable("ClaudeApiKey"));
            httpClient.DefaultRequestHeaders.Add("x-api-key", Environment.GetEnvironmentVariable("ClaudeApiKey"));
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var max_tokens = 2000;
            var system = messages[0].content;
            messages.RemoveAt(0);

            var requestBodyObj = new
            {
                model,
                system,
                max_tokens,
                messages,
                temperature = 0.7,
            };

            var requestBodyJson = JsonSerializer.Serialize(requestBodyObj);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
            {
                Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json")
            };

            var response = httpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Got non-success status code from Claude API");
            }
            var responseBody = response.Content.ReadAsStringAsync().Result;

            using var doc = JsonDocument.Parse(responseBody);
            var contentString = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();

            return contentString;
        }
    }
}
