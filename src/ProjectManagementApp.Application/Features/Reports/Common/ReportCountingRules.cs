using System.Linq.Expressions;
using ProjectManagementApp.Application.Common.Metrics;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Features.Reports.Common;

/// <summary>
/// The counting rules every report handler composes into its aggregate query. Re-exported verbatim
/// from the shared-kernel <see cref="MetricDefinitions"/> (<c>Common/Metrics/</c>, created by 001
/// T020, declared in <c>docs/shared-contracts.md</c> §8) — <b>not re-implemented, and not imported
/// from <c>Features/Dashboard/</c></b> (research R-4; importing 005's feature folder would make 006
/// depend on another feature's Application layer, forbidden by ADR-0006's addendum).
/// </summary>
/// <remarks>
/// <see cref="IsClosed"/> IS the re-open rule (FR-017): a re-opened task has <c>closed_at</c>
/// cleared and its status moved off <c>Done</c>, so the identical expression that counts a task as
/// closed also excludes it the moment it re-opens — for both Project Progress's <c>closedTasks</c>
/// and Task Completion's buckets. There is nothing extra to implement for the re-open exclusion; it
/// falls out of reusing this one predicate everywhere.
/// </remarks>
public static class ReportCountingRules
{
    public static Expression<Func<TaskItem, bool>> IsOverdue(DateOnly todayUtc) => MetricDefinitions.IsOverdue(todayUtc);

    public static Expression<Func<TaskItem, bool>> IsClosed => MetricDefinitions.IsClosed;

    public static Expression<Func<TaskItem, bool>> ClosedInWindow(DateTimeOffset from, DateTimeOffset to) =>
        MetricDefinitions.ClosedInWindow(from, to);

    /// <summary>0–1 fraction, matching 005's Dashboard exactly. Report handlers multiply by 100 for
    /// the contract's <c>completionPercent</c> field — the underlying rate is not re-derived.</summary>
    public static decimal CompletionRate(int closed, int total) => MetricDefinitions.CompletionRate(closed, total);

    /// <summary>
    /// Team Performance's <c>workload</c> — "currently assigned and not Done" — has no Dashboard
    /// counterpart, so this predicate is genuinely new to 006, not a re-export of anything.
    /// </summary>
    public static Expression<Func<TaskItem, bool>> IsOpenAssignment => t => t.Status != Domain.Enums.TaskStatus.Done;
}
