using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T036 — exactly one ReportGenerated row per successful request, and zero when the request 400s or
// 403s (nothing was actually generated). The pattern T081 (stage 5) asserts across all four reports
// at once; this is its first proof.
[Collection(ApiTestCollection.Name)]
public class ProjectProgressAuditTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ProjectProgressAuditTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private async Task<int> ReportGeneratedCountAsync()
    {
        await using var db = _fixture.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Database
            .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM activity_logs WHERE action = 'ReportGenerated'")
            .SingleAsync();
    }

    [Fact]
    public async Task ProjectProgress_SuccessfulRequest_WritesExactlyOneReportGeneratedRow()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "audit-success");

        var before = await ReportGeneratedCountAsync();
        (await GetProjectProgressResponseAsync(client, scenario.AdminToken, DefaultWindowQuery())).EnsureSuccessStatusCode();
        var after = await ReportGeneratedCountAsync();

        after.Should().Be(before + 1);
    }

    [Fact]
    public async Task ProjectProgress_ForbiddenRequest_WritesNoAuditRow()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "audit-403");

        var before = await ReportGeneratedCountAsync();
        var response = await GetProjectProgressResponseAsync(
            client, scenario.PmToken, $"{DefaultWindowQuery()}&projectScope={scenario.ProjectBId}");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        var after = await ReportGeneratedCountAsync();

        after.Should().Be(before, "nothing was generated — a 403 must not audit");
    }

    [Fact]
    public async Task ProjectProgress_ValidationFailure_WritesNoAuditRow()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "audit-400");

        var before = await ReportGeneratedCountAsync();
        // from > to — a validation failure, never reaches the handler's audit call.
        var response = await GetProjectProgressResponseAsync(client, scenario.AdminToken, "?from=2026-08-10&to=2026-08-01");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var after = await ReportGeneratedCountAsync();

        after.Should().Be(before, "nothing was generated — a 400 must not audit");
    }
}
