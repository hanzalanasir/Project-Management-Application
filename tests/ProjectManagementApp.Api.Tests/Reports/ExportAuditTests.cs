using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T076 — the other 🎯 test (DoD 9, FR-011): fetch a report once, then "export" that SAME previewed
// data to both PDF and CSV. The ReportGenerated count must increase by exactly ONE (from the
// original fetch), never three. Export is proven client-side-only and network-free by T075
// (export-network.spec.ts, spying on HttpClient) — there is no server call left for a PDF/CSV
// export to make, so this test's job is to nail down the ONE number that call produces, and
// document why a second/third increment is structurally impossible from the frontend's side.
[Collection(ApiTestCollection.Name)]
public class ExportAuditTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ExportAuditTests(ApiTestFixture fixture) { _fixture = fixture; }

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
    public async Task OneFetch_ThenExportingItToBothFormats_IncreasesTheCountByExactlyOne_NeverThree()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var before = await ReportGeneratedCountAsync();

        (await GetProjectProgressResponseAsync(client, pmToken, DefaultWindowQuery())).EnsureSuccessStatusCode();

        // "Export to PDF" and "Export to CSV" happen entirely in the browser (ReportExportService,
        // T078) over the JSON the fetch above already returned — there is no second or third HTTP
        // call for either action to make, which T075 proves directly against the export service.
        // Nothing further happens here at the API.

        var after = await ReportGeneratedCountAsync();

        after.Should().Be(before + 1, "one data request generates one audit row; exporting the same previewed data to PDF and CSV must not add any more");
    }
}
