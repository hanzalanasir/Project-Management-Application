using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// The 🎯 security test of this whole feature (T027, DoD 3, NFR-002) — meaningless on EF InMemory
// (ADR-0007 §2), which evaluates LINQ in memory and would let a fetch-then-filter leak pass. Two
// halves: (1) a PM's numbers are unchanged after ten tasks land on a project they cannot see, and
// (2) the generated SQL is inspected to confirm scope is `WHERE project_id IN (<subquery>)`.
[Collection(ApiTestCollection.Name)]
public class SummaryFilterAtSourceTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public SummaryFilterAtSourceTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Summary_TenTasksAddedToAnInvisibleProject_NumbersAreUnchanged()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two Filter", "pm2-filter-at-source@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "pm2-filter-at-source@example.com", "S3cure-P@ss1!");

        var (ownProjectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("PM's Own Project", null, "2026-08-01", null, null, null));
        await CreateTaskAsync(client, pmToken, ownProjectId, new CreateTaskRequest("Own Task", null, null, null, null));

        var before = await GetSummaryAsync(client, pmToken);

        // Ten tasks land on a DIFFERENT project the PM cannot see.
        var (invisibleProjectId, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("PM Cannot See This", null, "2026-08-01", null, null, null));
        for (var i = 0; i < 10; i++)
        {
            await CreateTaskAsync(client, pm2Token, invisibleProjectId, new CreateTaskRequest($"Invisible-{i}", null, null, null, null));
        }

        var after = await GetSummaryAsync(client, pmToken);

        after.GetProperty("visibleProjectCount").GetInt32().Should().Be(before.GetProperty("visibleProjectCount").GetInt32());
        after.GetProperty("tasksByStatus").GetProperty("ToDo").GetInt32().Should().Be(before.GetProperty("tasksByStatus").GetProperty("ToDo").GetInt32());
        after.GetProperty("overdueTaskCount").GetInt32().Should().Be(before.GetProperty("overdueTaskCount").GetInt32());
        after.GetProperty("completionRate").GetDouble().Should().Be(before.GetProperty("completionRate").GetDouble());
    }

    [Fact]
    public async Task Summary_GeneratedSql_ScopesTasksViaSubquery_NotPostQueryFilter()
    {
        var client = _fixture.CreateClient();
        await SeedScopeScenarioAsync(client, _fixture, "filter-sql");
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        _fixture.SqlCapture.Reset();
        await GetSummaryAsync(client, pmToken);

        var commandTexts = _fixture.SqlCapture.CommandTexts;
        commandTexts.Should().NotBeEmpty();

        // VisibleProjectScope (T008) is used directly for projectsByStatus and the personal-task
        // slice — both compose as `... WHERE project_id IN (SELECT id FROM projects WHERE ...)`.
        // At least one captured command must show this exact nested-subquery shape, proving the
        // resolver was never materialized into a parameter list before composition.
        var subqueryCommands = commandTexts.Where(sql =>
            sql.Contains("project_id", StringComparison.OrdinalIgnoreCase) &&
            sql.Contains("projects", StringComparison.OrdinalIgnoreCase) &&
            sql.Split("SELECT", StringSplitOptions.None).Length > 2).ToList();

        subqueryCommands.Should().NotBeEmpty(
            "at least one query (projectsByStatus or the personal-task slice) must scope via a nested SELECT against projects, not a post-fetch filter");
    }
}
