using Microsoft.AspNetCore.Mvc;
using DataAccess;

namespace PractissApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		public UserController()
		{
		}

		[HttpPost]
		public async Task<IActionResult> CreateUser([FromBody] CommonTypes.User user)
		{
			try
			{
				var result = await CosmosDbService.Instance.CreateUserAsync(user);
				return Ok(result);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return null;
			}
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetUser(string id)
		{
			var user = await CosmosDbService.Instance.GetUserAsync(id);
			if (user == null)
				return NotFound();

			return Ok(user);
		}

		[HttpGet("email/{email}")]
		public async Task<IActionResult> GetUserByEmail(string email)
		{
			var user = await CosmosDbService.Instance.GetUserByEmail(email);
			if (user != null)
			{
				return Ok(user);
			}
			else
			{
				return NotFound();
			}
		}

		[HttpGet("getallusers")]
		public async Task<IActionResult> GetAllUsers()
		{
			var users = await CosmosDbService.Instance.GetAllUsersAsync();
			return Ok(users);
		}

		[HttpGet("getallusersbydomain/{domain}")]
		public async Task<IActionResult> GetAllUsersByDomainl(string domain)
		{
			var users = await CosmosDbService.Instance.GetAllUsersByDomainAsync(domain);
			return Ok(users);
		}

        [HttpPut]
		public async Task<IActionResult> UpdateUser([FromBody] CommonTypes.User user)
		{
			var result = await CosmosDbService.Instance.UpdateUserAsync(user.Id, user);
			return Ok(result);
		}
	}

}
