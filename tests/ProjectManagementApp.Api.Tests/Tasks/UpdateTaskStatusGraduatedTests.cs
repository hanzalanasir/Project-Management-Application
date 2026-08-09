using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

// The feature's headline acceptance test (DoD 3, quickstart V2): the SAME assignee, on the SAME
// row — 403 on PUT /tasks/{id}, 200 on PUT /tasks/{id}/status. This is what "graduated
// authorization" means end to end.
[Collection(ApiTestCollection.Name)]
public class UpdateTaskStatusGraduatedTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public UpdateTaskStatusGraduatedTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task SameAssignee_SameRow_403OnFullEdit_Then200OnStatus()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Graduated Cell Project", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Graduated Cell Task", null, null, null, tmId));

        var fullEditResponse = await PutTaskAsync(client, tmToken, taskId, etag, new UpdateTaskRequest("Renamed by assignee", null, "Low", null));
        fullEditResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var statusResponse = await PutTaskStatusAsync(client, tmToken, taskId, etag, new UpdateTaskStatusRequest("InProgress"));
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await ReadJsonAsync(statusResponse);
        body.GetProperty("status").GetString().Should().Be("InProgress");
    }
}
