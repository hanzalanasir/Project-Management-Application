using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManagementApp.Api.Common;
using ProjectManagementApp.Api.Services;
using ProjectManagementApp.Application;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Infrastructure;
using ProjectManagementApp.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

// Fail fast at startup if required secrets are absent (Constitution V.4).
var signingKey = builder.Configuration["Jwt:SigningKey"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(signingKey))
{
    throw new InvalidOperationException(
        "Jwt:SigningKey is not configured. Set it via user-secrets (development) or an environment variable (production).");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not configured. Set it via user-secrets (development) or an environment variable (production).");
}

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Without this, Swashbuckle ignores C# nullable-reference-type annotations entirely and marks
    // every reference-typed property nullable/optional — which drifted the generated spec from the
    // contract's required/non-nullable fields (found running the T140 contract gate for real).
    options.SupportNonNullableReferenceTypes();
    options.NonNullableReferenceTypesAsRequired();
    options.SchemaFilter<ChangeUserRoleRequestSchemaFilter>();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.Configure<ProjectManagementApp.Api.Configuration.RefreshCookieOptions>(builder.Configuration.GetSection("RefreshCookie"));

// CSRF protection for the cookie-authenticated /refresh and /logout endpoints (FR-016) — see
// Common/CsrfProtection.cs for why this is a custom double-submit-cookie check rather than
// ASP.NET Core's IAntiforgery.
builder.Services.Configure<ProjectManagementApp.Api.Common.CsrfOptions>(options =>
{
    options.HeaderName = builder.Configuration["Csrf:HeaderName"] ?? "X-XSRF-TOKEN";
    options.Secure = builder.Configuration.GetValue("RefreshCookie:Secure", true);
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Disable the default inbound claim mapping so claim types on HttpContext.User are
        // exactly what TokenService wrote ("sub", "email", "role") — not remapped to long
        // ClaimTypes.* URIs. RoleClaimType/NameClaimType then tell [Authorize(Roles=...)] and
        // ClaimsPrincipal.IsInRole() which claim to actually check.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "role",
            NameClaimType = "sub"
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Authenticated by default (Constitution V.1) — only [AllowAnonymous] endpoints opt out:
    // register, login, refresh, health.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    });
});

var app = builder.Build();

// Migrate then seed (in that order) inside a startup scope, gated by Seed:Enabled
// (default on in Development, off in Production — research.md R-9).
using (var startupScope = app.Services.CreateScope())
{
    var db = startupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    if (app.Configuration.GetValue("Seed:Enabled", false))
    {
        var seeder = startupScope.ServiceProvider.GetRequiredService<IDataSeeder>();
        await seeder.SeedAsync(CancellationToken.None);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Same-origin: the API serves the Angular production bundle from wwwroot/ (ADR-0002) —
// this is what lets the refresh cookie use SameSite=Strict in every environment.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SPA client-side routing fallback. Explicitly anonymous — otherwise the global FallbackPolicy
// (RequireAuthenticatedUser) would 401 an unauthenticated deep link like /auth/login instead of
// serving the Angular shell, which then handles auth redirects itself client-side. Unmatched
// /api/* requests still 404 instead of silently returning index.html.
app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
}).AllowAnonymous();

app.Run();

public partial class Program
{
    // Exposed for WebApplicationFactory<Program> in integration tests.
}
