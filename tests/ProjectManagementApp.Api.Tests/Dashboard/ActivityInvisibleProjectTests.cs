using System.Net.Http.Headers;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// T033: activity for a project the caller can no longer see — ownership transferred away, or the
// project deleted (audit rows survive per 001-004's convention) — is scoped OUT for a non-Admin
// but remains visible to Admin. Mirrors spec 005's Edge Cases section exactly.
[Collection(ApiTestCollection.Name)]
public class ActivityInvisibleProjectTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ActivityInvisibleProjectTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Activity_AfterOwnershipTransfer_FormerOwnerLosesVisibility_AdminKeepsIt()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var pm2Id = await RegisterProjectManagerAsync(client, adminToken, "PM2 Invisible", "pm2-invisible@example.com", "S3cure-P@ss1!");

        var (projectId, etag) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Transfer Target", null, "2026-08-01", null, null, null));

        // Admin transfers ownership away from the original PM.
        var transferResponse = await PutProjectAsync(client, adminToken, projectId, etag,
            new UpdateProjectRequest("Transfer Target", null, "2026-08-01", null, "Planning", pm2Id));
        transferResponse.EnsureSuccessStatusCode();

        var pmActivity = await GetActivityAsync(client, pmToken, "?pageSize=100");
        pmActivity.GetProperty("items").EnumerateArray()
            .Should().NotContain(i => i.GetProperty("entityId").GetString() == projectId.ToString());

        var adminActivity = await GetActivityAsync(client, adminToken, "?pageSize=100");
        adminActivity.GetProperty("items").EnumerateArray()
            .Should().Contain(i => i.GetProperty("entityId").GetString() == projectId.ToString());
    }

    [Fact]
    public async Task Activity_AfterProjectDeletion_FormerOwnerLosesVisibility_AdminStillSeesTheSurvivingAuditRow()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Doomed Dashboard Project", null, "2026-08-01", null, null, null));

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/projects/{projectId}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        (await client.SendAsync(deleteRequest)).EnsureSuccessStatusCode();

        var pmActivity = await GetActivityAsync(client, pmToken, "?pageSize=100");
        pmActivity.GetProperty("items").EnumerateArray()
            .Should().NotContain(i => i.GetProperty("entityId").GetString() == projectId.ToString());

        // Admin's Unscoped read still surfaces the ProjectDeleted row — audit rows survive
        // deletion (spec US-002-05, FR-012), and Admin's visibility isn't project-membership-gated.
        var adminActivity = await GetActivityAsync(client, adminToken, "?pageSize=100");
        adminActivity.GetProperty("items").EnumerateArray()
            .Should().Contain(i => i.GetProperty("entityId").GetString() == projectId.ToString() && i.GetProperty("action").GetString() == "ProjectDeleted");
    }
}
