using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

[Collection(ApiTestCollection.Name)]
public class UpdateTaskStatusScopeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public UpdateTaskStatusScopeTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task TeamMember_WhoIsNotTheAssignee_Returns403_ScopeGateFiresBeforeMutationGate()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Status Scope Project", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        var otherTmId = await RegisterAndGetIdAsync(client, "Other TM", "other-tm-status-scope@example.com", "Other#Passw0rd1!");
        var otherTmToken = await LoginAsync(client, "other-tm-status-scope@example.com", "Other#Passw0rd1!");
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        await AssignTeamMemberAsync(_fixture.Services, projectId, otherTmId);
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Assigned To TM", null, null, null, tmId));

        var response = await PutTaskStatusAsync(client, otherTmToken, taskId, etag, new UpdateTaskStatusRequest("InProgress"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ProjectManager_OnTheirOwnProject_Returns200()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("PM Status Project", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("PM Status Task", null, null, null, null));

        var response = await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("InProgress"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
