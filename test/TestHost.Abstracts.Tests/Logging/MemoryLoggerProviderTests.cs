using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TestHost.Abstracts.Logging;

namespace TestHost.Abstracts.Tests.Logging;

public class MemoryLoggerProviderTests
{
    [Test]
    public async Task Constructor_WithValidOptions_ShouldSucceed()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);

        // Act
        var provider = new MemoryLoggerProvider(options);

        // Assert
        await Assert.That(provider).IsNotNull();
    }

    [Test]
    public async Task Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.That(() => new MemoryLoggerProvider(null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task CreateLogger_WithValidCategoryName_ShouldReturnLogger()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);

        // Act
        var logger = provider.CreateLogger("TestCategory");

        // Assert
        await Assert.That(logger).IsNotNull();
    }

    [Test]
    public async Task CreateLogger_WithNullCategoryName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);

        // Act & Assert
        await Assert.That(() => provider.CreateLogger(null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Logger_Log_ShouldAddEntryToLogs()
    {
        // Arrange
        var settings = new MemoryLoggerSettings { MinimumLevel = LogLevel.Information };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Test message");

        // Assert
        var logs = provider.Logs();
        await Assert.That(logs.Count).IsEqualTo(1);
        await Assert.That(logs[0].Message).IsEqualTo("Test message");
        await Assert.That(logs[0].Category).IsEqualTo("TestCategory");
        await Assert.That(logs[0].LogLevel).IsEqualTo(LogLevel.Information);
    }

    [Test]
    public async Task Logger_Log_WithException_ShouldCaptureException()
    {
        // Arrange
        var settings = new MemoryLoggerSettings { MinimumLevel = LogLevel.Error };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");
        var exception = new InvalidOperationException("Test exception");

        // Act
        logger.LogError(exception, "Error occurred");

        // Assert
        var logs = provider.Logs();
        await Assert.That(logs.Count).IsEqualTo(1);
        await Assert.That(logs[0].Message).IsEqualTo("Error occurred");
        await Assert.That(logs[0].Exception).IsEqualTo(exception);
    }

    [Test]
    public async Task Logger_Log_WithEventId_ShouldCaptureEventId()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");
        var eventId = new EventId(100, "TestEvent");

        // Act
        logger.Log(LogLevel.Information, eventId, "Test message");

        // Assert
        var logs = provider.Logs();
        await Assert.That(logs.Count).IsEqualTo(1);
        await Assert.That(logs[0].EventId).IsEqualTo(eventId);
    }

    [Test]
    public async Task Logger_Log_BelowMinimumLevel_ShouldNotLog()
    {
        // Arrange
        var settings = new MemoryLoggerSettings { MinimumLevel = LogLevel.Warning };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogDebug("Debug message");
        logger.LogInformation("Info message");

        // Assert
        var logs = provider.Logs();
        await Assert.That(logs).IsEmpty();
    }

    [Test]
    public async Task Logger_Log_WithCustomFilter_ShouldRespectFilter()
    {
        // Arrange
        var settings = new MemoryLoggerSettings
        {
            MinimumLevel = LogLevel.Debug,
            Filter = (category, level) => category.StartsWith("Allow")
        };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var allowedLogger = provider.CreateLogger("AllowedCategory");
        var blockedLogger = provider.CreateLogger("BlockedCategory");

        // Act
        allowedLogger.LogInformation("Allowed message");
        blockedLogger.LogInformation("Blocked message");

        // Assert
        var logs = provider.Logs();
        await Assert.That(logs.Count).IsEqualTo(1);
        await Assert.That(logs[0].Message).IsEqualTo("Allowed message");
    }

    [Test]
    public async Task Logs_WithCapacityExceeded_ShouldEnforceLimit()
    {
        // Arrange
        var settings = new MemoryLoggerSettings
        {
            MinimumLevel = LogLevel.Debug,
            Capacity = 5
        };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        for (int i = 0; i < 10; i++)
        {
            logger.LogInformation($"Message {i}");
        }

        // Assert
        var logs = provider.Logs();
        await Assert.That(logs.Count).IsEqualTo(5);
        // Should have kept the last 5 messages
        await Assert.That(logs[0].Message).IsEqualTo("Message 5");
        await Assert.That(logs[4].Message).IsEqualTo("Message 9");
    }

    [Test]
    public async Task Logs_ByCategoryName_ShouldFilterByCategory()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger1 = provider.CreateLogger("Category1");
        var logger2 = provider.CreateLogger("Category2");

        // Act
        logger1.LogInformation("Message from Category1");
        logger2.LogInformation("Message from Category2");
        logger1.LogInformation("Another message from Category1");

        // Assert
        var category1Logs = provider.Logs("Category1");
        var category2Logs = provider.Logs("Category2");

        await Assert.That(category1Logs.Count).IsEqualTo(2);
        await Assert.That(category2Logs.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Logs_ByCategoryName_ShouldBeCaseInsensitive()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Test message");

        // Assert
        var logs = provider.Logs("testcategory");
        await Assert.That(logs.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Logs_ByLogLevel_ShouldFilterByLevel()
    {
        // Arrange
        var settings = new MemoryLoggerSettings { MinimumLevel = LogLevel.Trace };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogDebug("Debug message");
        logger.LogInformation("Info message");
        logger.LogWarning("Warning message");
        logger.LogError("Error message");

        // Assert
        var warningAndAbove = provider.Logs(LogLevel.Warning);
        await Assert.That(warningAndAbove.Count).IsEqualTo(2);
        foreach (var log in warningAndAbove)
        {
            await Assert.That(log.LogLevel >= LogLevel.Warning).IsTrue();
        }
    }

    [Test]
    public async Task Logs_MultipleCategories_ShouldReturnAllLogs()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger1 = provider.CreateLogger("Category1");
        var logger2 = provider.CreateLogger("Category2");

        // Act
        logger1.LogInformation("Message 1");
        logger2.LogInformation("Message 2");
        logger1.LogInformation("Message 3");

        // Assert
        var allLogs = provider.Logs();
        await Assert.That(allLogs.Count).IsEqualTo(3);
    }

    [Test]
    public async Task Clear_ShouldRemoveAllLogs()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");
        logger.LogInformation("Message 1");
        logger.LogInformation("Message 2");

        // Act
        provider.Clear();

        // Assert
        var logs = provider.Logs();
        await Assert.That(logs).IsEmpty();
    }

    [Test]
    public async Task Logger_WithScope_ShouldCaptureScopes()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        using (logger.BeginScope("Scope1"))
        {
            using (logger.BeginScope("Scope2"))
            {
                logger.LogInformation("Message with scopes");
            }
        }

        // Assert
        var logs = provider.Logs();
        await Assert.That(logs.Count).IsEqualTo(1);
        await Assert.That(logs[0].Scopes.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Logger_IsEnabled_ShouldRespectMinimumLevel()
    {
        // Arrange
        var settings = new MemoryLoggerSettings { MinimumLevel = LogLevel.Warning };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act & Assert
        await Assert.That(logger.IsEnabled(LogLevel.Debug)).IsFalse();
        await Assert.That(logger.IsEnabled(LogLevel.Information)).IsFalse();
        await Assert.That(logger.IsEnabled(LogLevel.Warning)).IsTrue();
        await Assert.That(logger.IsEnabled(LogLevel.Error)).IsTrue();
    }

    [Test]
    public void Dispose_ShouldNotThrowException()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);

        // Act & Assert - Dispose should complete without throwing
        provider.Dispose();
    }

    [Test]
    public async Task Logger_WithStructuredLogging_ShouldFormatMessage()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("User {UserId} logged in from {IpAddress}", 123, "192.168.1.1");

        // Assert
        var logs = provider.Logs();
        await Assert.That(logs.Count).IsEqualTo(1);
        await Assert.That(logs[0].Message).Contains("123");
        await Assert.That(logs[0].Message).Contains("192.168.1.1");
    }

    [Test]
    public async Task Logs_ShouldHaveTimestamp()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");
        var beforeLog = DateTime.UtcNow;

        // Act
        logger.LogInformation("Test message");
        var afterLog = DateTime.UtcNow;

        // Assert
        var logs = provider.Logs();
        await Assert.That(logs.Count).IsEqualTo(1);
        await Assert.That(logs[0].Timestamp).IsGreaterThanOrEqualTo(beforeLog.AddSeconds(-1));
        await Assert.That(logs[0].Timestamp).IsLessThanOrEqualTo(afterLog.AddSeconds(1));
    }
}
