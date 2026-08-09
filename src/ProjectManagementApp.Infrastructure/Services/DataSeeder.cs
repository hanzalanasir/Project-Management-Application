using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Infrastructure.Services;

// US-001-06 — invoked once at boot (Program.cs), after migrations apply. Idempotent: existence
// checks plus the unique indexes on normalized_email/normalized_name are the real guarantee for
// two instances racing at startup (research.md R-9) — the check here is the optimization.
public class DataSeeder : IDataSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IActivityLogService _activityLog;
    private readonly IApplicationDbContext _db;
    private readonly SeedOptions _options;

    public DataSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IActivityLogService activityLog,
        IApplicationDbContext db,
        IOptions<SeedOptions> options)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _activityLog = activityLog;
        _db = db;
        _options = options.Value;
    }

    // Demo project seed (T079) — checked by name before insert, same idempotency shape as
    // EnsureUserAsync/EnsureRoleAsync. Only the projects half; demo TASKS arrive with 003
    // (spec 002 Assumptions).
    private static readonly (string Name, string? Description, ProjectStatus Status)[] DemoProjectSeeds =
    [
        ("Apollo Rollout", "Regional launch of the Apollo product line.", ProjectStatus.Active),
        ("Griffin Migration", "Legacy platform migration to the new stack.", ProjectStatus.Planning),
        ("Harbor Refresh", "UX refresh for the customer harbor portal.", ProjectStatus.OnHold),
    ];

    public async Task SeedAsync(CancellationToken ct)
    {
        await EnsureRoleAsync(Role.Admin.ToString());
        await EnsureRoleAsync(Role.ProjectManager.ToString());
        await EnsureRoleAsync(Role.TeamMember.ToString());

        await EnsureUserAsync(_options.Admin, Role.Admin, ct);
        var projectManager = await EnsureUserAsync(_options.ProjectManager, Role.ProjectManager, ct);
        await EnsureUserAsync(_options.TeamMember, Role.TeamMember, ct);

        if (_options.DemoDataEnabled)
        {
            await EnsureDemoProjectsAsync(projectManager, ct);
        }
    }

    private async Task EnsureDemoProjectsAsync(ApplicationUser owner, CancellationToken ct)
    {
        foreach (var (name, description, status) in DemoProjectSeeds)
        {
            var exists = await _db.Projects.AnyAsync(p => p.Name == name, ct);
            if (exists)
            {
                continue;
            }

            var now = DateTimeOffset.UtcNow;
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                StartDate = DateOnly.FromDateTime(now.UtcDateTime),
                Status = status,
                OwnerId = owner.Id,
                CreatedAt = now,
                UpdatedAt = now,
            };

            _db.Projects.Add(project);

            await _activityLog.LogAsync(
                actorId: null,
                action: nameof(AuditAction.ProjectCreated),
                entityType: "Project",
                entityId: project.Id.ToString(),
                changeSummary: $"Seeded demo project '{name}', owned by {owner.Email}",
                ct);
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
    }

    // Returns the account whether it already existed or was just created — T079's demo project
    // seed needs a real ProjectManager to own the demo projects, on both the fresh-DB and
    // already-seeded-instance startup paths.
    private async Task<ApplicationUser> EnsureUserAsync(SeedAccountOptions account, Role role, CancellationToken ct)
    {
        var existing = await _userManager.FindByEmailAsync(account.Email);
        if (existing is not null)
        {
            return existing;
        }

        var now = DateTimeOffset.UtcNow;
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = account.Email,
            Email = account.Email,
            EmailConfirmed = true,
            FullName = $"Seed {role}",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var createResult = await _userManager.CreateAsync(user, account.Password);
        if (!createResult.Succeeded)
        {
            // A concurrent instance may have just won the race on the unique index — re-check
            // rather than throwing, so a losing instance's startup does not fail (research R-9).
            var winner = await _userManager.FindByEmailAsync(account.Email);
            if (winner is not null)
            {
                return winner;
            }

            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed {role} account: {errors}");
        }

        await _userManager.AddToRoleAsync(user, role.ToString());

        await _activityLog.LogAsync(
            actorId: null,
            action: nameof(AuditAction.UserSeeded),
            entityType: "User",
            entityId: user.Id.ToString(),
            changeSummary: $"Seeded {role} account {account.Email}",
            ct);
        await _db.SaveChangesAsync(ct);

        return user;
    }
}
