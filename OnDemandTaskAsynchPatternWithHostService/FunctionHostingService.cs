using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace OnDemandTaskAsynchPatternWithHostService
{
    internal class FunctionHostingService : IHostedService
    {
        private const string assemblyPath = "\\FunctionLibrary\\bin\\Release\\net8.0\\FunctionLibrary.dll";
        private const string className = "FunctionLibrary.FunctionClass";
        private const string classMethod = "FunctionMethod";
        private CancellationTokenSource? _cts;
        private List<Task<object>>? tasks;
        private Task<int>? listeningTask;
        private bool notInterrupted;
        private Assembly? assembly;
        private Type? classType;
        private object? classInstance;
        private MethodInfo? method;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Console.WriteLine("MyBackgroundService started.");
            // Start your background work here
            tasks = new List<Task<object>>();
            notInterrupted = true;
            assembly = Assembly.LoadFrom(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
                @"..\..\..\.." + assemblyPath)));
            classType = assembly.GetType(className);
            if (classType == null)
            {
                Console.WriteLine($"Error: Type: {className} not found");
                return Task.FromException(new Exception($"Error: Type: {className} not found"));
            }
            classInstance = Activator.CreateInstance(classType);
            method = classType.GetMethod(classMethod);
            if (method == null)
            {
                Console.WriteLine($"Error: Method: {classMethod} not found");
                return Task.FromException(new Exception($"Error: Method: {classMethod} not found"));
            }

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
                        object[] parameter = { i++ };
                        if (method != null)
                        {
                            dynamic result = method.InvokeAsync(classInstance, parameter);
                            if (result != null)
                            {
                                tasks?.Add(result);
                            }
                        }
                        // tasks?.Add(PerformLongRunningOperationAsync(i++));
                    }


                }
            });

            Task.Run(async () =>
            {
                while (notInterrupted)
                {
                    object[] results = [];
                    int? count = tasks?.Count;
                    if (tasks != null && count.HasValue && count > 0)
                    {
                        results = await Task.WhenAll(tasks);
                        tasks.ForEach(task =>
                        {
                            Console.WriteLine($"\nStatus for Task {task.Id}: {task.Status}");
                            if (task.Status == TaskStatus.RanToCompletion)
                            {
                                count--;
                            }
                        });
                        if (count == 0)
                        {
                            tasks.Clear();
                            Console.WriteLine($"\nAll tasks completed. Tasks: {string.Join(", ", results)}");
                        }
                    }
                }
            });

            Console.WriteLine("Exit StartBankGroundTasks," +
                "but waiting for other tasks to be added and then waiting for " +
                "that tasks to complete");
        }
        /*
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
        */
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