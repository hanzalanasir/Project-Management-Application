using FluentAssertions;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Infrastructure.Persistence;
using ProjectManagementApp.Infrastructure.Services;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Services;

// Previously zero test coverage for QueryScopedAsync — 005's stage 1 T006 only verified the
// method exists and is implemented, not that its scoping is correct. Found via 005 stage 3: the
// original implementation compared ActivityLog.EntityId (the audited entity's own id) directly
// against the caller's visible-PROJECT ids, which can only ever match EntityType "Project" rows —
// every Task/TeamMember row was silently invisible to every non-Admin caller. Fixed by stamping
// ProjectId on ActivityLog at write time (new nullable column, migration AddActivityLogProjectId)
// and scoping reads on that column directly. These tests prove the fix and guard the regression.
[Collection(PostgresCollection.Name)]
public class ActivityLogServiceTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public ActivityLogServiceTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static ApplicationDbContext CreateDb(PostgresFixture fixture) => fixture.CreateDbContext();

    [Fact]
    public async Task QueryScopedAsync_TaskEntityRow_IsVisibleToAScopedCallerOfItsProject()
    {
        await using var db = CreateDb(_fixture);
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var svc = new ActivityLogService(db);
        await svc.LogAsync(null, "TaskCreated", "Task", taskId.ToString(), "Task created", CancellationToken.None, projectId: projectId);
        await db.SaveChangesAsync(CancellationToken.None);

        var scope = new ActivityScope(new[] { projectId }, Unscoped: false);
        var result = await svc.QueryScopedAsync(scope, 1, 20, CancellationToken.None);

        result.Items.Should().ContainSingle(e => e.EntityType == "Task" && e.EntityId == taskId.ToString());
    }

    [Fact]
    public async Task QueryScopedAsync_TeamMemberEntityRow_IsVisibleToAScopedCallerOfItsProject()
    {
        await using var db = CreateDb(_fixture);
        var projectId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var svc = new ActivityLogService(db);
        await svc.LogAsync(null, "TeamMemberAdded", "TeamMember", memberId.ToString(), "Added", CancellationToken.None, projectId: projectId);
        await db.SaveChangesAsync(CancellationToken.None);

        var scope = new ActivityScope(new[] { projectId }, Unscoped: false);
        var result = await svc.QueryScopedAsync(scope, 1, 20, CancellationToken.None);

        result.Items.Should().ContainSingle(e => e.EntityType == "TeamMember");
    }

    [Fact]
    public async Task QueryScopedAsync_RowForADifferentProject_IsExcluded()
    {
        await using var db = CreateDb(_fixture);
        var visibleProjectId = Guid.NewGuid();
        var otherProjectId = Guid.NewGuid();

        var svc = new ActivityLogService(db);
        await svc.LogAsync(null, "TaskCreated", "Task", Guid.NewGuid().ToString(), "In other project", CancellationToken.None, projectId: otherProjectId);
        await db.SaveChangesAsync(CancellationToken.None);

        var scope = new ActivityScope(new[] { visibleProjectId }, Unscoped: false);
        var result = await svc.QueryScopedAsync(scope, 1, 20, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task QueryScopedAsync_RowWithNoProjectId_UserEvent_IsExcludedFromScopedReads()
    {
        await using var db = CreateDb(_fixture);
        var projectId = Guid.NewGuid();

        var svc = new ActivityLogService(db);
        // No projectId passed — matches how Auth handlers (Login/Register/etc.) call LogAsync.
        await svc.LogAsync(Guid.NewGuid(), "UserLoggedIn", "User", Guid.NewGuid().ToString(), "Logged in", CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);

        var scope = new ActivityScope(new[] { projectId }, Unscoped: false);
        var result = await svc.QueryScopedAsync(scope, 1, 20, CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryScopedAsync_Unscoped_SeesEverything_RegardlessOfProjectId()
    {
        await using var db = CreateDb(_fixture);
        var svc = new ActivityLogService(db);

        await svc.LogAsync(null, "TaskCreated", "Task", Guid.NewGuid().ToString(), "Scoped row", CancellationToken.None, projectId: Guid.NewGuid());
        await svc.LogAsync(Guid.NewGuid(), "UserLoggedIn", "User", Guid.NewGuid().ToString(), "Unscoped row", CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);

        var scope = new ActivityScope(Array.Empty<Guid>(), Unscoped: true);
        var result = await svc.QueryScopedAsync(scope, 1, 20, CancellationToken.None);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryScopedAsync_TotalCount_IsScoped_NotTheSystemWideCount()
    {
        await using var db = CreateDb(_fixture);
        var visibleProjectId = Guid.NewGuid();
        var otherProjectId = Guid.NewGuid();

        var svc = new ActivityLogService(db);
        await svc.LogAsync(null, "TaskCreated", "Task", Guid.NewGuid().ToString(), "Visible", CancellationToken.None, projectId: visibleProjectId);
        for (var i = 0; i < 5; i++)
        {
            await svc.LogAsync(null, "TaskCreated", "Task", Guid.NewGuid().ToString(), "Invisible", CancellationToken.None, projectId: otherProjectId);
        }
        await db.SaveChangesAsync(CancellationToken.None);

        var scope = new ActivityScope(new[] { visibleProjectId }, Unscoped: false);
        var result = await svc.QueryScopedAsync(scope, 1, 20, CancellationToken.None);

        result.TotalCount.Should().Be(1);
    }
}
