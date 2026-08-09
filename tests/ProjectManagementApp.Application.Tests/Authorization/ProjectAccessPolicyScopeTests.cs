using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Tests.Authorization;

// The primary acceptance test of 002's security model (data-model.md §3, quickstart V4). Runs
// against real PostgreSQL, never EF InMemory (ADR-0007 §2) — the predicate must be proven to
// reach SQL, and out-of-scope rows must never be materialized, which is unrepresentable in-memory.
[Collection(PostgresCollection.Name)]
public class ProjectAccessPolicyScopeTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public ProjectAccessPolicyScopeTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ApplyScope_ThreeRoleMatrix_EachRoleSeesExactlyItsOwnRows()
    {
        await using var db = _fixture.CreateDbContext();

        var admin = new ApplicationUserBuilder().WithEmail("admin@scope-test.com").Build();
        var pm = new ApplicationUserBuilder().WithEmail("pm@scope-test.com").Build();
        var pm2 = new ApplicationUserBuilder().WithEmail("pm2@scope-test.com").Build();
        var tm = new ApplicationUserBuilder().WithEmail("tm@scope-test.com").Build();
        var unassignedTm = new ApplicationUserBuilder().WithEmail("unassigned-tm@scope-test.com").Build();

        var projectA = new ProjectBuilder().WithName("Project A").WithOwner(pm).Build();
        var projectB = new ProjectBuilder().WithName("Project B").WithOwner(pm2).Build();

        db.Users.AddRange(admin, pm, pm2, tm, unassignedTm);
        db.Projects.AddRange(projectA, projectB);
        db.TeamMembers.Add(new Domain.Entities.TeamMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectA.Id,
            UserId = tm.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new ProjectAccessPolicy(db);

        var adminResult = await policy.ApplyScope(db.Projects, Caller(admin.Id, Role.Admin)).ToListAsync();
        adminResult.Select(p => p.Id).Should().BeEquivalentTo(new[] { projectA.Id, projectB.Id });

        var pmResult = await policy.ApplyScope(db.Projects, Caller(pm.Id, Role.ProjectManager)).ToListAsync();
        pmResult.Select(p => p.Id).Should().BeEquivalentTo(new[] { projectA.Id });

        var pm2Result = await policy.ApplyScope(db.Projects, Caller(pm2.Id, Role.ProjectManager)).ToListAsync();
        pm2Result.Select(p => p.Id).Should().BeEquivalentTo(new[] { projectB.Id });

        var tmResult = await policy.ApplyScope(db.Projects, Caller(tm.Id, Role.TeamMember)).ToListAsync();
        tmResult.Select(p => p.Id).Should().BeEquivalentTo(new[] { projectA.Id });

        var unassignedResult = await policy.ApplyScope(db.Projects, Caller(unassignedTm.Id, Role.TeamMember)).ToListAsync();
        unassignedResult.Should().BeEmpty();
    }

    private static CurrentUser Caller(Guid userId, Role role) =>
        new(userId, "caller@scope-test.com", role.ToString(), "Caller");
}
