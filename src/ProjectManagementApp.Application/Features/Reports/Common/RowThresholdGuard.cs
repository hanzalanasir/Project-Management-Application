using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Reports.Common;

/// <summary>
/// The large-report guard (research R-5, quickstart V10). Runs an indexed <c>CountAsync()</c> over
/// the <b>same scoped-and-filtered query the real read would use</b>, before any paging or
/// projection, and fails with <see cref="ErrorKind.UnprocessableContent"/> (422) when the count
/// exceeds the configured threshold.
/// </summary>
/// <remarks>
/// Deliberately exact, not heuristic: no window-size estimate, no <c>EXPLAIN</c> planner
/// statistics, and no "fetch <c>threshold + 1</c> rows and check" — the first two make the
/// threshold advisory rather than a real limit, and the third materializes most of what this guard
/// exists to prevent. One extra round trip, answered from the same index the real query uses, is
/// orders of magnitude cheaper than serializing ~10,000 rows for a browser-side PDF render.
/// </remarks>
public static class RowThresholdGuard
{
    public static async Task<Result> CheckAsync<T>(IQueryable<T> scopedAndFilteredQuery, int threshold, CancellationToken ct)
    {
        var count = await scopedAndFilteredQuery.CountAsync(ct);
        return Evaluate(count, threshold);
    }

    /// <summary>
    /// The Activity Report's shape: it may never query <c>activity_logs</c> directly (FR-007), so
    /// it has no <see cref="IQueryable{T}"/> to hand the other overload. <paramref name="countProvider"/>
    /// is a <c>page=1, pageSize=1</c> probe through <see cref="IActivityLogService.QueryScopedAsync"/>
    /// — its <c>TotalCount</c> is the same scoped-and-filtered count the real page-1..N read would
    /// see, obtained by materializing at most one row. Same threshold comparison, same message —
    /// one guard, two ways of getting the count.
    /// </summary>
    public static async Task<Result> CheckAsync(Func<CancellationToken, Task<int>> countProvider, int threshold, CancellationToken ct)
    {
        var count = await countProvider(ct);
        return Evaluate(count, threshold);
    }

    private static Result Evaluate(int count, int threshold) => count > threshold
        ? Result.Failure(new Error(
            ErrorKind.UnprocessableContent,
            "Narrow the date range; this window exceeds the report row limit."))
        : Result.Success();
}
