using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using TestHost.Abstracts.Tests.Data;

namespace TestHost.Abstracts.Tests;

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
