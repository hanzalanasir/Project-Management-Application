using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Tests.Authorization;

// data-model.md §3's ApplyScope predicates, against real PostgreSQL (ADR-0007 §2) — the PM
// predicate must be proven to fold into a SQL join through the Project navigation, and the
// TeamMember predicate must be proven to scope by ASSIGNMENT, not membership: a TeamMember on a
// project's team sees only tasks assigned to them, not every task on that project (quickstart V5).
[Collection(PostgresCollection.Name)]
public class TaskAccessPolicyScopeTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public TaskAccessPolicyScopeTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static CurrentUser Caller(Guid userId, Role role) =>
        new(userId, "caller@task-scope-test.com", role.ToString(), "Caller");

    [Fact]
    public async Task ApplyScope_ThreeRoleMatrix_PmSeesOwnedProjectTasks_TmSeesOnlyAssignedTasks_NotEveryTaskOnTheirProject()
    {
        await using var db = _fixture.CreateDbContext();

        var admin = new ApplicationUserBuilder().WithEmail("admin@task-scope-test.com").Build();
        var pm = new ApplicationUserBuilder().WithEmail("pm@task-scope-test.com").Build();
        var pm2 = new ApplicationUserBuilder().WithEmail("pm2@task-scope-test.com").Build();
        var tm = new ApplicationUserBuilder().WithEmail("tm@task-scope-test.com").Build();
        var tm2 = new ApplicationUserBuilder().WithEmail("tm2@task-scope-test.com").Build();

        var projectA = new ProjectBuilder().WithName("Task Scope Project A").WithOwner(pm).Build();
        var projectB = new ProjectBuilder().WithName("Task Scope Project B").WithOwner(pm2).Build();

        // Both TM and TM2 are on project A's TEAM, but only TM is ASSIGNED task1 and only TM2 is
        // assigned task2 — proving scope is by assignment, not membership (data-model.md §3).
        var task1 = new TaskItem { Id = Guid.NewGuid(), ProjectId = projectA.Id, Project = projectA, Title = "T1", AssigneeId = tm.Id, Assignee = tm, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        var task2 = new TaskItem { Id = Guid.NewGuid(), ProjectId = projectA.Id, Project = projectA, Title = "T2", AssigneeId = tm2.Id, Assignee = tm2, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        var task3 = new TaskItem { Id = Guid.NewGuid(), ProjectId = projectA.Id, Project = projectA, Title = "T3 (unassigned)", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        var taskB = new TaskItem { Id = Guid.NewGuid(), ProjectId = projectB.Id, Project = projectB, Title = "Task B", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };

        db.Users.AddRange(admin, pm, pm2, tm, tm2);
        db.Projects.AddRange(projectA, projectB);
        db.Tasks.AddRange(task1, task2, task3, taskB);
        db.TeamMembers.AddRange(
            new TeamMember { Id = Guid.NewGuid(), ProjectId = projectA.Id, UserId = tm.Id, CreatedAt = DateTimeOffset.UtcNow },
            new TeamMember { Id = Guid.NewGuid(), ProjectId = projectA.Id, UserId = tm2.Id, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TaskAccessPolicy(db);

        var adminResult = await policy.ApplyScope(db.Tasks, Caller(admin.Id, Role.Admin)).ToListAsync();
        adminResult.Select(t => t.Id).Should().BeEquivalentTo(new[] { task1.Id, task2.Id, task3.Id, taskB.Id });

        var pmResult = await policy.ApplyScope(db.Tasks, Caller(pm.Id, Role.ProjectManager)).ToListAsync();
        pmResult.Select(t => t.Id).Should().BeEquivalentTo(new[] { task1.Id, task2.Id, task3.Id });

        var pm2Result = await policy.ApplyScope(db.Tasks, Caller(pm2.Id, Role.ProjectManager)).ToListAsync();
        pm2Result.Select(t => t.Id).Should().BeEquivalentTo(new[] { taskB.Id });

        var tmResult = await policy.ApplyScope(db.Tasks, Caller(tm.Id, Role.TeamMember)).ToListAsync();
        tmResult.Select(t => t.Id).Should().BeEquivalentTo(new[] { task1.Id });

        var tm2Result = await policy.ApplyScope(db.Tasks, Caller(tm2.Id, Role.TeamMember)).ToListAsync();
        tm2Result.Select(t => t.Id).Should().BeEquivalentTo(new[] { task2.Id });
    }

    [Fact]
    public async Task ApplyScope_ProjectManagerPredicate_ResolvesAsASqlJoin_NotAPostQueryFilter()
    {
        await using var db = _fixture.CreateDbContext();

        var pm = new ApplicationUserBuilder().WithEmail("pm-join@task-scope-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        var task = new TaskItem { Id = Guid.NewGuid(), ProjectId = project.Id, Project = project, Title = "Join Task", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };

        db.Users.Add(pm);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TaskAccessPolicy(db);
        var query = policy.ApplyScope(db.Tasks, Caller(pm.Id, Role.ProjectManager));

        // Proves the predicate reaches ownership THROUGH the Project navigation as SQL — if it
        // could not translate, EF would throw at ToQueryString() rather than materialize a join.
        var sql = query.ToQueryString();
        sql.Should().Contain("JOIN", "the PM scope predicate must fold into a SQL join through Project, not a client-evaluated filter");

        var result = await query.ToListAsync();
        result.Select(t => t.Id).Should().BeEquivalentTo(new[] { task.Id });
    }
}
