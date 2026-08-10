using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

// Live before/after tests, not independent facts: the SAME removal attempt that was 409 must
// become 204 once the blocking condition is cleared — by crossing the Done boundary, or by
// reassigning the open task away from the member.
[Collection(ApiTestCollection.Name)]
public class RemoveTeamMemberDoneBoundaryTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public RemoveTeamMemberDoneBoundaryTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task RemoveTeamMember_BlockedThenTaskMarkedDone_TheSameRemovalNowSucceeds()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Done Boundary Project", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));
        var (taskId, taskEtag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Boundary Task", null, null, null, tmId));
        var inProgress = await PutTaskStatusAsync(client, pmToken, taskId, taskEtag, new UpdateTaskStatusRequest("InProgress"));
        inProgress.StatusCode.Should().Be(HttpStatusCode.OK);

        var blockedAttempt = await RemoveTeamMemberAsync(client, pmToken, projectId, tmId);
        blockedAttempt.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var freshEtagResponse = await GetTaskByIdAsync(client, pmToken, taskId);
        var etagForDoneTransition = freshEtagResponse.Headers.ETag!.Tag;

        var doneResponse = await PutTaskStatusAsync(client, pmToken, taskId, etagForDoneTransition, new UpdateTaskStatusRequest("Done"));
        doneResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var retryAttempt = await RemoveTeamMemberAsync(client, pmToken, projectId, tmId);
        retryAttempt.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveTeamMember_BlockedThenTaskReassignedAway_TheSameRemovalNowSucceeds()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Reassign Unblock Project", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));
        var tm2Id = await RegisterAndGetIdAsync(client, "Reassign Unblock TM2", "reassign-unblock-tm2@example.com", "S3cure-P@ss1!");
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tm2Id));
        var (taskId, taskEtag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Reassign Task", null, null, null, tmId));
        var inProgress = await PutTaskStatusAsync(client, pmToken, taskId, taskEtag, new UpdateTaskStatusRequest("InProgress"));
        inProgress.StatusCode.Should().Be(HttpStatusCode.OK);

        var blockedAttempt = await RemoveTeamMemberAsync(client, pmToken, projectId, tmId);
        blockedAttempt.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var freshEtagResponse = await GetTaskByIdAsync(client, pmToken, taskId);
        var etagForReassign = freshEtagResponse.Headers.ETag!.Tag;
        var reassignResponse = await PutTaskAssigneeAsync(client, pmToken, taskId, etagForReassign, new ReassignTaskRequest(tm2Id));
        reassignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var retryAttempt = await RemoveTeamMemberAsync(client, pmToken, projectId, tmId);
        retryAttempt.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
