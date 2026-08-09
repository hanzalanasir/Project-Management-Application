using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Seeding;

[Collection(PostgresCollection.Name)]
public class DataSeederIdempotencyTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public DataSeederIdempotencyTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task SeedAsync_RunTwice_CreatesNoDuplicates()
    {
        var seeder = SeederTestHarness.Create(_fixture, out var db);

        await seeder.SeedAsync(CancellationToken.None);
        await seeder.SeedAsync(CancellationToken.None);

        (await db.Roles.CountAsync()).Should().Be(3);
        (await db.Users.CountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task SeedAsync_RunTwice_CreatesNoDuplicateDemoProjects_AllOwnedByTheSeededProjectManager()
    {
        var seeder = SeederTestHarness.Create(_fixture, out var db, demoDataEnabled: true);

        await seeder.SeedAsync(CancellationToken.None);
        var countAfterFirstRun = await db.Projects.CountAsync();
        countAfterFirstRun.Should().BeGreaterThan(0);

        await seeder.SeedAsync(CancellationToken.None);
        (await db.Projects.CountAsync()).Should().Be(countAfterFirstRun);

        var pm = await db.Users.SingleAsync(u => u.Email == "pm@example.com");
        (await db.Projects.AllAsync(p => p.OwnerId == pm.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_AfterOneSeededUserIsDeleted_RecreatesOnlyTheMissingOne()
    {
        var seeder = SeederTestHarness.Create(_fixture, out var db);
        await seeder.SeedAsync(CancellationToken.None);

        var admin = await db.Users.SingleAsync(u => u.Email == "admin@example.com");
        db.Users.Remove(admin);
        await db.SaveChangesAsync(CancellationToken.None);

        (await db.Users.CountAsync()).Should().Be(2);

        await seeder.SeedAsync(CancellationToken.None);

        var users = await db.Users.ToListAsync();
        users.Should().HaveCount(3);
        users.Should().ContainSingle(u => u.Email == "admin@example.com");
    }
}
