using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Auth;

[Collection(ApiTestCollection.Name)]
public class LoginEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public LoginEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);
    private sealed record LoginRequest(string Email, string Password);

    [Fact]
    public async Task Login_WithValidCredentials_Returns200_WithJwtClaims_SetCookie_AndNoRefreshTokenInBody()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Dana Rivera", "dana-login@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("dana-login@example.com", "S3cure-P@ss!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("refreshToken").And.NotContain("RefreshToken");

        var json = System.Text.Json.JsonDocument.Parse(body).RootElement;
        var accessToken = json.GetProperty("accessToken").GetString();
        accessToken.Should().NotBeNullOrEmpty();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "dana-login@example.com");
        jwt.Claims.Where(c => c.Type == "role").Should().ContainSingle().Which.Value.Should().Be("TeamMember");

        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        var cookie = cookies!.Single(c => c.StartsWith("refresh_token="));
        var lowerCookie = cookie.ToLowerInvariant();
        lowerCookie.Should().Contain("httponly").And.Contain("samesite=strict").And.Contain("path=/api/auth");
    }
}
