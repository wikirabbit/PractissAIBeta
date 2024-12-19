using CommonTypes;
using DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace PractissApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ModuleAssignmentController : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> CreateModuleAssignment([FromBody] CommonTypes.ModuleAssignment moduleAssignment)
		{
			var result = await CosmosDbService.Instance.CreateModuleAssignmentAsync(moduleAssignment);
			return Ok(result);
		}

		[HttpGet("learner/{learnerId}")]
		public async Task<IActionResult> GetModuleAssignmentByLearner(string learnerId)
		{
			var assignments = await CosmosDbService.Instance.GetModuleAssignmentByLearnerAsync(learnerId);
			return Ok(assignments);
		}

		[HttpGet("{moduleAssignmentId}")]
		public async Task<IActionResult> GetModuleAssignmentById(string moduleAssignmentId)
		{
			var assignment = await CosmosDbService.Instance.GetModuleAssignmentByIdAsync(moduleAssignmentId);
			return Ok(assignment);
		}

		[HttpGet("coach/{coachId}/module/{moduleId}")]
		public async Task<IActionResult> GetModuleAssignmentByCoach(string coachId, string moduleId)
		{
			var assignments = await CosmosDbService.Instance.SearchModuleAssignmentByCoachAsync(coachId, moduleId);
			return Ok(assignments);
		}
		[HttpGet("coach/{coachId}/module/{moduleId}/learner/{learnerId}")]
		public async Task<IActionResult> GetModuleAssignmentByCoachModuleLearner(string coachId, string moduleId, string learnerId)
		{
			var assignment = await CosmosDbService.Instance.GetModuleAssignmentByCoachModuleLearnerAsync(coachId, moduleId, learnerId);
			return Ok(assignment);
		}


		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateModuleAssignment(string id, [FromBody] ModuleAssignment moduleAssignment)
		{
			var result = await CosmosDbService.Instance.UpdateModuleAssignmentAsync(id, moduleAssignment);
			return Ok(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteModuleAssignment(string id)
		{
			await CosmosDbService.Instance.DeleteModuleAssignmentAsync(id);
			return NoContent();
		}

	}

}
