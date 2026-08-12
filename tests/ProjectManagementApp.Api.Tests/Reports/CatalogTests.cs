using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T024: the catalog is the frontend's single source of truth for "what reports exist" — exactly
// four descriptors (OQ-006-06), each fully self-describing so the picker (T030) needs no
// per-report form code.
[Collection(ApiTestCollection.Name)]
public class CatalogTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public CatalogTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Catalog_ReturnsExactlyFourDescriptors_PlainArray_NotPaged()
    {
        var client = _fixture.CreateClient();
        var token = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var response = await GetCatalogResponseAsync(client, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await GetCatalogAsync(client, token);
        body.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Array, "the body is a plain array — nothing to page");
        body.GetArrayLength().Should().Be(4);

        var types = body.EnumerateArray().Select(d => d.GetProperty("type").GetString()).ToList();
        types.Should().BeEquivalentTo(["ProjectProgress", "TaskCompletion", "TeamPerformance", "Activity"]);
    }

    [Fact]
    public async Task Catalog_EachDescriptor_ListsOrderedParametersWithRequiredFlagsAndAllThreeFormats()
    {
        var client = _fixture.CreateClient();
        var token = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var body = await GetCatalogAsync(client, token);

        foreach (var descriptor in body.EnumerateArray())
        {
            var formats = descriptor.GetProperty("formats").EnumerateArray().Select(f => f.GetString()).ToList();
            formats.Should().BeEquivalentTo(["json", "pdf", "csv"]);

            var parameters = descriptor.GetProperty("parameters").EnumerateArray().ToList();
            parameters.Should().NotBeEmpty();
            foreach (var parameter in parameters)
            {
                parameter.GetProperty("name").GetString().Should().NotBeNullOrEmpty();
                parameter.GetProperty("type").GetString().Should().NotBeNullOrEmpty();
                parameter.TryGetProperty("required", out _).Should().BeTrue();
            }

            // "from"/"to" are always required, always first — every report shares this shape.
            parameters[0].GetProperty("name").GetString().Should().Be("from");
            parameters[0].GetProperty("required").GetBoolean().Should().BeTrue();
            parameters[1].GetProperty("name").GetString().Should().Be("to");
            parameters[1].GetProperty("required").GetBoolean().Should().BeTrue();
        }
    }

    [Fact]
    public async Task Catalog_RequiresAuthentication_401()
    {
        var client = _fixture.CreateClient();
        var response = await GetCatalogResponseAsync(client, "not-a-real-token");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
