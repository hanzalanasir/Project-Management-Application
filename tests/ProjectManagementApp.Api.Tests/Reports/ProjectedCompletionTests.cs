using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T035 — the 🎯 projected-completion test (FR-017): `today(UTC) + ceil(openTasks / avgClosedPerDay)`
// where avgClosedPerDay = closedTasksInWindow / windowDays. Two null cases exist by design — no
// recent throughput, or no open tasks left — and neither may ever divide by zero or surface a past
// date as a "projection".
[Collection(ApiTestCollection.Name)]
public class ProjectedCompletionTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ProjectedCompletionTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ProjectedCompletion_SteadyThroughputWithOpenTasksRemaining_IsAPlausibleFutureDate()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Projected Steady", null, "2026-08-01", null, null, null));

        // 2 closed "now" (within window), 3 still open.
        for (var i = 0; i < 2; i++)
        {
            var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest($"Closed{i}", null, null, null, null));
            await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("Done"));
        }
        for (var i = 0; i < 3; i++)
        {
            await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest($"Open{i}", null, null, null, null));
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var body = await GetProjectProgressAsync(client, pmToken, DefaultWindowQuery());
        var row = body.GetProperty("rows").EnumerateArray().Single(r => r.GetProperty("projectId").GetString() == projectId.ToString());

        var projected = row.GetProperty("projectedCompletion");
        projected.ValueKind.Should().NotBe(System.Text.Json.JsonValueKind.Null);
        DateOnly.Parse(projected.GetString()!).Should().BeAfter(today.AddDays(-1), "a projection must never present a past date");
    }

    [Fact]
    public async Task ProjectedCompletion_NoClosuresInWindow_AvgClosedPerDayIsZero_IsNull()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Projected NoThroughput", null, "2026-08-01", null, null, null));

        for (var i = 0; i < 3; i++)
        {
            await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest($"Open{i}", null, null, null, null));
        }

        var body = await GetProjectProgressAsync(client, pmToken, DefaultWindowQuery());
        var row = body.GetProperty("rows").EnumerateArray().Single(r => r.GetProperty("projectId").GetString() == projectId.ToString());

        row.GetProperty("openTasks").GetInt32().Should().Be(3);
        row.GetProperty("projectedCompletion").ValueKind.Should().Be(System.Text.Json.JsonValueKind.Null);
    }

    [Fact]
    public async Task ProjectedCompletion_NoOpenTasksLeft_IsNull()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Projected AllDone", null, "2026-08-01", null, null, null));

        for (var i = 0; i < 3; i++)
        {
            var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest($"Done{i}", null, null, null, null));
            await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("Done"));
        }

        var body = await GetProjectProgressAsync(client, pmToken, DefaultWindowQuery());
        var row = body.GetProperty("rows").EnumerateArray().Single(r => r.GetProperty("projectId").GetString() == projectId.ToString());

        row.GetProperty("openTasks").GetInt32().Should().Be(0);
        row.GetProperty("projectedCompletion").ValueKind.Should().Be(System.Text.Json.JsonValueKind.Null);
    }
}
