using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T026 — the 🎯 catalog-not-audited test (FR-011). Every other report endpoint writes exactly one
// ReportGenerated row per call; the catalog never does, because it describes the API rather than
// querying it. Calling it repeatedly, as every role, must add zero rows.
[Collection(ApiTestCollection.Name)]
public class CatalogNotAuditedTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public CatalogNotAuditedTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task RepeatedCatalogCalls_AsEveryRole_WriteZeroActivityLogRows()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);

        await using var db = _fixture.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var countBefore = await db.Database
            .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM activity_logs")
            .SingleAsync();

        foreach (var token in new[] { adminToken, pmToken, tmToken, adminToken, pmToken })
        {
            (await GetCatalogResponseAsync(client, token)).EnsureSuccessStatusCode();
        }

        var countAfter = await db.Database
            .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM activity_logs")
            .SingleAsync();

        countAfter.Should().Be(countBefore, "the catalog is metadata, not a report generation — FR-011 requires zero audit rows");
    }
}
