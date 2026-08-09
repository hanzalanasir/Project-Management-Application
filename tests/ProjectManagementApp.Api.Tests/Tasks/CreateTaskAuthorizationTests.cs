using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

[Collection(ApiTestCollection.Name)]
public class CreateTaskAuthorizationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public CreateTaskAuthorizationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task CreateTask_AsTeamMember_Returns403_TaskMutationCreateDenied()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("TM Denied Project", null, "2026-08-01", null, "Planning", null));

        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var response = await CreateTaskAsync(client, tmToken, projectId, new CreateTaskRequest("Should Not Land", null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
