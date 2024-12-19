using ApiIntegrations.Misc;
using DataAccess;

namespace ConsoleWorker.Workers
{
	public class WhitelistUserAdderWorker : IWorker
    {
        public async Task Run(CancellationToken cancellationToken)
        {
            TimeSpan checkInterval = TimeSpan.FromMinutes(10); // Different interval

            while (!cancellationToken.IsCancellationRequested)
            {
				var whitelistUsers = await WhitelistService.Instance.GetAllWhitelistUsers();

				foreach (var wlUser in whitelistUsers)
				{
					var user = await CosmosDbService.Instance.GetUserByEmail(wlUser.Email);
					if (user == null)
					{
						var nameParts = wlUser.Name.Split(' ');
						user = new CommonTypes.User
						{
							Id = Guid.NewGuid().ToString(),
							FirstName = nameParts[0],
							LastName = nameParts[nameParts.Length-1],
							Email = wlUser.Email.Trim().ToLower(),
							
							Roles = "learner designer"
						};

						await CosmosDbService.Instance.CreateUserAsync(user);
					}
				}

				await Task.Delay(checkInterval, cancellationToken);
            }
        }
    }
}
