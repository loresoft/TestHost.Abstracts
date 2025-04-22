using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace TestHost.Abstracts.Logging;

/// <summary>
/// Extension methods for the <see cref="ILoggerFactory"/> class.
/// </summary>
public static class MemoryLoggerExtensions
{
    /// <summary>
    /// Adds a memory logger named 'MemoryLogger' to the factory.
    /// </summary>
    /// <param name="builder">The extension method argument.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var descriptor = ServiceDescriptor.Singleton<ILoggerProvider, MemoryLoggerProvider>();

        builder.Services.TryAddEnumerable(descriptor);

        return builder;
    }

    /// <summary>
    /// Adds a memory logger. Use <paramref name="settings"/> to enable logging for specific <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="builder">The extension method argument.</param>
    /// <param name="settings">The <see cref="MemoryLoggerSettings"/> to use.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder, MemoryLoggerSettings settings)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));
        if (settings is null)
            throw new ArgumentNullException(nameof(settings));

        var logger = new MemoryLoggerProvider(settings);
        var descriptor = ServiceDescriptor.Singleton<ILoggerProvider>(logger);

        builder.Services.TryAddEnumerable(descriptor);

        return builder;
    }

    /// <summary>
    /// Adds a memory logger. Use <paramref name="configure"/> to enable logging for specific <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="builder">The extension method argument.</param>
    /// <param name="configure">A delegate to configure the <see cref="MemoryLoggerSettings"/>.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder, Action<MemoryLoggerSettings> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        builder.AddMemoryLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}
