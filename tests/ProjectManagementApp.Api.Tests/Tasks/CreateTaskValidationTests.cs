using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

[Collection(ApiTestCollection.Name)]
public class CreateTaskValidationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public CreateTaskValidationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static async Task DeactivateAsync(HttpClient client, string adminToken, Guid userId)
    {
        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var getResponse = await client.SendAsync(getRequest);
        var etag = getResponse.Headers.ETag!.Tag;

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{userId}/status")
        {
            Content = JsonContent.Create(new { isActive = false }),
        };
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        putRequest.Headers.TryAddWithoutValidation("If-Match", etag);
        await client.SendAsync(putRequest);
    }

    [Fact]
    public async Task CreateTask_AssigneeNotInProjectPool_Returns400()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Pool Project", null, "2026-08-01", null, "Planning", null));
        var outsiderId = await RegisterAndGetIdAsync(client, "Outsider", "outsider-create-task@example.com", "Outsider#Passw0rd1!");

        var response = await CreateTaskAsync(client, pmToken, projectId, new CreateTaskRequest("Bad Assignee", null, null, null, outsiderId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await ReadJsonAsync(response);
        body.GetProperty("errors").TryGetProperty("assigneeId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateTask_DeactivatedAssignee_Returns400()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Deactivated Assignee Project", null, "2026-08-01", null, "Planning", null));

        var candidateId = await RegisterAndGetIdAsync(client, "Candidate", "deactivated-candidate-create-task@example.com", "Candidate#Passw0rd1!");
        await AssignTeamMemberAsync(_fixture.Services, projectId, candidateId);
        await DeactivateAsync(client, adminToken, candidateId);

        var response = await CreateTaskAsync(client, pmToken, projectId, new CreateTaskRequest("Bad Assignee", null, null, null, candidateId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await ReadJsonAsync(response);
        body.GetProperty("errors").TryGetProperty("assigneeId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateTask_DueDateOutsideProjectWindow_Returns400()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Window Project", null, "2026-08-01", "2026-08-31", "Planning", null));

        var response = await CreateTaskAsync(client, pmToken, projectId, new CreateTaskRequest("Out Of Window", null, null, "2026-12-25", null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await ReadJsonAsync(response);
        body.GetProperty("errors").TryGetProperty("dueDate", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateTask_OmittedAssignee_Returns201_UnassignedIsLegal()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Unassigned Ok Project", null, "2026-08-01", null, "Planning", null));

        var response = await CreateTaskAsync(client, pmToken, projectId, new CreateTaskRequest("No Assignee", null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await ReadJsonAsync(response);
        body.GetProperty("assignee").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
