using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using TestHost.Abstracts.Tests.Data;

namespace TestHost.Abstracts.Tests;

public class DatabaseInitialize : IHostedService
{
    private readonly ILogger<DatabaseInitialize> _logger;
    private readonly SampleDataContext _dataContext;

    public DatabaseInitialize(ILogger<DatabaseInitialize> logger, SampleDataContext dataContext)
    {
        _logger = logger;
        _dataContext = dataContext;
    }

    public static bool IsStarted { get; set; }

    public static bool IsStopped { get; set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initialize Database StartAsync()");

        // Ensure database is created and apply migrations
        await _dataContext.Database.EnsureCreatedAsync(cancellationToken);

        // Or use migrations
        //await dbContext.Database.MigrateAsync(cancellationToken);

        // Seed test data
        _dataContext.Users.AddRange(
            new User { Name = "Test User 1", Email = "user1@test.com" },
            new User { Name = "Test User 2", Email = "user2@test.com" }
        );

        await _dataContext.SaveChangesAsync(cancellationToken);

        IsStarted = true;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initialize Database StopAsync()");

        IsStopped = true;
        return Task.CompletedTask;
    }
}
