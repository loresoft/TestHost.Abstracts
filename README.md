# TestHost.Abstracts

[![CI](https://github.com/loresoft/TestHost.Abstracts/actions/workflows/dotnet.yml/badge.svg)](https://github.com/loresoft/TestHost.Abstracts/actions/workflows/dotnet.yml)
[![NuGet Version](https://img.shields.io/nuget/v/TestHost.Abstracts.svg?style=flat-square)](https://www.nuget.org/packages/TestHost.Abstracts/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TestHost.Abstracts.svg)](https://www.nuget.org/packages/TestHost.Abstracts/)

A flexible and powerful test host builder for .NET applications that provides abstractions for hosting test applications with dependency injection, configuration, and in-memory logging support.

## Features

- **Test Host Abstraction** - Base classes and interfaces for creating test host applications
- **Dependency Injection** - Full integration with Microsoft.Extensions.DependencyInjection
- **Configuration Support** - Leverage Microsoft.Extensions.Configuration for test settings
- **In-Memory Logger** - Capture and assert on log messages during tests
- **Async Lifecycle** - Proper async initialization and disposal patterns

## Installation

Install the package from NuGet:

```shell
dotnet add package TestHost.Abstracts
```

Or via Package Manager Console:

```powershell
Install-Package TestHost.Abstracts
```

## Quick Start

### 1. Create a Test Application

Inherit from `TestHostApplication` to create your test host:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestHost.Abstracts;
using TestHost.Abstracts.Logging;

public class TestApplication : TestHostApplication
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        // Add configuration sources
        builder.Configuration.AddUserSecrets<TestApplication>();
        
        // Configure logging with memory logger
        builder.Logging.AddMemoryLogger();

        // Register your services
        builder.Services.AddSingleton<IMyService, MyService>();
    }
}
```

### 2. Use in Your Tests

> **Note:** The examples below use [TUnit](https://github.com/thomhurst/TUnit), a modern .NET testing framework. You can also use this library with xUnit, NUnit, or MSTest.

```csharp
using Microsoft.Extensions.DependencyInjection;
using TestHost.Abstracts.Logging;

public class MyServiceTests
{
    [ClassDataSource<TestApplication>(Shared = SharedType.PerAssembly)]
    public required TestApplication Application { get; init; }

    [Test]
    public async Task TestServiceBehavior()
    {
        // Arrange - Get service from DI container
        var service = Application.Services.GetRequiredService<IMyService>();

        // Act
        service.DoSomething();

        // Assert - Verify behavior
        Assert.That(service.SomeProperty).IsTrue();

        // Assert on log messages
        var memoryLogger = Application.Services.GetService<MemoryLoggerProvider>();
        var logs = memoryLogger?.Logs();
        
        Assert.That(logs).Contains(log => log.Message.Contains("Expected log message"));
    }
}
```

## Core Components

### ITestHostApplication

The primary interface for test host applications:

```csharp
public interface ITestHostApplication : IAsyncDisposable
{
    /// <summary>
    /// Gets the host for the tests.
    /// </summary>
    IHost Host { get; }

    /// <summary>
    /// Gets the services configured for this test host.
    /// </summary>
    IServiceProvider Services { get; }
}
```

### TestHostApplication

Base class that implements `ITestHostApplication` with convenient lifecycle management:

- **Thread-safe Host Creation** - Lazy initialization with proper locking
- **Configurable Builder** - Override `CreateBuilderSettings()` to customize host builder settings
- **Application Configuration** - Override `ConfigureApplication()` to configure services and logging
- **Async Disposal** - Proper cleanup of host resources

#### Lifecycle Hooks

```csharp
public class TestApplication : TestHostApplication
{
    // Customize builder settings
    protected override HostApplicationBuilderSettings? CreateBuilderSettings()
    {
        return new HostApplicationBuilderSettings
        {
            EnvironmentName = "Testing"
        };
    }

    // Configure the application
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);
        
        // Your configuration here
    }

    // Custom host creation (advanced)
    protected override IHost CreateHost()
    {
        // Custom host creation logic
        return base.CreateHost();
    }
}
```

## In-Memory Logger

The in-memory logger captures log messages during test execution for verification and debugging.

### Features

- Capture log entries in memory
- Query logs by category, log level, or custom filters
- Thread-safe log collection
- Configurable capacity and filtering
- Structured logging support with scopes and state

### Adding the Memory Logger

```csharp
protected override void ConfigureApplication(HostApplicationBuilder builder)
{
    base.ConfigureApplication(builder);
    
    // Add memory logger with default settings
    builder.Logging.AddMemoryLogger();
    
    // Or with custom settings
    builder.Logging.AddMemoryLogger(options =>
    {
        options.MinimumLevel = LogLevel.Debug;
        options.Capacity = 2048;
        options.Filter = (category, level) => category.StartsWith("MyApp");
    });
}
```

### Querying Logs

```csharp
// Get the memory logger provider
var memoryLogger = Application.Services.GetService<MemoryLoggerProvider>();

// Get all logs
var allLogs = memoryLogger?.Logs();

// Get logs by category
var categoryLogs = memoryLogger?.Logs("MyApp.Services.MyService");

// Get logs by level (warning and above)
var warningLogs = memoryLogger?.Logs(LogLevel.Warning);

// Clear logs between tests
memoryLogger?.Clear();
```

### Asserting on Logs

```csharp
[Test]
public async Task VerifyLogging()
{
    // Arrange
    var service = Application.Services.GetRequiredService<IMyService>();
    var logger = Application.Services.GetService<MemoryLoggerProvider>();
    
    // Act
    service.PerformAction();
    
    // Assert
    var logs = logger?.Logs();
    
    await Assert.That(logs).IsNotEmpty();
    await Assert.That(logs).Contains(log => 
        log.LogLevel == LogLevel.Information &&
        log.Message.Contains("Action performed"));
}
```

### MemoryLoggerSettings

Configure the memory logger with these options:

- **`MinimumLevel`** - Minimum log level to capture (default: `LogLevel.Debug`)
- **`Capacity`** - Maximum number of log entries to keep (default: 1024)
- **`Filter`** - Custom filter function for fine-grained control

### MemoryLogEntry

Log entries captured include:

- **`Timestamp`** - DateTime when the log entry was created
- **`LogLevel`** - The log level of the entry (Trace, Debug, Information, Warning, Error, Critical)
- **`EventId`** - Event identifier associated with the log entry
- **`Category`** - Category name of the logger that created this entry
- **`Message`** - Formatted log message
- **`Exception`** - Exception associated with the log entry, if any (nullable)
- **`State`** - The state object passed to the logger (nullable)
- **`Scopes`** - Read-only collection of scope values that were active when the log entry was created

## Integration Testing with Docker Databases

TestHost.Abstracts works seamlessly with Testcontainers to provide isolated database environments for integration tests. This approach uses `IAsyncInitializer` to manage container lifecycle and `IHostedService` to seed the database.

### Install Testcontainers

```bash
dotnet add package Testcontainers.MsSql
```

### Integration with Test Containers

```csharp
using Testcontainers.MsSql;

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

    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        var connectionString = _msSqlContainer.GetConnectionString();
        builder.Services.AddDbContext<MyDbContext>(options =>
            options.UseSqlServer(connectionString));
    }

    public override async ValueTask DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
```

### Database Initialization with Hosted Services

```csharp
public class DatabaseInitialize : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseInitialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        await context.Database.EnsureCreatedAsync(cancellationToken);
        // Seed test data
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

// Register in ConfigureApplication
builder.Services.AddHostedService<DatabaseInitialize>();
```

### Write Database Tests

```c#
public class DatabaseTests
{
    [ClassDataSource<TestApplication>(Shared = SharedType.PerAssembly)]
    public required TestApplication Application { get; init; }

    [Test]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Arrange
        var dbContext = Application.Services.GetRequiredService<SampleDataContext>();

        // Act
        var user = await dbContext.Users.FindAsync([1]);

        // Assert
        await Assert.That(user).IsNotNull();
        await Assert.That(user.Name).IsEqualTo("Test User 1");
        await Assert.That(user.Email).IsEqualTo("user1@test.com");
    }

    [Test]
    public async Task GetAllUsers_ReturnsSeededUsers()
    {
        // Arrange
        var dbContext = Application.Services.GetRequiredService<SampleDataContext>();

        // Act
        var users = await dbContext.Users.ToListAsync();

        // Assert
        await Assert.That(users.Count).IsGreaterThanOrEqualTo(2);
    }
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
