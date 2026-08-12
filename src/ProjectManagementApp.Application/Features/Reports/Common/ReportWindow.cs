namespace ProjectManagementApp.Application.Features.Reports.Common;

/// <summary>
/// The pure window-validation predicates every report query's own FluentValidation validator
/// (<c>GetProjectProgressQueryValidator</c>, etc.) composes into its rules, so "from &gt; to" and the
/// max-window guard are defined exactly once rather than four times.
/// </summary>
/// <remarks>
/// <see cref="DateOnly"/> carries no offset of its own, so this arithmetic is inherently immune to
/// the process's local timezone — the same guarantee 005's <c>OverdueTimezoneTests</c> proved
/// structurally for the Dashboard's overdue boundary. "Today" for any UTC-boundary comparison
/// elsewhere in this feature must come from <c>DateTimeOffset.UtcNow</c>, never <c>DateTime.Now</c>
/// or <c>DateTime.Today</c> (research R-4).
/// </remarks>
public static class ReportWindow
{
    /// <summary>Both bounds are inclusive; <paramref name="from"/> must not be after <paramref name="to"/>.</summary>
    public static bool IsOrdered(DateOnly from, DateOnly to) => from <= to;

    /// <summary>The inclusive span in days must not exceed <paramref name="maxWindowDays"/> — a guard against absurd ranges.</summary>
    public static bool WithinMaxSpan(DateOnly from, DateOnly to, int maxWindowDays) =>
        to.DayNumber - from.DayNumber <= maxWindowDays;
}
