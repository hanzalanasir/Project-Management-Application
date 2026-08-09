using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

[Collection(ApiTestCollection.Name)]
public class GetTaskByIdTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public GetTaskByIdTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task GetTaskById_InScope_Returns200_WithETag()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Detail Project", null, "2026-08-01", null, null, null));
        var created = await ReadJsonAsync(await CreateTaskAsync(client, pmToken, projectId, new CreateTaskRequest("Detail Task", null, null, null, null)));
        var taskId = created.GetProperty("id").GetString();

        var response = await GetTaskByIdAsync(client, pmToken, taskId!);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.ETag.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTaskById_OutOfScope_Returns403()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two Detail Get", "pm2-get-detail@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "pm2-get-detail@example.com", "S3cure-P@ss1!");
        var (projectId, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("Not Yours Detail", null, "2026-08-01", null, null, null));
        var created = await ReadJsonAsync(await CreateTaskAsync(client, pm2Token, projectId, new CreateTaskRequest("Not Yours Task", null, null, null, null)));
        var taskId = created.GetProperty("id").GetString();

        var response = await GetTaskByIdAsync(client, pmToken, taskId!);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTaskById_UnknownId_Returns404()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var response = await GetTaskByIdAsync(client, pmToken, Guid.NewGuid().ToString());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTaskById_MalformedId_Returns400()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var response = await GetTaskByIdAsync(client, pmToken, "not-a-guid");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTaskById_AsAssignee_Returns200()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Assignee Detail Project", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        var created = await ReadJsonAsync(await CreateTaskAsync(client, pmToken, projectId, new CreateTaskRequest("Assigned Task", null, null, null, tmId)));
        var taskId = created.GetProperty("id").GetString();

        var response = await GetTaskByIdAsync(client, tmToken, taskId!);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
