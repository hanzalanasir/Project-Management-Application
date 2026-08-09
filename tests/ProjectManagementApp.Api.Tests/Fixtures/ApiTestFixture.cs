using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Fixtures;

// Reuses the Testcontainers PostgreSQL fixture from T035 (Infrastructure.Tests) rather than
// standing up a second container — one real database per test run, shared by every Api.Tests
// class in this collection (ADR-0007 §2, research.md R-7).
public class ApiTestFixture : IAsyncLifetime
{
    private readonly PostgresFixture _postgres = new();
    private WebApplicationFactory<Program> _factory = null!;

    // Registered unconditionally (harmless in production-shaped tests) so StatelessAuthTests
    // (T082) can observe SQL command counts without any app-side wiring for the test's sake.
    public readonly CommandCounterInterceptor CommandCounter = new();

    public async Task InitializeAsync()
    {
        await _postgres.InitializeAsync();

        // Set process environment variables rather than ConfigureAppConfiguration: Program.cs's
        // own WebApplication.CreateBuilder(args) call reads AddEnvironmentVariables() reliably
        // regardless of exactly when the WebApplicationFactory hosting shim applies its hooks,
        // whereas ConfigureAppConfiguration ordering against a minimal-hosting Program.cs proved
        // unreliable — it never took precedence over this machine's own dev user-secrets.
        var overrides = new Dictionary<string, string>
        {
            ["ConnectionStrings__DefaultConnection"] = _postgres.ConnectionString,
            ["Jwt__SigningKey"] = "integration-test-signing-key-at-least-32-characters-long",
            ["Jwt__Issuer"] = "ProjectManagementApp.Tests",
            ["Jwt__Audience"] = "ProjectManagementApp.Tests",
            ["Jwt__AccessTokenMinutes"] = "15",
            ["Jwt__RefreshTokenDays"] = "7",
            ["Identity__Password__RequiredLength"] = "8",
            ["Identity__Password__RequireDigit"] = "true",
            ["Identity__Password__RequireLowercase"] = "true",
            ["Identity__Password__RequireUppercase"] = "true",
            ["Identity__Password__RequireNonAlphanumeric"] = "true",
            ["RefreshCookie__Name"] = "refresh_token",
            ["RefreshCookie__SameSite"] = "Strict",
            ["RefreshCookie__Secure"] = "false",
            ["RefreshCookie__Path"] = "/api/auth",
            // US1 (register) depends on US6 (roles/seed accounts must exist first) — the same
            // dependency chain tasks.md documents for implementation order. Seeding here mirrors
            // real startup rather than special-casing tests around a bare-roles setup.
            ["Seed__Enabled"] = "true",
            ["Seed__Admin__Email"] = "admin@example.com",
            ["Seed__Admin__Password"] = "Admin#Passw0rd!",
            ["Seed__ProjectManager__Email"] = "pm@example.com",
            ["Seed__ProjectManager__Password"] = "Manager#Passw0rd!",
            ["Seed__TeamMember__Email"] = "member@example.com",
            ["Seed__TeamMember__Password"] = "Member#Passw0rd!",
            ["Users__Paging__DefaultPageSize"] = "20",
            ["Users__Paging__MaxPageSize"] = "100",
        };

        foreach (var (key, value) in overrides)
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IInterceptor>(CommandCounter);
            });
        });
    }

    public HttpClient CreateClient() => _factory.CreateClient();

    /// <summary>
    /// A client from an independent factory with additional test-only service overrides layered on
    /// top (e.g. a different <c>IOptions</c> value) — for tests that need a configuration this
    /// fixture's shared instance doesn't provide (T050's <c>MaskOutOfScopeAs404</c> case). Built as a
    /// fresh <see cref="WebApplicationFactory{TEntryPoint}"/> (not derived from the already-started
    /// <see cref="_factory"/>) — deriving via <c>_factory.WithWebHostBuilder(...)</c> re-triggers
    /// Program.cs's minimal-hosting startup (migrate + seed) concurrently with the original host's
    /// already-running instance, and both attempting `MigrateAsync()` against the same Testcontainers
    /// database raced ("relation activity_logs already exists") when trialled directly.
    /// </summary>
    public HttpClient CreateClient(Action<IServiceCollection> configureServices)
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            // The shared _factory (or PostgresFixture directly) has already migrated this database;
            // skip the redundant migration here rather than race it (see Program.cs).
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?> { ["SkipStartupMigration"] = "true" }));
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IInterceptor>(CommandCounter);
                configureServices(services);
            });
        });
        return factory.CreateClient();
    }

    public IServiceProvider Services => _factory.Services;

    public async Task ResetAsync()
    {
        // Respawn wipes every table in the "public" schema, including the roles/seed accounts
        // that only get created once (at host startup). Re-seed immediately after each reset so
        // every test class starts from the same real-bootstrap baseline (roles + 3 seed users),
        // not an empty roles table (seeding is idempotent — safe to call repeatedly).
        await _postgres.ResetAsync();

        using var scope = Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
        await seeder.SeedAsync(CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public class ApiTestCollection : ICollectionFixture<ApiTestFixture>
{
    public const string Name = "ApiTests";
}
