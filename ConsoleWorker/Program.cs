using ConsoleWorker.Workers;

namespace ConsoleWorker
{
	interface IWorker
	{
		Task Run(CancellationToken cancellationToken);
	}

	internal class Program
	{
		static async Task Main(string[] args)
		{
			DataAccess.CosmosDbService.RefreshConnectionDynamically = false;
			DataAccess.CosmosDbService.ConnectionString = StringResources.CosmosDbConnectionString;
			DataAccess.CosmosDbService.DatabaseName = StringResources.CosmosDatataseName;

			List<IWorker> workers = new List<IWorker>
			{
				// new FirefliesWorker(),
				new StatsWorker(),
				new WhitelistUserAdderWorker(),
				// new TestReportCleanupWorker()
            };

			var cancellationTokenSource = new CancellationTokenSource();
			var tasks = new List<Task>();

			foreach (var worker in workers)
			{
				tasks.Add(RunWorkerAsync(worker, cancellationTokenSource.Token));
			}

			// Keep the application running indefinitely
			Task.WaitAll(tasks.ToArray());
		}

		static async Task RunWorkerAsync(IWorker worker, CancellationToken cancellationToken)
		{
			// Assuming each worker internally decides how often to run, we just start them here.
			await worker.Run(cancellationToken);
		}
	}
}