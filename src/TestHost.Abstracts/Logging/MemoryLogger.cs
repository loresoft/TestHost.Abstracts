using Microsoft.Extensions.Logging;

namespace TestHost.Abstracts.Logging;

/// <summary>
/// An in-memory logger implementation that captures log entries for testing purposes.
/// </summary>
public class MemoryLogger : ILogger
{
    private readonly string _category;
    private readonly MemoryLoggerSettings _settings;
    private readonly IExternalScopeProvider _scopeProvider;
    private readonly Action<MemoryLogEntry> _logWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryLogger"/> class.
    /// </summary>
    /// <param name="category">The category name for messages produced by the logger.</param>
    /// <param name="settings">The configuration settings for the memory logger.</param>
    /// <param name="scopeProvider">The external scope provider for managing log scopes.</param>
    /// <param name="logWriter">The action that writes log entries to the memory buffer.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="category"/>, <paramref name="settings"/>,
    /// <paramref name="scopeProvider"/>, or <paramref name="logWriter"/> is <c>null</c>.
    /// </exception>
    public MemoryLogger(
        string category,
        MemoryLoggerSettings settings,
        IExternalScopeProvider scopeProvider,
        Action<MemoryLogEntry> logWriter)
    {
        if (category is null)
            throw new ArgumentNullException(nameof(category));
        if (settings is null)
            throw new ArgumentNullException(nameof(settings));
        if (scopeProvider is null)
            throw new ArgumentNullException(nameof(scopeProvider));
        if (logWriter is null)
            throw new ArgumentNullException(nameof(logWriter));

        _category = category;
        _settings = settings;
        _scopeProvider = scopeProvider;
        _logWriter = logWriter;

    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> that ends the logical operation scope on dispose, or <c>null</c> if not supported.
    /// </returns>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return _scopeProvider.Push(state);
    }

    /// <summary>
    /// Checks if the given log level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level to be checked.</param>
    /// <returns>
    /// <c>true</c> if the log level is enabled and the logger will write a log entry; otherwise, <c>false</c>.
    /// </returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        if (logLevel == LogLevel.None)
            return false;

        if (logLevel < _settings.MinimumLevel)
            return false;

        if (_settings.Filter != null)
            return _settings.Filter(_category, logLevel);

        return true;
    }

    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <typeparam name="TState">The type of the object to be written.</typeparam>
    /// <param name="logLevel">The log level of the entry.</param>
    /// <param name="eventId">The event identifier for the entry.</param>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <param name="exception">The exception related to this entry, or <c>null</c> if none.</param>
    /// <param name="formatter">Function to create a string message of the <paramref name="state"/> and <paramref name="exception"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> is <c>null</c>.</exception>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        // generate the message
        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message))
            return;

        // capture scopes if any
        var scopes = new List<object?>();
        _scopeProvider.ForEachScope((current, scopes) => scopes.Add(current), scopes);

        var entry = new MemoryLogEntry(
            timestamp: DateTime.UtcNow,
            logLevel: logLevel,
            eventId: eventId,
            category: _category,
            message: message,
            exception: exception,
            state: state,
            scopes: scopes);

        _logWriter(entry);
    }
}
