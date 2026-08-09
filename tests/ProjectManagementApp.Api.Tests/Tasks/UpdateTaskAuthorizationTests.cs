using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

[Collection(ApiTestCollection.Name)]
public class UpdateTaskAuthorizationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public UpdateTaskAuthorizationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task UpdateTask_CrossOwnerPM_Returns403_AndTaskIsUnchanged()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two Edit", "pm2-edit-task@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "pm2-edit-task@example.com", "S3cure-P@ss1!");
        var (projectId, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("Owned By PM2", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pm2Token, projectId, new CreateTaskRequest("Original Title", null, null, null, null));

        var response = await PutTaskAsync(client, pmToken, taskId, etag, new UpdateTaskRequest("Hijacked", null, "Low", null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var check = await ReadJsonAsync(await GetTaskByIdAsync(client, pm2Token, taskId));
        check.GetProperty("title").GetString().Should().Be("Original Title");
    }

    [Fact]
    public async Task UpdateTask_AsAdmin_Succeeds_ForAnyTask()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Admin Editable Project", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Admin Task", null, null, null, null));

        var response = await PutTaskAsync(client, adminToken, taskId, etag, new UpdateTaskRequest("Admin Edited", null, "High", null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
