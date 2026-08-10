using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

// projectId comes from the ROUTE only — the contract's AddTeamMemberRequest has no projectId
// property at all, so a widened payload can't smuggle a membership into a different project
// (mirrors 003's CreateTask FR-003 pattern).
[Collection(ApiTestCollection.Name)]
public class AddTeamMemberRouteAuthorityTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public AddTeamMemberRouteAuthorityTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task AddTeamMember_BodyProjectIdPointingElsewhere_IsIgnored_MembershipLandsOnRouteProject()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (routeProjectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Route Authority Project A", null, "2026-08-01", null, null, null));
        var otherPmId = await RegisterProjectManagerAsync(client, adminToken, "Elsewhere Owner", "elsewhere-owner@example.com", "S3cure-P@ss1!");
        var otherPmToken = await LoginAsync(client, "elsewhere-owner@example.com", "S3cure-P@ss1!");
        var (elsewhereProjectId, _) = await CreateProjectAsync(client, otherPmToken, new CreateProjectRequest("Route Authority Project B", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));

        var response = await AddTeamMemberAsync(client, pmToken, routeProjectId, new { userId = tmId, projectId = elsewhereProjectId });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.TeamMembers.AnyAsync(m => m.ProjectId == routeProjectId && m.UserId == tmId)).Should().BeTrue();
        (await db.TeamMembers.AnyAsync(m => m.ProjectId == elsewhereProjectId && m.UserId == tmId)).Should().BeFalse();
    }
}
