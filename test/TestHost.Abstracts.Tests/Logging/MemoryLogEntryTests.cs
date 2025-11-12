using Microsoft.Extensions.Logging;

using TestHost.Abstracts.Logging;

namespace TestHost.Abstracts.Tests.Logging;

public class MemoryLogEntryTests
{
    [Test]
    public async Task Constructor_WithRequiredProperties_ShouldSucceed()
    {
        // Arrange & Act
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: default,
            eventId: default,
            category: "TestCategory",
            message: "Test message");

        // Assert
        await Assert.That(entry.Category).IsEqualTo("TestCategory");
        await Assert.That(entry.Message).IsEqualTo("Test message");
        await Assert.That(entry.Timestamp).IsEqualTo(default(DateTime));
        await Assert.That(entry.LogLevel).IsEqualTo(default(LogLevel));
        await Assert.That(entry.EventId).IsEqualTo(default(EventId));
        await Assert.That(entry.Exception).IsNull();
        await Assert.That(entry.State).IsNull();
        await Assert.That(entry.Scopes).IsNotNull();
        await Assert.That(entry.Scopes).IsEmpty();
    }

    [Test]
    public async Task Constructor_WithAllProperties_ShouldSetAllValues()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var eventId = new EventId(100, "TestEvent");
        var exception = new InvalidOperationException("Test error");
        var scopes = new List<object?> { "Scope1", "Scope2" };
        var state = new { UserId = 123, RequestId = "abc-123" };

        // Act
        var entry = new MemoryLogEntry(
            timestamp: timestamp,
            logLevel: LogLevel.Error,
            eventId: eventId,
            category: "TestCategory",
            message: "Test message",
            exception: exception,
            state: state,
            scopes: scopes);

        // Assert
        await Assert.That(entry.Timestamp).IsEqualTo(timestamp);
        await Assert.That(entry.LogLevel).IsEqualTo(LogLevel.Error);
        await Assert.That(entry.EventId).IsEqualTo(eventId);
        await Assert.That(entry.Category).IsEqualTo("TestCategory");
        await Assert.That(entry.Message).IsEqualTo("Test message");
        await Assert.That(entry.Exception).IsEqualTo(exception);
        await Assert.That(entry.State).IsEqualTo(state);
        await Assert.That(entry.Scopes).IsEqualTo(scopes);
        await Assert.That(entry.Scopes.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ToString_WithMinimalData_ShouldFormatCorrectly()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: new EventId(0),
            category: "TestCategory",
            message: "Test message");

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains("info: TestCategory[0]");
        await Assert.That(result).Contains("      Test message");
    }

    [Test]
    public async Task ToString_WithEventId_ShouldIncludeEventId()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Warning,
            eventId: new EventId(42, "TestEvent"),
            category: "TestCategory",
            message: "Test message");

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains("warn: TestCategory[42]");
        await Assert.That(result).Contains("      Test message");
    }

    [Test]
    public async Task ToString_WithException_ShouldIncludeException()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Error,
            eventId: default,
            category: "TestCategory",
            message: "Error occurred",
            exception: exception);

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains("fail: TestCategory[0]");
        await Assert.That(result).Contains("      Error occurred");
        await Assert.That(result).Contains("InvalidOperationException");
        await Assert.That(result).Contains("Something went wrong");
    }

    [Test]
    public async Task ToString_WithScopes_ShouldIncludeScopes()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: default,
            category: "TestCategory",
            message: "Test message",
            scopes: new List<object?> { "Scope1", "Scope2" });

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains("info: TestCategory[0]");
        await Assert.That(result).Contains("      Test message");
        await Assert.That(result).Contains("      => ");
        await Assert.That(result).Contains("\"Scope1\"");
        await Assert.That(result).Contains("\"Scope2\"");
    }

    [Test]
    public async Task ToString_WithEmptyScopes_ShouldNotIncludeScopesSection()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: default,
            category: "TestCategory",
            message: "Test message",
            scopes: new List<object?>());

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains("info: TestCategory[0]");
        await Assert.That(result).Contains("      Test message");
        await Assert.That(result).DoesNotContain("=> ");
    }

    [Test]
    [Arguments(LogLevel.Trace, "trce")]
    [Arguments(LogLevel.Debug, "dbug")]
    [Arguments(LogLevel.Information, "info")]
    [Arguments(LogLevel.Warning, "warn")]
    [Arguments(LogLevel.Error, "fail")]
    [Arguments(LogLevel.Critical, "crit")]
    public async Task ToString_WithDifferentLogLevels_ShouldFormatCorrectly(LogLevel logLevel, string expectedPrefix)
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: logLevel,
            eventId: default,
            category: "TestCategory",
            message: "Test message");

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains($"{expectedPrefix}: TestCategory[0]");
        await Assert.That(result).Contains("      Test message");
    }

    [Test]
    public async Task ToString_WithCompleteEntry_ShouldFormatAllParts()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        var state = new { UserId = 42, Action = "UpdateProfile" };
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Error,
            eventId: new EventId(500, "ValidationError"),
            category: "App.Validation",
            message: "Validation failed for user input",
            exception: exception,
            state: state,
            scopes: new List<object?> { "Scope1" });

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains("fail: App.Validation[500]");
        await Assert.That(result).Contains("      Validation failed for user input");
        await Assert.That(result).Contains("      => ");
        await Assert.That(result).Contains("ArgumentException");
        await Assert.That(result).Contains("Invalid argument");
    }

    [Test]
    public async Task ToString_WithLongMessage_ShouldIncludeFullMessage()
    {
        // Arrange
        var longMessage = new string('A', 1000);
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: default,
            category: "TestCategory",
            message: longMessage);

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains(longMessage);
    }

    [Test]
    public async Task ToString_WithSpecialCharactersInMessage_ShouldHandleCorrectly()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: default,
            category: "TestCategory",
            message: "Message with special chars: @#$%^&*()_+-=[]{}|;':\",./<>?");

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains("Message with special chars: @#$%^&*()_+-=[]{}|;':\",./<>?");
    }

    [Test]
    public async Task Scopes_DefaultValue_ShouldBeEmptyList()
    {
        // Arrange & Act
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: default,
            eventId: default,
            category: "TestCategory",
            message: "Test message");

        // Assert
        await Assert.That(entry.Scopes).IsNotNull();
        await Assert.That(entry.Scopes).IsEmpty();
    }

    [Test]
    public async Task ToString_WithNestedExceptions_ShouldIncludeInnerException()
    {
        // Arrange
        var innerException = new ArgumentException("Inner error");
        var outerException = new InvalidOperationException("Outer error", innerException);
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Error,
            eventId: default,
            category: "TestCategory",
            message: "Error with nested exceptions",
            exception: outerException);

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains("InvalidOperationException");
        await Assert.That(result).Contains("Outer error");
        await Assert.That(result).Contains("ArgumentException");
        await Assert.That(result).Contains("Inner error");
    }

    [Test]
    public async Task ToString_MultiLineFormat_ShouldHaveCorrectStructure()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: new EventId(42),
            category: "TestCategory",
            message: "Test message");

        // Act
        var result = entry.ToString();
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        await Assert.That(lines.Length).IsEqualTo(2);
        await Assert.That(lines[0]).IsEqualTo("info: TestCategory[42]");
        await Assert.That(lines[1]).IsEqualTo("      Test message");
    }

    [Test]
    public async Task ToString_WithMultipleScopes_ShouldIncludeAllScopes()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: default,
            category: "TestCategory",
            message: "Test message",
            scopes: new List<object?> { "Scope1", "Scope2", "Scope3" });

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains("      => ");
        await Assert.That(result).Contains("\"Scope1\"");
        await Assert.That(result).Contains("\"Scope2\"");
        await Assert.That(result).Contains("\"Scope3\"");
    }

    [Test]
    public async Task ToString_LogLevelNone_ShouldUseNoneAsPrefix()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.None,
            eventId: default,
            category: "TestCategory",
            message: "Test message");

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains("none: TestCategory[0]");
    }

    [Test]
    public async Task ToString_WithState_ShouldIncludeState()
    {
        // Arrange
        var state = new { UserId = 123, RequestId = "abc-123" };
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: default,
            category: "TestCategory",
            message: "Test message",
            state: state);

        // Act
        var result = entry.ToString();

        // Assert
        await Assert.That(result).Contains("info: TestCategory[0]");
        await Assert.That(result).Contains("      Test message");
        await Assert.That(result).Contains("      => ");
        await Assert.That(result).Contains("UserId");
        await Assert.That(result).Contains("123");
        await Assert.That(result).Contains("RequestId");
        await Assert.That(result).Contains("abc-123");
    }

    [Test]
    public async Task State_DefaultValue_ShouldBeNull()
    {
        // Arrange & Act
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: default,
            eventId: default,
            category: "TestCategory",
            message: "Test message");

        // Assert
        await Assert.That(entry.State).IsNull();
    }
}
