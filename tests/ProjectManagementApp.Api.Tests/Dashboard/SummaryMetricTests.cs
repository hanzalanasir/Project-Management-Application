using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// T022: overdueTaskCount boundary, completionRate's exact fraction and empty-set zero,
// blockedTaskCount derivation, and visibleTeamMemberCount's distinct-across-projects count.
[Collection(ApiTestCollection.Name)]
public class SummaryMetricTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public SummaryMetricTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Summary_OverdueAndBlockedAndCompletionRate_MatchTheScenarioExactly()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "metric-boundary");

        var summary = await GetSummaryAsync(client, scenario.PmToken);

        // 6 tasks on A: 1 Done, 2 overdue (Ta ToDo-yesterday, Tf Blocked-yesterday); Tc is Done
        // despite a past due date (excluded regardless); Te is due TODAY (boundary — not overdue).
        summary.GetProperty("overdueTaskCount").GetInt32().Should().Be(2);
        summary.GetProperty("blockedTaskCount").GetInt32().Should().Be(summary.GetProperty("tasksByStatus").GetProperty("Blocked").GetInt32());
        summary.GetProperty("blockedTaskCount").GetInt32().Should().Be(1);
        summary.GetProperty("completionRate").GetDouble().Should().BeApproximately(1.0 / 6.0, 0.0001);
    }

    [Fact]
    public async Task Summary_CompletionRate_ExactFraction_ThreeOfTwelveDone()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Completion Rate Project", null, "2026-08-01", null, null, null));

        for (var i = 0; i < 12; i++)
        {
            var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest($"CR-{i}", null, null, null, null));
            if (i < 3)
            {
                await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("Done"));
            }
        }

        var summary = await GetSummaryAsync(client, pmToken);

        summary.GetProperty("completionRate").GetDouble().Should().BeApproximately(0.25, 0.0001);
    }

    [Fact]
    public async Task Summary_CompletionRate_NoTasks_IsZero_NeverDivideByZero()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await CreateProjectAsync(client, pmToken, new CreateProjectRequest("No Tasks Project", null, "2026-08-01", null, null, null));

        var summary = await GetSummaryAsync(client, pmToken);

        summary.GetProperty("completionRate").GetDouble().Should().Be(0);
    }

    [Fact]
    public async Task Summary_VisibleTeamMemberCount_UserOnSeveralVisibleProjects_CountedOnce()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);

        var (projectAId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Multi-Project A", null, "2026-08-01", null, null, null));
        var (projectBId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Multi-Project B", null, "2026-08-01", null, null, null));

        await AssignTeamMemberAsync(_fixture.Services, projectAId, tmId);
        await AssignTeamMemberAsync(_fixture.Services, projectBId, tmId);

        var summary = await GetSummaryAsync(client, pmToken);

        summary.GetProperty("visibleProjectCount").GetInt32().Should().Be(2);
        summary.GetProperty("visibleTeamMemberCount").GetInt32().Should().Be(1);
    }
}
