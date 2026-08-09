using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

// Half of the graduated model's acceptance test (the other half is
// UpdateTaskStatusGraduatedTests, T068) — the assignee is refused FullEdit, and the refusal names
// the narrower right they DO have.
[Collection(ApiTestCollection.Name)]
public class UpdateTaskGraduatedTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public UpdateTaskGraduatedTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task UpdateTask_AsTheAssignee_Returns403_WithADetailNamingTheNarrowerRight()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Graduated Edit Project", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Assignee Cannot Edit", null, null, null, tmId));

        var response = await PutTaskAsync(client, tmToken, taskId, etag, new UpdateTaskRequest("Renamed by assignee", null, "Low", null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var body = await ReadJsonAsync(response);
        body.GetProperty("detail").GetString().Should().Be("You may update the status of this task, but not its details.");
    }
}
