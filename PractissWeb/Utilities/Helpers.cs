using ApiIntegrations.Misc;
using CommonTypes;
using PractissWorkflow;
using System.Text.RegularExpressions;

namespace PractissWeb.Utilities
{
	public class Helpers
    {
        static Dictionary<string, string> profileImages = new Dictionary<string, string>();

        public static string GetBase64StringForImage(string imagePath)
        {
            // Assuming imagePath is a relative path from the project root
            string webRootPath = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\wwwroot"; // Gets the root path of the application

            // If the above directory doesn't exist, then it is likely running
            // as an azure app service, which has a different path
            if (!Directory.Exists(webRootPath))
            {
                webRootPath = AppDomain.CurrentDomain.BaseDirectory + "\\wwwroot";
            }

            string fullPath = Path.Combine(webRootPath, imagePath.TrimStart('/').Replace('/', '\\'));

            if (File.Exists(fullPath))
            {
                byte[] imageBytes = File.ReadAllBytes(fullPath);
                return Convert.ToBase64String(imageBytes);
            }
            else
            {
                throw new FileNotFoundException("Image file not found.", fullPath);
            }
        }

        public static string SqueezeWhitespace(string input)
        {
            if (input == null)
                return null;

            // This regular expression matches one or more whitespace characters
            string pattern = @"\s+";

            // Replace all occurrences of the pattern with a single space
            return Regex.Replace(input, pattern, " ");
        }

        public static async Task StreamDataWithTimeoutAsync(Stream inputStream, Stream outputStream)
        {
            byte[] buffer = new byte[8192]; // Adjust the buffer size based on your needs
            int bytesRead;
            var cts = new CancellationTokenSource();

            while (true)
            {
                cts.CancelAfter(TimeSpan.FromSeconds(5)); // Reset the cancellation token timeout

                try
                {
                    // Attempt to read with a cancellation token that cancels after 5 seconds
                    bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                    if (bytesRead == 0)
                    {
                        // End of stream
                        break;
                    }

                    await outputStream.WriteAsync(buffer, 0, bytesRead, cts.Token);
                    // Optionally, flush the outputStream to ensure data is sent immediately
                    await outputStream.FlushAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Handle the timeout (5 seconds of inactivity) here
                    Console.WriteLine("Operation canceled due to inactivity.");
                    break; // Exit the loop and end the operation
                }

                // Reset the CancellationTokenSource for the next read cycle
                cts = new CancellationTokenSource();
            }
        }

        public static async Task SendReportEmail(Report report)
        {
            // Send email
            var subjectTemplate = await TemplateService.Instance.GetTemplate("SignedUpEmailSubjectTemplate");
            var bodyTemplate = await TemplateService.Instance.GetTemplate("SignedUpEmailBodyTemplate");
            await SendgridClientLibrary.SendEmail(report.ModuleAssignment.Learner.Email,
                      string.Format(subjectTemplate, report.ModuleAssignment.Learner.FirstName, report.ModuleAssignment.Learner.LastName, report.Id),
                      string.Format(bodyTemplate, report.ModuleAssignment.Learner.FirstName, report.ModuleAssignment.Learner.LastName, report.Id));

        }

        public static async Task<string> WrapupInteractionAndGetReportId(string moduleAssignmentId)
        {
            // Check via Api first
            string reportId = await PractissApiClientLibrary.WrapupInteractionAsync(moduleAssignmentId);
            Report report;

            if (String.IsNullOrEmpty(reportId))
            {
                // Interaction was in streaming mode

                reportId = await InteractionWorkflow.WrapupInteraction(moduleAssignmentId);
                report = await PractissApiClientLibrary.GetReportByIdAsync(reportId);
            }
            else
            {
                report = await PractissApiClientLibrary.GetReportByIdAsync(reportId);
            }

            return reportId;
        }

        public static async Task<string> GetProfileImage(string userId)
        {
            if (profileImages.ContainsKey(userId))
            {
                return profileImages[userId];
            }
            else
            {
                var user = await PractissApiClientLibrary.GetUserAsync(userId);
                profileImages.Add(userId, user.ProfileImage);
                return user.ProfileImage;
            }
        }

        public static void CalculateFinalScore(QuantitativeFeedback quantitativeFeedback)
        {
            // Initialize sum and count of available scores
            int sum = 0;
            int count = 0;

            // Check each score and add to sum if available, also increment count
            if (quantitativeFeedback.Clarity != -1) { sum += quantitativeFeedback.Clarity; count++; }
            if (quantitativeFeedback.Influence != -1) { sum += quantitativeFeedback.Influence; count++; }
            if (quantitativeFeedback.EmotionalIntelligence != -1) { sum += quantitativeFeedback.EmotionalIntelligence; count++; }
            if (quantitativeFeedback.Listening != -1) { sum += quantitativeFeedback.Listening; count++; }
        }

        public static int CalculateFinalScoreV2(QuantitativeFeedbackV2 quantitativeFeedback)
        {
            // Initialize sum and count of available scores
            int sum = 0;
            int count = 0;

            // Add each score to the sum and increment count for each non-negative score
            if (quantitativeFeedback.ClarityOfCommunication != -1) { sum += quantitativeFeedback.ClarityOfCommunication; count++; }
            if (quantitativeFeedback.ActiveListening != -1) { sum += quantitativeFeedback.ActiveListening; count++; }
            if (quantitativeFeedback.EmotionalIntelligence != -1) { sum += quantitativeFeedback.EmotionalIntelligence; count++; }
            if (quantitativeFeedback.Persuasiveness != -1) { sum += quantitativeFeedback.Persuasiveness; count++; }
            if (quantitativeFeedback.ProblemSolvingAndAdaptability != -1) { sum += quantitativeFeedback.ProblemSolvingAndAdaptability; count++; }
            if (quantitativeFeedback.ProfessionalismAndDecorum != -1) { sum += quantitativeFeedback.ProfessionalismAndDecorum; count++; }
            if (quantitativeFeedback.ImpactAndInfluence != -1) { sum += quantitativeFeedback.ImpactAndInfluence; count++; }
            if (quantitativeFeedback.ArticulateCommunication != -1) { sum += quantitativeFeedback.ArticulateCommunication; count++; }

            // Calculate average if count is more than 0, otherwise return 0 as final score
            double average = count > 0 ? (double)sum / Math.Max(count, 6) : 0;
            var finalScore = average * 10;

            return (int)finalScore;
        }

        public static int RevalidateScoresV2(Report report)
        {
            var quantitativeFeedback = report.QuantitativeFeedbackV2;

            // Initialize sum and count of available scores
            int sum = 0;
            int countSignalsObserved = 0;

            // Add each score to the sum and increment count for each non-negative score
            if (quantitativeFeedback.ClarityOfCommunication != -1) { sum += quantitativeFeedback.ClarityOfCommunication; countSignalsObserved++; }
            if (quantitativeFeedback.ActiveListening != -1) { sum += quantitativeFeedback.ActiveListening; countSignalsObserved++; }
            if (quantitativeFeedback.EmotionalIntelligence != -1) { sum += quantitativeFeedback.EmotionalIntelligence; countSignalsObserved++; }
            if (quantitativeFeedback.Persuasiveness != -1) { sum += quantitativeFeedback.Persuasiveness; countSignalsObserved++; }
            if (quantitativeFeedback.ProblemSolvingAndAdaptability != -1) { sum += quantitativeFeedback.ProblemSolvingAndAdaptability; countSignalsObserved++; }
            if (quantitativeFeedback.ProfessionalismAndDecorum != -1) { sum += quantitativeFeedback.ProfessionalismAndDecorum; countSignalsObserved++; }
            if (quantitativeFeedback.ImpactAndInfluence != -1) { sum += quantitativeFeedback.ImpactAndInfluence; countSignalsObserved++; }
            if (quantitativeFeedback.ArticulateCommunication != -1) { sum += quantitativeFeedback.ArticulateCommunication; countSignalsObserved++; }

            // Calculate how many questions were answered successfully
            int countQuestionsAnswered = 0;
            int countQuestionsUnanswered = 0;
            if (report.AdditionalQuestions != null && report.AdditionalQuestions.Count != 0)
            {
                countQuestionsAnswered = report.AdditionalQuestions.Where(x => x.Value == true).Count();
                countQuestionsUnanswered = report.AdditionalQuestions.Count - countQuestionsAnswered;
            }

            if (report.Conversation.Count < 7)
            {
                // conversation was too short
                quantitativeFeedback.ClarityOfCommunication = -1;
                quantitativeFeedback.ActiveListening = -1;
                quantitativeFeedback.EmotionalIntelligence = -1;
                quantitativeFeedback.Persuasiveness = -1;
                quantitativeFeedback.ProblemSolvingAndAdaptability = -1;
                quantitativeFeedback.ProfessionalismAndDecorum = -1;
                quantitativeFeedback.ArticulateCommunication = -1;
                quantitativeFeedback.ImpactAndInfluence = -1;

                foreach (var question in report.AdditionalQuestions.Keys)
                {
                    report.AdditionalQuestions[question] = false;
                }

                if (report.Conversation.Count <= 2)
                {
                    // no conversation happened
                    report.MarkdownV2 = String.Empty;
                    return 0;
                }
            }

            if (report.AdditionalQuestions.Count > 0)
            {
                return 100 * countQuestionsAnswered / (countQuestionsAnswered + countQuestionsUnanswered);
            }
            else
            {
                return 100;
            }
        }

        public static int CalculateFinalScoreV3(QuantitativeFeedbackV3 quantitativeFeedback)
        {
            // Initialize sum and count of available scores
            int sum = 0;
            int count = 0;

            // Add each score to the sum and increment count for each non-negative score
            if (quantitativeFeedback.ClarityOfCommunication != -1) { sum += quantitativeFeedback.ClarityOfCommunication; count++; }
            if (quantitativeFeedback.ActiveListening != -1) { sum += quantitativeFeedback.ActiveListening; count++; }
            if (quantitativeFeedback.EmotionalIntelligence != -1) { sum += quantitativeFeedback.EmotionalIntelligence; count++; }
            if (quantitativeFeedback.ProblemSolvingAndAdaptability != -1) { sum += quantitativeFeedback.ProblemSolvingAndAdaptability; count++; }
            if (quantitativeFeedback.ProfessionalismAndDecorum != -1) { sum += quantitativeFeedback.ProfessionalismAndDecorum; count++; }
            if (quantitativeFeedback.InfluentialCommunication != -1) { sum += quantitativeFeedback.InfluentialCommunication; count++; }

            // Calculate average if count is more than 0, otherwise return 0 as final score
            double average = count > 0 ? (double)sum / 6 : 0;
            var finalScore = average * 10;

            return (int)finalScore;
        }

        public static int RevalidateScoresV3(Report report)
        {
            var quantitativeFeedback = report.QuantitativeFeedbackV3;

            // Initialize sum and count of available scores
            int sum = 0;
            int countSignalsObserved = 0;

            // Add each score to the sum and increment count for each non-negative score
            if (quantitativeFeedback.ClarityOfCommunication != -1) { sum += quantitativeFeedback.ClarityOfCommunication; countSignalsObserved++; }
            if (quantitativeFeedback.ActiveListening != -1) { sum += quantitativeFeedback.ActiveListening; countSignalsObserved++; }
            if (quantitativeFeedback.EmotionalIntelligence != -1) { sum += quantitativeFeedback.EmotionalIntelligence; countSignalsObserved++; }
            if (quantitativeFeedback.ProblemSolvingAndAdaptability != -1) { sum += quantitativeFeedback.ProblemSolvingAndAdaptability; countSignalsObserved++; }
            if (quantitativeFeedback.ProfessionalismAndDecorum != -1) { sum += quantitativeFeedback.ProfessionalismAndDecorum; countSignalsObserved++; }
            if (quantitativeFeedback.InfluentialCommunication != -1) { sum += quantitativeFeedback.InfluentialCommunication; countSignalsObserved++; }

            // Calculate how many questions were answered successfully
            int countQuestionsAnswered = 0;
            int countQuestionsUnanswered = 0;
            if (report.AdditionalQuestions != null && report.AdditionalQuestions.Count != 0)
            {
                countQuestionsAnswered = report.AdditionalQuestions.Where(x => x.Value == true).Count();
                countQuestionsUnanswered = report.AdditionalQuestions.Count - countQuestionsAnswered;
            }

            if (report.Conversation.Count < 7)
            {
                // conversation was too short
                quantitativeFeedback.ClarityOfCommunication = -1;
                quantitativeFeedback.ActiveListening = -1;
                quantitativeFeedback.EmotionalIntelligence = -1;
                quantitativeFeedback.ProblemSolvingAndAdaptability = -1;
                quantitativeFeedback.ProfessionalismAndDecorum = -1;
                quantitativeFeedback.InfluentialCommunication = -1;


                foreach (var question in report.AdditionalQuestions.Keys)
                {
                    report.AdditionalQuestions[question] = false;
                }

                if (report.Conversation.Count <= 2)
                {
                    // no conversation happened
                    report.MarkdownV3 = String.Empty;
                    return 0;
                }
            }

            if (report.AdditionalQuestions.Count > 0)
            {
                return  100 * countQuestionsAnswered / (countQuestionsAnswered + countQuestionsUnanswered);
            }
            else
            {
                return 100;
            }
        }

        public static Module ParseModule(string input)
        {
            var module = new Module();
            module.Id = Guid.NewGuid().ToString();

            var sections = Regex.Split(input, @"\*\*(?=[^*]+:\*\*)").Where(section => !string.IsNullOrWhiteSpace(section));

            string currentSection = "";

            foreach (var section in sections)
            {
                if (section.StartsWith("Title:"))
                {
                    module.Title = section.Replace("Title:**", "").Trim();
                }
                else if (section.StartsWith("Description:"))
                {
                    module.Description = section.Replace("Description:**", "").Trim();
                }
                else if (section.StartsWith("Scenario for AI Interaction:"))
                {
					module.Situation = section.Replace("Scenario for AI Interaction:**", "").Trim();
                }
                else if (section.StartsWith("Evaluation:"))
                {
					module.Evaluation = section.Replace("Evaluation:**", "").Trim();
                }
                else if (section.StartsWith("Avatar:"))
                {
                    module.Avatar = new Avatar();
                    module.Avatar.Name = section.Replace("Avatar:**", "").Trim();
                }
            }

            // Trim the final results to remove any extra new line characters
            module.Description = module.Description.Trim();
            module.Situation = module.Situation.Trim();

			// Split the input into lines, trim spaces and hyphen from the start of each line, then reassemble
			module.Evaluation = String.Join("\n", module.Evaluation.Split(new[] { "\n" }, StringSplitOptions.None)
				.Select(line => line.TrimStart().TrimStart('-').TrimStart()));


			return module;
        }

        public static async Task CreateDefaultAvatars(string userId, List<Avatar> avatars)
        {

            foreach (var avatarId in new string[]{"318b43fa-3d99-40c2-9e39-dc3af4f217af",
                                                    "666347d4-c565-49f2-a35b-afa59f103ed5",
                                                    "f02be141-1f2d-4a80-9422-b045a2cee7e3",
                                                    "22947825-9d23-489c-82ea-7f985b848f6a" })
            {
                var referenceAvatar = await PractissApiClientLibrary.GetAvatarAsync(avatarId);
                var defaultAvatar = avatars.Where(a => a.Name == referenceAvatar.Name && a.Personality == referenceAvatar.Personality).FirstOrDefault();

                if (defaultAvatar == null)
                {
                    Avatar clone = new Avatar()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = referenceAvatar.Name,
                        AuthorId = userId,
                        Image = referenceAvatar.Image,
                        ImageCode = referenceAvatar.ImageCode,
                        Personality = referenceAvatar.Personality,
                        VoiceName = referenceAvatar.VoiceName,
                        VoiceSampleUrl = referenceAvatar.VoiceSampleUrl
                    };
                    await PractissApiClientLibrary.CreateAvatarAsync(clone);
                    avatars.Add(clone);
                }
            }
        }
    }
}