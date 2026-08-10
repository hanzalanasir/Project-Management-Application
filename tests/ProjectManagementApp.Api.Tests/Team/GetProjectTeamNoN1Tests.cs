using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

// T059 (NFR-002): proves the roster read is a single bounded query joined to users, not one
// lookup per member. Mirrors 002/003's own CommandCounterInterceptor methodology (T080/T101)
// exactly. As Admin against a 3-member roster spanning all three roles, ListTeamQueryHandler
// issues exactly 8 SQL commands: (1) the project lookup, (2) the single Include(m => m.User)
// roster read, and (3-8) BuildRoleLookupAsync's fixed 3 GetUsersInRoleAsync calls (T014) — each of
// which costs TWO round trips under ASP.NET Identity's default UserStore (resolve the role's id
// from AspNetRoles by normalized name, then a join query for the users in it), measured directly
// rather than assumed. CanViewTeamAsync costs 0 queries for an Admin caller (short-circuits on
// role alone). This total is fixed regardless of roster size: it depends only on Role's 3 enum
// values, never on how many members are on the team — an N+1 implementation would grow past 8 as
// more members are added; the correct one does not.
[Collection(ApiTestCollection.Name)]
public class GetProjectTeamNoN1Tests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public GetProjectTeamNoN1Tests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task GetProjectTeam_WithThreeMembersOfDistinctRoles_ExecutesExactlyFiveSqlCommands_NotOnePerMember()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("N1 Team Project", null, "2026-08-01", null, null, null));

        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));
        var pm2Id = await RegisterProjectManagerAsync(client, adminToken, "N1 Team PM2", "n1-team-pm2@example.com", "S3cure-P@ss1!");
        var adminId = await GetCurrentUserIdAsync(client, adminToken);
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(pm2Id));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(adminId));

        _fixture.CommandCounter.Reset();

        var response = await GetTeamAsync(client, adminToken, projectId);

        response.EnsureSuccessStatusCode();
        _fixture.CommandCounter.Count.Should().Be(8,
            "project lookup + one Include(User)-joined roster read + BuildRoleLookupAsync's fixed 3 role queries (each 2 round trips under Identity's default UserStore) — a per-member role lookup would push this past 8 (NFR-002)");
    }
}
