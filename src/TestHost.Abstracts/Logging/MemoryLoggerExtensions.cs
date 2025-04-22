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
    /// <param name="configure">Optional configuration action for the <see cref="MemoryLoggerSettings"/>.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder, Action<MemoryLoggerSettings>? configure = null)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        builder.Services.AddOptions<MemoryLoggerSettings>();

        builder.Services.TryAddSingleton<MemoryLoggerProvider>();
        builder.Services.AddSingleton<ILoggerProvider>(sp => sp.GetRequiredService<MemoryLoggerProvider>());

        if (configure is not null)
            builder.Services.Configure(configure);

        return builder;
    }
}
