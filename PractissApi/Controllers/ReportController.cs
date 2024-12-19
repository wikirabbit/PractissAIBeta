using CommonTypes;
using DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace PractissApi.Controllers
{
	public class ReportSearchRequest
	{
		public string? LearnerId { get; set; }
		public string? CoachId { get; set; }
	}

	[Route("api/[controller]")]
	[ApiController]
	public class ReportController : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> CreateReport([FromBody] Report report)
		{
			await CosmosDbService.Instance.CreateReportAsync(report);
			return Ok();
		}

		[HttpGet("{reportId}/regeneratereport")]
		public async Task<IActionResult> RegenerateReport(string reportId)
		{
			var report = await PractissWorkflow.InteractionWorkflow.RegenerateReport(reportId);

			return Ok(report);
		}

		[HttpPost("search")]
		public async Task<IActionResult> GetReports([FromBody] ReportSearchRequest request)
		{
			// Log the incoming request for debugging purposes
			Logger.LogInfo($"Received request with LearnerId: {request?.LearnerId}, CoachId: {request?.CoachId}");

			// Access parameters from the request body
			var reports = await CosmosDbService.Instance.SearchReportsByLearnerOrCoachIdAsync(request.LearnerId, request.CoachId);

			return Ok(reports);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetReport(string id)
		{
			var report = await CosmosDbService.Instance.GetReportByIdAsync(id);

			return Ok(report);
		}

		[HttpPut]
		public async Task<IActionResult> UpdateReport([FromBody] Report report)
		{
			var result = await CosmosDbService.Instance.UpdateReportAsync(report.Id, report);
			return Ok(result);
		}
	}

}
