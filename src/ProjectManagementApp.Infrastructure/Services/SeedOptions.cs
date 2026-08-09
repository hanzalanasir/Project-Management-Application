namespace ProjectManagementApp.Infrastructure.Services;

public class SeedAccountOptions
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Bound from Seed:Enabled and Seed:{Admin,ProjectManager,TeamMember}:{Email,Password}.
// Credentials resolve from configuration only (user-secrets in dev / environment in prod) —
// never hardcoded (Constitution V.4, quickstart V11).
public class SeedOptions
{
    public bool Enabled { get; set; }
    public SeedAccountOptions Admin { get; set; } = new();
    public SeedAccountOptions ProjectManager { get; set; } = new();
    public SeedAccountOptions TeamMember { get; set; } = new();

    // Separate from Enabled (T079): the three accounts above are load-bearing for every test that
    // logs in, so Enabled stays on in ApiTestFixture. Demo PROJECTS are dev-convenience only and
    // would otherwise pollute every Api.Tests count/scope assertion made against the seeded
    // ProjectManager (found running the full 002 regression after adding demo seeding) — default
    // off, and ApiTestFixture deliberately never sets this.
    public bool DemoDataEnabled { get; set; }
}
