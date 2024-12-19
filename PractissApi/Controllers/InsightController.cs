using CommonTypes;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace PractissApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class InsightController : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> CreateInsight([FromBody] Insight insight)
		{
			await CosmosDbService.Instance.CreateInsightAsync(insight);
			return Ok();
		}

		[HttpGet("{userId}")]
		public async Task<IActionResult> GetInsights(string userId)
		{
			var insights = await CosmosDbService.Instance.GetInsightsAsync(userId);

			return Ok(insights);
		}

		[HttpGet("user/{userId}/insight/{insightId}")]
		public async Task<IActionResult> GetInsightById(string userId, string insightId)
		{
			var insight = await CosmosDbService.Instance.GetInsightByIdAsync(userId, insightId);

			return Ok(insight);
		}

		[HttpPut]
		public async Task<IActionResult> UpdateInsight([FromBody] Insight insight)
		{
			var result = await CosmosDbService.Instance.CreateInsightAsync(insight);
			return Ok(result);
		}

		[HttpDelete("user/{userId}/insight/{insightId}")]
		public async Task<IActionResult> DeleteInsight(string userId, string insightId)
		{
			var insight = await CosmosDbService.Instance.GetInsightByIdAsync(userId, insightId);
			await CosmosDbService.Instance.DeleteInsightAsync(insight);
			return NoContent();
		}
	}

}
