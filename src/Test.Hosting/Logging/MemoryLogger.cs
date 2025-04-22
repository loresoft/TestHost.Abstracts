using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace TestHost.Abstracts.Logging;

/// <summary>
/// A memory logger
/// </summary>
/// <seealso cref="ILogger" />
public class MemoryLogger : ILogger
{
    private readonly string _name;
    private readonly MemoryLoggerSettings _settings;
    private readonly IExternalScopeProvider? _externalScopeProvider;
    private readonly Action<MemoryLogEntry> _logWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryLogger"/> class.
    /// </summary>
    /// <param name="name">The name of the Logger.</param>
    /// <param name="settings">The logger filter settings.</param>
    /// <param name="externalScopeProvider">The external scope provider.</param>
    /// <param name="logWriter">The log entries.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public MemoryLogger(
        string name,
        MemoryLoggerSettings settings,
        IExternalScopeProvider? externalScopeProvider,
        Action<MemoryLogEntry> logWriter)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));
        if (settings is null)
            throw new ArgumentNullException(nameof(settings));
        if (logWriter is null)
            throw new ArgumentNullException(nameof(logWriter));

        _name = name;
        _settings = settings;
        _externalScopeProvider = externalScopeProvider;
        _logWriter = logWriter;
    }


    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _externalScopeProvider?.Push(state);
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        if (logLevel == LogLevel.None)
            return false;

        if (_settings.MinLevel != null && logLevel < _settings.MinLevel)
            return false;

        if (_settings.Filter != null)
            return _settings.Filter(_name, logLevel);

        return true;
    }

    /// <inheritdoc />
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

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message))
            return;

        var logEvent = new MemoryLogEntry(_name, logLevel, eventId, state, exception, message);
        _logWriter(logEvent);
    }
}
