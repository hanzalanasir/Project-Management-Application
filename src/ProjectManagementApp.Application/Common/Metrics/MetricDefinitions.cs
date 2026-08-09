using System.Linq.Expressions;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Common.Metrics;

// Shared kernel — the predicates behind every count that appears on more than one surface (005, 006).
// Both consumers import these; neither re-implements them.
public static class MetricDefinitions
{
    // "today" is UTC, FIXED. Not configurable.
    public static Expression<Func<TaskItem, bool>> IsOverdue(DateOnly todayUtc) =>
        t => t.DueDate != null && t.DueDate < todayUtc && t.Status != Domain.Enums.TaskStatus.Done;

    public static Expression<Func<TaskItem, bool>> IsClosed =>
        t => t.Status == Domain.Enums.TaskStatus.Done;

    public static decimal CompletionRate(int closed, int total) =>
        total == 0 ? 0m : (decimal)closed / total;

    public static Expression<Func<TaskItem, bool>> ClosedInWindow(DateTimeOffset from, DateTimeOffset to) =>
        t => t.ClosedAt != null && t.ClosedAt >= from && t.ClosedAt <= to;
}
