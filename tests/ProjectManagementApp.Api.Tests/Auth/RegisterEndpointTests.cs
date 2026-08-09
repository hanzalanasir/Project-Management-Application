using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Auth;

[Collection(ApiTestCollection.Name)]
public class RegisterEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public RegisterEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);

    [Fact]
    public async Task Register_WithValidData_Returns201_WithLocation_AndNoPasswordField()
    {
        var client = _fixture.CreateClient();
        var request = new RegisterRequest("Dana Rivera", "dana@example.com", "S3cure-P@ss!", "S3cure-P@ss!");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("password", "response must never include a password/passwordHash field")
            .And.NotContain("Password");

        var json = System.Text.Json.JsonDocument.Parse(body).RootElement;
        json.GetProperty("email").GetString().Should().Be("dana@example.com");
        json.GetProperty("role").GetString().Should().Be("TeamMember");
        json.GetProperty("fullName").GetString().Should().Be("Dana Rivera");
    }
}
