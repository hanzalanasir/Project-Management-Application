using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Persistence;
using ProjectManagementApp.Infrastructure.Services;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Seeding;

// Builds a real DataSeeder wired to the Testcontainers PostgreSQL fixture, with a minimal
// service provider (Identity stores + ActivityLogService) — no HTTP host required.
public static class SeederTestHarness
{
    public const string AdminPassword = "Admin#Passw0rd!";
    public const string ProjectManagerPassword = "Manager#Passw0rd!";
    public const string TeamMemberPassword = "Member#Passw0rd!";

    public static DataSeeder Create(PostgresFixture fixture, out ApplicationDbContext db, bool demoDataEnabled = false)
    {
        db = fixture.CreateDbContext();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(db);
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        services.AddScoped<IActivityLogService, ActivityLogService>();

        var provider = services.BuildServiceProvider();

        var seedOptions = Options.Create(new SeedOptions
        {
            Enabled = true,
            DemoDataEnabled = demoDataEnabled,
            Admin = new SeedAccountOptions { Email = "admin@example.com", Password = AdminPassword },
            ProjectManager = new SeedAccountOptions { Email = "pm@example.com", Password = ProjectManagerPassword },
            TeamMember = new SeedAccountOptions { Email = "member@example.com", Password = TeamMemberPassword }
        });

        return new DataSeeder(
            provider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>(),
            provider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<ApplicationRole>>(),
            provider.GetRequiredService<IActivityLogService>(),
            db,
            seedOptions);
    }
}
