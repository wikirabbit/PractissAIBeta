using DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace PractissApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ModuleController : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> CreateModule([FromBody] CommonTypes.Module module)
		{
			var author = await CosmosDbService.Instance.GetUserAsync(module.Author.Id);
			author.UserStats.ModulesCreated++;
			await CosmosDbService.Instance.UpdateUserAsync(author.Id, author);

			var result = await CosmosDbService.Instance.CreateModuleAsync(module);
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetModule(string id)
		{
			var module = await CosmosDbService.Instance.GetModuleAsync(id);
			if (module == null)
				return NotFound();

			return Ok(module);
		}

		[HttpGet("author/{authorId}")]
		public async Task<IActionResult> GetModulesByAuthor(string authorId)
		{
			var modules = await CosmosDbService.Instance.GetModulesByAuthor(authorId);
			return Ok(modules);
		}

		[HttpGet("search")]
		public async Task<IActionResult> SearchForModule([FromQuery] string moduleName, [FromQuery] string authorName)
		{
			var modules = await CosmosDbService.Instance.SearchForModule(moduleName, authorName);
			return Ok(modules);
		}


		[HttpPut]
		public async Task<IActionResult> UpdateModule([FromBody] CommonTypes.Module module)
		{
			var result = await CosmosDbService.Instance.UpdateModuleAsync(module.Id, module);
			return Ok(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteModule(string id)
		{
			await CosmosDbService.Instance.DeleteModuleAsync(id);
			return NoContent();
		}
	}

}
