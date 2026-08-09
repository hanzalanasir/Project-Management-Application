using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

// No status workflow in v1 (OQ-003-03) — any status may move to any other, including out of Done.
[Collection(ApiTestCollection.Name)]
public class UpdateTaskStatusWorkflowTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public UpdateTaskStatusWorkflowTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task AnyStatus_MayMoveToAnyOther_IncludingOutOfDone()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Workflow Freedom Project", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Workflow Task", null, null, null, null));

        var toDone = await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("Done"));
        toDone.StatusCode.Should().Be(HttpStatusCode.OK);
        var doneEtag = toDone.Headers.ETag!.Tag;

        // Out of Done, straight back to ToDo — no enforced workflow.
        var backToToDo = await PutTaskStatusAsync(client, pmToken, taskId, doneEtag, new UpdateTaskStatusRequest("ToDo"));
        backToToDo.StatusCode.Should().Be(HttpStatusCode.OK);
        var toDoEtag = backToToDo.Headers.ETag!.Tag;

        var toBlocked = await PutTaskStatusAsync(client, pmToken, taskId, toDoEtag, new UpdateTaskStatusRequest("Blocked"));
        toBlocked.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task InvalidStatusEnumValue_Returns400()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Invalid Status Project", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Invalid Status Task", null, null, null, null));

        var response = await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("NotARealStatus"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AsAdmin_ForEveryRole_StatusChangeSucceeds()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Admin Status Project", null, "2026-08-01", null, null, null));
        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Admin Status Task", null, null, null, null));

        var response = await PutTaskStatusAsync(client, adminToken, taskId, etag, new UpdateTaskStatusRequest("InReview"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
