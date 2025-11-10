using System.Diagnostics;

namespace FunctionLibrary
{
    public class FunctionClass
    {
        public async Task<int> FunctionMethod(int id)
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
    }
}
