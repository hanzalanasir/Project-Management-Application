using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Persistence;

namespace ProjectManagementApp.Application.Tests.Builders;

// Extends ProjectsScenario's fixture with the 003 shape (tasks.md T023, ADR-0007 §4): a second
// TeamMember (TM2, also on project A) and three tasks — T1 (assigned TM), T2 (assigned TM2), T3
// (unassigned) — all on project A. TM2 is required to prove scope is by ASSIGNMENT, not
// membership: both TM and TM2 are on project A's team, but each sees only their own assigned task.
public sealed class TasksScenario
{
    public ProjectsScenario Projects { get; } = new();

    public ApplicationUser Tm2 { get; } = new ApplicationUserBuilder().WithEmail("tm2@scenario.test").Build();

    public TaskItem Task1 { get; }
    public TaskItem Task2 { get; }
    public TaskItem Task3 { get; }

    public TasksScenario()
    {
        Task1 = new TaskBuilder().WithTitle("T1").WithProject(Projects.ProjectA).WithAssignee(Projects.Tm).Build();
        Task2 = new TaskBuilder().WithTitle("T2").WithProject(Projects.ProjectA).WithAssignee(Tm2).Build();
        Task3 = new TaskBuilder().WithTitle("T3").WithProject(Projects.ProjectA).Build();
    }

    public async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        await Projects.SeedAsync(db, ct);

        db.Users.Add(Tm2);
        db.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            ProjectId = Projects.ProjectA.Id,
            UserId = Tm2.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        db.Tasks.AddRange(Task1, Task2, Task3);
        await db.SaveChangesAsync(ct);
    }
}
