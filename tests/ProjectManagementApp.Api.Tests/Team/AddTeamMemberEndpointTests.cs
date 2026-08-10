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

[Collection(ApiTestCollection.Name)]
public class AddTeamMemberEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public AddTeamMemberEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task AddTeamMember_AsOwningProjectManager_Returns201_WithLocation_AddedByRecorded_AndAudited()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var pmId = await GetCurrentUserIdAsync(client, pmToken);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Add Member Project", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));

        var response = await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/projects/{projectId}/team/{tmId}");

        var body = await ReadJsonAsync(response);
        body.GetProperty("userId").GetString().Should().Be(tmId.ToString());
        body.GetProperty("role").GetString().Should().Be("TeamMember");

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var membership = await db.TeamMembers.SingleAsync(m => m.ProjectId == projectId && m.UserId == tmId);
        membership.AddedBy.Should().Be(pmId);

        var auditRow = await db.ActivityLogs.SingleOrDefaultAsync(a => a.EntityId == membership.Id.ToString() && a.Action == "TeamMemberAdded");
        auditRow.Should().NotBeNull();
        auditRow!.ActorId.Should().Be(pmId);
    }
}
