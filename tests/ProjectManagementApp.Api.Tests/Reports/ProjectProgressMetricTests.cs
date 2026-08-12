using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T034 — completion math (spec DoD 6): completionPercent = closed / total * 100, exactly 25 with
// 3-of-12 closed; 0 (never a divide-by-zero or NaN) on a project with zero tasks; openTasks +
// closedTasks always equals totalTasks.
[Collection(ApiTestCollection.Name)]
public class ProjectProgressMetricTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ProjectProgressMetricTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ProjectProgress_ThreeOfTwelveClosed_CompletionPercentIsExactlyTwentyFive_AndCountsSumToTotal()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Metric Project", null, "2026-08-01", null, null, null));

        for (var i = 0; i < 12; i++)
        {
            var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest($"M{i}", null, null, null, null));
            if (i < 3)
            {
                await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("Done"));
            }
        }

        var body = await GetProjectProgressAsync(client, pmToken, DefaultWindowQuery());
        var row = body.GetProperty("rows").EnumerateArray().Single(r => r.GetProperty("projectId").GetString() == projectId.ToString());

        row.GetProperty("totalTasks").GetInt32().Should().Be(12);
        row.GetProperty("closedTasks").GetInt32().Should().Be(3);
        row.GetProperty("openTasks").GetInt32().Should().Be(9);
        row.GetProperty("completionPercent").GetDouble().Should().Be(25.0);
        (row.GetProperty("openTasks").GetInt32() + row.GetProperty("closedTasks").GetInt32())
            .Should().Be(row.GetProperty("totalTasks").GetInt32());
    }

    [Fact]
    public async Task ProjectProgress_ZeroTaskProject_CompletionPercentIsZero_NeverDivideByZero()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Empty Metric Project", null, "2026-08-01", null, null, null));

        var body = await GetProjectProgressAsync(client, pmToken, DefaultWindowQuery());
        var row = body.GetProperty("rows").EnumerateArray().Single(r => r.GetProperty("projectId").GetString() == projectId.ToString());

        row.GetProperty("totalTasks").GetInt32().Should().Be(0);
        row.GetProperty("completionPercent").GetDouble().Should().Be(0);
    }
}
