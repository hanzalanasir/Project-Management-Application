using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Tasks;

// Proves the cascade/RESTRICT behavior 001 configured on tasks.project_id/assignee_id
// (data-model.md §1) — 003 is the first feature to observe it directly.
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
    public async Task DeletingProject_CascadesItsTasks_AndActivityLogsSurvive()
    {
        await using var db = _fixture.CreateDbContext();

        var owner = NewUser("task-cascade-owner@example.com");
        var project = NewProject("Task Cascade Project", owner);
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
        db.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            ActorId = owner.Id,
            Action = "TaskCreated",
            EntityType = "Task",
            EntityId = task.Id.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            ChangeSummary = "Created Cascade Task"
        });
        await db.SaveChangesAsync(CancellationToken.None);

        db.Projects.Remove(project);
        await db.SaveChangesAsync(CancellationToken.None);

        (await db.Tasks.FindAsync(task.Id)).Should().BeNull();
        var survivingLog = await db.ActivityLogs.SingleOrDefaultAsync(a => a.EntityId == task.Id.ToString());
        survivingLog.Should().NotBeNull();
    }

    [Fact]
    public async Task DeletingUser_WhoIsAssignedATask_IsRestricted()
    {
        var owner = NewUser("task-restrict-owner@example.com");
        var assignee = NewUser("task-restrict-assignee@example.com");
        var project = NewProject("Task Restrict Project", owner);
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Restrict Task",
            Project = project,
            Assignee = assignee,
            AssigneeId = assignee.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await using (var seed = _fixture.CreateDbContext())
        {
            seed.Users.AddRange(owner, assignee);
            seed.Projects.Add(project);
            seed.Tasks.Add(task);
            await seed.SaveChangesAsync(CancellationToken.None);
        }

        // Fresh, untracked context — same reasoning as 002's equivalent test: only the assignee is
        // loaded, so EF cannot client-side-null the FK itself, and the real RESTRICT constraint
        // must be what fires.
        await using var db = _fixture.CreateDbContext();
        var trackedAssignee = await db.Users.SingleAsync(u => u.Id == assignee.Id);
        db.Users.Remove(trackedAssignee);
        var act = async () => await db.SaveChangesAsync(CancellationToken.None);

        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
