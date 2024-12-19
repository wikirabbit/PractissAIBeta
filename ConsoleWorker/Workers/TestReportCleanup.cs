using DataAccess;

namespace ConsoleWorker.Workers
{
	// Example of adding another worker
	public class TestReportCleanupWorker : IWorker
    {
        public async Task Run(CancellationToken cancellationToken)
        {
            TimeSpan checkInterval = TimeSpan.FromMinutes(10); // Different interval

            while (!cancellationToken.IsCancellationRequested)
            {
				await CleanupTestReports();
				await Task.Delay(checkInterval, cancellationToken);
            }
        }

		async Task CleanupTestReports()
		{
			try
			{
				var user = await CosmosDbService.Instance.GetUserByEmail("vijay@practiss.ai");
				var reports = await CosmosDbService.Instance.SearchReportsByLearnerOrCoachIdAsync(user.Id, null);

				foreach (var report in reports)
				{
					if (report.Conversation.Count < 6)
					{
						await CosmosDbService.Instance.DeleteReportAsync(report.Id);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
