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
public class ReassignTaskValidationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ReassignTaskValidationTests(ApiTestFixture fixture)
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
    public async Task ReassignTask_CandidateNotOnProjectTeam_Returns400_WithFieldError()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Reassign Pool Project", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Pool Task", null, null, null, null));
        var outsiderId = await RegisterAndGetIdAsync(client, "Outsider", "outsider-reassign@example.com", "S3cure-P@ss1!");

        var response = await PutTaskAssigneeAsync(client, pmToken, taskId, etag, new ReassignTaskRequest(outsiderId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await ReadJsonAsync(response);
        body.GetProperty("errors").TryGetProperty("assigneeId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ReassignTask_DeactivatedCandidate_Returns400()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Reassign Deactivated Project", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Deactivated Task", null, null, null, null));
        var candidateId = await RegisterAndGetIdAsync(client, "Soon Deactivated", "deactivated-reassign@example.com", "S3cure-P@ss1!");
        await AssignTeamMemberAsync(_fixture.Services, projectId, candidateId);
        await DeactivateAsync(client, adminToken, candidateId);

        var response = await PutTaskAssigneeAsync(client, pmToken, taskId, etag, new ReassignTaskRequest(candidateId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
