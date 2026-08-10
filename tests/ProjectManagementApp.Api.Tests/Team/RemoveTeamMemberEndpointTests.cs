using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

[Collection(ApiTestCollection.Name)]
public class RemoveTeamMemberEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public RemoveTeamMemberEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task RemoveTeamMember_AsOwningProjectManager_Returns204_AuditsBeforeDeletion_AndRequiresNoIfMatch()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var pmId = await GetCurrentUserIdAsync(client, pmToken);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Remove Endpoint Project", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));

        var rosterBody = await ReadJsonAsync(await GetTeamAsync(client, pmToken, projectId));
        var membershipId = rosterBody.EnumerateArray().Single(m => m.GetProperty("userId").GetString() == tmId.ToString()).GetProperty("membershipId").GetString();

        // No If-Match header sent at all — there is nothing for it to protect (research R-2).
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/projects/{projectId}/team/{tmId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var membershipStillExists = await db.TeamMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == tmId);
        membershipStillExists.Should().BeFalse();

        var auditRow = await db.ActivityLogs.SingleOrDefaultAsync(a => a.EntityId == membershipId && a.Action == "TeamMemberRemoved");
        auditRow.Should().NotBeNull("the audit row survives even though the membership row it describes is gone");
        auditRow!.ActorId.Should().Be(pmId);
    }
}
