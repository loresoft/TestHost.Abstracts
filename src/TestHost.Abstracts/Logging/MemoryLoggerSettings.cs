using Microsoft.Extensions.Logging;

namespace TestHost.Abstracts.Logging;

/// <summary>
/// Configuration settings for the memory logger.
/// </summary>
public class MemoryLoggerSettings
{
    /// <summary>
    /// Gets or sets the minimum log level that will be captured by the memory logger.
    /// </summary>
    /// <value>
    /// The minimum <see cref="LogLevel"/> to log. The default value is <see cref="LogLevel.Debug"/>.
    /// </value>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// Gets or sets the maximum number of log entries to store in memory.
    /// </summary>
    /// <value>
    /// The capacity of the memory buffer. The default value is 1024.
    /// </value>
    public int Capacity { get; set; } = 1024;

    /// <summary>
    /// Gets or sets an optional filter function to determine whether a log entry should be captured.
    /// </summary>
    /// <value>
    /// A function that takes a category name and log level, and returns <c>true</c> if the log entry should be captured; otherwise, <c>false</c>.
    /// The default value is <c>null</c>, meaning no additional filtering is applied beyond the <see cref="MinimumLevel"/>.
    /// </value>
    public Func<string, LogLevel, bool>? Filter { get; set; }
}
