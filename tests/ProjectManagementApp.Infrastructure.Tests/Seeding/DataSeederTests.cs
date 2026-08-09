using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Services;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Seeding;

[Collection(PostgresCollection.Name)]
public class DataSeederTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public DataSeederTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task SeedAsync_OnEmptyDatabase_CreatesExactlyThreeRolesAndThreeUsers_EachWithHashedPassword()
    {
        var seeder = SeederTestHarness.Create(_fixture, out var db);

        await seeder.SeedAsync(CancellationToken.None);

        var roles = await db.Roles.ToListAsync();
        var users = await db.Users.ToListAsync();

        roles.Should().HaveCount(3);
        roles.Select(r => r.Name).Should().BeEquivalentTo("Admin", "ProjectManager", "TeamMember");

        users.Should().HaveCount(3);
        foreach (var user in users)
        {
            user.PasswordHash.Should().NotBeNullOrEmpty();
            user.PasswordHash.Should().NotBe(SeederTestHarness.AdminPassword);
            user.PasswordHash.Should().NotBe(SeederTestHarness.ProjectManagerPassword);
            user.PasswordHash.Should().NotBe(SeederTestHarness.TeamMemberPassword);
        }
    }
}
