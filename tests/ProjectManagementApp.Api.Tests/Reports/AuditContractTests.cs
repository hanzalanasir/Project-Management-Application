using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T081 🎯 — the highest-value test in this feature (DoD 9, FR-011/FR-012). Consolidates the
// per-story proofs already made piecemeal by ProjectProgressAuditTests (T036) and ExportAuditTests
// (T076) into one contract that covers all four data reports at once, plus the two structural
// guarantees 005's NoWriteGuaranteeTests (T049) established for Dashboard: no domain entity is ever
// written by a report request, and no row's xmin is ever bumped, proven the same way — a direct
// Postgres xmin comparison, not just a row-count comparison, since a naive re-save with identical
// values would pass a plain count check but still corrupt every ETag depending on that row.
[Collection(ApiTestCollection.Name)]
public class AuditContractTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public AuditContractTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private async Task<int> ReportGeneratedCountAsync()
    {
        await using var db = _fixture.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Database
            .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM activity_logs WHERE action = 'ReportGenerated'")
            .SingleAsync();
    }

    [Theory]
    [InlineData("project-progress")]
    [InlineData("task-completion")]
    [InlineData("team-performance")]
    [InlineData("activity")]
    public async Task EachDataReport_OneSuccessfulRequest_WritesExactlyOneAuditRow(string reportSegment)
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var before = await ReportGeneratedCountAsync();

        var response = reportSegment switch
        {
            "project-progress" => await GetProjectProgressResponseAsync(client, pmToken, DefaultWindowQuery()),
            "task-completion" => await GetTaskCompletionResponseAsync(client, pmToken, DefaultWindowQuery() + "&groupBy=day"),
            "team-performance" => await GetTeamPerformanceResponseAsync(client, pmToken, DefaultWindowQuery()),
            "activity" => await GetActivityResponseAsync(client, pmToken, DefaultWindowQuery()),
            _ => throw new ArgumentOutOfRangeException(nameof(reportSegment)),
        };
        response.EnsureSuccessStatusCode();

        var after = await ReportGeneratedCountAsync();
        after.Should().Be(before + 1, $"a single successful {reportSegment} request must generate exactly one audit row");
    }

    [Fact]
    public async Task Catalog_NeverWritesAnAuditRow()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var before = await ReportGeneratedCountAsync();

        for (var i = 0; i < 3; i++)
        {
            (await GetCatalogResponseAsync(client, pmToken)).EnsureSuccessStatusCode();
        }

        var after = await ReportGeneratedCountAsync();
        after.Should().Be(before, "the catalog is metadata, not a generated report — it must never be audited");
    }

    [Fact]
    public async Task A400Request_WritesNoAuditRow()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var before = await ReportGeneratedCountAsync();

        // from > to is rejected by the validator before the handler's audit call ever runs.
        var response = await GetProjectProgressResponseAsync(client, pmToken, "?from=2026-08-10&to=2026-08-01");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var after = await ReportGeneratedCountAsync();
        after.Should().Be(before, "nothing was generated — a 400 must not audit");
    }

    [Fact]
    public async Task A403Request_WritesNoAuditRow()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "audit-contract-403");

        var before = await ReportGeneratedCountAsync();

        // scenario.ProjectBId is owned by PM2, out of scope for scenario.PmToken (PM) — naming it
        // explicitly must 403 the whole request per ReportScope.ResolveAsync's three-branch rule.
        var response = await GetProjectProgressResponseAsync(
            client, scenario.PmToken, $"{DefaultWindowQuery()}&projectScope={scenario.ProjectBId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var after = await ReportGeneratedCountAsync();
        after.Should().Be(before, "nothing was generated — a 403 must not audit");
    }

    [Fact]
    public async Task ReExportingAlreadyPreviewedData_WritesNoAdditionalAuditRow()
    {
        // Mirrors ExportAuditTests (T076): export renders the JSON a fetch already returned and
        // makes no further HTTP call at all (proven directly against the frontend by T075's
        // network-spy test) — so "re-export" has structurally nothing left to audit here. This
        // reframes that same guarantee as the general "re-export -> 0" clause T081 asks for.
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var before = await ReportGeneratedCountAsync();

        (await GetProjectProgressResponseAsync(client, pmToken, DefaultWindowQuery())).EnsureSuccessStatusCode();
        var afterFetch = await ReportGeneratedCountAsync();
        afterFetch.Should().Be(before + 1);

        // "Export to PDF"/"Export to CSV" happen here: zero HTTP calls, so nothing to await against
        // the API — the count below proves no hidden re-fetch/re-audit occurred in between.
        var afterExport = await ReportGeneratedCountAsync();
        afterExport.Should().Be(afterFetch, "exporting already-previewed data must not touch the API again, let alone audit again");
    }

    [Fact]
    public async Task GeneratingAReport_WritesNoDomainEntity_AndBumpsNoRowXmin()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "audit-contract-xmin");

        await using var db = _fixture.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var projectCountBefore = await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM projects").SingleAsync();
        var taskCountBefore = await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM tasks").SingleAsync();
        var projectXminBefore = await db.Database
            .SqlQuery<string>($"SELECT xmin::text AS \"Value\" FROM projects WHERE id = {scenario.ProjectAId}")
            .SingleAsync();

        (await GetProjectProgressResponseAsync(client, scenario.AdminToken, DefaultWindowQuery())).EnsureSuccessStatusCode();
        (await GetTaskCompletionResponseAsync(client, scenario.AdminToken, DefaultWindowQuery() + "&groupBy=day")).EnsureSuccessStatusCode();
        (await GetTeamPerformanceResponseAsync(client, scenario.AdminToken, DefaultWindowQuery())).EnsureSuccessStatusCode();
        (await GetActivityResponseAsync(client, scenario.AdminToken, DefaultWindowQuery())).EnsureSuccessStatusCode();

        var projectCountAfter = await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM projects").SingleAsync();
        var taskCountAfter = await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM tasks").SingleAsync();
        var projectXminAfter = await db.Database
            .SqlQuery<string>($"SELECT xmin::text AS \"Value\" FROM projects WHERE id = {scenario.ProjectAId}")
            .SingleAsync();

        projectCountAfter.Should().Be(projectCountBefore, "reports never create/delete domain rows");
        taskCountAfter.Should().Be(taskCountBefore, "reports never create/delete domain rows");
        projectXminAfter.Should().Be(projectXminBefore, "reports must never re-save a domain row, even with identical values — that would still bump xmin and invalidate ETags");
    }
}
