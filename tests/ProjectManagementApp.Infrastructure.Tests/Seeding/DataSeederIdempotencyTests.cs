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
