using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// T050 — proves the overdue boundary is anchored to UTC and cannot drift with the host's local
// timezone (research R-2). A configurable/locale-dependent boundary here would let a deployment
// silently compute a different "overdue" than 006's future Reports surface, breaking NFR-002
// parity. Two complementary halves:
//   1. A source scan (same technique as T031/T049) proving no file under Features/Dashboard/ ever
//      reads DateTime.Now/DateTime.Today/TimeZoneInfo.Local — only DateTimeOffset.UtcNow, which
//      the .NET runtime guarantees has a zero offset regardless of the host's local timezone
//      setting (confirmed directly below). This is the actual mechanism that makes "run the
//      assertion under a non-UTC process clock" a non-event: there is no local-clock read left in
//      the code path for a timezone change to affect.
//   2. Honesty note on what could NOT be exercised in this environment: TimeZoneInfo.Local on
//      Windows is resolved from the OS registry setting, not the TZ environment variable (verified
//      empirically — setting TZ and calling TimeZoneInfo.ClearCachedData() left
//      TimeZoneInfo.Local unchanged), and actually changing the sandbox's OS-level timezone is a
//      system-wide, hard-to-reverse action out of scope for an automated test. The guarantee is
//      therefore proven structurally (no locale-dependent API is reachable) rather than by
//      empirically flipping the host clock.
[Collection(ApiTestCollection.Name)]
public class OverdueTimezoneTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public OverdueTimezoneTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public void NoSourceFileUnderFeaturesDashboard_ReadsTheLocalClock()
    {
        var repoRoot = FindRepoRoot();
        var dashboardDir = Path.Combine(repoRoot, "src", "ProjectManagementApp.Application", "Features", "Dashboard");
        Directory.Exists(dashboardDir).Should().BeTrue();

        var localClockTokens = new[] { "DateTime.Now", "DateTime.Today", "TimeZoneInfo.Local", "TimeZoneInfo.ConvertTime" };

        var offendingFiles = Directory.EnumerateFiles(dashboardDir, "*.cs", SearchOption.AllDirectories)
            .Where(file => File.ReadAllLines(file)
                .Where(line => !line.TrimStart().StartsWith("//"))
                .Any(line => localClockTokens.Any(line.Contains)))
            .ToList();

        offendingFiles.Should().BeEmpty(
            "the overdue boundary must be computed from DateTimeOffset.UtcNow only — any local-clock read here " +
            "would let a deployment's OS timezone setting silently change the overdue count (research R-2)");
    }

    [Fact]
    public void DateTimeOffsetUtcNow_OffsetIsAlwaysZero_RegardlessOfHostLocalTimezone()
    {
        // The runtime guarantee that makes the boundary immune to the host's local timezone: this
        // is not application logic, it's a .NET contract. As long as Features/Dashboard/ never
        // reads the local clock (proven above), this guarantee is the whole story.
        DateTimeOffset.UtcNow.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task OverdueBoundary_MatchesQuickstartV6_UnderTheHostsActualLocalTimezone()
    {
        // Functional confirmation the boundary is correct at all (V6's scenario), run under
        // whatever local timezone this machine actually has — which the two tests above prove is
        // irrelevant to the result, since the code path never consults it.
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "tz-boundary");

        var summary = await GetSummaryAsync(client, scenario.PmToken);

        // Ta (ToDo, due yesterday) and Tf (Blocked, due yesterday) overdue; Tc (Done, due
        // yesterday) excluded regardless of date; Te (due today) not yet overdue.
        summary.GetProperty("overdueTaskCount").GetInt32().Should().Be(2);
    }

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
