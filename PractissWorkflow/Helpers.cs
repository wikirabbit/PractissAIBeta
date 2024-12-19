using CommonTypes;
using System.Text.RegularExpressions;

namespace PractissWorkflow
{
	public class Helpers
    {
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


		public static Dictionary<string, bool> ExtractAdditionalQuestionsFeedback(string text)
        {
            // Adjust the pattern to match lines ending with "[Yes]" or "[No]".
            var questionPattern = @"(.+?) (Yes|No)( - .*)?$";
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var feedback = new Dictionary<string, bool>();

            foreach (var line in lines)
            {
                var cleanedLine = line.Trim().TrimStart('*').Trim().Replace("[", "").Replace("]", " - "); // Trim spaces and leading asterisks
                var match = Regex.Match(cleanedLine, questionPattern);
                if (match.Success)
                {
                    // Extract the whole question as key, excluding the final " [Yes]" or " [No]"
                    var question = match.Groups[1].Value;
                    var answer = match.Groups[2].Value;

                    // Convert "Yes"/"No" to boolean
                    bool answerBool = answer.Equals("Yes", StringComparison.OrdinalIgnoreCase);

                    feedback[question] = answerBool;
                }
            }
            return feedback;
        }
    }
}