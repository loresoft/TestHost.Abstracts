using Microsoft.Extensions.DependencyInjection;

using TestHost.Abstracts.Logging;

namespace TestHost.Abstracts.Tests;

public class TestHostApplicationTests
{
    [ClassDataSource<TestApplication>(Shared = SharedType.PerAssembly)]
    public required TestApplication Application { get; init; }

    [Test]
    public async Task GetService()
    {
        var service = Application.Services.GetRequiredService<IService>();

        await Assert.That(Service.IsRun).IsFalse();
        service.Run();

        await Assert.That(Service.IsRun).IsTrue();
        await Assert.That(ApplicationInitializer.IsStarted).IsTrue();

        var memoryLogger = Application.Services.GetService<MemoryLoggerProvider>();
        await Assert.That(memoryLogger).IsNotNull();

        var logs = memoryLogger?.GetEntries().ToList();
        await Assert.That(logs).IsNotEmpty();

        await Assert.That(logs).Contains(match => match.Message.Contains("Service Run()"));
        await Assert.That(logs).Contains(match => match.Message.Contains("Initialize Database StartAsync()"));
    }
}
