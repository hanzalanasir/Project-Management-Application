using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T046: the `assigneeId` filter is honoured for Admin/ProjectManager within their scope, but a
// TeamMember can never use it to trend a colleague — their own id is silently substituted instead.
[Collection(ApiTestCollection.Name)]
public class TaskCompletionAssigneeFilterTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public TaskCompletionAssigneeFilterTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private async Task<(Guid ProjectId, string PmToken, string TmToken, Guid TmId, Guid Tm2Id)> SeedAsync()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        var tm2Id = await RegisterAndGetIdAsync(client, "TM2 AssigneeFilter", "tm2-assignee-filter@example.com", "S3cure-P@ss1!");

        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Assignee Filter Project", null, "2026-08-01", null, null, null));
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        await AssignTeamMemberAsync(_fixture.Services, projectId, tm2Id);

        var (tmTaskId, tmEtag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("TmTask", null, null, null, tmId));
        await PutTaskStatusAsync(client, pmToken, tmTaskId, tmEtag, new UpdateTaskStatusRequest("Done"));
        await SetTaskClosedAtAsync(_fixture.Services, tmTaskId, new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero));

        var (tm2TaskId, tm2Etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Tm2Task", null, null, null, tm2Id));
        await PutTaskStatusAsync(client, pmToken, tm2TaskId, tm2Etag, new UpdateTaskStatusRequest("Done"));
        await SetTaskClosedAtAsync(_fixture.Services, tm2TaskId, new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero));

        return (projectId, pmToken, tmToken, tmId, tm2Id);
    }

    [Fact]
    public async Task ProjectManager_AssigneeFilter_NarrowsToJustThatMember()
    {
        var (projectId, pmToken, _, tmId, _) = await SeedAsync();
        var client = _fixture.CreateClient();

        var body = await GetTaskCompletionAsync(client, pmToken,
            $"?from=2026-06-15&to=2026-06-15&groupBy=day&projectScope={projectId}&assigneeId={tmId}");

        body.GetProperty("buckets")[0].GetProperty("completedCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task TeamMember_NamingAColleaguesAssigneeId_IsConstrainedToOwnCompletionsInstead()
    {
        var (projectId, _, tmToken, _, tm2Id) = await SeedAsync();
        var client = _fixture.CreateClient();

        // TM asks for TM2's trend — must get back their OWN completion count (1), not TM2's.
        var body = await GetTaskCompletionAsync(client, tmToken,
            $"?from=2026-06-15&to=2026-06-15&groupBy=day&projectScope={projectId}&assigneeId={tm2Id}");

        body.GetProperty("buckets")[0].GetProperty("completedCount").GetInt32().Should().Be(1);
    }
}
