using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using TestHost.Abstracts.Logging;

namespace TestHost.Abstracts.Tests;

public class LoggingTests
{
    [ClassDataSource<TestApplication>(Shared = SharedType.PerAssembly)]
    public required TestApplication Application { get; init; }

    [Test]
    public async Task MemoryLogger()
    {
        var logger = Application.Services.GetRequiredService<ILogger<LoggingTests>>();
        await Assert.That(logger).IsNotNull();

        logger.LogInformation("This is a test log message.");

        // Retrieve the MemoryLoggerProvider to access the logs
        var memoryLoggerProvider = Application.Services.GetRequiredService<MemoryLoggerProvider>();
        await Assert.That(memoryLoggerProvider).IsNotNull();

        // Verify that the log message was captured
        var logs = memoryLoggerProvider.Logs();
        await Assert.That(logs).Contains(log => log.Message.Contains("This is a test log message."));
    }
}
