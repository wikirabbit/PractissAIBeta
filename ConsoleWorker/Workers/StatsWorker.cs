using ApiIntegrations.Misc;
using CommonTypes;
using DataAccess;

namespace ConsoleWorker.Workers
{
	public class StatsWorker : IWorker
    {
        public async Task Run(CancellationToken cancellationToken)
        {
            TimeSpan checkInterval = TimeSpan.FromMinutes(20);

            while (!cancellationToken.IsCancellationRequested)
            {
				await CollectStats();
				await Task.Delay(checkInterval, cancellationToken);
            }
        }

        private async Task CollectStats()
        {
            var whitelistUsers = await WhitelistService.Instance.GetAllWhitelistUsers();

            foreach (var wlUser in whitelistUsers)
            {
                var userStats = await GetStatsForUser(wlUser.Email);
                if (userStats != null && userStats.User.UserStats.LastInteractionDateTime != null)
                {
                    WhitelistService.Instance.UpdateUserStatsInSpreadsheet(wlUser.GroupName, wlUser.Email, userStats);
                }
            }
        }

        private async Task<UserStatsForReporting> GetStatsForUser(string email)
        {
            var user = await CosmosDbService.Instance.GetUserByEmail(email);

            if (user == null)
            {
                return null;
            }

            var userStats = new UserStatsForReporting();
            userStats.User = user;

            if (user.UserStats != null)
            {
                userStats.TimeOnPlatform = (long)user.UserStats.TotalRoleplayMinutes;

                if (!string.IsNullOrEmpty(user.UserStats.LastInteractionDateTime))
                {
                    userStats.DaysInactive = (long)(DateTime.UtcNow - DateTime.Parse(user.UserStats.LastInteractionDateTime)).TotalDays;
                }
                else
                {
                    userStats.DaysInactive = - 1;
				}
            }

            var allReports = await CosmosDbService.Instance.SearchReportsByLearnerOrCoachIdAsync(user.Id, null);
			var abortedReports = await CosmosDbService.Instance.SearchAbortedReportsByLearnerOrCoachIdAsync(user.Id, null);

			userStats.SessionsAborted = abortedReports.ToList().Count;
			userStats.SessionsCompleted = allReports.ToList().Count - userStats.SessionsAborted;
            

			foreach (var report in allReports)
            {
                userStats.CommentsProvided += report.FeedbackCommentsCount;
                userStats.ReactionsProvided += report.FeedbackReactionsCount;
            }

            return userStats;
        }
    }
}
