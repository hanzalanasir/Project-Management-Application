using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

[Collection(ApiTestCollection.Name)]
public class DeleteTaskAuthorizationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public DeleteTaskAuthorizationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    // Delete is not in a TeamMember's mutation set at all — unlike StatusChange, even the assignee
    // is refused. This reconfirms the graduated boundary from stage 3 in a new context.
    [Fact]
    public async Task DeleteTask_AsTheAssignee_Returns403()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Delete Auth Project", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        var (taskId, _) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Assignee Cannot Delete", null, null, null, tmId));

        var response = await DeleteTaskAsync(client, tmToken, taskId);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var check = await GetTaskByIdAsync(client, pmToken, taskId);
        check.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteTask_CrossOwnerPM_Returns403()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two Delete Task", "pm2-delete-task@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "pm2-delete-task@example.com", "S3cure-P@ss1!");
        var (projectId, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("Not Yours To Delete", null, "2026-08-01", null, null, null));
        var (taskId, _) = await CreateTaskAndGetIdEtagAsync(client, pm2Token, projectId, new CreateTaskRequest("Cross Owner", null, null, null, null));

        var response = await DeleteTaskAsync(client, pmToken, taskId);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteTask_AsAdmin_Succeeds_ForAnyTask()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Admin Deletable Task Project", null, "2026-08-01", null, null, null));
        var (taskId, _) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Admin Task", null, null, null, null));

        var response = await DeleteTaskAsync(client, adminToken, taskId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // No workflow enforcement in v1 (OQ-003-03) — Done is not a lock against deletion either.
    [Fact]
    public async Task DeleteTask_ADoneTask_IsPermitted()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Delete Done Task Project", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Finished Task", null, null, null, null));
        var statusResponse = await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("Done"));
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await DeleteTaskAsync(client, pmToken, taskId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
