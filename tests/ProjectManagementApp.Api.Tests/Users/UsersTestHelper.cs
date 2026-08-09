using System.Net.Http.Json;
using System.Text.Json;

namespace ProjectManagementApp.Api.Tests.Users;

public static class UsersTestHelper
{
    public sealed record LoginRequest(string Email, string Password);
    public sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);

    public const string AdminEmail = "admin@example.com";
    public const string AdminPassword = "Admin#Passw0rd!";
    public const string ProjectManagerEmail = "pm@example.com";
    public const string ProjectManagerPassword = "Manager#Passw0rd!";
    public const string TeamMemberEmail = "member@example.com";
    public const string TeamMemberPassword = "Member#Passw0rd!";

    public static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    public static async Task<Guid> RegisterAndGetIdAsync(HttpClient client, string fullName, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(fullName, email, password, password));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return Guid.Parse(body.GetProperty("id").GetString()!);
    }
}
