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

[Collection(ApiTestCollection.Name)]
public class UpdateTaskImmutabilityTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public UpdateTaskImmutabilityTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task UpdateTask_BodyProjectIdField_IsStructurallyAbsent_TaskStaysInItsOriginalProject()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Immutable Project", null, "2026-08-01", null, null, null));
        var (otherProjectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Other Immutable Project", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Immutable Task", null, null, null, null));

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/tasks/{taskId}")
        {
            Content = JsonContent.Create(new { title = "Still Immutable", priority = "Medium", projectId = otherProjectId }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        request.Headers.TryAddWithoutValidation("If-Match", etag);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var check = await ReadJsonAsync(await GetTaskByIdAsync(client, pmToken, taskId));
        check.GetProperty("projectId").GetString().Should().Be(projectId.ToString());
    }

    [Fact]
    public async Task UpdateTask_NoOpUpdate_StillRefreshesUpdatedAt_AndAudits()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("No-op Project", null, "2026-08-01", null, null, null));
        var created = await ReadJsonAsync(await CreateTaskAsync(client, pmToken, projectId, new CreateTaskRequest("No-op Task", null, "Medium", null, null)));
        var taskId = created.GetProperty("id").GetString()!;
        var firstGet = await GetTaskByIdAsync(client, pmToken, taskId);
        var originalETag = firstGet.Headers.ETag!.Tag;
        var originalBody = await ReadJsonAsync(firstGet);
        var originalUpdatedAt = originalBody.GetProperty("updatedAt").GetString();

        await Task.Delay(50);

        var response = await PutTaskAsync(client, pmToken, taskId, originalETag, new UpdateTaskRequest("No-op Task", null, "Medium", null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.GetProperty("updatedAt").GetString().Should().NotBe(originalUpdatedAt);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var auditEntries = await db.ActivityLogs.Where(a => a.EntityId == taskId && a.Action == "TaskUpdated").ToListAsync();
        auditEntries.Should().NotBeEmpty();
    }
}
