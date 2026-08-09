using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Identity;
using ProjectManagementApp.Infrastructure.Persistence;
using ProjectManagementApp.Infrastructure.Services;

namespace ProjectManagementApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // The (serviceProvider, options) overload is required, not the single-arg Action<...>: EF
        // Core's "register interceptors via DI" story (used by tests' CommandCounterInterceptor,
        // T080) only resolves IInterceptor from the app container when the options delegate is
        // handed that container. Found during 002 — the single-arg overload silently discarded any
        // DI-registered interceptor, which is why the pre-existing CommandCounter-based test
        // (StatelessAuthTests) was passing on a trivial "found 0" assertion no matter what the
        // endpoint actually did. In production, GetServices<IInterceptor>() returns empty, so this
        // is a no-op there.
        services.AddDbContext<ApplicationDbContext>((sp, options) => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetServices<IInterceptor>()));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // AddIdentityCore, not AddIdentity — this API is JWT-bearer only (T039); the full AddIdentity
        // also wires a cookie authentication scheme we do not use, and requires the ASP.NET Core
        // shared framework, which a plain class library does not reference.
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = configuration.GetValue("Identity:Password:RequiredLength", 8);
                options.Password.RequireDigit = configuration.GetValue("Identity:Password:RequireDigit", true);
                options.Password.RequireLowercase = configuration.GetValue("Identity:Password:RequireLowercase", true);
                options.Password.RequireUppercase = configuration.GetValue("Identity:Password:RequireUppercase", true);
                options.Password.RequireNonAlphanumeric = configuration.GetValue("Identity:Password:RequireNonAlphanumeric", true);

                options.Lockout.MaxFailedAccessAttempts = configuration.GetValue("Identity:Lockout:MaxFailedAccessAttempts", 5);
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
                    configuration.GetValue("Identity:Lockout:DefaultLockoutMinutes", 15));
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IActivityLogService, ActivityLogService>();

        services.Configure<SeedOptions>(configuration.GetSection("Seed"));
        services.AddScoped<IDataSeeder, DataSeeder>();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
