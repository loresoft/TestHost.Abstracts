using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace TestHost.Abstracts.Logging;

/// <summary>
/// Represents a single log entry captured by the memory logger.
/// </summary>
public class MemoryLogEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryLogEntry"/> class.
    /// </summary>
    /// <param name="timestamp">The timestamp when the log entry was created.</param>
    /// <param name="logLevel">The log level of the entry.</param>
    /// <param name="eventId">The event identifier associated with the log entry.</param>
    /// <param name="category">The category name of the logger that created this entry.</param>
    /// <param name="message">The formatted log message.</param>
    /// <param name="exception">The exception associated with the log entry, if any.</param>
    /// <param name="state">The state object associated with the log entry.</param>
    /// <param name="scopes">The collection of scope values that were active when the log entry was created.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="category"/> or <paramref name="message"/> is <c>null</c>.
    /// </exception>
    public MemoryLogEntry(
        DateTime timestamp,
        LogLevel logLevel,
        EventId eventId,
        string category,
        string message,
        Exception? exception = null,
        object? state = null,
        IReadOnlyList<object?>? scopes = null)
    {
        if (category is null)
            throw new ArgumentNullException(nameof(category));
        if (message is null)
            throw new ArgumentNullException(nameof(message));

        Timestamp = timestamp;
        LogLevel = logLevel;
        EventId = eventId;
        Category = category;
        Message = message;
        Exception = exception;
        State = state;
        Scopes = scopes ?? new List<object?>();
    }

    /// <summary>
    /// Gets the timestamp when the log entry was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> value representing when the log entry was captured.
    /// </value>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Gets the log level of the entry.
    /// </summary>
    /// <value>
    /// A <see cref="Microsoft.Extensions.Logging.LogLevel"/> value indicating the severity of the log entry.
    /// </value>
    public LogLevel LogLevel { get; }

    /// <summary>
    /// Gets the event identifier associated with the log entry.
    /// </summary>
    /// <value>
    /// An <see cref="EventId"/> that uniquely identifies the type of event being logged.
    /// </value>
    public EventId EventId { get; }

    /// <summary>
    /// Gets the category name of the logger that created this entry.
    /// </summary>
    /// <value>
    /// A string representing the category name, typically the fully qualified name of the class using the logger.
    /// This value is never <c>null</c>.
    /// </value>
    public string Category { get; }

    /// <summary>
    /// Gets the formatted log message.
    /// </summary>
    /// <value>
    /// A string containing the formatted message text. This value is never <c>null</c>.
    /// </value>
    public string Message { get; }

    /// <summary>
    /// Gets the exception associated with the log entry, if any.
    /// </summary>
    /// <value>
    /// An <see cref="System.Exception"/> instance if an exception was logged; otherwise, <c>null</c>.
    /// </value>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the state object associated with the log entry.
    /// </summary>
    /// <value>
    /// An object containing additional state information passed to the logger, or <c>null</c> if no state was provided.
    /// This is typically used for structured logging scenarios to capture contextual data.
    /// </value>
    public object? State { get; }

    /// <summary>
    /// Gets the collection of scope values that were active when the log entry was created.
    /// </summary>
    /// <value>
    /// A read-only list of scope objects that were active at the time of logging.
    /// This value is never <c>null</c>, but may be an empty collection if no scopes were active.
    /// </value>
    public IReadOnlyList<object?> Scopes { get; }

    /// <summary>
    /// Returns a formatted string representation of the log entry.
    /// </summary>
    /// <returns>A string containing the log level, category, event ID, message, scopes, and exception (if present).</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();

        // Format: <log level>: <category>[<event id>]
        sb.Append(GetLogLevelString(LogLevel));
        sb.Append(": ");
        sb.Append(Category);
        sb.Append('[');
        sb.Append(EventId.Id);
        sb.Append(']');
        sb.AppendLine();

        // Message on next line with indentation
        sb.Append(' ', 6);
        sb.Append(Message);

        if (State is not null)
        {
            sb.AppendLine();
            sb.Append(' ', 6);
            sb.Append("=> ");
            sb.Append(JsonSerializer.Serialize(State));
        }

        // Scopes if present
        foreach (var scope in Scopes)
        {
            sb.AppendLine();
            sb.Append(' ', 6);
            sb.Append("=> ");
            sb.Append(JsonSerializer.Serialize(scope));
        }

        // Exception if present
        if (Exception != null)
        {
            sb.AppendLine();
            sb.Append(Exception);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a <see cref="LogLevel"/> to its short string representation.
    /// </summary>
    /// <param name="logLevel">The log level to convert.</param>
    /// <returns>A short string representation of the log level (e.g., "info", "warn", "fail").</returns>
    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => logLevel.ToString().ToLowerInvariant()
        };
    }
}
