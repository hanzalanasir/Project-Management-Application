using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

// A status request body widened with title/assigneeId/priority has no property to bind them to —
// structurally inert, not merely rejected (DoD 4). Verified directly in the database, not just the
// response DTO.
[Collection(ApiTestCollection.Name)]
public class UpdateTaskStatusPayloadTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public UpdateTaskStatusPayloadTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task StatusRequest_WithExtraFields_Returns200_OnlyStatusChanges_VerifiedInTheDatabase()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Payload Widening Project", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        var otherId = await RegisterAndGetIdAsync(client, "Other Assignee", "other-assignee-payload@example.com", "Other#Passw0rd1!");
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        await AssignTeamMemberAsync(_fixture.Services, projectId, otherId);
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Original Title", null, "Medium", null, tmId));

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/tasks/{taskId}/status")
        {
            Content = JsonContent.Create(new { status = "InReview", title = "HACKED", assigneeId = otherId, priority = "Critical" }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tmToken);
        request.Headers.TryAddWithoutValidation("If-Match", etag);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.GetProperty("status").GetString().Should().Be("InReview");

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var task = await db.Tasks.AsNoTracking().SingleAsync(t => t.Id == Guid.Parse(taskId));
        task.Title.Should().Be("Original Title");
        task.AssigneeId.Should().Be(tmId);
        task.Priority.ToString().Should().Be("Medium");
    }
}
