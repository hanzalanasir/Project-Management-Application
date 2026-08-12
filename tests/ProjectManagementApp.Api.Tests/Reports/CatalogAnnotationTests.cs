using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T025: descriptors are role-annotated — a TeamMember must see Team Performance marked "self only"
// (spec's least-privilege rule made visible before the request is even made), and the catalog
// itself exposes no project or task data (it is metadata about the API, not a query over it).
[Collection(ApiTestCollection.Name)]
public class CatalogAnnotationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public CatalogAnnotationTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Catalog_TeamMember_SeesTeamPerformanceAnnotated_SelfOnly()
    {
        var client = _fixture.CreateClient();
        var token = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);

        var body = await GetCatalogAsync(client, token);
        var teamPerformance = body.EnumerateArray().Single(d => d.GetProperty("type").GetString() == "TeamPerformance");

        teamPerformance.GetProperty("note").GetString().Should().Be("self only");
    }

    [Fact]
    public async Task Catalog_AdminAndProjectManager_SeeNoNoteOnTeamPerformance()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        foreach (var token in new[] { adminToken, pmToken })
        {
            var body = await GetCatalogAsync(client, token);
            var teamPerformance = body.EnumerateArray().Single(d => d.GetProperty("type").GetString() == "TeamPerformance");
            var note = teamPerformance.GetProperty("note");
            (note.ValueKind == System.Text.Json.JsonValueKind.Null || note.GetString() is null)
                .Should().BeTrue("only a TeamMember's Team Performance descriptor is annotated");
        }
    }

    [Fact]
    public async Task Catalog_ExposesNoProjectOrTaskData()
    {
        var client = _fixture.CreateClient();
        var token = await LoginAsync(client, AdminEmail, AdminPassword);

        var body = await GetCatalogAsync(client, token);
        foreach (var descriptor in body.EnumerateArray())
        {
            var propertyNames = descriptor.EnumerateObject().Select(p => p.Name).ToList();
            propertyNames.Should().BeEquivalentTo(["type", "title", "note", "parameters", "formats"]);
        }
    }
}
