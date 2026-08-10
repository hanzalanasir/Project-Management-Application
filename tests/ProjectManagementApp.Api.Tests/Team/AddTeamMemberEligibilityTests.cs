using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

// The key eligibility test (quickstart V2, FR-016): eligibility to be ADDED is "any active user of
// any global role" — role never leaks into this check. Only "deactivated" gates it. Distinct from
// AddTeamMemberAuthorizationTests, which is about who may PERFORM the add.
[Collection(ApiTestCollection.Name)]
public class AddTeamMemberEligibilityTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public AddTeamMemberEligibilityTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task AddTeamMember_TeamMember_IsEligible()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Eligibility Project TM", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));

        var response = await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddTeamMember_ProjectManagerWhoDoesNotOwnTheProject_IsEligible()
    {
        // The point of this feature: membership is a link, not a role. A PM who does not own the
        // project can still be added as a contributor — that changes what they can SEE, not what
        // they ARE.
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Eligibility Project PM2", null, "2026-08-01", null, null, null));
        var pm2Id = await RegisterProjectManagerAsync(client, adminToken, "Eligible PM2", "eligible-pm2@example.com", "S3cure-P@ss1!");

        var response = await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(pm2Id));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddTeamMember_Admin_IsEligible()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Eligibility Project Admin", null, "2026-08-01", null, null, null));
        var adminId = await GetCurrentUserIdAsync(client, await LoginAsync(client, AdminEmail, AdminPassword));

        var response = await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(adminId));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddTeamMember_DeactivatedUser_Returns400_TheOnlyAddTimeGate()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Eligibility Project Inactive", null, "2026-08-01", null, null, null));
        var inactiveId = await RegisterAndGetIdAsync(client, "Soon Inactive", "soon-inactive@example.com", "S3cure-P@ss1!");
        await DeactivateUserAsync(client, adminToken, inactiveId);

        var response = await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(inactiveId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
