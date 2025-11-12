using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Testcontainers.MsSql;

using TestHost.Abstracts.Logging;
using TestHost.Abstracts.Tests.Data;

using TUnit.Core.Interfaces;

namespace TestHost.Abstracts.Tests;

public class TestApplication : TestHostApplication, IAsyncInitializer
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("P@ssw0rd123!")
        .Build();

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
        await base.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        builder.Configuration.AddUserSecrets<TestApplication>();
        builder.Logging.AddMemoryLogger();

        // change database from container default
        var connectionBuilder = new SqlConnectionStringBuilder(_msSqlContainer.GetConnectionString());
        connectionBuilder.InitialCatalog = "SampleDataDocker";

        // Register DbContext with container connection string
        builder.Services.AddDbContext<SampleDataContext>(options =>
            options.UseSqlServer(connectionBuilder.ToString())
        );

        builder.Services.AddSingleton<IService, Service>();
        builder.Services.AddHostedService<DatabaseInitialize>();
    }
}
