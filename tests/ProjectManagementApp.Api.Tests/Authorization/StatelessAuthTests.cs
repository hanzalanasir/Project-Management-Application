using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Authorization;

// NFR-002: authorization on a protected read costs zero database round-trips (stateless JWT
// validation). GetCurrentUserQueryHandler must project UserDto directly from
// ICurrentUserService's token-derived claims, never from IApplicationDbContext
// (/speckit.analyze finding E1 — this claim previously had no verifying task, T082).
[Collection(ApiTestCollection.Name)]
public class StatelessAuthTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public StatelessAuthTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);
    private sealed record LoginRequest(string Email, string Password);

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ExecutesZeroSqlStatements()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Stateless User", "stateless@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("stateless@example.com", "S3cure-P@ss!"));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginBody.GetProperty("accessToken").GetString();

        _fixture.CommandCounter.Reset();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        _fixture.CommandCounter.Count.Should().Be(0, "GET /api/auth/me must resolve identity from the token alone — zero SQL statements (NFR-002)");
    }
}
