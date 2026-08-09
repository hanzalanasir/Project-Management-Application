namespace ProjectManagementApp.Api.Configuration;

public class RefreshCookieOptions
{
    public string Name { get; set; } = "refresh_token";
    public string SameSite { get; set; } = "Strict";
    public bool Secure { get; set; } = true;
    public string Path { get; set; } = "/api/auth";
}
