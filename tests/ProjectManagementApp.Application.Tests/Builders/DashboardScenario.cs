using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Persistence;

namespace ProjectManagementApp.Application.Tests.Builders;

// Extends TeamScenario with the 005 shape (tasks.md T016): LONELY — a TeamMember on no team
// anywhere, required by the zero-scope test (T021/T032) — plus a task set on project A spanning
// every TaskStatus, with a mix of overdue, Done, and no-due-date rows so the metric tests
// (overdue boundary, completion rate, zero-seeding) have real data to aggregate over.
public sealed class DashboardScenario
{
    public TeamScenario Team { get; } = new();

    public ApplicationUser Lonely { get; } = new ApplicationUserBuilder().WithEmail("lonely@scenario.test").Build();

    // Spans every TaskStatus. DTodo/DBlocked are overdue (due yesterday, not Done); DInReview is
    // due today (not overdue, per the strictly-before-today boundary); DDone closed with a due
    // date in the past (Done excludes it from overdue regardless); DNoDueDate has no due date at
    // all (never overdue).
    public TaskItem DToDo { get; }
    public TaskItem DInProgress { get; }
    public TaskItem DInReview { get; }
    public TaskItem DDone { get; }
    public TaskItem DBlocked { get; }
    public TaskItem DNoDueDate { get; }

    public DashboardScenario()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        var projectA = Team.Tasks.Projects.ProjectA;
        var assignee = Team.Tasks.Projects.Tm;

        DToDo = new TaskBuilder().WithTitle("D-ToDo-Overdue").WithProject(projectA)
            .WithStatus(Domain.Enums.TaskStatus.ToDo).WithDueDate(yesterday).WithAssignee(assignee).Build();
        DInProgress = new TaskBuilder().WithTitle("D-InProgress").WithProject(projectA)
            .WithStatus(Domain.Enums.TaskStatus.InProgress).WithDueDate(today.AddDays(7)).WithAssignee(assignee).Build();
        DInReview = new TaskBuilder().WithTitle("D-InReview-DueToday").WithProject(projectA)
            .WithStatus(Domain.Enums.TaskStatus.InReview).WithDueDate(today).Build();
        DDone = new TaskBuilder().WithTitle("D-Done").WithProject(projectA)
            .WithStatus(Domain.Enums.TaskStatus.Done).WithDueDate(yesterday).WithClosedAt(DateTimeOffset.UtcNow).WithAssignee(assignee).Build();
        DBlocked = new TaskBuilder().WithTitle("D-Blocked-Overdue").WithProject(projectA)
            .WithStatus(Domain.Enums.TaskStatus.Blocked).WithDueDate(yesterday).Build();
        DNoDueDate = new TaskBuilder().WithTitle("D-NoDueDate").WithProject(projectA)
            .WithStatus(Domain.Enums.TaskStatus.ToDo).WithDueDate(null).Build();
    }

    public async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        await Team.SeedAsync(db, ct);

        db.Users.Add(Lonely);
        db.Tasks.AddRange(DToDo, DInProgress, DInReview, DDone, DBlocked, DNoDueDate);
        await db.SaveChangesAsync(ct);
    }
}
