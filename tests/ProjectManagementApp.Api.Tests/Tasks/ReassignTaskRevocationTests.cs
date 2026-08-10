using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

// The access-revocation test (T090, quickstart V8): after reassigning away from TM, their very
// next GET must return 403. No grace period, no cached access — scope is re-evaluated fresh on
// every read, verified live against real Postgres, never a mocked scope check.
[Collection(ApiTestCollection.Name)]
public class ReassignTaskRevocationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ReassignTaskRevocationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ReassignTask_AwayFromTM_TMsNextGet_Returns403_Immediately()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Revocation Project", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        var tm2Id = await RegisterAndGetIdAsync(client, "TM2 Revocation", "tm2-revocation@example.com", "S3cure-P@ss1!");
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        await AssignTeamMemberAsync(_fixture.Services, projectId, tm2Id);
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Revoke Me", null, null, null, tmId));

        // Sanity: TM can read it before reassignment.
        var preCheck = await GetTaskByIdAsync(client, tmToken, taskId);
        preCheck.StatusCode.Should().Be(HttpStatusCode.OK);

        var reassignResponse = await PutTaskAssigneeAsync(client, pmToken, taskId, etag, new ReassignTaskRequest(tm2Id));
        reassignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var postCheck = await GetTaskByIdAsync(client, tmToken, taskId);
        postCheck.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
