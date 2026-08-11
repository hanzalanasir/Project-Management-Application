using System.Net.Http.Headers;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// T043: a task assigned to the caller in a project they are NOT (or no longer) a member of is
// excluded from personalTasks. 004's removal-block invariant means this can only occur for a
// CLOSED task (an open assignment blocks removal — OpenAssignedTaskCheck), so this test closes
// the task first, removes the membership, and confirms the now-invisible-project task vanishes
// from the personal slice even though task.AssigneeId still points at the caller.
[Collection(ApiTestCollection.Name)]
public class PersonalSliceScopeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public PersonalSliceScopeTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task PersonalTasks_ExcludesAssignedTaskOnceCallerIsNoLongerAProjectMember()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);

        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Boundary Project", null, "2026-08-01", null, null, null));
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);

        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Boundary Task", null, null, null, tmId));

        // While the assignment is open (not Done) and the caller is a member, it counts.
        var beforeRemoval = await GetSummaryAsync(client, tmToken);
        beforeRemoval.GetProperty("personalTasks").GetProperty("assignedTotal").GetInt32().Should().Be(1);

        // Close the task (Done) — required before removal is even allowed (OpenAssignedTaskCheck).
        await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("Done"));

        // Remove TM from the project's team — the task keeps AssigneeId == TM, but TM is no
        // longer a member, so the project drops out of TM's visible-project set.
        using var removeRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/projects/{projectId}/team/{tmId}");
        removeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var removeResponse = await client.SendAsync(removeRequest);
        removeResponse.EnsureSuccessStatusCode();

        var afterRemoval = await GetSummaryAsync(client, tmToken);
        afterRemoval.GetProperty("visibleProjectCount").GetInt32().Should().Be(0);
        afterRemoval.GetProperty("personalTasks").GetProperty("assignedTotal").GetInt32().Should().Be(0);
    }
}
