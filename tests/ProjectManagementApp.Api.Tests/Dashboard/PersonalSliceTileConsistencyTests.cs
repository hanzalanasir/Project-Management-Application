using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// The 🎯 tile-consistency test (T042). For a TeamMember, personalTasks.byStatus and the summary's
// own tasksByStatus must be the IDENTICAL numbers — there is no second, divergent project-wide
// count for a TeamMember in v1 (personal-view, Clarifications 2026-07-22).
[Collection(ApiTestCollection.Name)]
public class PersonalSliceTileConsistencyTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public PersonalSliceTileConsistencyTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Summary_ForTeamMember_TasksByStatusAndPersonalTasksByStatus_AreIdentical()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "tile-consistency");

        var summary = await GetSummaryAsync(client, scenario.TmToken);

        var tasksByStatus = summary.GetProperty("tasksByStatus");
        var personalByStatus = summary.GetProperty("personalTasks").GetProperty("byStatus");

        foreach (var key in new[] { "ToDo", "InProgress", "InReview", "Done", "Blocked" })
        {
            tasksByStatus.GetProperty(key).GetInt32().Should().Be(personalByStatus.GetProperty(key).GetInt32(),
                $"tasksByStatus.{key} must equal personalTasks.byStatus.{key} for a TeamMember");
        }

        summary.GetProperty("overdueTaskCount").GetInt32().Should().Be(summary.GetProperty("personalTasks").GetProperty("overdueCount").GetInt32());
    }

    [Fact]
    public async Task Summary_ForAdminAndPm_TasksByStatusIsProjectWide_PersonalTasksIsSeparate()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "tile-consistency-pm");

        var summary = await GetSummaryAsync(client, scenario.PmToken);

        // The PM is unassigned in this scenario — tasksByStatus (project-wide, 6 tasks) must NOT
        // equal personalTasks (0), proving these are two distinct slices for a manager.
        var totalByStatus = summary.GetProperty("tasksByStatus").EnumerateObject().Sum(p => p.Value.GetInt32());
        var personalTotal = summary.GetProperty("personalTasks").GetProperty("assignedTotal").GetInt32();

        totalByStatus.Should().Be(6);
        personalTotal.Should().Be(0);
        totalByStatus.Should().NotBe(personalTotal);
    }
}
