namespace ConsoleWorker.Workers
{
	// Example of adding another worker
	public class AnotherWorker : IWorker
    {
        public async Task Run(CancellationToken cancellationToken)
        {
            TimeSpan checkInterval = TimeSpan.FromMinutes(10); // Different interval

            while (!cancellationToken.IsCancellationRequested)
            {
                // Your processing logic here
                await Task.Delay(checkInterval, cancellationToken);
            }
        }
    }
}
