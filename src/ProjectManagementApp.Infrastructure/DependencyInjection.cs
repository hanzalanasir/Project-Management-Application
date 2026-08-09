using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

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
