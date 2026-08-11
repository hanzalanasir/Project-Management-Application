using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Controllers;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// T049 — the highest-value test in this feature. 005's whole premise is "read-only aggregation";
// this test turns that claim into a fact rather than a comment. Four independent proofs:
//   1. activity_logs row count is unchanged after exercising both endpoints as every role.
//   2. the seeded task/project rows' xmin (Postgres's own row-version) is unchanged — nothing was
//      even re-saved with identical values, which a naive SaveChangesAsync() no-op could still do.
//   3. DashboardController declares no HttpPost/HttpPut/HttpDelete/HttpPatch action anywhere.
//   4. CanMutateAsync (001/002's mutation-authorization hook) is referenced nowhere under
//      Features/Dashboard/ — there is nothing here to authorize a mutation for.
[Collection(ApiTestCollection.Name)]
public class NoWriteGuaranteeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public NoWriteGuaranteeTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ExercisingBothEndpoints_AsEveryRole_WritesNothing()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "no-write");

        await using var db = _fixture.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var activityCountBefore = await db.Database
            .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM activity_logs")
            .SingleAsync();
        var taskXminBefore = await db.Database
            .SqlQuery<string>($"SELECT xmin::text AS \"Value\" FROM tasks WHERE id = {Guid.Parse(scenario.TaId)}")
            .SingleAsync();
        var projectXminBefore = await db.Database
            .SqlQuery<string>($"SELECT xmin::text AS \"Value\" FROM projects WHERE id = {scenario.ProjectAId}")
            .SingleAsync();

        var tokens = new[] { scenario.AdminToken, scenario.PmToken, scenario.Pm2Token, scenario.TmToken, scenario.Tm2Token };
        foreach (var token in tokens)
        {
            for (var i = 0; i < 3; i++)
            {
                (await GetSummaryResponseAsync(client, token)).EnsureSuccessStatusCode();
                (await GetActivityResponseAsync(client, token, "?page=1&pageSize=50")).EnsureSuccessStatusCode();
            }
        }

        var activityCountAfter = await db.Database
            .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM activity_logs")
            .SingleAsync();
        var taskXminAfter = await db.Database
            .SqlQuery<string>($"SELECT xmin::text AS \"Value\" FROM tasks WHERE id = {Guid.Parse(scenario.TaId)}")
            .SingleAsync();
        var projectXminAfter = await db.Database
            .SqlQuery<string>($"SELECT xmin::text AS \"Value\" FROM projects WHERE id = {scenario.ProjectAId}")
            .SingleAsync();

        activityCountAfter.Should().Be(activityCountBefore,
            "reading the dashboard must never itself produce an activity_logs row");
        taskXminAfter.Should().Be(taskXminBefore, "no domain row may be re-saved, even with identical values");
        projectXminAfter.Should().Be(projectXminBefore, "no domain row may be re-saved, even with identical values");
    }

    [Fact]
    public void DashboardController_DeclaresNoMutationVerbs()
    {
        var mutationAttributes = new[] { typeof(HttpPostAttribute), typeof(HttpPutAttribute), typeof(HttpDeleteAttribute), typeof(HttpPatchAttribute) };

        var offendingMethods = typeof(DashboardController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttributes(inherit: true).Any(a => mutationAttributes.Contains(a.GetType())))
            .Select(m => m.Name)
            .ToList();

        offendingMethods.Should().BeEmpty("no POST/PUT/DELETE/PATCH route may exist under /api/dashboard — the feature is strictly read-only");
    }

    [Fact]
    public void NoSourceFileUnderFeaturesDashboard_ReferencesCanMutateAsync()
    {
        var repoRoot = FindRepoRoot();
        var dashboardDir = Path.Combine(repoRoot, "src", "ProjectManagementApp.Application", "Features", "Dashboard");
        Directory.Exists(dashboardDir).Should().BeTrue();

        var offendingFiles = Directory.EnumerateFiles(dashboardDir, "*.cs", SearchOption.AllDirectories)
            .Where(file => File.ReadAllLines(file)
                .Where(line => !line.TrimStart().StartsWith("//"))
                .Any(line => line.Contains("CanMutateAsync")))
            .ToList();

        offendingFiles.Should().BeEmpty(
            "CanMutateAsync authorizes mutations — 005 performs none, so nothing here should ever call it");
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
