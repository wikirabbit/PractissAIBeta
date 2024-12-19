using ApiIntegrations.LLM;
using ElevenLabs;
using ElevenLabs.Voices;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebSocketSharp;

namespace ApiIntegrations.STT
{
	public class Payload
	{
		public string Text { get; set; }
		public VoiceSettings VoiceSettings { get; set; }
		public GenerationConfig GenerationConfig { get; set; } // Optional

		public string XiApiKey { get; set; }
	}

	public class VoiceSettings
	{
		public double Stability { get; set; }
		public double SimilarityBoost { get; set; }
	}

	public class GenerationConfig
	{
		public List<int> ChunkLengthSchedule { get; set; }
	}

	public class ElevenLabs
	{
		private static IReadOnlyList<Voice> elevenLabsVoices;
		private static Dictionary<string, double> stability = new Dictionary<string, double>();
		private static Dictionary<string, double> similarityBoost = new Dictionary<string, double>();

		static ElevenLabs()
		{
			stability.Add("", 0.5);
			similarityBoost.Add("", 0.75);
			stability.Add("angry", 0.10);
			similarityBoost.Add("angry", 0.75);

			ElevenLabsClient api;
			do
			{
				try
				{
					api = new ElevenLabsClient(Environment.GetEnvironmentVariable("ElevenLabsApiKey"));
					elevenLabsVoices = api.VoicesEndpoint.GetAllVoicesAsync().Result;
				}
				catch (Exception ex)
				{
					Thread.Sleep(1000);
				}
			} 
			while (elevenLabsVoices == null);
		}

		public static async Task<byte[]> GetSTTBytesViaHttp(string voiceName, string personality, string input)
		{
			byte[] response = null;

			Helpers.RetryDelegate retry = Helpers.Retry;
			retry(() =>
			{
				response = GetSTTBytesViaHttpRetryAttempt(voiceName, personality, input).Result;
			}, 3, "MakeElevenLabsHttpRequest");

			return response;
		}

		public static async Task<Stream> GetSTTBytesViaHttpStream(string voiceName, string personality, string input)
		{
			Stream response = null;

			Helpers.RetryDelegate retry = Helpers.Retry;
			retry(() =>
			{
				response = GetSTTBytesViaHttpStreamRetryAttempt(voiceName, personality, input).Result;
			}, 3, "MakeElevenLabsHttpStreamRequest");

			return response;
		}

		private static async Task<byte[]> GetSTTBytesViaHttpRetryAttempt(string voiceName, string personality, string input)
		{
			var api = new ElevenLabsClient(Environment.GetEnvironmentVariable("ElevenLabsApiKey"));
			var voice = elevenLabsVoices.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(voiceName));
			var voiceId = voice?.Id;

			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri("https://api.elevenlabs.io/");
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));
				client.DefaultRequestHeaders.Add("xi-api-key", Environment.GetEnvironmentVariable("ElevenLabsApiKey"));

				var requestBody = new
				{
					model_id = "eleven_turbo_v2",
					text = input,
					voice_settings = new
					{
						similarity_boost = similarityBoost[personality],
						stability = stability[personality],
						style = 0,
						use_speaker_boost = false
					},
				};

				var json = JsonSerializer.Serialize(requestBody);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				try
				{
					var response = await client.PostAsync($"v1/text-to-speech/{voiceId}", content);
					response.EnsureSuccessStatusCode();

					byte[] audioBytes = await response.Content.ReadAsByteArrayAsync();
					return audioBytes;
				}
				catch (Exception ex)
				{
					DataAccess.Logger.LogError(ex.Message);
					throw;
				}
			}
		}

		private static async Task<Stream> GetSTTBytesViaHttpStreamRetryAttempt(string voiceName, string personality, string input)
		{
			var api = new ElevenLabsClient(Environment.GetEnvironmentVariable("ElevenLabsApiKey"));
			var voice = elevenLabsVoices.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(voiceName));
			var voiceId = voice?.Id;

			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri("https://api.elevenlabs.io/");
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));
				client.DefaultRequestHeaders.Add("xi-api-key", Environment.GetEnvironmentVariable("ElevenLabsApiKey"));

				var requestBody = new
				{
					model_id = "eleven_turbo_v2",
					text = input,
					voice_settings = new
					{
						similarity_boost = similarityBoost[personality],
						stability = stability[personality],
						style = 0,
						use_speaker_boost = false
					},
				};

				var json = JsonSerializer.Serialize(requestBody);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				try
				{
					HttpResponseMessage response = await client.PostAsync($"v1/text-to-speech/{voiceId}/stream?optimize_streaming_latency=5&output_format=mp3_44100_64", content);

					var result = await response.Content.ReadAsStreamAsync();
					return result;
				}
				catch (Exception ex)
				{
					DataAccess.Logger.LogError(ex.Message);
					throw;
				}
			}
		}

		public static WebSocket CreateWSConnection(string voiceName, ProcessReceivedAudioStream processReceiveAudioChunk)
		{
			try
			{
				var api = new ElevenLabsClient(Environment.GetEnvironmentVariable("ElevenLabsApiKey"));
				var voice = elevenLabsVoices.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(voiceName));
				var voiceId = voice?.Id;

				string model = GetModelIdForVoice(voiceName);

				string uri = $"wss://api.elevenlabs.io/v1/text-to-speech/{voiceId}/stream-input?model_id={model}";

				var wsClient = new WebSocket(uri);
				wsClient.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

				int sequenceNumber = 0;

				wsClient.OnMessage += (sender, e) =>
				{
					using (JsonDocument doc = JsonDocument.Parse(e.Data))
					{
						JsonElement root = doc.RootElement;

						// Check if audio is JSON null
						JsonElement audioProperty;
						root.TryGetProperty("audio", out audioProperty);
						if (root.TryGetProperty("audio", out audioProperty))
						{
							var base64Audio = audioProperty.GetString();

							if (base64Audio != null)
							{
								DataAccess.Logger.LogWarning("Received Chunk " + base64Audio.Substring(0, Math.Min(40, base64Audio.Length)));
							}
							processReceiveAudioChunk(base64Audio, sequenceNumber++);
						}
						else
						{
							DataAccess.Logger.LogInfo("No audio data received");
						}

					}
				};

				// Handle connection close
				wsClient.OnClose += (sender, e) =>
				{
					processReceiveAudioChunk(null, sequenceNumber++);
					DataAccess.Logger.LogInfo("Connection closed");
				};

				// Handle errors
				wsClient.OnError += (sender, e) =>
				{
					DataAccess.Logger.LogError("Error: " + e.Message);
				};

				// Connect to the WebSocket server
				wsClient.Connect();

				return wsClient;
			}
			catch (WebSocketException e)
			{
				DataAccess.Logger.LogError($"WebSocketException: {e.Message}. Attempting to reconnect...");
				throw;
			}
			catch (Exception e)
			{
				DataAccess.Logger.LogError($"Exception: {e.Message}");
				throw; // For non-WebSocket exceptions, you might want to fail immediately or handle differently
			}
		}

		public static async Task SendWsDataAsync(WebSocket wsClient, string personality, string text, bool includeGenerationConfig)
		{
			Payload payload = new Payload
			{
				Text = text, // This is your variable that holds the text
				VoiceSettings = new VoiceSettings()
				{
					SimilarityBoost = similarityBoost[personality ?? String.Empty],
					Stability = stability[personality ?? String.Empty],
				},
				XiApiKey = Environment.GetEnvironmentVariable("ElevenLabsApiKey") // This is your variable that holds the API key
			};

			if (includeGenerationConfig)
			{
				payload.GenerationConfig = new GenerationConfig
				{
					ChunkLengthSchedule = new List<int> { 450 }
				};
			}

			string jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });

			wsClient.Send(jsonPayload);
		}

		private static string GetModelIdForVoice(string voiceName)
		{
			var voice = elevenLabsVoices.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(voiceName));

			if (new List<string>() { "patrick", "nelly" }.Contains(voiceName.ToLowerInvariant()))
			{
				return "eleven_multilingual_v1";
			}
			else
			{
				if (voice.Category.ToLowerInvariant() == "premade")
					return "eleven_turbo_v2";
				else
					return "eleven_multilingual_v1";
			}
		}
	}
}
