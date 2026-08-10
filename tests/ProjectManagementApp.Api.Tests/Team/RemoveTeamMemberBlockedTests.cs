using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

// The real assertion here (spec B.7, research R-5) is stronger than "returns 409": a blocked
// removal must be a TOTAL no-op — the team_members row survives, no activity_logs row is written
// at all (not even a failed-attempt one), and the blocking task itself is untouched.
[Collection(ApiTestCollection.Name)]
public class RemoveTeamMemberBlockedTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public RemoveTeamMemberBlockedTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task RemoveTeamMember_WithOpenAssignedTask_Returns409_AndIsATotalNoOp()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Blocked Removal Project", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));
        var (taskId, taskEtag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Blocking Task", null, null, null, tmId));
        var statusResponse = await PutTaskStatusAsync(client, pmToken, taskId, taskEtag, new UpdateTaskStatusRequest("InProgress"));
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var taskBeforeAttempt = await ReadTaskJsonAsync(await GetTaskByIdAsync(client, pmToken, taskId));

        var response = await RemoveTeamMemberAsync(client, pmToken, projectId, tmId);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await ReadTaskJsonAsync(response);
        problem.GetProperty("detail").GetString().Should().Contain("1 open task");

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var membershipStillExists = await db.TeamMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == tmId);
        membershipStillExists.Should().BeTrue("a blocked removal must change nothing");

        var anyRemovalAudit = await db.ActivityLogs.AnyAsync(a => a.Action == "TeamMemberRemoved");
        anyRemovalAudit.Should().BeFalse("a blocked removal writes NO audit row at all, not even a failed-attempt one");

        var taskAfterAttempt = await ReadTaskJsonAsync(await GetTaskByIdAsync(client, pmToken, taskId));
        taskAfterAttempt.GetProperty("status").GetString().Should().Be(taskBeforeAttempt.GetProperty("status").GetString());
        taskAfterAttempt.GetProperty("updatedAt").GetString().Should().Be(taskBeforeAttempt.GetProperty("updatedAt").GetString(), "004 must never touch the task it merely reads");
    }

    private static async Task<JsonElement> ReadTaskJsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();
}
