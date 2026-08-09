using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

[Collection(ApiTestCollection.Name)]
public class CreateTaskRouteAuthorityTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public CreateTaskRouteAuthorityTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task CreateTask_BodyProjectIdPointingElsewhere_IsIgnored_TaskLandsUnderTheRouteProject()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (routeProjectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Route Project", null, "2026-08-01", null, "Planning", null));
        var (otherProjectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Other Project", null, "2026-08-01", null, "Planning", null));

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/projects/{routeProjectId}/tasks")
        {
            // CreateTaskRequest has no projectId property (additionalProperties: false in the
            // contract) — this stray field must be silently ignored, never bound.
            Content = JsonContent.Create(new { title = "Smuggled Task", projectId = otherProjectId }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await ReadJsonAsync(response);
        body.GetProperty("projectId").GetString().Should().Be(routeProjectId.ToString());
    }

    [Fact]
    public async Task CreateTask_InAProjectThePmDoesNotOwn_Returns403()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Owned By PM", null, "2026-08-01", null, "Planning", null));

        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        await RegisterProjectManagerAsync(client, adminToken, "Other PM", "other-pm-create-task@example.com", "Other#Passw0rd1!");
        var otherPmToken = await LoginAsync(client, "other-pm-create-task@example.com", "Other#Passw0rd1!");

        var response = await CreateTaskAsync(client, otherPmToken, projectId, new CreateTaskRequest("Should Not Land", null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTask_UnknownProjectId_Returns404()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var response = await CreateTaskAsync(client, pmToken, Guid.NewGuid(), new CreateTaskRequest("Should Not Land", null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
