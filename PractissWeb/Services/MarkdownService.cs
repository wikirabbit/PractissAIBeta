using CommonTypes;
using Markdig;
using System.Text;

namespace PractissWeb.Services
{
    public class MarkdownService
    {
        public string ConvertMarkdownToHtml(string markdown)
        {
            var html = Markdown.ToHtml(markdown ?? string.Empty, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
            return html;
        }

        public string ConvertConversationToMarkdown(List<Message> messages)
        {
            StringBuilder conversation = new StringBuilder();

            for (int i = 0; i < messages.Count - 1; i++)
            {
                // Replace brackets with bold syntax in Markdown
                string formattedContent = messages[i].content.Replace("[", "**").Replace("]", "**");

                // Split the message content into lines
                string[] lines = formattedContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                // Append the first line with the list item syntax
                conversation.Append("- " + lines[0]);

                // Append subsequent lines with indentation
                for (int j = 1; j < lines.Length; j++)
                {
                    conversation.Append("\n    " + lines[j]); // Note: 4 spaces for indentation
                }

                // Add a new line after each message to separate list items
                if (i < messages.Count - 1)
                {
                    conversation.Append("\n");
                }
            }

            return conversation.ToString();
        }

        public string AddSpeakerClassesAndNamesToHtml(string htmlContent)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Select all list items
            var listItems = doc.DocumentNode.SelectNodes("//li");
            if (listItems != null)
            {
                bool isSpeaker1 = true; // Alternating flag
                foreach (var listItem in listItems)
                {
                    var strongTag = listItem.SelectSingleNode(".//strong");
                    if (strongTag != null)
                    {
                        var speakerName = strongTag.InnerText; // Extracts the text inside the <strong> tag
                        var speakerClass = isSpeaker1 ? "speaker1" : "speaker2";
                        listItem.Attributes.Add("class", speakerClass);

                        // Remove the <strong> tag and replace it with a span having the speaker-name class
                        strongTag.Remove(); // First remove the <strong> tag
                        listItem.InnerHtml = $"<span class='speaker-name'>{speakerName}:</span> {listItem.InnerHtml.Replace(":", "").Trim()}";

                        isSpeaker1 = !isSpeaker1; // Alternate the flag
                    }
                }
            }

            return doc.DocumentNode.OuterHtml;
        }

    }
}