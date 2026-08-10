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
    // EnsureUserAsync/EnsureRoleAsync.
    private static readonly (string Name, string? Description, ProjectStatus Status)[] DemoProjectSeeds =
    [
        ("Apollo Rollout", "Regional launch of the Apollo product line.", ProjectStatus.Active),
        ("Griffin Migration", "Legacy platform migration to the new stack.", ProjectStatus.Planning),
        ("Harbor Refresh", "UX refresh for the customer harbor portal.", ProjectStatus.OnHold),
    ];

    // Demo task seed (T100, 003) — the tasks half of IV.5's "demo projects WITH tasks", deferred
    // from 002 (spec 002 Assumptions) until this feature existed to own it. Every demo project gets
    // the same spread of statuses so the Dashboard (005) and Reports (006) have something to chart
    // without a fresh install being empty. Deliberately unassigned — assigning a seeded task would
    // require a team_members row, which is 004's table; the seeder here only ever reads Projects.
    private static readonly (string Title, ProjectManagementApp.Domain.Enums.TaskStatus Status, TaskPriority Priority)[] DemoTaskSeeds =
    [
        ("Kickoff checklist", ProjectManagementApp.Domain.Enums.TaskStatus.Done, TaskPriority.Medium),
        ("Draft rollout plan", ProjectManagementApp.Domain.Enums.TaskStatus.InProgress, TaskPriority.High),
        ("Stakeholder review", ProjectManagementApp.Domain.Enums.TaskStatus.InReview, TaskPriority.Medium),
        ("Backlog grooming", ProjectManagementApp.Domain.Enums.TaskStatus.ToDo, TaskPriority.Low),
        ("Resolve vendor blocker", ProjectManagementApp.Domain.Enums.TaskStatus.Blocked, TaskPriority.Critical),
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
            var demoProjects = await EnsureDemoProjectsAsync(projectManager, ct);
            await EnsureDemoTasksAsync(demoProjects, ct);
        }
    }

    private async Task<List<Project>> EnsureDemoProjectsAsync(ApplicationUser owner, CancellationToken ct)
    {
        var projects = new List<Project>();

        foreach (var (name, description, status) in DemoProjectSeeds)
        {
            var existing = await _db.Projects.SingleOrDefaultAsync(p => p.Name == name, ct);
            if (existing is not null)
            {
                projects.Add(existing);
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

            projects.Add(project);
        }

        return projects;
    }

    private async Task EnsureDemoTasksAsync(IReadOnlyList<Project> demoProjects, CancellationToken ct)
    {
        foreach (var project in demoProjects)
        {
            foreach (var (title, status, priority) in DemoTaskSeeds)
            {
                var exists = await _db.Tasks.AnyAsync(t => t.ProjectId == project.Id && t.Title == title, ct);
                if (exists)
                {
                    continue;
                }

                var now = DateTimeOffset.UtcNow;
                var task = new TaskItem
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    Title = title,
                    Status = status,
                    Priority = priority,
                    // Mirrors UpdateTaskStatusCommandHandler's own derivation rule (spec B.7) — the
                    // seeder bypasses that handler, so it must reproduce the invariant by hand rather
                    // than leave a Done task with a null closed_at.
                    ClosedAt = status == ProjectManagementApp.Domain.Enums.TaskStatus.Done ? now : null,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                _db.Tasks.Add(task);

                await _activityLog.LogAsync(
                    actorId: null,
                    action: nameof(AuditAction.TaskCreated),
                    entityType: "Task",
                    entityId: task.Id.ToString(),
                    changeSummary: $"Seeded demo task '{title}' in project '{project.Name}'",
                    ct);
                await _db.SaveChangesAsync(ct);
            }
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
