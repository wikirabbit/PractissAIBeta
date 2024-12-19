using ApiIntegrations.LLM;
using ApiIntegrations.Meetings;
using CommonTypes;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleWorker.Workers
{
	public class FirefliesWorker : IWorker
    {
        public async Task Run(CancellationToken cancellationToken)
        {
            TimeSpan checkInterval = TimeSpan.FromMinutes(30);

            while (!cancellationToken.IsCancellationRequested)
            {
                await ProcessNewFirefliesTranscripts();

                await Task.Delay(checkInterval, cancellationToken);
            }
        }

        private async Task ProcessNewFirefliesTranscripts()
        {
            DataAccess.CosmosDbService cosmosDbService = DataAccess.CosmosDbService.Instance;
            var users = await cosmosDbService.GetAllUsersAsync();
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.Integrations.FirefliesApiKey))
                {
                    long watermark;
                    // Check if the watermark exists; if not, set it to 6 months in the past.
                    if (!user.RecommendationWatermarks.TryGetValue("Fireflies", out watermark))
                    {
                        watermark = ApiIntegrations.Helpers.ConvertDateTimeToUnixMilliseconds(DateTime.UtcNow.AddMonths(-6));
                    }

                    var firefliesClient = new FirefliesApiClientLibrary(user.Integrations.FirefliesApiKey);
                    var (firefliesUserId, firefliesUserName) = await firefliesClient.GetUserIdByEmailAsync(user.Email);

                    if (firefliesUserId == null)
                        continue;

                    var (transcriptIds, nextWatermark) = await firefliesClient.GetTranscriptIdsAfterDateAsync(firefliesUserId, watermark);

                    Console.WriteLine($"Next Watermark: {nextWatermark}");
                    Console.WriteLine("Transcript IDs:");
                    foreach (var id in transcriptIds)
                    {
                        try
                        {
                            Console.WriteLine(id);

                            var (meetingTitle, date, attendees, transcript) = await firefliesClient.GetTranscriptAsync(id);

                            foreach (var attendee in attendees)
                            {
                                var attendingUser = await cosmosDbService.GetUserByEmail(attendee.Email);
                                if(attendingUser == null || String.IsNullOrWhiteSpace(attendingUser.Integrations.FirefliesApiKey))
                                {
                                    // attendee is not a practiss user 
                                    // or has firefly enabled, so skip
                                    continue;
                                }    

                                if (!transcript.Contains(firefliesUserName.Split(' ')[0]))
                                {
                                    // user was invited but not present at the meeting
                                    continue;
                                }

                                var insight = await cosmosDbService.GetInsightByTitleAsync(attendingUser.Id, meetingTitle);
                                if(insight != null && insight.QuantitativeFeedbackV3 != null)
                                {
                                    // insight has already been generated
                                    continue;
                                }

								Console.WriteLine("\n\nProcessing " + meetingTitle);

								var report = await GetReportMarkdownForTranscript(attendee.DisplayName, attendingUser.Bio, meetingTitle, date.ToLongDateString(), transcript);
                                Console.WriteLine(transcript);
                                Console.WriteLine(report);

								Roleplay roleplay = ExtractRoleplay(report);
                                Roleplay alternateRoleplay = ExtractAlternateRoleplay(report);
								QuantitativeFeedbackV3 feedback = ExtractQuantitativeFeedbackV3(report);

                                if (insight == null)
                                {
                                    insight = new Insight()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        Source = "Fireflies",
                                        UserId = attendingUser.Id,
                                        Title = meetingTitle,
                                        Date = date.ToString("o"),
                                        MarkdownV3 = report,
                                        QuantitativeFeedbackV3 = feedback,
                                        Roleplay = roleplay,
                                        AlternateRoleplay = alternateRoleplay
                                    };

                                    await cosmosDbService.CreateInsightAsync(insight);
                                }
                                else
                                {
                                    insight.MarkdownV3 = report;
                                    insight.Roleplay = roleplay;
                                    insight.AlternateRoleplay = alternateRoleplay;
                                    insight.QuantitativeFeedbackV3 = feedback;

                                    await cosmosDbService.UpdateInsightAsync(insight.Id, insight);
                                }              
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
					}

                    // Update the watermark for the user
                    user.RecommendationWatermarks["Fireflies"] = nextWatermark;
                    await cosmosDbService.UpdateUserAsync(user.Id, user);
                }
            }
        }

		private static Roleplay ExtractRoleplay(string text)
		{
			var purposeMatch = Regex.Match(text, @"Purpose:\*\*\s*(.+?)\n");
			var scenarioMatch = Regex.Match(text, @"Scenario:\*\*\s*(.+?)\n", RegexOptions.Singleline);

			return new Roleplay
			{
				Purpose = purposeMatch.Groups[1].Value.Trim(),
				Scenario = scenarioMatch.Groups[1].Value.Trim()
			};
		}

        private static Roleplay ExtractAlternateRoleplay(string text)
        {
            var purposeMatch = Regex.Match(text, @"Alternate Purpose:\*\*\s*(.+?)\n");
            var scenarioMatch = Regex.Match(text, @"Alternate Scenario:\*\*\s*(.+?)\n", RegexOptions.Singleline);

            return new Roleplay
            {
                Purpose = purposeMatch.Groups[1].Value.Trim(),
                Scenario = scenarioMatch.Groups[1].Value.Trim()
            };
        }

        private static QuantitativeFeedback ExtractQuantitativeFeedback(string text)
        {
            var clarityMatch = Regex.Match(text, @"Clarity:\*\*\s*(\d+)");
            var influenceMatch = Regex.Match(text, @"Influence:\*\*\s*(\d+)");
            var eiMatch = Regex.Match(text, @"Emotional Intelligence:\*\*\s*(\d+)");
            var listeningMatch = Regex.Match(text, @"Listening:\*\*\s*(\d+)|Unable to score");
            var articulateMatch = Regex.Match(text, @"Articulate Communication:\*\*\s*(\d+)");

            return new QuantitativeFeedback
            {
                Clarity = clarityMatch.Success ? int.Parse(clarityMatch.Groups[1].Value) : -1,
                Influence = influenceMatch.Success ? int.Parse(influenceMatch.Groups[1].Value) : -1,
                EmotionalIntelligence = eiMatch.Success ? int.Parse(eiMatch.Groups[1].Value) : -1,
                Listening = listeningMatch.Success && listeningMatch.Groups[1].Value != string.Empty ? int.Parse(listeningMatch.Groups[1].Value) : -1,
                ArticulateCommunication = articulateMatch.Success ? int.Parse(articulateMatch.Groups[1].Value) : -1
            };
        }

        public static QuantitativeFeedbackV2 ExtractQuantitativeFeedbackV2(string text)
        {
            var clarityMatch = Regex.Match(text, @"Clarity of Communication:\*\*\s*(\d+)");
            var listeningMatch = Regex.Match(text, @"Active Listening:\*\*\s*(\d+)");
            var eiMatch = Regex.Match(text, @"Emotional Intelligence:\*\*\s*(\d+)");
            var persuasivenessMatch = Regex.Match(text, @"Persuasiveness:\*\*\s*(\d+)");
            var problemSolvingMatch = Regex.Match(text, @"Problem Solving and Adaptability:\*\*\s*(\d+)");
            var professionalismMatch = Regex.Match(text, @"Professionalism and Decorum:\*\*\s*(\d+)");
            var impactMatch = Regex.Match(text, @"Impact and Influence:\*\*\s*(\d+)");
            var articulateMatch = Regex.Match(text, @"Articulate Communication:\*\*\s*(\d+)");

            return new QuantitativeFeedbackV2
            {
                ClarityOfCommunication = clarityMatch.Success ? int.Parse(clarityMatch.Groups[1].Value) : -1,
                ActiveListening = listeningMatch.Success ? int.Parse(listeningMatch.Groups[1].Value) : -1,
                EmotionalIntelligence = eiMatch.Success ? int.Parse(eiMatch.Groups[1].Value) : -1,
                Persuasiveness = persuasivenessMatch.Success ? int.Parse(persuasivenessMatch.Groups[1].Value) : -1,
                ProblemSolvingAndAdaptability = problemSolvingMatch.Success ? int.Parse(problemSolvingMatch.Groups[1].Value) : -1,
                ProfessionalismAndDecorum = professionalismMatch.Success ? int.Parse(professionalismMatch.Groups[1].Value) : -1,
                ImpactAndInfluence = impactMatch.Success ? int.Parse(impactMatch.Groups[1].Value) : -1,
                ArticulateCommunication = articulateMatch.Success ? int.Parse(articulateMatch.Groups[1].Value) : -1
            };
        }

        public static QuantitativeFeedbackV3 ExtractQuantitativeFeedbackV3(string text)
        {
            var clarityMatch = Regex.Match(text, @"Clarity of Communication:\*\*\s*(\d+)");
            var listeningMatch = Regex.Match(text, @"Active Listening:\*\*\s*(\d+)");
            var eiMatch = Regex.Match(text, @"Emotional Intelligence:\*\*\s*(\d+)");
            var problemSolvingMatch = Regex.Match(text, @"Problem Solving and Adaptability:\*\*\s*(\d+)");
            var professionalismMatch = Regex.Match(text, @"Professionalism and Decorum:\*\*\s*(\d+)");
            var influenceMatch = Regex.Match(text, @"Influential Communication:\*\*\s*(\d+)");

            return new QuantitativeFeedbackV3
            {
                ClarityOfCommunication = clarityMatch.Success ? int.Parse(clarityMatch.Groups[1].Value) : -1,
                ActiveListening = listeningMatch.Success ? int.Parse(listeningMatch.Groups[1].Value) : -1,
                EmotionalIntelligence = eiMatch.Success ? int.Parse(eiMatch.Groups[1].Value) : -1,
                ProblemSolvingAndAdaptability = problemSolvingMatch.Success ? int.Parse(problemSolvingMatch.Groups[1].Value) : -1,
                ProfessionalismAndDecorum = professionalismMatch.Success ? int.Parse(professionalismMatch.Groups[1].Value) : -1,
                InfluentialCommunication = influenceMatch.Success ? int.Parse(influenceMatch.Groups[1].Value) : -1,
            };
        }

        private static async Task<string> GetReportMarkdownForTranscript(string firefliesDisplayName, string practissUserBio, string meetingTitle, string date, string transcript)
        {
            // Console.WriteLine(transcript);

            var promptTemplate = System.IO.File.ReadAllText("ContextualPrompt.txt");
            var systemPrompt = string.Format(promptTemplate, 
                firefliesDisplayName, 
                meetingTitle,
                date,
                practissUserBio);

			var llm = new ClaudeApiClientLibrary();
            var messages = new List<Message>();
            messages.Add(new Message() { role = "system", content = systemPrompt });
			messages.Add(new Message() { role = "user", content = transcript });

            var response = await llm.MakeApiRequest(messages, "m4turbo"); 
            Console.WriteLine(response);

            return response;
        }

		private static string GetMeetingAttendees(List<Attendee> attendees, string myName)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var attendee in attendees)
            {
                if (attendee.DisplayName != myName)
                {
                    sb.Append(attendee.DisplayName);
                    sb.Append(" and (");
                    sb.Append(attendee.Email);
                    sb.Append(") ");
                }
            }

            return sb.ToString();
        }

		private static string GetMeetingAttendeesShort(List<Attendee> attendees, string myName)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var attendee in attendees)
			{
				if (attendee.DisplayName != myName)
				{
                    sb.Append(attendee.DisplayName.Split(' ')[0]);
                    sb.Append(' ');
				}
			}

			return sb.ToString().Trim();
		}
	}
}
