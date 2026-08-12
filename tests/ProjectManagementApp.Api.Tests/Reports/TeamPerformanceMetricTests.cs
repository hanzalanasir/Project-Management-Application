using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T056: throughput counts tasks whose closed_at falls in the window; workload counts currently
// assigned, not-Done tasks; overdueCount uses the shared overdue rule (due < today, not Done).
[Collection(ApiTestCollection.Name)]
public class TeamPerformanceMetricTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public TeamPerformanceMetricTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task TeamPerformance_ThroughputWorkloadAndOverdue_AreComputedCorrectly()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);

        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Metric TeamPerf Project", null, "2026-08-01", null, null, null));
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1).ToString("yyyy-MM-dd");

        // Throughput: 2 tasks closed "now" (within the default window).
        for (var i = 0; i < 2; i++)
        {
            var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest($"Closed{i}", null, null, null, tmId));
            await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("Done"));
        }

        // Workload: 3 open (not-Done) tasks currently assigned.
        for (var i = 0; i < 3; i++)
        {
            await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest($"Open{i}", null, null, null, tmId));
        }

        // Overdue: 1 open task, due yesterday, still assigned.
        await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Overdue", null, null, yesterday, tmId));

        var body = await GetTeamPerformanceAsync(client, pmToken, DefaultWindowQuery());
        var row = body.GetProperty("rows").EnumerateArray().Single(r => r.GetProperty("userId").GetString() == tmId.ToString());

        row.GetProperty("throughput").GetInt32().Should().Be(2);
        row.GetProperty("workload").GetInt32().Should().Be(4, "3 open tasks plus the 1 overdue task are all not-Done");
        row.GetProperty("overdueCount").GetInt32().Should().Be(1);
    }
}
