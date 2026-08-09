namespace ProjectManagementApp.Infrastructure.Identity;

// Placed here (not Api/Configuration, despite T069's original path) because TokenService — the
// actual consumer — lives in Infrastructure, and Infrastructure cannot depend on Api (Clean
// Architecture's inward dependency rule, Constitution II.2). Program.cs's own fail-fast checks
// (T039) read Jwt:SigningKey/connection string directly via IConfiguration and do not need this
// strongly-typed class; this is what TokenService binds against.
public class JwtOptions
{
    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
