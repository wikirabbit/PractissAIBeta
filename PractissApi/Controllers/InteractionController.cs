using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWorkflow;

namespace PractissApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class InteractionController : ControllerBase
	{
		[HttpPost("{moduleAssignmentId}/getnextresponse")]
		public async Task<IActionResult> GetNextResponse(string moduleAssignmentId, [FromBody] string userResponse)
		{
			if (userResponse == "empty")
				userResponse = null;

			var base64AudioResponse = await InteractionWorkflow.GetNextResponse(moduleAssignmentId, userResponse);
			return Ok(base64AudioResponse);
		}

		[HttpPost("{moduleAssignmentId}/getnextresponsestream")]
		public async Task<IActionResult> GetNextResponseStream(string moduleAssignmentId, [FromBody] string userResponse)
		{
			if (string.IsNullOrEmpty(userResponse) || userResponse == "empty")
			{
				userResponse = null;
			}

			HttpContext.Response.ContentType = "audio/mpeg";

			try
			{
				var stream = await InteractionWorkflow.GetNextResponseStream(
					moduleAssignmentId,
					userResponse);

				return new FileStreamResult(stream, "audio/mpeg");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred while streaming: {ex.Message}");
				throw;
			}
		}

		[HttpPost("{moduleAssignmentId}/wrapupinteraction")]
		public async Task<IActionResult> WrapupInteraction(string moduleAssignmentId)
		{
			string reportId = await PractissWorkflow.InteractionWorkflow.WrapupInteraction(moduleAssignmentId);

			return Ok(reportId);
		}

		[HttpPost("{moduleAssignmentId}/addinteractionreaction")]
		public async Task<JsonResult> AddInteractionReaction(string moduleAssignmentId, [FromBody] Reaction reaction)
		{
			PractissWorkflow.InteractionWorkflow.UpdateInteractionReaction(moduleAssignmentId, reaction.Index, reaction);

			return new JsonResult(new { success = false, message = "No reaction found." });
		}
	}
}