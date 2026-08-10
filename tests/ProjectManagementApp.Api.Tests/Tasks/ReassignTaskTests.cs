using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

[Collection(ApiTestCollection.Name)]
public class ReassignTaskTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ReassignTaskTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ReassignTask_ToAValidPoolMember_Returns200_AndAuditsFromTo()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Reassign Project", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        var tm2Id = await RegisterAndGetIdAsync(client, "TM2 Reassign", "tm2-reassign@example.com", "S3cure-P@ss1!");
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        await AssignTeamMemberAsync(_fixture.Services, projectId, tm2Id);
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Reassign Me", null, null, null, tmId));

        var response = await PutTaskAssigneeAsync(client, pmToken, taskId, etag, new ReassignTaskRequest(tm2Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.GetProperty("assignee").GetProperty("id").GetString().Should().Be(tm2Id.ToString());

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var auditRow = await db.ActivityLogs.SingleOrDefaultAsync(a => a.EntityId == taskId && a.Action == "TaskReassigned");
        auditRow.Should().NotBeNull();
    }

    [Fact]
    public async Task ReassignTask_ToNull_Returns200_Unassigned()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Unassign Project", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Unassign Me", null, null, null, tmId));

        var response = await PutTaskAssigneeAsync(client, pmToken, taskId, etag, new ReassignTaskRequest(null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.GetProperty("assignee").ValueKind.Should().Be(System.Text.Json.JsonValueKind.Null);
    }
}
