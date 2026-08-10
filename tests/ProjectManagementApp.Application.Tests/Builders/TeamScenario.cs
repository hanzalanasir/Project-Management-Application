using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Persistence;

namespace ProjectManagementApp.Application.Tests.Builders;

// Extends TasksScenario with the 004 shape (tasks.md T018): one deactivated user (INACTIVE), not
// on any project's team, seeded but never a member — the only add-time eligibility gate is that
// the target user is active (FR-016), so US1's tests need a real deactivated user to refuse.
public sealed class TeamScenario
{
    public TasksScenario Tasks { get; } = new();

    public ApplicationUser Inactive { get; } = new ApplicationUserBuilder().WithEmail("inactive@scenario.test").Inactive().Build();

    public async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        await Tasks.SeedAsync(db, ct);

        db.Users.Add(Inactive);
        await db.SaveChangesAsync(ct);
    }
}
