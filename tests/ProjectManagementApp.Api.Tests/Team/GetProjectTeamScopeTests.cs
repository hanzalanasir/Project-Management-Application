using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

// The visibility matrix (quickstart V4, FR-013/FR-014): Admin sees any project's roster; an
// owning PM and a member-but-not-owner PM both see it (the same divergent cell TeamAccessPolicy's
// unit tests already prove — this proves it end-to-end through the real endpoint); a plain
// TeamMember who is a member sees the FULL roster, not just themselves; a non-member is refused.
[Collection(ApiTestCollection.Name)]
public class GetProjectTeamScopeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public GetProjectTeamScopeTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task GetProjectTeam_AsAdmin_Returns200_ForAnyProject()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Scope Admin", null, "2026-08-01", null, null, null));

        var response = await GetTeamAsync(client, adminToken, projectId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProjectTeam_AsOwningProjectManager_Returns200()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Scope Owner PM", null, "2026-08-01", null, null, null));

        var response = await GetTeamAsync(client, pmToken, projectId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProjectTeam_AsProjectManagerWhoIsMemberButNotOwner_Returns200()
    {
        // The same divergent cell as CanManageTeamAsync_ProjectManager_MemberButNotOwner_Denied
        // (T012), proven here through the real HTTP endpoint rather than the policy in isolation.
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var ownerPmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, ownerPmToken, new CreateProjectRequest("Scope Member PM", null, "2026-08-01", null, null, null));
        var pm2Id = await RegisterProjectManagerAsync(client, adminToken, "Member PM2", "member-pm2@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "member-pm2@example.com", "S3cure-P@ss1!");
        await AddTeamMemberAsync(client, ownerPmToken, projectId, new AddTeamMemberRequest(pm2Id));

        var response = await GetTeamAsync(client, pm2Token, projectId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProjectTeam_AsMemberTeamMember_Returns200_WithFullRoster()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Scope Member TM", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));
        var otherId = await RegisterProjectManagerAsync(client, adminToken, "Other Roster PM", "other-roster-pm@example.com", "S3cure-P@ss1!");
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(otherId));

        var response = await GetTeamAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword), projectId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        // Full roster, not just the caller themselves.
        body.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task GetProjectTeam_AsNonMember_Returns403()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Scope Non-Member", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);

        var response = await GetTeamAsync(client, tmToken, projectId);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProjectTeam_UnknownProjectId_Returns404()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var response = await GetTeamAsync(client, pmToken, Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
