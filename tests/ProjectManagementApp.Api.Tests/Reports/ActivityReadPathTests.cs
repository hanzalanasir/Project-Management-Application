using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Reports;

// T065 — the 🎯 service-not-table test (FR-007). Same source-scan technique 005 used for its own
// activity feed: the Activity Report must read exclusively through
// IActivityLogService.QueryScopedAsync, never a direct LINQ query against db.ActivityLogs.
[Collection(ApiTestCollection.Name)]
public class ActivityReadPathTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ActivityReadPathTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public void NoSourceFileUnderFeaturesReports_ContainsALiteralActivityLogsQuery()
    {
        var repoRoot = FindRepoRoot();
        var reportsDir = Path.Combine(repoRoot, "src", "ProjectManagementApp.Application", "Features", "Reports");
        Directory.Exists(reportsDir).Should().BeTrue();

        var offendingFiles = Directory.EnumerateFiles(reportsDir, "*.cs", SearchOption.AllDirectories)
            .Where(file => File.ReadAllLines(file)
                .Where(line => !line.TrimStart().StartsWith("//"))
                .Any(line => line.Contains(".ActivityLogs")))
            .ToList();

        offendingFiles.Should().BeEmpty(
            "the Activity Report may only read the audit trail through IActivityLogService.QueryScopedAsync — " +
            "a direct db.ActivityLogs query here would bypass 001's scoping guarantees entirely (FR-007)");
    }

    [Fact]
    public void GetActivityReportQueryHandler_ReferencesIActivityLogService()
    {
        var repoRoot = FindRepoRoot();
        var handlerFile = Path.Combine(repoRoot, "src", "ProjectManagementApp.Application", "Features", "Reports",
            "GetActivityReport", "GetActivityReportQueryHandler.cs");

        File.Exists(handlerFile).Should().BeTrue();
        var contents = File.ReadAllText(handlerFile);
        contents.Should().Contain("IActivityLogService");
        contents.Should().Contain("QueryScopedAsync");
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
