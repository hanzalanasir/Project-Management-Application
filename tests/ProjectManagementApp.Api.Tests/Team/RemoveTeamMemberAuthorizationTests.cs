using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

[Collection(ApiTestCollection.Name)]
public class RemoveTeamMemberAuthorizationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public RemoveTeamMemberAuthorizationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task RemoveTeamMember_AsNonOwningProjectManager_Returns403()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Remove Auth Non Owner", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));
        var pm2Id = await RegisterProjectManagerAsync(client, adminToken, "Remove Auth PM2", "remove-auth-pm2@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "remove-auth-pm2@example.com", "S3cure-P@ss1!");
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(pm2Id));

        var response = await RemoveTeamMemberAsync(client, pm2Token, projectId, tmId);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveTeamMember_AsTeamMember_Returns403_AtTheRoleGate()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Remove Auth TM", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));

        var response = await RemoveTeamMemberAsync(client, tmToken, projectId, tmId);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveTeamMember_OwningProjectManager_RemovingThemselves_IsPermitted()
    {
        // No "can't remove yourself" guard exists anywhere in spec — an owner PM who added
        // themselves as a contributor can remove themselves same as any other member.
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var pmId = await GetCurrentUserIdAsync(client, pmToken);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Remove Self PM", null, "2026-08-01", null, null, null));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(pmId));

        var response = await RemoveTeamMemberAsync(client, pmToken, projectId, pmId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveTeamMember_Admin_RemovingThemselves_IsPermitted()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var adminId = await GetCurrentUserIdAsync(client, adminToken);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Remove Self Admin", null, "2026-08-01", null, null, null));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(adminId));

        var response = await RemoveTeamMemberAsync(client, adminToken, projectId, adminId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
