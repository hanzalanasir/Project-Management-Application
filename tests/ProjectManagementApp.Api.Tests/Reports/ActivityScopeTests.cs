using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T066: per-role scoping with a scoped totalCount; entityType/actorId/projectId filters narrow
// WITHIN scope (never widen it); a named out-of-scope projectId 403s.
[Collection(ApiTestCollection.Name)]
public class ActivityScopeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ActivityScopeTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static string WindowQuery()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return $"?from={today.AddDays(-1):yyyy-MM-dd}&to={today:yyyy-MM-dd}";
    }

    [Fact]
    public async Task PmSeesOnlyOwnProjectsActivity_Pm2SeesOnlyTheirs_AdminSeesBoth()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM2 ActivityScope", "pm2-activityscope@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "pm2-activityscope@example.com", "S3cure-P@ss1!");

        var (projectAId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Activity Scope A", null, "2026-08-01", null, null, null));
        var (projectBId, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("Activity Scope B", null, "2026-08-01", null, null, null));

        var pmBody = await GetActivityAsync(client, pmToken, WindowQuery());
        pmBody.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("entityId").GetString())
            .Should().Contain(projectAId.ToString()).And.NotContain(projectBId.ToString());

        var pm2Body = await GetActivityAsync(client, pm2Token, WindowQuery());
        pm2Body.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("entityId").GetString())
            .Should().Contain(projectBId.ToString()).And.NotContain(projectAId.ToString());

        var adminBody = await GetActivityAsync(client, adminToken, WindowQuery());
        var adminEntityIds = adminBody.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("entityId").GetString()).ToList();
        adminEntityIds.Should().Contain(projectAId.ToString()).And.Contain(projectBId.ToString());
    }

    [Fact]
    public async Task EntityTypeFilter_NarrowsWithinScope()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Activity Filter Project", null, "2026-08-01", null, null, null));
        await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("FilterTask", null, null, null, null));

        var body = await GetActivityAsync(client, pmToken, WindowQuery() + "&entityType=Project");
        var entityTypes = body.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("entityType").GetString()).ToList();
        entityTypes.Should().OnlyContain(t => t == "Project");
    }

    [Fact]
    public async Task NamedOutOfScopeProjectId_Returns403()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM2 ActivityForbidden", "pm2-activity-forbidden@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "pm2-activity-forbidden@example.com", "S3cure-P@ss1!");
        var (pm2ProjectId, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("Activity Forbidden B", null, "2026-08-01", null, null, null));

        var response = await GetActivityResponseAsync(client, pmToken, WindowQuery() + $"&projectId={pm2ProjectId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TotalCount_IsScopedNotSystemWide()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM2 ActivityTotalCount", "pm2-activity-totalcount@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "pm2-activity-totalcount@example.com", "S3cure-P@ss1!");

        await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Activity TotalCount A", null, "2026-08-01", null, null, null));
        await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("Activity TotalCount B", null, "2026-08-01", null, null, null));

        var pmBody = await GetActivityAsync(client, pmToken, WindowQuery());
        var adminBody = await GetActivityAsync(client, adminToken, WindowQuery());

        pmBody.GetProperty("totalCount").GetInt32().Should().BeLessThan(adminBody.GetProperty("totalCount").GetInt32());
    }
}
