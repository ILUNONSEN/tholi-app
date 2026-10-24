using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

if (args.Contains("--check"))
{
    Console.WriteLine(JsonSerializer.Serialize(new {
        component = "Tholi.Runtime", mode = "local", modelLoaded = false,
        cloudConnected = false, listenerStarted = false
    }));
    return;
}

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<RuntimeWorker>();
await builder.Build().RunAsync();

internal sealed class RuntimeWorker(ILogger<RuntimeWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Tholi local runtime ready. No model, cloud connection, or network listener is active.");
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
