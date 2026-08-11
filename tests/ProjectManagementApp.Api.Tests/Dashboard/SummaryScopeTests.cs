using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// The 🎯 three-role scope matrix (T019, quickstart V1). Admin sees everything; PM sees only
// owned projects/tasks; TM sees only member-of projects, with task/overdue tiles counting ONLY
// tasks assigned to them (personal-view, Clarifications 2026-07-22) — not every task on the
// projects they belong to.
[Collection(ApiTestCollection.Name)]
public class SummaryScopeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public SummaryScopeTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Summary_Admin_SeesAllProjectsAndAllTasksAcrossScope()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "scope-admin");

        var summary = await GetSummaryAsync(client, scenario.AdminToken);

        summary.GetProperty("scope").GetString().Should().Be("Admin");
        summary.GetProperty("visibleProjectCount").GetInt32().Should().Be(2);
        var tasksByStatus = summary.GetProperty("tasksByStatus");
        tasksByStatus.GetProperty("ToDo").GetInt32().Should().Be(2);
        tasksByStatus.GetProperty("InProgress").GetInt32().Should().Be(1);
        tasksByStatus.GetProperty("Done").GetInt32().Should().Be(1);
        tasksByStatus.GetProperty("InReview").GetInt32().Should().Be(1);
        tasksByStatus.GetProperty("Blocked").GetInt32().Should().Be(1);
        summary.GetProperty("overdueTaskCount").GetInt32().Should().Be(2);
        summary.GetProperty("visibleTeamMemberCount").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task Summary_ProjectManager_SeesOnlyOwnedProjectAndItsTasks_ProjectWide()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "scope-pm");

        var summary = await GetSummaryAsync(client, scenario.PmToken);

        summary.GetProperty("scope").GetString().Should().Be("ProjectManager");
        summary.GetProperty("visibleProjectCount").GetInt32().Should().Be(1);
        var tasksByStatus = summary.GetProperty("tasksByStatus");
        tasksByStatus.GetProperty("ToDo").GetInt32().Should().Be(2);
        tasksByStatus.GetProperty("Done").GetInt32().Should().Be(1);
        summary.GetProperty("overdueTaskCount").GetInt32().Should().Be(2);
        summary.GetProperty("visibleTeamMemberCount").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task Summary_ProjectManagerWithNoTasksOnOwnedProject_Returns200_AllZeros()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "scope-pm2");

        var summary = await GetSummaryAsync(client, scenario.Pm2Token);

        summary.GetProperty("scope").GetString().Should().Be("ProjectManager");
        summary.GetProperty("visibleProjectCount").GetInt32().Should().Be(1);
        var tasksByStatus = summary.GetProperty("tasksByStatus");
        foreach (var key in new[] { "ToDo", "InProgress", "InReview", "Done", "Blocked" })
        {
            tasksByStatus.GetProperty(key).GetInt32().Should().Be(0);
        }
        summary.GetProperty("overdueTaskCount").GetInt32().Should().Be(0);
        summary.GetProperty("visibleTeamMemberCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task Summary_TeamMember_TileCountsAreAssignedToThemOnly_NotProjectWide()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "scope-tm");

        var summary = await GetSummaryAsync(client, scenario.TmToken);

        summary.GetProperty("scope").GetString().Should().Be("TeamMember");
        // TM is a member of A only — project scope is membership-based, still 1.
        summary.GetProperty("visibleProjectCount").GetInt32().Should().Be(1);

        // But the task tiles are PERSONAL: Ta(ToDo)/Tb(InProgress)/Tc(Done) assigned to TM = 3
        // total, NOT the 6 tasks that exist on project A (Td is TM2's, Te/Tf are unassigned).
        var tasksByStatus = summary.GetProperty("tasksByStatus");
        tasksByStatus.GetProperty("ToDo").GetInt32().Should().Be(1);
        tasksByStatus.GetProperty("InProgress").GetInt32().Should().Be(1);
        tasksByStatus.GetProperty("Done").GetInt32().Should().Be(1);
        tasksByStatus.GetProperty("InReview").GetInt32().Should().Be(0);
        tasksByStatus.GetProperty("Blocked").GetInt32().Should().Be(0);
        summary.GetProperty("overdueTaskCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task Summary_RequiresAuthentication_401()
    {
        var client = _fixture.CreateClient();

        var response = await GetSummaryResponseAsync(client, "not-a-real-token");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
