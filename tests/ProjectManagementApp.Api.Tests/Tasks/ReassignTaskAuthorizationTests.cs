using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

[Collection(ApiTestCollection.Name)]
public class ReassignTaskAuthorizationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ReassignTaskAuthorizationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    // Reassign is not in a TeamMember's mutation set at all — unlike StatusChange, even reassigning
    // their OWN task to THEMSELVES is refused. This attribute-level 403 fires before the handler is
    // ever reached, which is fine here since no narrower-right message is required (unlike FullEdit).
    [Fact]
    public async Task ReassignTask_TeamMemberReassigningOwnTaskToSelf_Returns403()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("TM Reassign Project", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Own Task", null, null, null, tmId));

        var response = await PutTaskAssigneeAsync(client, tmToken, taskId, etag, new ReassignTaskRequest(tmId));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReassignTask_CrossOwnerPM_Returns403()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two Reassign", "pm2-reassign@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "pm2-reassign@example.com", "S3cure-P@ss1!");
        var (projectId, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("Not Yours To Reassign", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pm2Token, projectId, new CreateTaskRequest("Cross Owner Task", null, null, null, null));

        var response = await PutTaskAssigneeAsync(client, pmToken, taskId, etag, new ReassignTaskRequest(null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReassignTask_AsAdmin_Succeeds()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Admin Reassignable Project", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Admin Task", null, null, null, null));

        var response = await PutTaskAssigneeAsync(client, adminToken, taskId, etag, new ReassignTaskRequest(null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
