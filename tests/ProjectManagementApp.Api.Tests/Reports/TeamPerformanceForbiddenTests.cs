using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T055 — the contrasting case to T054: Admin and ProjectManager ARE entitled to know their own
// scope boundary, so a named out-of-scope userId 403s for them (unlike the TeamMember silent clamp).
[Collection(ApiTestCollection.Name)]
public class TeamPerformanceForbiddenTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public TeamPerformanceForbiddenTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ProjectManager_NamingAnOutOfScopeUserId_Returns403()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM2 TeamPerfForbidden", "pm2-teamperf-forbidden@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "pm2-teamperf-forbidden@example.com", "S3cure-P@ss1!");

        var (_, pm2ProjectId) = (await CreateProjectAsync(client, pmToken, new CreateProjectRequest("PM Owned Project", null, "2026-08-01", null, null, null)),
            (await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("PM2 Owned Project", null, "2026-08-01", null, null, null))).Id);

        var strangerId = await RegisterAndGetIdAsync(client, "Stranger", "stranger-teamperf@example.com", "S3cure-P@ss1!");
        await AssignTeamMemberAsync(_fixture.Services, pm2ProjectId, strangerId);

        var response = await GetTeamPerformanceResponseAsync(client, pmToken, DefaultWindowQuery() + $"&userId={strangerId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_NamingAUserWithNoTeamMembershipAnywhere_Returns403()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var strangerId = await RegisterAndGetIdAsync(client, "Nobody", "nobody-teamperf@example.com", "S3cure-P@ss1!");

        var response = await GetTeamPerformanceResponseAsync(client, adminToken, DefaultWindowQuery() + $"&userId={strangerId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "the user is a real account but has no team_members row, so is outside every scope's member pool");
    }
}
