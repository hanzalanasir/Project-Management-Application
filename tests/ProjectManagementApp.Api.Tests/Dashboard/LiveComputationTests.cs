using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// T051 — proves "live per request, no caching" (Clarifications 2026-07-22, quickstart V14) is
// actually true, not just documented intent. Fetches the summary, mutates a task's status through
// 003's own endpoint (never touching Dashboard code), re-fetches, and asserts the changed value
// appears immediately — no staleness window, no cache to invalidate.
[Collection(ApiTestCollection.Name)]
public class LiveComputationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public LiveComputationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Summary_ReflectsATaskStatusChange_Immediately_WithGeneratedAtAdvancing()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Live Computation Project", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Live Task", null, null, null, null));

        var before = await GetSummaryAsync(client, pmToken);
        before.GetProperty("tasksByStatus").GetProperty("Done").GetInt32().Should().Be(0);
        before.GetProperty("completionRate").GetDouble().Should().Be(0);
        var generatedAtBefore = before.GetProperty("generatedAt").GetDateTimeOffset();

        // Force real wall-clock separation so generatedAt has something to advance past.
        await Task.Delay(20);

        var statusResponse = await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("Done"));
        statusResponse.EnsureSuccessStatusCode();

        var after = await GetSummaryAsync(client, pmToken);

        after.GetProperty("tasksByStatus").GetProperty("Done").GetInt32().Should().Be(1);
        after.GetProperty("completionRate").GetDouble().Should().Be(1);
        after.GetProperty("generatedAt").GetDateTimeOffset().Should().BeAfter(generatedAtBefore,
            "the summary must be recomputed on every request, never served from a cache");
    }
}
