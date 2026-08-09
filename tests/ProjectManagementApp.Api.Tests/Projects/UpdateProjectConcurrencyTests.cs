using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Projects;

// The second 🎯 non-negotiable test (tasks.md warning) — the second assertion (the stale write did
// NOT land) is the real one; a test that only checks the status code would pass against a silent
// last-write-wins (data-model.md §6.4, ADR-0004).
[Collection(ApiTestCollection.Name)]
public class UpdateProjectConcurrencyTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public UpdateProjectConcurrencyTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task UpdateProject_FirstUpdateWithCurrentETag_Succeeds_ThenReplayingTheStaleETag_Returns409_AndTheStaleWriteDoesNotLand()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (id, etag) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Original", null, "2026-08-01", null, "Planning", null));

        var firstResponse = await PutProjectAsync(client, pmToken, id, etag, new UpdateProjectRequest("Updated Once", null, "2026-08-01", null, "Active", null));
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newETag = firstResponse.Headers.ETag!.Tag;
        newETag.Should().NotBe(etag);

        var replayResponse = await PutProjectAsync(client, pmToken, id, etag, new UpdateProjectRequest("Clobbered", null, "2026-08-01", null, "Active", null));
        replayResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{id}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var getResponse = await client.SendAsync(getRequest);
        var body = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Updated Once");
    }

    [Fact]
    public async Task UpdateProject_MissingIfMatch_Returns400_NotALastWriteWinsSuccess()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (id, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("No IfMatch", null, "2026-08-01", null, "Planning", null));

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/projects/{id}")
        {
            Content = JsonContent.Create(new UpdateProjectRequest("Should Not Apply", null, "2026-08-01", null, "Active", null)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
