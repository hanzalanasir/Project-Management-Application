using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// The 🎯 stable-contract test (T020, DoD 4): all five ProjectStatus and all five TaskStatus keys
// are present — zeros included, never omitted — regardless of whether any rows exist for them.
// This is what makes the payload a typed contract rather than a variable dictionary.
[Collection(ApiTestCollection.Name)]
public class SummaryContractShapeTests : IAsyncLifetime
{
    private static readonly string[] AllProjectStatuses = ["Planning", "Active", "OnHold", "Completed", "Cancelled"];
    private static readonly string[] AllTaskStatuses = ["ToDo", "InProgress", "InReview", "Done", "Blocked"];

    private readonly ApiTestFixture _fixture;

    public SummaryContractShapeTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Summary_AllEnumKeysPresent_ForACallerWithRealData()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "shape-admin");

        var summary = await GetSummaryAsync(client, scenario.AdminToken);

        var projectsByStatus = summary.GetProperty("projectsByStatus");
        foreach (var key in AllProjectStatuses)
        {
            projectsByStatus.TryGetProperty(key, out _).Should().BeTrue($"'{key}' must always be present, even at zero");
        }

        var tasksByStatus = summary.GetProperty("tasksByStatus");
        foreach (var key in AllTaskStatuses)
        {
            tasksByStatus.TryGetProperty(key, out _).Should().BeTrue($"'{key}' must always be present, even at zero");
        }

        var personalByStatus = summary.GetProperty("personalTasks").GetProperty("byStatus");
        foreach (var key in AllTaskStatuses)
        {
            personalByStatus.TryGetProperty(key, out _).Should().BeTrue($"'{key}' must always be present, even at zero");
        }
    }

    [Fact]
    public async Task Summary_AllEnumKeysPresent_ForACallerWithNoMatchingRows()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "shape-empty");

        // PM2 owns project B, which has no tasks at all — every TaskStatus key must still appear.
        var summary = await GetSummaryAsync(client, scenario.Pm2Token);

        var tasksByStatus = summary.GetProperty("tasksByStatus");
        foreach (var key in AllTaskStatuses)
        {
            tasksByStatus.GetProperty(key).GetInt32().Should().Be(0);
        }

        var projectsByStatus = summary.GetProperty("projectsByStatus");
        projectsByStatus.GetProperty("Planning").GetInt32().Should().Be(1);
        foreach (var key in AllProjectStatuses.Where(k => k != "Planning"))
        {
            projectsByStatus.GetProperty(key).GetInt32().Should().Be(0);
        }
    }
}
