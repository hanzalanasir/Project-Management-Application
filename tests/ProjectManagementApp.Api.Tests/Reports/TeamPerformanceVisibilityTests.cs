using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T057: a member with no activity in the window is still a ROW OF ZEROS, never omitted (absence is
// visible); a deactivated member with in-window throughput is still shown, flagged isActive:false.
[Collection(ApiTestCollection.Name)]
public class TeamPerformanceVisibilityTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public TeamPerformanceVisibilityTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task MemberWithNoActivity_AppearsAsARowOfZeros_NotOmitted()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var idleId = await RegisterAndGetIdAsync(client, "Idle Member", "idle-teamperf@example.com", "S3cure-P@ss1!");

        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Idle TeamPerf Project", null, "2026-08-01", null, null, null));
        await AssignTeamMemberAsync(_fixture.Services, projectId, idleId);

        var body = await GetTeamPerformanceAsync(client, pmToken, DefaultWindowQuery());
        var row = body.GetProperty("rows").EnumerateArray().Single(r => r.GetProperty("userId").GetString() == idleId.ToString());

        row.GetProperty("throughput").GetInt32().Should().Be(0);
        row.GetProperty("workload").GetInt32().Should().Be(0);
        row.GetProperty("overdueCount").GetInt32().Should().Be(0);
        row.GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task DeactivatedMember_WithInWindowThroughput_IsStillShown_FlaggedInactive()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var memberId = await RegisterAndGetIdAsync(client, "Soon Deactivated", "deactivated-teamperf@example.com", "S3cure-P@ss1!");

        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Deactivated TeamPerf Project", null, "2026-08-01", null, null, null));
        await AssignTeamMemberAsync(_fixture.Services, projectId, memberId);

        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("ClosedByDeactivated", null, null, null, memberId));
        await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("Done"));

        await using (var db = _fixture.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>())
        {
            var user = await db.Users.SingleAsync(u => u.Id == memberId);
            user.IsActive = false;
            await db.SaveChangesAsync();
        }

        var body = await GetTeamPerformanceAsync(client, pmToken, DefaultWindowQuery());
        var row = body.GetProperty("rows").EnumerateArray().Single(r => r.GetProperty("userId").GetString() == memberId.ToString());

        row.GetProperty("isActive").GetBoolean().Should().BeFalse();
        row.GetProperty("throughput").GetInt32().Should().Be(1);
    }
}
