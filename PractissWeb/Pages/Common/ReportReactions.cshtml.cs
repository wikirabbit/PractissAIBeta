using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWorkflow;

namespace PractissWeb.Pages.Common
{
    public class SendReactionData
    {
        public int Index { get; set; }
        public bool ThumbsUp { get; set; }
        public bool ThumbsDown { get; set; }
        public string Comment { get; set; }
    }

    public class ReceiveReactionData
    {
        public string UserId { get; set; }
        public string ReportId { get; set; }
        public int CardBodyIndex { get; set; } // 0 for Report, 1 for Interaction
        public int FeedbackIndex { get; set; }
        public string Type { get; set; } // "thumbsUp", "thumbsDown", or "comment"
        public bool IsSolid { get; set; }
        public string Comment { get; set; }
    }

    public class ReportReactionsModel : BasePageModel
    {
        public ReportReactionsModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }


        // Handler for POST request
        public async Task<IActionResult> OnPostReactionAsync([FromBody] ReceiveReactionData reactionData)
        {
            // Convert ReceiveReactionData to Reaction
            Reaction reaction = new Reaction
            {
                Index = reactionData.FeedbackIndex,
                ThumbsUp = reactionData.Type == "thumbsup" && reactionData.IsSolid,
                ThumbsDown = reactionData.Type == "thumbsdown" && reactionData.IsSolid,
                Comments = reactionData.Comment
            };

            // Check the WidgetIndex to determine whether to update a report or interaction reaction
            if (reactionData.CardBodyIndex == 0)
            {
                // Update report reaction
                await InteractionWorkflow.UpdateReportReaction(reactionData.ReportId, 
                    reactionData.UserId, 
                    reactionData.FeedbackIndex,
                    reactionData.Type,
                    reactionData.IsSolid,
                    reactionData.Comment);
            }
            else if (reactionData.CardBodyIndex == 1)
            {
                // Update interaction reaction
                await InteractionWorkflow.UpdateConversationReaction(reactionData.ReportId, 
                    reactionData.UserId, 
                    reactionData.FeedbackIndex,
                    reactionData.Type,
                    reactionData.IsSolid,
                    reactionData.Comment);
            }
            else
            {
                // Log or handle unexpected WidgetIndex
                DataAccess.Logger.LogError($"Unexpected WidgetIndex: {reactionData.CardBodyIndex}");
                return new JsonResult(new { success = false, message = "Invalid WidgetIndex provided." });
            }

            // Assuming DataAccess.Logger.LogInfo exists and works as expected
            DataAccess.Logger.LogInfo($"Reaction updated for UserId: {reactionData.UserId}, ReportId: {reactionData.ReportId}, WidgetIndex: {reactionData.CardBodyIndex}");

            return new JsonResult(new { success = true, message = "Reaction updated successfully" });
        }
    }
}