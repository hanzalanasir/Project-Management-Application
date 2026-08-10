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
public class AddTeamMemberDuplicateTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public AddTeamMemberDuplicateTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task AddTeamMember_AlreadyAMember_Returns409_ExactlyOneRowRemains()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Duplicate Add Project", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));

        var first = await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var count = await db.TeamMembers.CountAsync(m => m.ProjectId == projectId && m.UserId == tmId);
        count.Should().Be(1);
    }

    [Fact]
    public async Task AddTeamMember_UnknownProject_Returns404()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));

        var response = await AddTeamMemberAsync(client, pmToken, Guid.NewGuid(), new AddTeamMemberRequest(tmId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddTeamMember_UnknownUser_Returns404()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Duplicate Add Unknown User", null, "2026-08-01", null, null, null));

        var response = await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
