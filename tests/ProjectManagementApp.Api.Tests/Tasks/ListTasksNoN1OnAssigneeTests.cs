using System.Net.Http.Headers;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

// T101 (NFR-002): proves "no N+1 on project or assignee" against the real SQL executed, mirroring
// 002's ListProjectsNoN1OnOwnerTests (T080). Uses the FLAT `/api/tasks` route deliberately, not the
// nested `/api/projects/{projectId}/tasks` one — the nested route issues one extra command up front
// (GetProjectByIdQuery, the existence/visibility pre-check, T047) before ever reaching
// ListTasksQueryHandler, which is a real but SEPARATE, constant-cost query, not an N+1 concern. On
// the flat route, ListTasksQueryHandler issues exactly TWO commands by design (CountAsync, then the
// page fetch with .Include(t => t.Assignee)) — no .Include(t => t.Project) is needed because
// TaskSummaryDto.ProjectId is the scalar FK, not a navigation. An N+1 implementation would show
// 2 + (one lookup per distinct assignee); the correct one stays at exactly 2 regardless of how many
// distinct assignees exist.
[Collection(ApiTestCollection.Name)]
public class ListTasksNoN1OnAssigneeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ListTasksNoN1OnAssigneeTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ListTasks_WithThreeDistinctAssignees_ExecutesExactlyTwoSqlCommands_NotOnePerAssignee()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("N1 Tasks Project", null, "2026-08-01", null, null, null));

        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        var tm2Id = await RegisterAndGetIdAsync(client, "N1 TM2", "n1-tm2@example.com", "S3cure-P@ss1!");
        var tm3Id = await RegisterAndGetIdAsync(client, "N1 TM3", "n1-tm3@example.com", "S3cure-P@ss1!");
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        await AssignTeamMemberAsync(_fixture.Services, projectId, tm2Id);
        await AssignTeamMemberAsync(_fixture.Services, projectId, tm3Id);

        await CreateTaskAsync(client, pmToken, projectId, new CreateTaskRequest("N1 Task A", null, null, null, tmId));
        await CreateTaskAsync(client, pmToken, projectId, new CreateTaskRequest("N1 Task B", null, null, null, tm2Id));
        await CreateTaskAsync(client, pmToken, projectId, new CreateTaskRequest("N1 Task C", null, null, null, tm3Id));

        _fixture.CommandCounter.Reset();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/tasks");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        _fixture.CommandCounter.Count.Should().Be(2,
            "CountAsync + the assignee-joined page fetch is exactly two commands regardless of how many distinct assignees the three tasks have — a per-assignee lookup would push this past 2 (NFR-002)");
    }
}
