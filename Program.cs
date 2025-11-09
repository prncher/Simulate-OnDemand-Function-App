using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OnDemandTaskAsynchPatternWithHostService
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Register the IHostedService implementation
                services.AddHostedService<FunctionHostingService>();
            })
            .UseConsoleLifetime() // Enables SIGTERM handling
            .Build()
            .RunAsync();
        }
    }
}
