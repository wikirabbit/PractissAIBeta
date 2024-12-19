using ApiIntegrations.Misc;
using DataAccess;
using System.Text.RegularExpressions;

namespace ApiIntegrations
{
	public class Helpers
    {
        public delegate void RetryDelegate(Action retryCode, int maxTries, string description);

        public static void Retry(Action retryCode, int maxTries, string description)
        {
            for (int i = 0; i < maxTries; i++)
            {
                try
                {
                    retryCode();
                    return;
                }
                catch (Exception e)
                {
                    if (i == maxTries - 1)
                    {
                        throw;
                    }
                    Logger.LogError(e.ToString());
                    Logger.LogError($@"{description} : Retry attempt #{i + 1}");
                }
            }
        }

		public static string SanitizeText(string input, string avatarName)
		{
			input = RemoveMarkdown(input);
			input = RemoveAvatarName(input, avatarName);
			input = RemoveSubstringsWithinBrackets(input);
			input = RemoveSubstringUpUntilColon(input);
			input = input.Replace("*", "");

			return input;
		}

		static string RemoveAvatarName(string input, string avatarName)
		{
			try
			{
				// Regular expression to match words (sequences of letter characters)
				Regex regex = new Regex(@"\b\w+\b");

				// Find matches in the input string
				MatchCollection matches = regex.Matches(input);

				if (matches.Count > 0)
				{
					if (matches[0].Value.ToLower() != avatarName.ToLower())
					{
						return input;
					}
					else
					{
						return input.Substring(matches[1].Index);
					}
				}
			}catch(Exception ex) 
			{
				string message = String.Format("Removing avatar name had an issue - avatar name {0}, input {1}", avatarName, input);
				SendgridClientLibrary.SendErrorAlert(message).Wait();
				Logger.LogError(message);
			}

			return input;
		}

		static string RemoveSubstringsWithinBrackets(string input)
		{
			var stack = new Stack<(char type, int index)>();
			var removalIndices = new List<(int start, int end)>();

			for (int i = 0; i < input.Length; i++)
			{
				char current = input[i];
				if (current == '(' || current == '[' || current == '{')
				{
					stack.Push((current, i));
				}
				else if ((current == ')' || current == ']' || current == '}') && stack.Count > 0)
				{
					var (openingBracket, startIndex) = stack.Peek();
					// Ensure matching types
					if ((current == ')' && openingBracket == '(') ||
						(current == ']' && openingBracket == '[') ||
						(current == '}' && openingBracket == '{'))
					{
						stack.Pop(); // Remove matched pair
						removalIndices.Add((startIndex, i));
					}
					else
					{
						// Handle mismatch case, optional: throw an exception or handle error
						Console.WriteLine("Mismatched brackets found.");
						return input; // Return original input or handle as needed
					}
				}
			}

			// Removing substrings from the end to not mess with the indices
			removalIndices.Reverse();
			foreach (var (start, end) in removalIndices)
			{
				input = input.Remove(start, end - start + 1);
			}

			return input;
		}

		static string RemoveMarkdown(string markdown)
		{
			if (string.IsNullOrEmpty(markdown))
				return markdown;

			// Patterns for matching and removing Markdown
			string[] patterns = {
		@"\*\*(.*?)\*\*", // Bold
        @"__(.*?)__", // Bold
        @"\*(.*?)\*", // Italic
        @"_(.*?)_", // Italic
        @"\~\~(.*?)\~\~", // Strikethrough
        @"\[(.*?)\]\((.*?)\)", // Links
        @"!\[(.*?)\]\((.*?)\)", // Images
        @"#(.*?)\n", // Headers
        @"`(.*?)`", // Inline code
        @"\n-{3,}", // Horizontal rule
        // Add more patterns as needed
    };

			string plainText = markdown;

			// Remove Markdown formatting
			foreach (string pattern in patterns)
			{
				plainText = Regex.Replace(plainText, pattern, "$1");
			}

			// Optional: Remove any remaining HTML tags that might be present
			plainText = Regex.Replace(plainText, "<.*?>", string.Empty);

			return plainText;
		}

		static string RemoveSubstringUpUntilColon(string input)
		{
			// Find the index of the first colon
			int colonIndex = input.IndexOf(':');

			// Check if the colon was found
			if (colonIndex != -1)
			{
				// Remove the substring up until and including the colon
				// Add 1 to start the substring after the colon
				return input.Substring(colonIndex + 1).TrimStart();
			}

			// Return the original string if no colon was found
			return input;
		}

		static public long ConvertDateTimeToUnixMilliseconds(DateTime dateTime)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return (long)(dateTime.ToUniversalTime() - epoch).TotalMilliseconds;
		}
	}
}