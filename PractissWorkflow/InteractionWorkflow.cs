using ApiIntegrations.LLM;
using ApiIntegrations.Misc;
using CommonTypes;
using DataAccess;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;

namespace PractissWorkflow
{
	public class RoleplayDialogResponse
    {
        public string dialog_response { get; set; }
        public string emotional_state { get; set; }
        public string status { get; set; }
    }

    public class RoleplayEvaluationResponse
    {
        public string summary { get; set; }
        public List<string> areas_of_strength { get; set; }
        public List<string> areas_of_reflection { get; set; }
        public string performance_rating { get; set; }
    }

    public class InvalidInteractionException : Exception
    { }

    public class RetriesExhaustedException : Exception
    { }

    public class SessionsExhaustedException : Exception
    { }

    public class InteractionWorkflow
    {
        public static Dictionary<string, List<Message>> MessagesDict = new Dictionary<string, List<Message>>();
        public static Dictionary<string, InteractionStats> InteractionStatsDict = new Dictionary<string, InteractionStats>();
		public static Dictionary<string, Feedback> FeedbackDict = new Dictionary<string, Feedback>();

		public static Dictionary<string, CommonTypes.Module> ModulesDict = new Dictionary<string, CommonTypes.Module>();
		public static Dictionary<string, ModuleAssignment> ModuleAssignmentDict = new Dictionary<string, ModuleAssignment>();
        
        delegate void StoreInteractionEvaluationDelegate(string moduleAssignmentId, Report report);

		private static async Task<ModuleAssignment> GetModuleAssignment(string moduleAssignmentId, bool forceRefresh = false)
		{
			if (forceRefresh)
			{
				if (ModuleAssignmentDict.ContainsKey(moduleAssignmentId))
				{
					ModuleAssignmentDict.Remove(moduleAssignmentId);
				}
			}

			ModuleAssignment moduleAssignment;

			if (ModuleAssignmentDict.ContainsKey(moduleAssignmentId))
				moduleAssignment = ModuleAssignmentDict[moduleAssignmentId];
			else
			{
				moduleAssignment = await CosmosDbService.Instance.GetModuleAssignmentByIdAsync(moduleAssignmentId);
				ModuleAssignmentDict.Add(moduleAssignmentId, moduleAssignment);
			}

			return moduleAssignment;
		}

		private static async Task<CommonTypes.Module> GetModule(string moduleId, bool forceRefresh = false)
		{
			if(forceRefresh)
			{
				if(ModulesDict.ContainsKey(moduleId))
				{
					ModulesDict.Remove(moduleId);
				}
			}

			CommonTypes.Module module;

			if (ModulesDict.ContainsKey(moduleId))
			{
				module = ModulesDict[moduleId];

			}
			else
			{
				module = await CosmosDbService.Instance.GetModuleAsync(moduleId);
				ModulesDict.Add(moduleId, module);
			}

			return module;
		}

        private static List<Message> GetMessages(string userResponse, string interactionId)
        {
            if (String.IsNullOrWhiteSpace(userResponse))
                return new List<Message>();
            else
                return MessagesDict[interactionId];
		}

        private static Feedback GetFeedback(string interactionId, User user = null)
        {
			Feedback feedback;

			if (FeedbackDict.ContainsKey(interactionId))
			{
				feedback = FeedbackDict[interactionId];

			}
			else
			{
                feedback = new Feedback();
                feedback.FeedbackProvider = user;
                FeedbackDict.Add(interactionId, feedback);  
			}

			return feedback;
		}

		private static string GetInteractionId(ModuleAssignment assigment)
        {
            return String.Format("{0}: {1} {2}: {3} {4}: {5} ",
              assigment.Id,
              assigment.Learner != null ? assigment.Learner.FirstName : "",
              assigment.Learner != null ? assigment.Learner.LastName : "",
              assigment.Coach != null ? assigment.Coach.FirstName : "",
              assigment.Coach != null ? assigment.Coach.LastName : "",
              assigment.InteractionsCount);
        } 

		static ILangModelClientLibrary GetRoleplayLLMForUser(User user)
		{

			switch (user.RoleplayLLM)
			{
				case LLMType.Gemini:
					return new GeminiApiClientLibrary();

				case LLMType.OpenAI:
					return new GptApiClientLibrary();

				case LLMType.Mistral:
					return new MistralApiClientLibrary();

				default:
					return new GptApiClientLibrary();
			}
		}

		static ILangModelClientLibrary GetReportLLMForUser(User user)
		{

			switch (user.ReportLLM)
			{
				case LLMType.Gemini:
					return new MistralApiClientLibrary();

				case LLMType.OpenAI:
					return new GptApiClientLibrary();

				case LLMType.Mistral:
					return new MistralApiClientLibrary();

				default:
					return new GptApiClientLibrary();
			}
		}

		public static async Task<string> GetNextResponse(string moduleAssignmentId, string userResponse, string llmMode = "nonfunction", string audioMode = "nonstreaming", ProcessReceivedSentenceStream processSentence = null)
        {
            var moduleAssignment = await GetModuleAssignment(moduleAssignmentId, String.IsNullOrEmpty(userResponse));
			var module = await GetModule(moduleAssignment.Module.Id, String.IsNullOrEmpty(userResponse));
			var interactionId = GetInteractionId(moduleAssignment);
            var messages = GetMessages(userResponse, interactionId);

            moduleAssignment.Learner.UserStats.LastInteractionDateTime = DateTime.UtcNow.ToString("o");
            moduleAssignment.Learner.UserStats.InteractionsCount++;
            CosmosDbService.Instance.UpdateUserAsync(moduleAssignment.Learner.Id, moduleAssignment.Learner);
			CosmosDbService.Instance.UpdateModuleAssignmentAsync(moduleAssignment.Id, moduleAssignment);

			// Get LLM for the user
			var llm = GetRoleplayLLMForUser(moduleAssignment.Learner);

			if (String.IsNullOrWhiteSpace(userResponse) && await WhitelistService.Instance.CheckWhitelist(moduleAssignment.Learner.Email) != null)
			{
				await SendgridClientLibrary.SendEmailToPractissTeam(moduleAssignment.Learner, "Successfully Started Roleplay", module.Title);
			}

			Logger.LogError($@"{interactionId} : ProcessUserResponse Request : {userResponse}");

            if (string.IsNullOrWhiteSpace(userResponse))
            {
				module.InteractionsCount++;
				moduleAssignment.InteractionsCount++;
				CosmosDbService.Instance.UpdateModuleAssignmentAsync(moduleAssignmentId, moduleAssignment);

				interactionId = GetInteractionId(moduleAssignment);
                
                messages.Add(new Message()
                {
                    role = "system",
                    content = moduleAssignment.Module.Situation + String.Format(StringResources.Prompt_Situation_Suffix, module.Avatar.Name, moduleAssignment.Learner.FirstName)
                });

                MessagesDict.Remove(interactionId);
                MessagesDict.Add(interactionId, messages);

                InteractionStatsDict.Remove(interactionId);
                InteractionStatsDict.Add(interactionId, new InteractionStats() { InteractionStartTime = DateTime.UtcNow.ToString("o") });

                FeedbackDict.Remove(interactionId);
                FeedbackDict.Add(interactionId, new Feedback() { FeedbackProvider = moduleAssignment.Learner });
            }

            // If this is not the first message, 
            if (messages.Count > 1)
            {
                messages.Add(new Message()
                {
                    role = "user",
                    content = String.Format("[{0}] : {1}\n[{2}] : ",
                    moduleAssignment.Learner.FirstName,
                    userResponse,
					moduleAssignment.Module.Avatar.Name
					)
                });
            }
         
            int retries = 5;
            do
            {
                retries--;
                string response = "", stringResponse = "";

                switch (llmMode)
                {
                    case "nonfunction":
                        stringResponse = await llm.MakeApiRequest(
                            messages,
                            "m4");
                        stringResponse = ApiIntegrations.Helpers.SanitizeText(stringResponse, module.Avatar.Name);
                        break;

                    case "function":
                        response = await llm.MakeFunctionCallApiRequest(
                            messages,
                            StringResources.Function_Generate_Roleplay_Response,
                            "generate_roleplay_response",
                            "m4");

                        var llmResponse = JsonConvert.DeserializeObject<RoleplayDialogResponse>(response);
                        stringResponse = String.Format("<{0}> {1}", llmResponse.emotional_state, llmResponse.dialog_response);
                        break;

					case "streaming":
						stringResponse = llm.MakeStreamingApiRequest(
							messages,
							processSentence,
							"m4",
							module.Avatar.Name).Result;
						stringResponse = ApiIntegrations.Helpers.SanitizeText(stringResponse, module.Avatar.Name);
						break;

					default:
                        break;
                };


                Logger.LogInfo(stringResponse);
				if (messages.Count > 1)
				{
					messages.RemoveAt(messages.Count - 1);

					messages.Add(new Message()
					{
						role = "user",
						content = String.Format("[{0}] : {1}",
						moduleAssignment.Learner.FirstName,
						userResponse
						)
					});
				}

				messages.Add(new Message()
                {
                    role = "assistant",
                    content = String.Format("[{0}] : {1}",
                    module.Avatar.Name,
                    stringResponse)
                });

                switch(audioMode)
                {
                    case "nonstreaming":
                        return Convert.ToBase64String(await ApiIntegrations.STT.ElevenLabs.GetSTTBytesViaHttp(moduleAssignment.Module.Avatar.VoiceName, moduleAssignment.Module.Avatar.Personality, stringResponse));                      

                    case "streaming":
                        return stringResponse; 
                    
                    default: break;

                }
            }
            while (retries > 0);

            throw new RetriesExhaustedException();
        }

		/// <summary>
		/// For Api Entrypoint
		/// </summary>
		/// <param name="moduleAssignmentId"></param>
		/// <param name="userResponse"></param>
		/// <returns></returns>
        public static async Task<Stream> GetNextResponseStream(
            string moduleAssignmentId,
            string userResponse)
        {
			var moduleAssignment = await GetModuleAssignment(moduleAssignmentId);

			var llmResponse = await GetNextResponse(moduleAssignmentId,
                    userResponse,
                    "nonfunction",
                    "streaming");

            Logger.LogWarning(llmResponse);

            var stream = await ApiIntegrations.STT.ElevenLabs.GetSTTBytesViaHttpStream(moduleAssignment.Module.Avatar.VoiceName, moduleAssignment.Module.Avatar.Personality, llmResponse);
            return stream;
        }


		/// <summary>
		/// For UI Entrypoint
		/// </summary>
		/// <param name="moduleAssignmentId"></param>
		/// <param name="userResponse"></param>
		/// <param name="httpResponse"></param>
		/// <returns></returns>
		public static async Task GetNextResponseStream(
			string moduleAssignmentId,
			string userResponse,
			HttpResponse httpResponse)
        {
			int expectedAudioChunkSequenceNumber = 0;
			var outOfOrderAudioChunkResponses = new SortedDictionary<int, string>();

			ManualResetEvent sttComplete = new ManualResetEvent(false);

			// Wrap the delegate in a try-catch to ensure memory stream is correctly handled
			try
			{
				ProcessReceivedAudioStream processReceivedAudioChunk = async (audioChunk, sequenceNumber) =>
				{
					try
					{
						if (sequenceNumber == expectedAudioChunkSequenceNumber)
						{
							if (audioChunk == null)
							{
								sttComplete.Set();
								return;
							}

							byte[] chunkBytes = Convert.FromBase64String(audioChunk);
							await httpResponse.Body.WriteAsync(chunkBytes, 0, chunkBytes.Length);
							await httpResponse.Body.FlushAsync();
							Logger.LogWarning("Written Chunk to Http Response stream");

							expectedAudioChunkSequenceNumber++;

							while (outOfOrderAudioChunkResponses.ContainsKey(expectedAudioChunkSequenceNumber))
							{
								var nextChunk = outOfOrderAudioChunkResponses[expectedAudioChunkSequenceNumber];

								outOfOrderAudioChunkResponses.Remove(expectedAudioChunkSequenceNumber);

								if (nextChunk == null)
								{
									sttComplete.Set();
									return;
								}

								chunkBytes = Convert.FromBase64String(nextChunk);
								await httpResponse.Body.WriteAsync(chunkBytes, 0, chunkBytes.Length);
								await httpResponse.Body.FlushAsync();
								Logger.LogWarning("Written Chunk to Http Response stream");
								expectedAudioChunkSequenceNumber++;
							}
						}
						else if (sequenceNumber > expectedAudioChunkSequenceNumber)
						{
							outOfOrderAudioChunkResponses[sequenceNumber] = audioChunk;
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error processing audio chunk: {ex.Message}");
					}
				};

				var moduleAssignment = await GetModuleAssignment(moduleAssignmentId, String.IsNullOrEmpty(userResponse));
				var wsClient = ApiIntegrations.STT.ElevenLabs.CreateWSConnection(moduleAssignment.Module.Avatar.VoiceName, processReceivedAudioChunk);

				bool includeGenerationConfig = true;

				int expectedSentenceFragmentSequenceNumber = 0;
				var outOfOrderSentenceFragmentResponses = new SortedDictionary<int, string>();

				ProcessReceivedSentenceStream processReceivedSentenceFragment = (sentence, sequenceNumber) =>
				{
					if (sequenceNumber == expectedSentenceFragmentSequenceNumber)
					{
						// Process the received sentence fragment
						Logger.LogWarning("Sending Fragment To ElevenLabs " + sentence.Substring(0, Math.Min(40, sentence.Length)));

						ApiIntegrations.STT.ElevenLabs.SendWsDataAsync(wsClient, moduleAssignment.Module.Avatar.Personality, sentence, includeGenerationConfig);
						includeGenerationConfig = false; // only include for the first time
						expectedSentenceFragmentSequenceNumber++;

						// Check if the next sentence fragment is in the out of order map
						while (outOfOrderSentenceFragmentResponses.ContainsKey(expectedSentenceFragmentSequenceNumber))
						{
							var nextSentence = outOfOrderSentenceFragmentResponses[expectedSentenceFragmentSequenceNumber];
							outOfOrderSentenceFragmentResponses.Remove(expectedSentenceFragmentSequenceNumber);
							ApiIntegrations.STT.ElevenLabs.SendWsDataAsync(wsClient, moduleAssignment.Module.Avatar.Personality, nextSentence, false);
							expectedSentenceFragmentSequenceNumber++;
						}
					}
					else if (sequenceNumber > expectedSentenceFragmentSequenceNumber)
					{
						// Store out of order sentence fragments
						outOfOrderSentenceFragmentResponses[sequenceNumber] = sentence;
					}
				};

				await GetNextResponse(moduleAssignmentId,
					userResponse,
					"streaming",
					"streaming",
					processReceivedSentenceFragment);

				sttComplete.WaitOne();

				return;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw; // Re-throw the exception to signal the error condition
			}
		}


		public static async Task<string> WrapupInteraction(string moduleAssignmentId)
        {
			// Interaction started in different channel - api vs ui
			// return null so caller can process it via the other channel
			if(!ModuleAssignmentDict.ContainsKey(moduleAssignmentId))
			{
				return null; 
			}

            var moduleAssignment = await GetModuleAssignment(moduleAssignmentId);
            moduleAssignment.Module = await CosmosDbService.Instance.GetModuleAsync(moduleAssignment.Module.Id);
            var interactionId = GetInteractionId(moduleAssignment);
            var user = await CosmosDbService.Instance.GetUserAsync(moduleAssignment.Learner.Id);

			// Get LLM for the user
			var llm = GetReportLLMForUser(moduleAssignment.Learner);
			// var llm = new ClaudeApiClientLibrary();

            List<Message> messages = MessagesDict[interactionId];

			// Update Stats
			var interactionStats = InteractionStatsDict[interactionId];
            interactionStats.InteractionEndTime = DateTime.UtcNow.ToString("o");
            interactionStats.RoleplayMinutes = (DateTime.Parse(interactionStats.InteractionEndTime)
                                                - DateTime.Parse(interactionStats.InteractionStartTime)).TotalMinutes;

            
            user.UserStats.TotalRoleplayMinutes += interactionStats.RoleplayMinutes;
            user.UserStats.LastInteractionDateTime = DateTime.UtcNow.ToString("o");
			user.UserStats.InteractionsCount += messages.Count;

			await CosmosDbService.Instance.UpdateUserAsync(user.Id, user);

			Logger.LogError($@"Generating feedback for InteractionId - {interactionId} ");

            // Remove last one if it was spoken by the user. 
            // Gemini doesn't like if 2 user messages come 
            // together. One spoken by the user, and one prompt
            // modifier for the results prompt by the platform.

            if (messages[messages.Count - 1].role == "user")
            {
                messages.RemoveAt(messages.Count - 1);
            }

            messages.Add(new Message() { role = "user", content=" <ended the role play>" });


            // Remove the first one as it was a context setting system prompt
            messages.RemoveAt(0);

			List<Message> conversation = new List<Message>();
			foreach (var message in messages)
			{
				conversation.Add(new Message() { content = message.content, role = message.role });
			}

            var promptFormatString = StringResources.ContentPromptV3;
            promptFormatString += GetFormattedAdditionalQuestions(moduleAssignment.Module.Evaluation);

            // Start a new message list for evaluation
            messages = new List<Message>() { new Message()
            {
                role = "system",
                content = String.Format(promptFormatString,
                moduleAssignment.Learner.FirstName,
                moduleAssignment.Module.Title,
                DateTime.Now.ToLongDateString(),
                moduleAssignment.Module.Description)
            }};

            messages.Add(new Message()
            {
                role = "user",
                content = GetTranscript(conversation)
            });


			string evaluation = null;

			int maxRetries = 3;
			int attempt = 0;

            while (attempt < maxRetries)
            {
                evaluation = await llm.MakeApiRequest(messages, "m4turbo"); 

                // Check if evaluation contains "**" or "##"
                if (evaluation.Contains("**") || evaluation.Contains("##"))
                {
                    // Console.WriteLine("Evaluation successful.");
                    break;
                }
            }

            Report report = new Report();
            report.Id = Guid.NewGuid().ToString();
            
            report.Conversation = conversation;
            report.QuantitativeFeedbackV3 = Helpers.ExtractQuantitativeFeedbackV3(evaluation);
            report.AdditionalQuestions = Helpers.ExtractAdditionalQuestionsFeedback(evaluation);
            report.MarkdownV3 = evaluation;
            report.InteractionStats = interactionStats;
 			report.CurrentFeedback.Add(GetFeedback(interactionId));

            report.Date = DateTime.Now.ToString("o");
            report.ModuleAssignment = moduleAssignment;

            try
            {
                await CosmosDbService.Instance.CreateReportAsync(report);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

			if (await WhitelistService.Instance.CheckWhitelist(user.Email) != null)
			{
				await SendgridClientLibrary.SendEmailToPractissTeam(user, "Successfully Generated Report", report.Id);
			}

			return report.Id;
        }
		public static async Task<Report> RegenerateReport(string reportId)
		{
            var report = await CosmosDbService.Instance.GetReportByIdAsync(reportId);
            var moduleAssignment = report.ModuleAssignment;
			moduleAssignment.Module = await CosmosDbService.Instance.GetModuleAsync(moduleAssignment.Module.Id);

			// Get LLM for the user
			var llm = GetReportLLMForUser(moduleAssignment.Learner);
			// var llm = new ClaudeApiClientLibrary();

            List <Message> messages = new List<Message>();
            for (int i = 1; i < report.Conversation.Count - 1; i++)
			{
				messages.Add(new Message()
				{
					role = report.Conversation[i].role,
					content = report.Conversation[i].content
				});
            }

			var promptFormatString = StringResources.ContentPromptV3;
			promptFormatString += GetFormattedAdditionalQuestions(moduleAssignment.Module.Evaluation);

            // Start a new message list for evaluation
            messages = new List<Message>() { new Message()
			{
				role = "system",
				content = String.Format(promptFormatString,
				moduleAssignment.Learner.FirstName,
				moduleAssignment.Module.Title,
				DateTime.Parse(report.Date).ToLongDateString(), 
				moduleAssignment.Module.Description)
			}};

			messages.Add(new Message()
			{
				role = "user",
				content = GetTranscript(report.Conversation)
			});

			string evaluation = null;

			int maxRetries = 3;
			int attempt = 0;

			while (attempt < maxRetries)
			{
				evaluation = await llm.MakeApiRequest(messages, "m4turbo"); 

				// Check if evaluation contains "**" or "##"
				if (evaluation != null && (evaluation.Contains("**") || evaluation.Contains("##")))
				{
					// Console.WriteLine("Evaluation successful.");
					break;
				}
			}

			report.QuantitativeFeedbackV3 = Helpers.ExtractQuantitativeFeedbackV3(evaluation);
			report.AdditionalQuestions = Helpers.ExtractAdditionalQuestionsFeedback(evaluation);
			report.PriorFeedbacksV2.Add(new PriorFeedback() { 
				Feedbacks = report.CurrentFeedback,
				Conversation = report.Conversation,
				Markdown = report.Markdown,
				Markdown2 = report.MarkdownV2,
                Markdown3 = report.MarkdownV3
            });
            report.MarkdownV3 = evaluation;
            report.CurrentFeedback = new List<Feedback>();

			report.Date = DateTime.Now.ToString("o");

			try
			{
				await CosmosDbService.Instance.UpdateReportAsync(report.Id, report);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.ToString());
			}

			return report;
		}

        public static string GetTranscript(List<Message> messages)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Message message in messages)
            {
                switch(message.role)
                {
                    case "user":
                        sb.AppendLine( message.content); 
                        break;

                    default:
						sb.AppendLine(message.content.Replace("\r\n", " ").Replace("\n", ""));
						break;
				}
            }

            return sb.ToString();
        }

		// For in-progress interactions from the Interaction page
		public static void UpdateInteractionReaction(string moduleAssignmentId, int index, Reaction reaction)
		{
			var moduleAssignment = ModuleAssignmentDict[moduleAssignmentId];
			var interactionId = GetInteractionId(moduleAssignment); // Assuming ModuleAssignment has an Id property
			var feedback = GetFeedback(interactionId);

			// Find or create the reaction in the InteractionReactions list
			var existingReaction = feedback.InteractionReactions.FirstOrDefault(r => r.Index == index);
			if (existingReaction != null)
			{
                // Update existing reaction
                if (!String.IsNullOrWhiteSpace(reaction.Comments))
                {
                    existingReaction.ThumbsUp = reaction.ThumbsUp;
                    existingReaction.ThumbsDown = reaction.ThumbsDown;
                }
                else
                {
                    // if comment reaction, then don't update the thumbs up or thumbs down
                    existingReaction.Comments = reaction.Comments;
                }
			}
			else
			{
				// Add new reaction
				reaction.Index = index;
				feedback.InteractionReactions.Add(reaction);
			}
		}

		public static async Task<Reaction> FetchReportReaction(string reportId, string userId, int feedbackIndex)
		{
			var report = await CosmosDbService.Instance.GetReportByIdAsync(reportId);
			foreach (var feedback in report.CurrentFeedback)
			{
				if (feedback.FeedbackProvider.Id == userId)
				{
					return feedback.ReportReactions.FirstOrDefault(r => r.Index == feedbackIndex);
				}
			}
			return null; // Return null if no matching reaction is found
		}

		public static async Task<Reaction> FetchConversationReaction(string reportId, string userId, int feedbackIndex)
		{
			var report = await CosmosDbService.Instance.GetReportByIdAsync(reportId);
			foreach (var feedback in report.CurrentFeedback)
			{
				if (feedback.FeedbackProvider.Id == userId)
				{
					return feedback.InteractionReactions.FirstOrDefault(r => r.Index == feedbackIndex);
				}
			}
			return null; // Return null if no matching reaction is found
		}



		// For the first cardbody of the ReportDetails page
		public static async Task UpdateReportReaction(string reportId, 
														string userId, 
														int index,
														string type,
														bool isSolid,
														string comment)
		{
			var report = await CosmosDbService.Instance.GetReportByIdAsync(reportId);

			if (report.CurrentFeedback.Count == 0)
			{
				var newFeedback = new Feedback();
				newFeedback.FeedbackProvider = await CosmosDbService.Instance.GetUserAsync(userId);
                report.CurrentFeedback.Add(newFeedback);
            }

            // Loop through the CurrentFeedback
            foreach (var feedback in report.CurrentFeedback)
			{
				if (feedback.FeedbackProvider.Id == userId)
				{
					var existingReaction = feedback.ReportReactions.FirstOrDefault(r => r.Index == index);
					if (existingReaction == null)
					{
						existingReaction = new Reaction() { Index = index };
                        feedback.ReportReactions.Add(existingReaction);
                    }

					switch(type)
					{
						case "thumbsup":
							existingReaction.ThumbsUp = isSolid;
                            existingReaction.ThumbsDown = false;
                            break;
						case "thumbsdown":
							existingReaction.ThumbsDown = isSolid;
                            existingReaction.ThumbsUp = false;
                            break;
						case "comment":
							existingReaction.Comments = comment; 
							break;
						default:
							break;
					}

					await CosmosDbService.Instance.UpdateReportAsync(reportId, report);
					return;
				}		
			}

			return;
		}


		// For the second card body of the ReportDetails page
		public static async Task UpdateConversationReaction(string reportId,
                                                        string userId,
                                                        int index,
                                                        string type,
                                                        bool isSolid,
                                                        string comment)
        {
            var report = await CosmosDbService.Instance.GetReportByIdAsync(reportId);

            if (report.CurrentFeedback.Count == 0)
            {
                var newFeedback = new Feedback();
                newFeedback.FeedbackProvider = await CosmosDbService.Instance.GetUserAsync(userId);
                report.CurrentFeedback.Add(newFeedback);
            }

            // Loop through the CurrentFeedback
            foreach (var feedback in report.CurrentFeedback)
            {
                if (feedback.FeedbackProvider.Id == userId)
                {
                    var existingReaction = feedback.InteractionReactions.FirstOrDefault(r => r.Index == index);
                    if (existingReaction == null)
                    {
                        existingReaction = new Reaction() { Index = index };
                        feedback.InteractionReactions.Add(existingReaction);
                    }

                    switch (type)
                    {
                        case "thumbsup":
                            existingReaction.ThumbsUp = isSolid;
                            existingReaction.ThumbsDown = false;
                            break;
                        case "thumbsdown":
                            existingReaction.ThumbsDown = isSolid;
                            existingReaction.ThumbsUp = false;
                            break;
                        case "comment":
                            existingReaction.Comments = comment;
                            break;
                        default:
                            break;
                    }

                    await CosmosDbService.Instance.UpdateReportAsync(reportId, report);
                    return;
                }
            }

            return;
        }

		public static string GetFormattedAdditionalQuestions(string additionalQuestions)
		{
            if (String.IsNullOrWhiteSpace(additionalQuestions))
                return null;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(StringResources.AdditionalQuestionsPrefix);

            // Split the input string, removing any empty entries
            string[] lines = additionalQuestions.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Process each line except the last empty one, if any
            for (int i = 0; i < lines.Length; i++)
            {
                // Replace "[Learner]" with "Participant" and append the formatted line
                string formattedLine = "*  " + lines[i].Replace("the [LEARNER]", "participant").Replace("the [Learner]", "participant").Replace("the LEARNER", "participant").Replace("the Learner", "participant").Replace("the learner", "participant").Replace("[LEARNER]", "participant").Replace("[Learner]", "participant").Replace("LEARNER", "participant").Replace("Learner", "participant").Replace("learner", "participant") + " [Yes/No]";
				formattedLine += " - [If no, insert explanation justifying it]";

                sb.AppendLine(formattedLine);
            }

            return sb.ToString();
        }
	}
}