using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Tholi.Core;
using Tholi.Platform;

if (args.Contains("--check"))
{
    Console.WriteLine(JsonSerializer.Serialize(new UnconnectedDesktopAdapter().Capabilities));
    return;
}

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<JourneyCoordinator>();
builder.Services.AddSingleton<IDesktopAdapter, UnconnectedDesktopAdapter>();
builder.Services.AddHostedService<AgentWorker>();
await builder.Build().RunAsync();

internal sealed class AgentWorker(
    IDesktopAdapter desktop, ILogger<AgentWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Tholi session agent ready on {Platform}. {Detail}",
            desktop.Capabilities.Platform, desktop.Capabilities.Detail);
        logger.LogInformation("Local IPC is a future integration boundary. No desktop observation or input is active.");
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
