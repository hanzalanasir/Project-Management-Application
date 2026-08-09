using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Users;

[Collection(ApiTestCollection.Name)]
public class ListUsersEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ListUsersEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ListUsers_AsAdmin_SeesAllSeededUsers_IncludingAnyDeactivated()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("totalCount").GetInt32().Should().Be(3);
        var emails = body.GetProperty("items").EnumerateArray().Select(e => e.GetProperty("email").GetString()).ToList();
        emails.Should().Contain(new[] { AdminEmail, ProjectManagerEmail, TeamMemberEmail });
    }

    [Fact]
    public async Task ListUsers_AsNonAdmin_Returns403()
    {
        var client = _fixture.CreateClient();
        var memberToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
