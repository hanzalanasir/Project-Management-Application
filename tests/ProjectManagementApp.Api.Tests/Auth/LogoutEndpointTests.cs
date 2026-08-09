using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;

namespace ProjectManagementApp.Api.Tests.Auth;

[Collection(ApiTestCollection.Name)]
public class LogoutEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public LogoutEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);
    private sealed record LoginRequest(string Email, string Password);

    [Fact]
    public async Task Logout_Returns204_RevokesToken_ClearsCookie_AndAudits()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Logout User", "logout-test@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("logout-test@example.com", "S3cure-P@ss!"));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginBody.GetProperty("accessToken").GetString();
        var xsrfToken = CsrfTestHelper.ExtractXsrfToken(loginResponse);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout").WithCsrfHeader(xsrfToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies!.Should().Contain(c => c.StartsWith("refresh_token=") && c.Contains("expires="));

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == "logout-test@example.com");
        var tokens = await db.RefreshTokens.Where(t => t.UserId == user.Id).ToListAsync();
        tokens.Should().OnlyContain(t => t.RevokedAt != null);
    }
}
