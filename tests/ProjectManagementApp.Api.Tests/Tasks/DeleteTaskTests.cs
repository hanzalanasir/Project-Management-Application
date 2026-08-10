using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

// No If-Match required for delete (ADR-0007 §3, same as 002's DeleteProject) — the TaskDeleted
// audit row is written BEFORE removal and survives it (spec US-003-06).
[Collection(ApiTestCollection.Name)]
public class DeleteTaskTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public DeleteTaskTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task DeleteTask_Returns204_RetainsAuditRow_SecondDeleteReturns404_NoIfMatchNeeded()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Delete Task Project", null, "2026-08-01", null, null, null));
        var (taskId, _) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("To Delete", null, null, null, null));

        // Deliberately no If-Match header — delete has no lost-update failure mode.
        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/tasks/{taskId}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var deleteResponse = await client.SendAsync(deleteRequest);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.Tasks.AnyAsync(t => t.Id == Guid.Parse(taskId))).Should().BeFalse();

            var auditRow = await db.ActivityLogs.SingleOrDefaultAsync(a => a.EntityId == taskId && a.Action == "TaskDeleted");
            auditRow.Should().NotBeNull();
            auditRow!.ChangeSummary.Should().Contain("To Delete");
        }

        using var secondDeleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/tasks/{taskId}");
        secondDeleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var secondDeleteResponse = await client.SendAsync(secondDeleteRequest);
        secondDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
