using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Api.Tests.Team;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.CrossFeature;

// The proof this whole feature exists for (DoD 8, closing 004 plan Follow-up 2): 003's
// AssigneeValidator accepts a candidate IFF a matching team_members row exists — the pool 004
// maintains is exactly the pool 003 validates against. Both sides read the shared team_members
// table via IApplicationDbContext only; neither feature calls into the other's MediatR handlers
// (confirmed by reading AssigneeValidator.cs — it queries db.TeamMembers directly, and 004's
// handlers never reference anything under Features/Tasks).
[Collection(ApiTestCollection.Name)]
public class TeamBacksTaskAssignmentTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public TeamBacksTaskAssignmentTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ReassigningATask_BeforeAdding_Is400_AfterAdding_Is200_TheSameAssignmentNowSucceeds()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Cross Feature Project", null, "2026-08-01", null, null, null));
        var candidateId = await RegisterAndGetIdAsync(client, "Cross Feature Candidate", "cross-feature-candidate@example.com", "S3cure-P@ss1!");
        var (taskId, taskEtag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Cross Feature Task", null, null, null, null));

        // 1. Try assigning to a candidate who is not yet on the team — 400 (not in the pool).
        var reassignBeforeAdd = await PutTaskAssigneeAsync(client, pmToken, taskId, taskEtag, new ReassignTaskRequest(candidateId));
        reassignBeforeAdd.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemBeforeAdd = await ReadJsonAsync(reassignBeforeAdd);
        problemBeforeAdd.GetProperty("errors").GetProperty("assigneeId")[0].GetString().Should().Contain("not a team member");

        // 2. Add the candidate to the team — 201.
        var addResponse = await TeamTestHelper.AddTeamMemberAsync(client, pmToken, projectId, new TeamTestHelper.AddTeamMemberRequest(candidateId));
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 3. Retry the exact same assignment — now accepted (200), proving the pool 004 maintains
        // is exactly the pool 003 validates against.
        var reassignAfterAdd = await PutTaskAssigneeAsync(client, pmToken, taskId, taskEtag, new ReassignTaskRequest(candidateId));
        reassignAfterAdd.StatusCode.Should().Be(HttpStatusCode.OK);
        var reassignedTask = await ReadJsonAsync(reassignAfterAdd);
        reassignedTask.GetProperty("assignee").GetProperty("id").GetString().Should().Be(candidateId.ToString());
    }
}
