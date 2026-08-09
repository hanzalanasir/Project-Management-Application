using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Projects;

// Proves the cascade/RESTRICT behavior 001 configured (data-model.md §1, research.md R-5) —
// 002 is the first feature with a delete endpoint, so the first point these are observable.
[Collection(PostgresCollection.Name)]
public class CascadeBehaviorTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public CascadeBehaviorTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static ApplicationUser NewUser(string email) => new()
    {
        Id = Guid.NewGuid(),
        UserName = email,
        NormalizedUserName = email.ToUpperInvariant(),
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        FullName = email,
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    private static Project NewProject(string name, ApplicationUser owner) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Owner = owner,
        OwnerId = owner.Id,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task DeletingProject_CascadesToTasksAndTeamMembers()
    {
        await using var db = _fixture.CreateDbContext();

        var owner = NewUser("cascade-full-owner@example.com");
        var member = NewUser("cascade-full-member@example.com");
        var project = NewProject("Cascade Full", owner);
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Cascade Full Task",
            Project = project,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var teamMember = new TeamMember
        {
            Id = Guid.NewGuid(),
            Project = project,
            User = member,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Users.AddRange(owner, member);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        db.TeamMembers.Add(teamMember);
        await db.SaveChangesAsync(CancellationToken.None);

        db.Projects.Remove(project);
        await db.SaveChangesAsync(CancellationToken.None);

        (await db.Tasks.FindAsync(task.Id)).Should().BeNull();
        (await db.TeamMembers.FindAsync(teamMember.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeletingUser_WhoOwnsAProject_IsRestricted()
    {
        var owner = NewUser("restricted-owner@example.com");
        var project = NewProject("Restricted Project", owner);

        await using (var seed = _fixture.CreateDbContext())
        {
            seed.Users.Add(owner);
            seed.Projects.Add(project);
            await seed.SaveChangesAsync(CancellationToken.None);
        }

        // A fresh, untracked context: only the user is loaded, so EF has no in-memory knowledge
        // of the dependent Project row and cannot client-side-null the FK itself (which is what
        // produced an InvalidOperationException before the database was ever reached). This way
        // the DELETE actually reaches PostgreSQL and the real RESTRICT constraint fires.
        await using var db = _fixture.CreateDbContext();
        var trackedOwner = await db.Users.SingleAsync(u => u.Id == owner.Id);
        db.Users.Remove(trackedOwner);
        var act = async () => await db.SaveChangesAsync(CancellationToken.None);

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeletingProject_DoesNotCascadeActivityLogs()
    {
        await using var db = _fixture.CreateDbContext();

        var owner = NewUser("cascade-audit-owner@example.com");
        var project = NewProject("Cascade Audit", owner);

        db.Users.Add(owner);
        db.Projects.Add(project);
        db.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            ActorId = owner.Id,
            Action = "ProjectCreated",
            EntityType = "Project",
            EntityId = project.Id.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            ChangeSummary = "Created Cascade Audit"
        });
        await db.SaveChangesAsync(CancellationToken.None);

        db.Projects.Remove(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var survivingLog = await db.ActivityLogs.SingleOrDefaultAsync(a => a.EntityId == project.Id.ToString());
        survivingLog.Should().NotBeNull();
    }
}
