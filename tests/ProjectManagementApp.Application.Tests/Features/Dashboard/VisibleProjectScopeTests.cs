using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Dashboard.Common;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Dashboard;

// Proves T008's two claims: the three-role scope set is correct, and the resolver composes as an
// un-materialized subquery rather than a fetch-then-filter (research R-4). The second claim is
// only meaningful against real PostgreSQL SQL translation (ADR-0007 §2) — ToQueryString() on a
// composed query is the closest we get to "inspect the generated SQL" without a running server.
[Collection(PostgresCollection.Name)]
public class VisibleProjectScopeTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public VisibleProjectScopeTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static ICurrentUserService CurrentUserFor(ApplicationUser user, string role)
    {
        var svc = Substitute.For<ICurrentUserService>();
        svc.Current.Returns(new CurrentUser(user.Id, user.Email!, role, user.FullName));
        return svc;
    }

    [Fact]
    public async Task Resolve_Admin_SeesAllProjects()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var scope = VisibleProjectScope.Resolve(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.Admin, "Admin").Current);

        var ids = await scope.ToListAsync();
        ids.Should().BeEquivalentTo(new[] { scenario.ProjectA.Id, scenario.ProjectB.Id });
    }

    [Fact]
    public async Task Resolve_ProjectManager_SeesOnlyOwnedProjects()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var scope = VisibleProjectScope.Resolve(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.Pm, "ProjectManager").Current);

        var ids = await scope.ToListAsync();
        ids.Should().ContainSingle().Which.Should().Be(scenario.ProjectA.Id);
    }

    [Fact]
    public async Task Resolve_TeamMember_SeesOnlyMemberOfProjects()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var scope = VisibleProjectScope.Resolve(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.Tm, "TeamMember").Current);

        var ids = await scope.ToListAsync();
        ids.Should().ContainSingle().Which.Should().Be(scenario.ProjectA.Id);
    }

    [Fact]
    public async Task Resolve_UnassignedTeamMember_SeesNoProjects()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var scope = VisibleProjectScope.Resolve(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.UnassignedTm, "TeamMember").Current);

        var ids = await scope.ToListAsync();
        ids.Should().BeEmpty();
    }

    [Fact]
    public async Task Resolve_ComposesAsASubquery_NeverMaterializedBeforeComposition()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        // The load-bearing assertion: compose the un-materialized scope into a downstream query
        // (mirroring how every 005 handler will consume it) and confirm EF translates it as a
        // nested SELECT — i.e. `WHERE project_id IN (SELECT ...)` — never as a parameterized list
        // of pre-fetched ids, which is what a `ToListAsync()` inside the resolver would produce.
        var scope = VisibleProjectScope.Resolve(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.Admin, "Admin").Current);

        var composed = db.Tasks.Where(t => scope.Contains(t.ProjectId));
        var sql = composed.ToQueryString();

        sql.Should().Contain("SELECT").And.Contain("FROM").And.Contain("projects");
        // Two SELECTs: the outer tasks query and the inner projects subquery. A materialized list
        // would instead show a literal array/parameter, with only one SELECT against "tasks".
        sql.Split("SELECT", StringSplitOptions.None).Length.Should().BeGreaterThan(2);
    }
}
