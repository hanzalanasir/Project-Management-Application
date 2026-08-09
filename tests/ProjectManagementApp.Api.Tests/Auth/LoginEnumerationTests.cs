using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Auth;

[Collection(ApiTestCollection.Name)]
public class LoginEnumerationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public LoginEnumerationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);
    private sealed record LoginRequest(string Email, string Password);

    [Fact]
    public async Task Login_WrongPasswordAndUnknownEmail_ReturnIdentical401Bodies()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Known User", "known@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));

        var wrongPasswordResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("known@example.com", "TotallyWrong1!"));
        var unknownEmailResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("nobody-at-all@example.com", "Whatever1!"));

        wrongPasswordResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        unknownEmailResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var wrongPasswordBody = await wrongPasswordResponse.Content.ReadAsStringAsync();
        var unknownEmailBody = await unknownEmailResponse.Content.ReadAsStringAsync();
        wrongPasswordBody.Should().Be(unknownEmailBody, "no user enumeration — the two failure modes must be byte-identical");
    }
}
