using CommonTypes;
using DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace PractissApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AvatarController : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> CreateAvatar([FromBody] Avatar avatar)
		{
			var result = await CosmosDbService.Instance.CreateAvatarAsync(avatar);
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetAvatar(string id)
		{
			var avatar = await CosmosDbService.Instance.GetAvatarAsync(id);
			if (avatar == null)
				return NotFound();

			return Ok(avatar);
		}

		[HttpGet("author/{authorId}")]
		public async Task<IActionResult> GetAvatarsByAuthor(string authorId)
		{
			var avatars = await CosmosDbService.Instance.GetAvatarsByAuthorIdAsync(authorId);
			return Ok(avatars);
		}


		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateAvatar(string id, [FromBody] Avatar avatar)
		{
			var result = await CosmosDbService.Instance.UpdateAvatarsAsync(id, avatar);
			return Ok(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteAvatar(string id)
		{
			await CosmosDbService.Instance.DeleteAvatarAsync(id);
			return NoContent();
		}
	}
}
