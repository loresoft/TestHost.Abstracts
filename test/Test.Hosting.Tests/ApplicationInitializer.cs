using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Test.Hosting.Tests;

public class ApplicationInitializer(ILogger<ApplicationInitializer> logger) : IHostedService
{
    public static bool IsStarted = false;

    public static bool IsStopped = false;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Initialize Database StartAsync()");

        IsStarted = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Initialize Database StopAsync()");

        IsStopped = true;
        return Task.CompletedTask;
    }
}
