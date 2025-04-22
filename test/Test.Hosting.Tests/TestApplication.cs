using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestHost.Abstracts.Tests;

public class TestApplication : TestHostApplication
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        builder.Services.AddSingleton<IService, Service>();
        builder.Services.AddHostedService<ApplicationInitializer>();
    }
}
