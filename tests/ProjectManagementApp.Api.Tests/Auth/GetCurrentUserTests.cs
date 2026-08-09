using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Auth;

[Collection(ApiTestCollection.Name)]
public class GetCurrentUserTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public GetCurrentUserTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);
    private sealed record LoginRequest(string Email, string Password);

    [Fact]
    public async Task GetCurrentUser_WithValidToken_Returns200_WithTokenIdentity()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Me Test", "me-test@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("me-test@example.com", "S3cure-P@ss!"));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginBody.GetProperty("accessToken").GetString();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("email").GetString().Should().Be("me-test@example.com");
        body.GetProperty("role").GetString().Should().Be("TeamMember");
        body.GetProperty("fullName").GetString().Should().Be("Me Test");
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_Returns401()
    {
        var client = _fixture.CreateClient();
        var response = await client.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
