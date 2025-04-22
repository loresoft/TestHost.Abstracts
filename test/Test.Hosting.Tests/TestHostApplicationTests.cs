using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Test.Hosting.Logging;

namespace Test.Hosting.Tests;

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

        var memoryLogger = Application.Services
            .GetServices<ILoggerProvider>()
            .OfType<MemoryLoggerProvider>()
            .FirstOrDefault();

        await Assert.That(memoryLogger).IsNotNull();

        var logs = memoryLogger?.GetEntries().ToList();

        await Assert.That(logs).IsNotEmpty();
    }
}
