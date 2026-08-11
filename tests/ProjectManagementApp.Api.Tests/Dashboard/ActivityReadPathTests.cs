using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// The 🎯 service-not-table test (T031, FR-006, DoD 6). Two complementary halves — a source scan
// proves it structurally (a coincidentally-correct direct query on a small test dataset couldn't
// hide from this), and a functional check proves the endpoint actually returns activity that only
// IActivityLogService.QueryScopedAsync could have produced correctly scoped.
[Collection(ApiTestCollection.Name)]
public class ActivityReadPathTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ActivityReadPathTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public void NoSourceFileUnderFeaturesDashboard_QueriesActivityLogsDirectly()
    {
        var repoRoot = FindRepoRoot();
        var dashboardDir = Path.Combine(repoRoot, "src", "ProjectManagementApp.Application", "Features", "Dashboard");
        Directory.Exists(dashboardDir).Should().BeTrue();

        // The only legal way to read the audit trail is IActivityLogService.QueryScopedAsync — a
        // literal ".ActivityLogs" reference in actual code (not a doc-comment mentioning it)
        // anywhere under Features/Dashboard/ would mean a direct LINQ query against
        // db.ActivityLogs, the forbidden path (FR-006, ADR-0006 addendum). Comment lines are
        // excluded so a doc-comment explaining the rule doesn't trip the rule itself.
        var offendingFiles = Directory.EnumerateFiles(dashboardDir, "*.cs", SearchOption.AllDirectories)
            .Where(file => File.ReadAllLines(file)
                .Where(line => !line.TrimStart().StartsWith("//"))
                .Any(line => line.Contains(".ActivityLogs")))
            .ToList();

        offendingFiles.Should().BeEmpty(
            "no file under Features/Dashboard/ may query db.ActivityLogs directly — read through IActivityLogService.QueryScopedAsync only");
    }

    [Fact]
    public void GetDashboardActivityQueryHandlerSource_CallsQueryScopedAsync()
    {
        var repoRoot = FindRepoRoot();
        var handlerFile = Path.Combine(repoRoot, "src", "ProjectManagementApp.Application", "Features", "Dashboard",
            "GetActivity", "GetDashboardActivityQueryHandler.cs");

        File.Exists(handlerFile).Should().BeTrue();
        File.ReadAllText(handlerFile).Should().Contain("QueryScopedAsync",
            "the handler must read through IActivityLogService.QueryScopedAsync, not reimplement scoping itself");
    }

    [Fact]
    public async Task Activity_ReturnsRowsThatOnlyTheScopedServiceCouldHaveProduced()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "read-path");

        // Functional confirmation the wiring actually works end-to-end: PM2 (owns project B only)
        // sees exactly the one row for project B's own creation — never any of project A's rows,
        // which only a correctly-scoped read (not a raw unscoped table query) would guarantee.
        var activity = await GetActivityAsync(client, scenario.Pm2Token, "?pageSize=100");
        activity.GetProperty("totalCount").GetInt32().Should().Be(1);
        activity.GetProperty("items")[0].GetProperty("entityType").GetString().Should().Be("Project");
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
