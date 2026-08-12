using System.Globalization;

namespace ProjectManagementApp.Application.Features.Reports.GetTaskCompletion;

/// <summary>
/// Enumerates every period in a window (UTC) so the series is continuous and zero-filled — a chart
/// renders a real zero rather than a gap — then merges in SQL-computed per-day counts. Buckets are
/// generated FIRST, independent of any data, which is what guarantees zero-fill: a period with no
/// matching day-count simply keeps its default of 0.
/// </summary>
public static class BucketGenerator
{
    public static IReadOnlyList<TaskCompletionBucketDto> Generate(
        DateOnly from, DateOnly to, string groupBy, IReadOnlyDictionary<DateOnly, int> dayCounts)
    {
        return groupBy switch
        {
            "day" => GenerateDayBuckets(from, to, dayCounts),
            "week" => GenerateWeekBuckets(from, to, dayCounts),
            "month" => GenerateMonthBuckets(from, to, dayCounts),
            _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, "groupBy must be day, week, or month."),
        };
    }

    private static IReadOnlyList<TaskCompletionBucketDto> GenerateDayBuckets(
        DateOnly from, DateOnly to, IReadOnlyDictionary<DateOnly, int> dayCounts)
    {
        var buckets = new List<TaskCompletionBucketDto>();
        for (var day = from; day <= to; day = day.AddDays(1))
        {
            var count = dayCounts.GetValueOrDefault(day);
            buckets.Add(new TaskCompletionBucketDto(day, day.ToString("yyyy-MM-dd"), count));
        }
        return buckets;
    }

    private static IReadOnlyList<TaskCompletionBucketDto> GenerateWeekBuckets(
        DateOnly from, DateOnly to, IReadOnlyDictionary<DateOnly, int> dayCounts)
    {
        var buckets = new List<TaskCompletionBucketDto>();
        var weekStart = MondayOnOrBefore(from);

        while (weekStart <= to)
        {
            var weekEnd = weekStart.AddDays(6);
            var count = dayCounts
                .Where(kv => kv.Key >= weekStart && kv.Key <= weekEnd)
                .Sum(kv => kv.Value);

            var isoYear = ISOWeek.GetYear(weekStart.ToDateTime(TimeOnly.MinValue));
            var isoWeek = ISOWeek.GetWeekOfYear(weekStart.ToDateTime(TimeOnly.MinValue));
            var label = $"{isoYear}-W{isoWeek:D2}";

            buckets.Add(new TaskCompletionBucketDto(weekStart, label, count));
            weekStart = weekStart.AddDays(7);
        }

        return buckets;
    }

    private static IReadOnlyList<TaskCompletionBucketDto> GenerateMonthBuckets(
        DateOnly from, DateOnly to, IReadOnlyDictionary<DateOnly, int> dayCounts)
    {
        var buckets = new List<TaskCompletionBucketDto>();
        var monthStart = new DateOnly(from.Year, from.Month, 1);

        while (monthStart <= to)
        {
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var count = dayCounts
                .Where(kv => kv.Key >= monthStart && kv.Key <= monthEnd)
                .Sum(kv => kv.Value);

            buckets.Add(new TaskCompletionBucketDto(monthStart, monthStart.ToString("yyyy-MM"), count));
            monthStart = monthStart.AddMonths(1);
        }

        return buckets;
    }

    private static DateOnly MondayOnOrBefore(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }
}
