using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Test.Hosting.Logging;

namespace Test.Hosting;

/// <summary>
/// XUnit collection fixture that supports <see cref="HostApplicationBuilder"/>
/// </summary>
public abstract class TestHostApplication : ITestHostApplication
{
    private readonly Lazy<IHost> _host;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestHostApplication"/> class.
    /// </summary>
    protected TestHostApplication()
    {
        _host = new Lazy<IHost>(CreateHost);
    }

    /// <summary>
    /// Gets the host for this test.
    /// </summary>
    /// <value>
    /// The host for this test.
    /// </value>
    public IHost Host => _host.Value;

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
}
