using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace OnDemandTaskAsynchPatternWithHostService
{
    internal class FunctionHostingService : IHostedService
    {
        private CancellationTokenSource? _cts;
        private List<Task<int>>? tasks;
        private Task<int>? listeningTask;
        private bool notInterrupted;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Console.WriteLine("MyBackgroundService started.");
            // Start your background work here
            tasks = new List<Task<int>>();
            notInterrupted = true;
            StartBankGroundTasks();

            return Task.CompletedTask;
        }

        private void StartBankGroundTasks()
        {
            Task.Run(() =>
            {
                // Add to list of tasks to run concurrently
                int i = 1;
                while (notInterrupted)
                {
                    Console.WriteLine("Press 'A' to add a new thread");
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.A)
                    {
                        Console.WriteLine("Starting action!");
                        tasks?.Add(PerformLongRunningOperationAsync(i++));
                    }


                }
            });

            Task.Run(async () =>
            {
                while (notInterrupted)
                {
                    int[] results = [];
                    if (tasks != null && tasks.Count > 0)
                    {
                        results = await Task.WhenAll(tasks);
                        Console.WriteLine($"\nAll tasks completed. Results: {string.Join(", ", results)}");
                        tasks.Clear();
                    }
                }
            });

            Console.WriteLine("StartBankGroundTasks Thread is existing," +
                "but still waiting for other tasks to add and then waiting for " +
                "that tasks to complete");
        }

        private async Task<int> PerformLongRunningOperationAsync(int id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"Task {id}: Starting operation...");
            // Simulate an I/O-bound operation with Task.Delay
            // Random delay between 1 and 25 seconds
            await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 25)));
            Console.WriteLine($"Task {id}: Operation completed.");
            stopwatch.Stop();
            Console.WriteLine($"\nTotal elapsed time for this thread {id}: {stopwatch.ElapsedMilliseconds} ms");

            return id;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("MyBackgroundService stopping gracefully...");
            // Perform cleanup here, e.g., save state, close connections
            notInterrupted = false;
            _cts?.Cancel(); // Signal background tasks to stop
            return Task.CompletedTask;
        }
    }
}