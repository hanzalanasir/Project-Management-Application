using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// T041: personalTasks counts only tasks where assignee_id == caller. A colleague's tasks on the
// SAME project are excluded, and a user with no assignments gets zeros, not 404.
[Collection(ApiTestCollection.Name)]
public class PersonalSliceTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public PersonalSliceTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task PersonalTasks_ForTm_CountsOnlyTasksAssignedToTm_NotTm2sTasksOnSameProject()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "personal-slice");

        var tmSummary = await GetSummaryAsync(client, scenario.TmToken);
        var tmPersonal = tmSummary.GetProperty("personalTasks");
        // Ta(ToDo)/Tb(InProgress)/Tc(Done) assigned to TM — Td (TM2's) must not appear.
        tmPersonal.GetProperty("assignedTotal").GetInt32().Should().Be(3);
        tmPersonal.GetProperty("byStatus").GetProperty("ToDo").GetInt32().Should().Be(1);

        var tm2Summary = await GetSummaryAsync(client, scenario.Tm2Token);
        var tm2Personal = tm2Summary.GetProperty("personalTasks");
        // Only Td is TM2's.
        tm2Personal.GetProperty("assignedTotal").GetInt32().Should().Be(1);
        tm2Personal.GetProperty("byStatus").GetProperty("ToDo").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task PersonalTasks_ForPmWithNoAssignments_ReturnsZeros_Not404()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "personal-slice-pm");

        // PM owns project A but is never an assignee of any task on it in this scenario.
        var summary = await GetSummaryAsync(client, scenario.PmToken);

        var personal = summary.GetProperty("personalTasks");
        personal.GetProperty("assignedTotal").GetInt32().Should().Be(0);
        personal.GetProperty("overdueCount").GetInt32().Should().Be(0);
    }
}
