using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

// The second assertion (the stale write did NOT land) is the real one — a test that only checks
// the status code would pass against a silent last-write-wins (ADR-0004, mirrors 002's equivalent).
[Collection(ApiTestCollection.Name)]
public class UpdateTaskConcurrencyTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public UpdateTaskConcurrencyTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task UpdateTask_FirstUpdateWithCurrentETag_Succeeds_ThenReplayingTheStaleETag_Returns409_AndTheStaleWriteDoesNotLand()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Concurrency Project", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Original", null, null, null, null));

        var firstResponse = await PutTaskAsync(client, pmToken, taskId, etag, new UpdateTaskRequest("Updated Once", null, "High", null));
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newETag = firstResponse.Headers.ETag!.Tag;
        newETag.Should().NotBe(etag);

        var replayResponse = await PutTaskAsync(client, pmToken, taskId, etag, new UpdateTaskRequest("Clobbered", null, "Low", null));
        replayResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var check = await ReadJsonAsync(await GetTaskByIdAsync(client, pmToken, taskId));
        check.GetProperty("title").GetString().Should().Be("Updated Once");
    }

    [Fact]
    public async Task UpdateTask_MissingIfMatch_Returns400_NotALastWriteWinsSuccess()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("No IfMatch Project", null, "2026-08-01", null, null, null));
        var (taskId, _) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("No IfMatch Task", null, null, null, null));

        var response = await PutTaskAsync(client, pmToken, taskId, null, new UpdateTaskRequest("Should Not Apply", null, "High", null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
