using CommonTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace ApiIntegrations.Meetings
{
	public class TranscriptData
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("transcript")]
        public Transcript Transcript { get; set; }
    }

    public class Transcript
    {
        [JsonProperty("sentences")]
        public List<Sentence> Sentences { get; set; }
    }

    public class Sentence
    {
        [JsonProperty("speaker_name")]
        public string SpeakerName { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class FirefliesApiClientLibrary
    {
		private readonly HttpClient _httpClient;
		private const string BaseUrl = "https://api.fireflies.ai/graphql";

        public FirefliesApiClientLibrary(string apiKey)
        {
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
		}
		public async Task<(string, string)> GetUserIdByEmailAsync(string targetEmail)
		{
			var query = @"query Users {
                    users {
                        user_id
                        email
						name
                    }
                  }";

			var jsonResult = await ExecuteGraphQlQueryAsync(JsonConvert.SerializeObject(new { query = query }));
			
			var jsonObject = JObject.Parse(jsonResult);
			var users = jsonObject["data"]["users"] as JArray;

			var user = users?.FirstOrDefault(u => string.Equals((string)u["email"], targetEmail, StringComparison.OrdinalIgnoreCase));

			return (user?["user_id"]?.ToString(), user?["name"]?.ToString());
		}


		public async Task<(List<string> TranscriptIds, long NextWatermark)> GetTranscriptIdsAfterDateAsync(string userId, long currentWatermark, int limit = 50)
		{
			var query = @"query Transcripts($userId: String!, $limit: Int!) { 
                        transcripts(user_id: $userId, limit: $limit) { 
                            title 
                            id 
                            date 
							duration
                        } 
                      }";
			var variables = new
			{
				userId = userId,
				limit = limit
			};
			var payload = new
			{
				query = query,
				variables = variables
			};

			var jsonResult = await ExecuteGraphQlQueryAsync(JsonConvert.SerializeObject(payload));
			var jsonObject = JObject.Parse(jsonResult);
			var transcripts = jsonObject["data"]["transcripts"] as JArray;

			var transcriptIds = new List<string>();
			long newWatermark = currentWatermark;

			foreach (var transcript in transcripts)
			{
				long transcriptDate = (long)transcript["date"];
				long transcriptDuration = ((long)transcript["duration"] + 1) * 60 * 1000;

				if (transcriptDate > currentWatermark)
				{
					transcriptIds.Add((string)transcript["id"]);
					newWatermark = Math.Max(newWatermark, transcriptDate + transcriptDuration);
				}
			}

			return (transcriptIds, newWatermark);
		}

		public async Task<(string Title, DateTime Date, List<Attendee> Attendees, string CleansedTranscript)> GetTranscriptAsync(string transcriptId)
		{
			var query = @"query Transcript($transcriptId: String!) {
                        transcript(id: $transcriptId) {
                            id 
                            sentences { index speaker_name speaker_id text raw_text start_time end_time } 
                            title 
                            participants 
                            date 
                            meeting_attendees { displayName email phoneNumber name location } 
                            summary { action_items outline overview shorthand_bullet } 
                        } 
                    }";

			var variables = new { transcriptId = transcriptId };
			var payload = new { query, variables };
			var jsonTranscript = await ExecuteGraphQlQueryAsync(JsonConvert.SerializeObject(payload));

			// Using JObject to parse the JSON response safely
			var jsonObject = JObject.Parse(jsonTranscript);

			// Safely extract title, attendees, and prepare for extracting the cleansed transcript
			var title = jsonObject["data"]?["transcript"]?["title"]?.ToString();
			var date = jsonObject["data"]?["transcript"]?["date"]?.ToString();
			var attendeesList = jsonObject["data"]?["transcript"]?["meeting_attendees"] as JArray;
			var attendees = new List<Attendee>();

			if (attendeesList != null)
			{
				foreach (var attendee in attendeesList)
				{
					var displayName = attendee["displayName"]?.ToString();
					var email = attendee["email"]?.ToString();
					if (!string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(email))
					{
						attendees.Add(new Attendee
						{
							DisplayName = displayName,
							Email = email
						});
					}
				}
			}

			var cleansedTranscript = ExtractCleansedTranscript(jsonTranscript);

			return (title, EpochToDateTime(date), attendees, cleansedTranscript);
		}

		public static DateTime EpochToDateTime(string epochString)
		{
			DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			if (long.TryParse(epochString, out long epoch))
			{
				epochStart = epochStart.AddMilliseconds(epoch);
			}

			return epochStart;
		}


		private async Task<string> ExecuteGraphQlQueryAsync(string payload)
		{
			using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, BaseUrl))
			{
				requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "a7576472-d54a-494f-ab06-12d8112d4153");
				requestMessage.Content = new StringContent(payload, Encoding.UTF8, "application/json");

				var response = await _httpClient.SendAsync(requestMessage);

				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					throw new ApplicationException($"GraphQL query failed: {response.StatusCode}, Body: {errorContent}");
				}

				return await response.Content.ReadAsStringAsync();
			}
		}

		private static string ExtractCleansedTranscript(string jsonTranscript)
		{
			TranscriptData transcriptData = JsonConvert.DeserializeObject<TranscriptData>(jsonTranscript);

			StringBuilder transcript = new StringBuilder();

			if (transcriptData?.Data?.Transcript?.Sentences != null)
			{
				foreach (var sentence in transcriptData.Data.Transcript.Sentences)
				{
					transcript.AppendLine($"{sentence.SpeakerName}: {sentence.Text}");
				}
			}

			return transcript.ToString();
		}
	}
}
