using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Auth;

[Collection(ApiTestCollection.Name)]
public class CsrfProtectionTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public CsrfProtectionTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);
    private sealed record LoginRequest(string Email, string Password);

    [Fact]
    public async Task Refresh_WithoutXsrfHeader_Returns400()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Csrf Missing", "csrf-missing@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("csrf-missing@example.com", "S3cure-P@ss!"));

        // No X-XSRF-TOKEN header attached, even though the client's cookie jar has the cookie.
        var response = await client.PostAsync("/api/auth/refresh", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_WithMismatchedXsrfHeader_Returns400()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Csrf Mismatch", "csrf-mismatch@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("csrf-mismatch@example.com", "S3cure-P@ss!"));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginBody.GetProperty("accessToken").GetString();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-XSRF-TOKEN", "not-the-real-token");
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
