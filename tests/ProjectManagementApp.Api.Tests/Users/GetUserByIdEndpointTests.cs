using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Users;

[Collection(ApiTestCollection.Name)]
public class GetUserByIdEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public GetUserByIdEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task GetUserById_KnownId_Returns200_WithETagHeader()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var targetId = await RegisterAndGetIdAsync(client, "Detail Target", "detail-target@example.com", "S3cure-P@ss!");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{targetId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.ETag.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("email").GetString().Should().Be("detail-target@example.com");
    }

    [Fact]
    public async Task GetUserById_UnknownId_Returns404()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
