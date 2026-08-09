using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Persistence;

[Collection(PostgresCollection.Name)]
public class MigrationAppliesTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public MigrationAppliesTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task InitialCreate_CreatesAllFiveConstitutionEntitiesAsRealTables()
    {
        await using var db = _fixture.CreateDbContext();

        var tableNames = await db.Database
            .SqlQuery<string>($"SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'")
            .ToListAsync();

        tableNames.Should().Contain(new[] { "users", "roles", "refresh_tokens", "activity_logs", "projects", "tasks", "team_members" });
    }

    [Fact]
    public async Task ProjectsAndTasksAndUsers_HaveXminRowVersionColumn()
    {
        await using var db = _fixture.CreateDbContext();

        var owner = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "xmin-test@example.com",
            NormalizedUserName = "XMIN-TEST@EXAMPLE.COM",
            Email = "xmin-test@example.com",
            NormalizedEmail = "XMIN-TEST@EXAMPLE.COM",
            FullName = "Xmin Test",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.Users.Add(owner);
        await db.SaveChangesAsync(CancellationToken.None);

        owner.Version.Should().NotBe(0u);
    }

    [Fact]
    public async Task Tasks_CascadeDeletes_WhenParentProjectIsDeleted()
    {
        await using var db = _fixture.CreateDbContext();

        var owner = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "cascade-owner@example.com",
            NormalizedUserName = "CASCADE-OWNER@EXAMPLE.COM",
            Email = "cascade-owner@example.com",
            NormalizedEmail = "CASCADE-OWNER@EXAMPLE.COM",
            FullName = "Cascade Owner",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Cascade Project",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Owner = owner,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Cascade Task",
            Project = project,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.Users.Add(owner);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        db.Projects.Remove(project);
        await db.SaveChangesAsync(CancellationToken.None);

        (await db.Tasks.FindAsync(task.Id)).Should().BeNull();
    }
}
