using Microsoft.Extensions.Logging;

namespace TestHost.Abstracts.Tests;

public class Service : IService
{
    private readonly ILogger<Service> _logger;

    public Service(ILogger<Service> logger)
    {
        _logger = logger;
    }

    public static bool IsRun = false;

    public bool Run()
    {
        _logger.LogInformation("Service Run()");

        IsRun = true;
        return IsRun;
    }
}
