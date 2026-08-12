using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Persistence;

namespace ProjectManagementApp.Application.Tests.Builders;

// Extends TeamScenario with the 006 shape (tasks.md T023): tasks closed at KNOWN dates spread
// across a fixed window (for Task Completion's day/week/month bucketing), one re-opened task (for
// the FR-017 exclusion rule shared with Project Progress), one overdue task, and two members
// (Tm/Tm2) with deliberately different throughput (for Team Performance).
public sealed class ReportsScenario
{
    public TeamScenario Team { get; } = new();

    // The window every report test in stages 2-4 windows its from/to around.
    public static readonly DateOnly WindowFrom = new(2026, 7, 1);
    public static readonly DateOnly WindowTo = new(2026, 7, 31);

    // Tm: two closures in-window (throughput 2). Tm2: one closure in-window (throughput 1) — the
    // deliberate asymmetry Team Performance's throughput metric needs to be meaningful.
    public TaskItem RClosedEarlyInWindow { get; }
    public TaskItem RClosedMidWindow { get; }
    public TaskItem RClosedLateWindow_Tm2 { get; }

    // Was closed, then re-opened (003's status handler cleared closed_at and moved status off
    // Done) — must drop out of both Project Progress's closedTasks and every Task Completion
    // bucket (FR-017).
    public TaskItem RReopened { get; }

    // Due inside the window, still open as of "now" — overdue for Project Progress/dashboard-parity
    // tests (T083).
    public TaskItem ROverdueOpen { get; }

    // Open, no due date, assigned to Tm2 — contributes to Tm2's workload without affecting
    // throughput or overdue.
    public TaskItem ROpenWorkload_Tm2 { get; }

    public ReportsScenario()
    {
        var projectA = Team.Tasks.Projects.ProjectA;
        var tm = Team.Tasks.Projects.Tm;
        var tm2 = Team.Tasks.Tm2;

        RClosedEarlyInWindow = new TaskBuilder().WithTitle("R-Closed-Early").WithProject(projectA)
            .WithStatus(Domain.Enums.TaskStatus.Done).WithAssignee(tm)
            .WithDueDate(WindowFrom.AddDays(2)).WithClosedAt(ToUtc(WindowFrom.AddDays(2))).Build();

        RClosedMidWindow = new TaskBuilder().WithTitle("R-Closed-Mid").WithProject(projectA)
            .WithStatus(Domain.Enums.TaskStatus.Done).WithAssignee(tm)
            .WithDueDate(WindowFrom.AddDays(14)).WithClosedAt(ToUtc(WindowFrom.AddDays(14))).Build();

        RClosedLateWindow_Tm2 = new TaskBuilder().WithTitle("R-Closed-Late-Tm2").WithProject(projectA)
            .WithStatus(Domain.Enums.TaskStatus.Done).WithAssignee(tm2)
            .WithDueDate(WindowFrom.AddDays(27)).WithClosedAt(ToUtc(WindowFrom.AddDays(27))).Build();

        RReopened = new TaskBuilder().WithTitle("R-Reopened").WithProject(projectA)
            .WithStatus(Domain.Enums.TaskStatus.InProgress).WithAssignee(tm)
            .WithDueDate(WindowFrom.AddDays(10)).WithClosedAt(null).Build();

        ROverdueOpen = new TaskBuilder().WithTitle("R-Overdue-Open").WithProject(projectA)
            .WithStatus(Domain.Enums.TaskStatus.ToDo).WithAssignee(tm)
            .WithDueDate(WindowFrom.AddDays(20)).Build();

        ROpenWorkload_Tm2 = new TaskBuilder().WithTitle("R-Open-Workload-Tm2").WithProject(projectA)
            .WithStatus(Domain.Enums.TaskStatus.InProgress).WithAssignee(tm2)
            .WithDueDate(null).Build();
    }

    private static DateTimeOffset ToUtc(DateOnly date) => new(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    public async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        await Team.SeedAsync(db, ct);

        db.Tasks.AddRange(RClosedEarlyInWindow, RClosedMidWindow, RClosedLateWindow_Tm2, RReopened, ROverdueOpen, ROpenWorkload_Tm2);
        await db.SaveChangesAsync(ct);
    }
}
