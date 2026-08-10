using System.Linq;
using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

// FR-013/data-model.md §2: each roster row's `role` reflects the member's GLOBAL role (001), read
// only — never anything project-specific, since team_members carries no role column at all. A
// deactivated member still appears by default (IncludeInactiveMembersInRoster defaults true) —
// they are flagged via `isActive`, not silently filtered out.
[Collection(ApiTestCollection.Name)]
public class GetProjectTeamDisplayTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public GetProjectTeamDisplayTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task GetProjectTeam_EachRow_CarriesTheMembersGlobalRole()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Display Roles", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));
        var adminId = await GetCurrentUserIdAsync(client, adminToken);
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(adminId));

        var response = await GetTeamAsync(client, pmToken, projectId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        var rows = body.EnumerateArray().ToList();
        rows.Single(r => r.GetProperty("userId").GetString() == tmId.ToString())
            .GetProperty("role").GetString().Should().Be("TeamMember");
        rows.Single(r => r.GetProperty("userId").GetString() == adminId.ToString())
            .GetProperty("role").GetString().Should().Be("Admin");
    }

    [Fact]
    public async Task GetProjectTeam_DeactivatedMember_StillAppears_FlaggedAsInactive()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Display Deactivated", null, "2026-08-01", null, null, null));
        var memberId = await RegisterAndGetIdAsync(client, "Soon Deactivated Member", "soon-deactivated-member@example.com", "S3cure-P@ss1!");
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(memberId));
        await DeactivateUserAsync(client, adminToken, memberId);

        var response = await GetTeamAsync(client, pmToken, projectId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        var row = body.EnumerateArray().Single(r => r.GetProperty("userId").GetString() == memberId.ToString());
        row.GetProperty("isActive").GetBoolean().Should().BeFalse();
    }
}
