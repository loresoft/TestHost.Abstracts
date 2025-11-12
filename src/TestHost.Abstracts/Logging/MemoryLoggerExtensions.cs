using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace TestHost.Abstracts.Logging;

/// <summary>
/// Extension methods for adding memory logger to <see cref="ILoggingBuilder"/>.
/// </summary>
public static class MemoryLoggerExtensions
{
    /// <summary>
    /// Adds a memory logger to the logging builder.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to add the memory logger to.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        // Register MemoryLoggerSettings for configuration
        builder.Services.AddOptions<MemoryLoggerSettings>();

        // Register MemoryLoggerProvider as a singleton
        builder.Services.TryAddSingleton<MemoryLoggerProvider>();

        // Register ILoggerProvider to resolve to MemoryLoggerProvider
        var descriptor = ServiceDescriptor.Singleton<ILoggerProvider>(sp => sp.GetRequiredService<MemoryLoggerProvider>());
        builder.Services.Add(descriptor);

        return builder;
    }

    /// <summary>
    /// Adds a memory logger to the logging builder with the specified configuration.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to add the memory logger to.</param>
    /// <param name="configure">A delegate to configure the <see cref="MemoryLoggerSettings"/>.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <c>null</c>.</exception>
    public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder, Action<MemoryLoggerSettings> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        builder.AddMemoryLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}
