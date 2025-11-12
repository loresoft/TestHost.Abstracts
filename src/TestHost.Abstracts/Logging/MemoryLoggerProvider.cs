using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TestHost.Abstracts.Logging;

/// <summary>
/// Provides an in-memory logger implementation that stores log entries in a memory buffer for testing purposes.
/// </summary>
[ProviderAlias("MemoryLogger")]
public class MemoryLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentQueue<MemoryLogEntry> _logEntries = new();
    private readonly ConcurrentDictionary<string, MemoryLogger> _loggers = new();
    private readonly MemoryLoggerSettings _settings;

    private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryLoggerProvider"/> class.
    /// </summary>
    /// <param name="settings">The configuration settings for the memory logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <c>null</c>.</exception>
    public MemoryLoggerProvider(IOptions<MemoryLoggerSettings> settings)
    {
        if (settings is null)
            throw new ArgumentNullException(nameof(settings));

        _settings = settings.Value;
    }


    /// <summary>
    /// Sets the external scope provider for this logger provider.
    /// </summary>
    /// <param name="scopeProvider">The external scope provider to use for managing log scopes.</param>
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    /// <summary>
    /// Creates a new <see cref="ILogger"/> instance for the specified category.
    /// </summary>
    /// <param name="category">The category name for the logger.</param>
    /// <returns>A new <see cref="ILogger"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="category"/> is <c>null</c>.</exception>
    public ILogger CreateLogger(string category)
    {
        if (category is null)
            throw new ArgumentNullException(nameof(category));

        return _loggers.GetOrAdd(category, _ =>
            new MemoryLogger(category, _settings, _scopeProvider, AddLogEntry));
    }


    /// <summary>
    /// Adds a log entry to the memory buffer and enforces the capacity limit.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    internal void AddLogEntry(MemoryLogEntry entry)
    {
        _logEntries.Enqueue(entry);

        // Enforce capacity limit
        while (_logEntries.Count > _settings.Capacity)
            _logEntries.TryDequeue(out _);
    }


    /// <summary>
    /// Gets all log entries currently stored in memory.
    /// </summary>
    /// <returns>A read-only list of all <see cref="MemoryLogEntry"/> instances.</returns>
    public IReadOnlyList<MemoryLogEntry> Logs()
    {
        return [.. _logEntries];
    }

    /// <summary>
    /// Gets all log entries for the specified category name.
    /// </summary>
    /// <param name="categoryName">The category name to filter by (case-insensitive).</param>
    /// <returns>A read-only list of <see cref="MemoryLogEntry"/> instances matching the specified category.</returns>
    public IReadOnlyList<MemoryLogEntry> Logs(string categoryName)
    {
        return [.. _logEntries.Where(e => string.Equals(e.Category, categoryName, StringComparison.OrdinalIgnoreCase))];
    }

    /// <summary>
    /// Gets all log entries at or above the specified log level.
    /// </summary>
    /// <param name="logLevel">The minimum log level to include.</param>
    /// <returns>A read-only list of <see cref="MemoryLogEntry"/> instances at or above the specified log level.</returns>
    public IReadOnlyList<MemoryLogEntry> Logs(LogLevel logLevel)
    {
        return [.. _logEntries.Where(e => e.LogLevel >= logLevel)];
    }

    /// <summary>
    /// Clears all log entries from the memory buffer.
    /// </summary>
    public void Clear()
    {
        while (_logEntries.TryDequeue(out _)) ;
    }


    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="MemoryLoggerProvider"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;

        if (!disposing)
            return;

        // dispose managed state (managed objects)
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
