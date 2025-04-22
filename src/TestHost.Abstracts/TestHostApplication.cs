using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using TestHost.Abstracts.Logging;

namespace TestHost.Abstracts;

/// <summary>
/// A base class for hosting a test application.
/// </summary>
public abstract class TestHostApplication : ITestHostApplication
{
#if NET9_0_OR_GREATER
    private readonly Lock _lockObject = new();
#else
    private readonly object _lockObject = new();
#endif

    private IHost? _host;

    /// <summary>
    /// Gets the host for this test.
    /// </summary>
    /// <value>
    /// The host for this test.
    /// </value>
    public IHost Host
    {
        get
        {
            if (_host != null)
                return _host;

            lock (_lockObject)
                _host ??= CreateHost();

            return _host;
        }
    }

    /// <summary>
    /// Gets the services configured for this test
    /// </summary>
    /// <value>
    /// The services configured for this test.
    /// </value>
    public IServiceProvider Services => Host.Services;

    /// <summary>
    /// Create the test host program abstraction.
    /// </summary>
    /// <returns>An initialized <see cref="IHost"/>.</returns>
    protected virtual IHost CreateHost()
    {
        var settings = CreateBuilderSettings();

        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(settings);

        ConfigureApplication(builder);

        var app = builder.Build();

        app.Start();

        return app;
    }

    /// <summary>
    /// Creates the settings for constructing an Microsoft.Extensions.Hosting.HostApplicationBuilder.
    /// </summary>
    /// <returns>A new instance of <see cref="HostApplicationBuilderSettings"/></returns>
    protected virtual HostApplicationBuilderSettings? CreateBuilderSettings()
    {
        return null;
    }

    /// <summary>
    /// Configures the application using the specified <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The host application builder to configure.</param>
    protected virtual void ConfigureApplication(HostApplicationBuilder builder)
    {
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddMemoryLogger();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_host == null)
            return;

        await _host.StopAsync();

        _host.Dispose();
        _host = null;

        GC.SuppressFinalize(this);
    }
}
