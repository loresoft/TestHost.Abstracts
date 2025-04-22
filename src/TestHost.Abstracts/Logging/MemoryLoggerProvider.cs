using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TestHost.Abstracts.Logging;

/// <summary>
/// A memory logging provider
/// </summary>
/// <seealso cref="ILoggerProvider" />
/// <seealso cref="ISupportExternalScope" />
[ProviderAlias("MemoryLogger")]
public class MemoryLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentQueue<MemoryLogEntry> _logEntries = new();
    private readonly MemoryLoggerSettings _settings;
    private IExternalScopeProvider? _scopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryLoggerProvider"/> class.
    /// </summary>
    public MemoryLoggerProvider()
        : this(settings: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryLoggerProvider"/> class.
    /// </summary>
    /// <param name="settings">The <see cref="MemoryLoggerSettings"/>.</param>
    public MemoryLoggerProvider(MemoryLoggerSettings? settings)
    {
        _settings = settings ?? new MemoryLoggerSettings();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryLoggerProvider"/> class.
    /// </summary>
    /// <param name="options">The <see cref="IOptions{MemoryLoggerSettings}"/>.</param>
    public MemoryLoggerProvider(IOptions<MemoryLoggerSettings> options)
        : this(options.Value)
    {
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string name)
    {
        return new MemoryLogger(name, _settings, _scopeProvider, WriteLog);
    }

    /// <inheritdoc />
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }


    /// <summary>
    /// Clears the queued log messages.
    /// </summary>
    public void Clear()
    {
        #if NET6_0_OR_GREATER
        _logEntries.Clear();
        #else
        while(_logEntries.TryDequeue(out _));
        #endif
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Clear();
    }


    /// <summary>
    /// Gets the log entries.
    /// </summary>
    /// <returns>A readonly list of log entries</returns>
    public IReadOnlyList<MemoryLogEntry> GetEntries()
    {
        return [.. _logEntries];
    }


    private void WriteLog(MemoryLogEntry logEntry)
    {
        _logEntries.Enqueue(logEntry);

        // ensure capacity
        while (_logEntries.Count > _settings.Capacity)
            if (!_logEntries.TryDequeue(out _))
                break;
    }
}
