using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

// Who may PERFORM the add (CanManageTeamAsync + the role gate) — distinct from
// AddTeamMemberEligibilityTests, which is about who may BE added.
[Collection(ApiTestCollection.Name)]
public class AddTeamMemberAuthorizationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public AddTeamMemberAuthorizationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task AddTeamMember_ProjectManagerWhoDoesNotOwnTheProject_Returns403()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Auth Project Owner", null, "2026-08-01", null, null, null));
        var pm2Id = await RegisterProjectManagerAsync(client, adminToken, "Non Owner PM", "non-owner-pm@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "non-owner-pm@example.com", "S3cure-P@ss1!");
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));

        var response = await AddTeamMemberAsync(client, pm2Token, projectId, new AddTeamMemberRequest(tmId));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddTeamMember_TeamMember_Returns403_AtTheRoleGate()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Auth Project TM Gate", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var adminId = await GetCurrentUserIdAsync(client, await LoginAsync(client, AdminEmail, AdminPassword));

        var response = await AddTeamMemberAsync(client, tmToken, projectId, new AddTeamMemberRequest(adminId));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddTeamMember_Admin_SucceedsOnAnyProject()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Auth Project Admin", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));

        var response = await AddTeamMemberAsync(client, adminToken, projectId, new AddTeamMemberRequest(tmId));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
