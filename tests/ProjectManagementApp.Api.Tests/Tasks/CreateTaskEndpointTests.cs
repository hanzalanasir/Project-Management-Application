using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

[Collection(ApiTestCollection.Name)]
public class CreateTaskEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public CreateTaskEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task CreateTask_AsProjectManagerOwningTheProject_Returns201_WithLocationAndETag_AndDefaults()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Task Endpoint Project", null, "2026-08-01", "2026-12-31", "Planning", null));

        var response = await CreateTaskAsync(client, pmToken, projectId, new CreateTaskRequest("Draft rollout checklist", null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("/api/tasks/");
        response.Headers.ETag.Should().NotBeNull();

        var body = await ReadJsonAsync(response);
        body.GetProperty("title").GetString().Should().Be("Draft rollout checklist");
        body.GetProperty("status").GetString().Should().Be("ToDo");
        body.GetProperty("priority").GetString().Should().Be("Medium");
        body.GetProperty("projectId").GetString().Should().Be(projectId.ToString());
    }
}
