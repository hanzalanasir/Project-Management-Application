using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Api.Tests.Reports;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;

namespace ProjectManagementApp.Api.Tests.CrossFeature;

// T083 🎯 — the cross-feature payoff for both 005 and 006 (closes 005 plan Follow-up 4). 006's
// GetProjectProgress deliberately imports ReportCountingRules -> MetricDefinitions rather than
// re-implementing "overdue" (data-model.md's own note: this is the only thing that makes this test
// pass "by construction" rather than by coincidence — importing from Features/Dashboard/ itself is
// forbidden by ADR-0006's addendum, so the shared kernel in Common/Metrics/ is the only legal path
// both surfaces can share). If a future change re-implements the predicate in one place and not the
// other, this is the test that would catch the drift.
//
// Neither side windows "overdue" by from/to — GetDashboardSummaryQueryHandler and
// GetProjectProgressQueryHandler both evaluate MetricDefinitions.IsOverdue(today) against every
// open task in scope "as of now", so any from/to window on the Project Progress call is
// comparison-neutral; the test still supplies one because GetProjectProgressQuery requires it.
//
// Non-UTC process clock: see OverdueTimezoneTests (005 T050) for the honest precedent this test
// follows. TimeZoneInfo.Local on Windows is resolved from the OS registry, not the TZ environment
// variable (verified empirically there), so actually flipping the sandbox's OS-level timezone is
// out of scope here too. The structural guarantee is identical and re-verified below: neither
// GetDashboardSummaryQueryHandler's overdue path nor GetProjectProgressQueryHandler's imports a
// local-clock read, so a host timezone change cannot affect either side's number, let alone the two
// sides differently.
[Collection(ApiTestCollection.Name)]
public class DashboardReportParityTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public DashboardReportParityTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public void DateTimeOffsetUtcNow_OffsetIsAlwaysZero_RegardlessOfHostLocalTimezone()
    {
        DateTimeOffset.UtcNow.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void NoSourceFileUnderFeaturesReports_ReadsTheLocalClock()
    {
        var repoRoot = FindRepoRoot();
        var reportsDir = Path.Combine(repoRoot, "src", "ProjectManagementApp.Application", "Features", "Reports");
        Directory.Exists(reportsDir).Should().BeTrue();

        var localClockTokens = new[] { "DateTime.Now", "DateTime.Today", "TimeZoneInfo.Local", "TimeZoneInfo.ConvertTime" };

        var offendingFiles = Directory.EnumerateFiles(reportsDir, "*.cs", SearchOption.AllDirectories)
            .Where(file => File.ReadAllLines(file)
                .Where(line => !line.TrimStart().StartsWith("//"))
                .Any(line => localClockTokens.Any(line.Contains)))
            .ToList();

        offendingFiles.Should().BeEmpty(
            "the overdue boundary must be computed from DateTimeOffset.UtcNow only, exactly like 005's — " +
            "any local-clock read here would let the two surfaces drift under a non-UTC deployment (NFR-002)");
    }

    [Theory]
    [InlineData(nameof(ScopeScenarioCaller.Admin))]
    [InlineData(nameof(ScopeScenarioCaller.Pm))]
    public async Task DashboardOverdueTaskCount_EqualsSumOfProjectProgressOverdueTasks_ForTheSameCallerAndWindow(string caller)
    {
        var client = _fixture.CreateClient();
        var scenario = await ReportsTestHelper.SeedScopeScenarioAsync(client, _fixture, $"parity-{caller}");
        var token = caller == nameof(ScopeScenarioCaller.Admin) ? scenario.AdminToken : scenario.PmToken;

        var summary = await GetSummaryAsync(client, token);
        var dashboardOverdueCount = summary.GetProperty("overdueTaskCount").GetInt32();

        var report = await GetProjectProgressAsync(client, token, DefaultWindowQuery());
        var reportOverdueSum = report.GetProperty("rows").EnumerateArray()
            .Sum(row => row.GetProperty("overdueTasks").GetInt32());

        reportOverdueSum.Should().Be(dashboardOverdueCount,
            "005's Dashboard overdueTaskCount and 006's summed Project Progress overdueTasks must agree for the same caller and scope, since both import the same MetricDefinitions.IsOverdue predicate");
    }

    private enum ScopeScenarioCaller { Admin, Pm }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ProjectManagementApp.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Could not locate repo root (ProjectManagementApp.slnx not found).");
    }
}
