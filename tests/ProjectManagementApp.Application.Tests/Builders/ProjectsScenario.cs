using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Persistence;

namespace ProjectManagementApp.Application.Tests.Builders;

// The standard 002 fixture scenario reused across US2-US5 tests (quickstart V4's exact shape):
// an Admin, two ProjectManagers (PM owns A, PM2 owns B — the cross-owner denial test needs a
// second manager), two TeamMembers (one assigned to A, one assigned to nothing), and projects A/B.
// From builders, never the production seeder (ADR-0007 §4).
public sealed class ProjectsScenario
{
    public ApplicationUser Admin { get; } = new ApplicationUserBuilder().WithEmail("admin@scenario.test").Build();
    public ApplicationUser Pm { get; } = new ApplicationUserBuilder().WithEmail("pm@scenario.test").Build();
    public ApplicationUser Pm2 { get; } = new ApplicationUserBuilder().WithEmail("pm2@scenario.test").Build();
    public ApplicationUser Tm { get; } = new ApplicationUserBuilder().WithEmail("tm@scenario.test").Build();
    public ApplicationUser UnassignedTm { get; } = new ApplicationUserBuilder().WithEmail("unassigned-tm@scenario.test").Build();

    public Project ProjectA { get; }
    public Project ProjectB { get; }

    public ProjectsScenario()
    {
        ProjectA = new ProjectBuilder().WithName("Project A").WithOwner(Pm).Build();
        ProjectB = new ProjectBuilder().WithName("Project B").WithOwner(Pm2).Build();
    }

    public async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        db.Users.AddRange(Admin, Pm, Pm2, Tm, UnassignedTm);
        db.Projects.AddRange(ProjectA, ProjectB);
        db.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            ProjectId = ProjectA.Id,
            UserId = Tm.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(ct);
    }
}
