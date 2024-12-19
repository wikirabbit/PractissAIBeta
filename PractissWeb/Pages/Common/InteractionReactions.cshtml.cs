using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Common
{
    public class InteractionReactionsModel : BasePageModel
    {
        [BindProperty]
        public ReceivedInteractionReactionData Reaction { get; set; }

        [BindProperty]
        public ReceivedInteractionCommentData Comment { get; set; }

        public InteractionReactionsModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public void OnGet()
        {
            // This method is empty since we're focusing on POST handlers
        }

        public async Task<IActionResult> OnPostReactionAsync([FromBody] ReceivedInteractionReactionData reactionData)
        {
            var reaction = new CommonTypes.Reaction()
            {
                ThumbsUp = (reactionData.Reaction == "up"),
                ThumbsDown = (reactionData.Reaction == "down"),
                Index = reactionData.Index
            };

            await PractissApiClientLibrary.AddInteractionReactionAsync(reactionData.ModuleAssignmentId, reaction);


            return new JsonResult(new { success = true, message = "Reaction processed successfully." });
        }

        public async Task<IActionResult> OnPostCommentAsync([FromBody] ReceivedInteractionCommentData commentData)
        {

            var reaction = new CommonTypes.Reaction() { Comments = commentData.Comment, Index = commentData.Index };

            await PractissApiClientLibrary.AddInteractionReactionAsync(commentData.ModuleAssignmentId, reaction);

            return new JsonResult(new { success = true, message = "Comment processed successfully." });
        }
    }

    public class ReceivedInteractionReactionData
    {
        public string ModuleAssignmentId { get; set; }
        public int Index { get; set; }
        public string Reaction { get; set; }
        // Include other properties as necessary
    }

    public class ReceivedInteractionCommentData
    {
        public string ModuleAssignmentId { get; set; }
        public int Index { get; set; }
        public string Comment { get; set; }
        // Include other properties as necessary
    }
}
