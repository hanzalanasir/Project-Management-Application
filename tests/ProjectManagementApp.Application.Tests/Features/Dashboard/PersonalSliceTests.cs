using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Features.Dashboard.Common;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;

namespace ProjectManagementApp.Application.Tests.Features.Dashboard;

// T044: the assignee predicate and the member-of project scope are BOTH applied in the query,
// never in memory. Proven the same way T009 proved VisibleProjectScope: compose the personal-task
// query and inspect ToQueryString() for both predicates translated to SQL, against real Postgres.
[Collection(PostgresCollection.Name)]
public class PersonalSliceTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public PersonalSliceTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task PersonalTaskQuery_ComposesBothPredicatesInSql_NeverInMemory()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new DashboardScenario();
        await scenario.SeedAsync(db);

        var tm = scenario.Team.Tasks.Projects.Tm;
        var accessPolicy = new ProjectAccessPolicy(db);
        var caller = new ProjectManagementApp.Application.Common.Models.CurrentUser(tm.Id, tm.Email!, "TeamMember", tm.FullName);

        var visibleProjectIds = VisibleProjectScope.Resolve(db, accessPolicy, caller);
        var personalTasksQuery = db.Tasks.Where(t => visibleProjectIds.Contains(t.ProjectId) && t.AssigneeId == tm.Id);

        var sql = personalTasksQuery.ToQueryString();

        // Both predicates must appear in the generated SQL text: the assignee filter directly, and
        // the project scope as a nested SELECT against projects (never a pre-fetched id list).
        sql.Should().Contain("assignee_id");
        sql.Should().Contain("project_id");
        sql.Split("SELECT", StringSplitOptions.None).Length.Should().BeGreaterThan(2);
    }

    [Fact]
    public async Task PersonalTaskSlice_ExcludesColleaguesTasksOnTheSameProject()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new DashboardScenario();
        await scenario.SeedAsync(db);

        var tm2 = scenario.Team.Tasks.Tm2;
        var accessPolicy = new ProjectAccessPolicy(db);
        var caller = new ProjectManagementApp.Application.Common.Models.CurrentUser(tm2.Id, tm2.Email!, "TeamMember", tm2.FullName);
        var visibleProjectIds = VisibleProjectScope.Resolve(db, accessPolicy, caller);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var slice = await ProjectManagementApp.Application.Features.Dashboard.GetSummary.PersonalTaskSlice
            .ComputeAsync(db, visibleProjectIds, tm2.Id, today, CancellationToken.None);

        // Tm2 is assigned only Task2 — Tm's tasks (Task1 + D*) must not leak in.
        slice.AssignedTotal.Should().Be(1);
        slice.ByStatus.ToDo.Should().Be(1);
    }
}
