using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Reports.Common;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Reports;

// T013: proves T012's three-branch contract against real PostgreSQL SQL translation (ADR-0007
// §2) — "all" auto-narrows with no 403; a named all-in-scope list succeeds; a list with even one
// out-of-scope id 403s the WHOLE request; a malformed id or empty list 400s before any scope
// check runs; and the "all" branch composes as a subquery, never a materialized list.
[Collection(PostgresCollection.Name)]
public class ReportScopeTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public ReportScopeTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static CurrentUser CurrentUserFor(ApplicationUser user, string role) =>
        new(user.Id, user.Email!, role, user.FullName);

    [Fact]
    public async Task ResolveAsync_NullProjectScope_SilentlyNarrows_NoForbidden()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var result = await ReportScope.ResolveAsync(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.Pm, "ProjectManager"), projectScope: null, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var ids = await result.Value!.ToListAsync();
        ids.Should().ContainSingle().Which.Should().Be(scenario.ProjectA.Id);
    }

    [Fact]
    public async Task ResolveAsync_LiteralAll_SilentlyNarrows_NoForbidden()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var result = await ReportScope.ResolveAsync(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.Pm2, "ProjectManager"), projectScope: "all", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var ids = await result.Value!.ToListAsync();
        ids.Should().ContainSingle().Which.Should().Be(scenario.ProjectB.Id);
    }

    [Fact]
    public async Task ResolveAsync_NamedSingleOutOfScopeId_ReturnsForbidden()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var result = await ReportScope.ResolveAsync(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.Pm, "ProjectManager"), projectScope: scenario.ProjectB.Id.ToString(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
    }

    [Fact]
    public async Task ResolveAsync_NamedListAllInScope_SucceedsCoveringExactlyThoseIds()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        // Admin sees both A and B — a comma-separated list of both, named explicitly, must succeed.
        await scenario.SeedAsync(db);

        var projectScope = $"{scenario.ProjectA.Id},{scenario.ProjectB.Id}";
        var result = await ReportScope.ResolveAsync(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.Admin, "Admin"), projectScope, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var ids = await result.Value!.ToListAsync();
        ids.Should().BeEquivalentTo(new[] { scenario.ProjectA.Id, scenario.ProjectB.Id });
    }

    [Fact]
    public async Task ResolveAsync_NamedListWithOneOutOfScopeId_ForbidsTheWholeRequest_NotPartial()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        // Admin's second project (owned by admin, not PM) so PM has exactly one in-scope id here.
        var adminProject = new ProjectBuilder().WithName("Admin-owned").WithOwner(scenario.Admin).Build();
        db.Projects.Add(adminProject);
        await db.SaveChangesAsync();

        var projectScope = $"{scenario.ProjectA.Id},{adminProject.Id}";
        var result = await ReportScope.ResolveAsync(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.Pm, "ProjectManager"), projectScope, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden,
            "a list must not be partially fulfilled — one out-of-scope id fails the whole request");
    }

    [Fact]
    public async Task ResolveAsync_MalformedId_ReturnsValidation()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var result = await ReportScope.ResolveAsync(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.Pm, "ProjectManager"), projectScope: "not-a-guid", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Validation);
    }

    [Fact]
    public async Task ResolveAsync_EmptyListEntry_ReturnsValidation()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var result = await ReportScope.ResolveAsync(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.Pm, "ProjectManager"), projectScope: $"{scenario.ProjectA.Id},", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Validation);
    }

    [Fact]
    public async Task ResolveAsync_LiteralAll_ComposesAsASubquery_NeverMaterialized()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var result = await ReportScope.ResolveAsync(db, new ProjectAccessPolicy(db),
            CurrentUserFor(scenario.Admin, "Admin"), projectScope: "all", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var composed = db.Tasks.Where(t => result.Value!.Contains(t.ProjectId));
        var sql = composed.ToQueryString();

        sql.Should().Contain("SELECT").And.Contain("projects");
        sql.Split("SELECT", StringSplitOptions.None).Length.Should().BeGreaterThan(2);
    }
}
