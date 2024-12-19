using CommonTypes;
using DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace PractissApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CoachClientRelationshipController : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> CreateClient([FromBody] CoachClientRelationship relationship)
		{
			var result = await CosmosDbService.Instance.CreateClientAsync(relationship);
			return Ok(result);
		}

		[HttpGet("{coachId}")]
		public async Task<IActionResult> GetClients(string coachId)
		{
			var clients = await CosmosDbService.Instance.GetClientsAsync(coachId);
			return Ok(clients);
		}

		[HttpDelete("coach/{coachId}/relationship/{relationshipId}")]
		public async Task<IActionResult> DeleteClient(string relationshipId, string coachId)
		{
			await CosmosDbService.Instance.DeleteClientAsync(relationshipId, coachId);
			return NoContent();
		}
	}

}
