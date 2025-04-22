using Microsoft.Extensions.Logging;

namespace Test.Hosting.Logging;

/// <summary>
/// Memory logger settings
/// </summary>
public class MemoryLoggerSettings
{
    /// <summary>
    /// Gets or sets the memory log capacity size.
    /// </summary>
    public int Capacity { get; set; } = 1024;

    /// <summary>
    /// Gets or sets the minimum level to log.
    /// </summary>
    /// <value>
    /// The minimum level to log.
    /// </value>
    public LogLevel? MinLevel { get; set; }

    /// <summary>
    /// The function used to filter events based on the log level.
    /// </summary>
    public Func<string, LogLevel, bool>? Filter { get; set; }
}
