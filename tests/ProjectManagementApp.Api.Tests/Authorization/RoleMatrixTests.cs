using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Authorization;

[Collection(ApiTestCollection.Name)]
public class RoleMatrixTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public RoleMatrixTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record LoginRequest(string Email, string Password);

    private static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    [Fact]
    public async Task AdminProbe_NoToken_Returns401()
    {
        var client = _fixture.CreateClient();
        var response = await client.GetAsync("/api/admin-probe");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminProbe_TeamMemberToken_Returns403()
    {
        var client = _fixture.CreateClient();
        var token = await LoginAsync(client, "member@example.com", "Member#Passw0rd!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin-probe");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminProbe_ProjectManagerToken_Returns403()
    {
        var client = _fixture.CreateClient();
        var token = await LoginAsync(client, "pm@example.com", "Manager#Passw0rd!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin-probe");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminProbe_AdminToken_Returns200()
    {
        var client = _fixture.CreateClient();
        var token = await LoginAsync(client, "admin@example.com", "Admin#Passw0rd!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin-probe");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
