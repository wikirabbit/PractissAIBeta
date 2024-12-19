using CommonTypes;
using DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace PractissApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BookmarkedModuleController : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> CreateBookmarkedModule([FromBody] BookmarkedModule bookmarkedModule)
		{
			var result = await CosmosDbService.Instance.CreateBookmarkedModuleAsync(bookmarkedModule);
			return Ok(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> RemoveBookmarkedModule(string id)
		{
			await CosmosDbService.Instance.RemoveBookmarkedModuleAsync(id);
			return NoContent();
		}

		[HttpGet("{userId}")]
		public async Task<IActionResult> GetBookmarkedModules(string userId)
		{
			var result = await CosmosDbService.Instance.GetBookmarkedModulesByUserIdAsync(userId);
			return Ok(result);
		}
	}

}
