using ApiIntegrations.Misc;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PractissWeb.Utilities;
using PractissWorkflow;
using System;

namespace PractissWeb.Controllers
{
    [Route("Interaction")]
    public class InteractionController : Controller
    {
        [HttpGet("GetNextResponseStream")]
        public async Task GetNextResponseStreamV2(string moduleAssignmentId, string userResponse)
        {
			Response.ContentType = "audio/mpeg";

            try
            {
                await InteractionWorkflow.GetNextResponseStream(moduleAssignmentId, userResponse, HttpContext.Response);
            }
            catch (Exception ex)
            {
				var userId = HttpContext.Session.GetString("UserId");
				await SendgridClientLibrary.SendErrorAlert(ex.ToString(), userId);
				throw;
			}
        }

        [HttpGet("WrapupInteraction")]
        public async Task<IActionResult> WrapupInteraction(string moduleAssignmentId)
        {
            try
            {
                string reportId = await Utilities.Helpers.WrapupInteractionAndGetReportId(moduleAssignmentId);

                return Ok(reportId);
            }
			catch (Exception ex)
			{
				var userId = HttpContext.Session.GetString("UserId");
				await SendgridClientLibrary.SendErrorAlert(ex.ToString(), userId);
				throw;
			}
		}

        [HttpGet("RegenerateReport")]
        public async Task<IActionResult> RegenerateReport(string reportId)
        {
            try
            {
                await PractissApiClientLibrary.RegenerateReportAsync(reportId);

                return Ok();
            }
			catch (Exception ex)
			{
                var userId = HttpContext.Session.GetString("UserId");
				await SendgridClientLibrary.SendErrorAlert(ex.ToString(), userId);
				throw;
			}
		}
    }
}
