using FluentAssertions;
using ProjectManagementApp.Application.Features.Reports.Common;

namespace ProjectManagementApp.Application.Tests.Features.Reports;

// T011: from > to is rejected; both boundaries are inclusive; the max-window guard fires exactly
// at the configured span. DateOnly carries no offset, so nothing here needs a clock at all —
// that absence is itself the proof the window is immune to the process's local timezone.
public class ReportWindowTests
{
    [Fact]
    public void IsOrdered_FromBeforeTo_IsTrue()
    {
        ReportWindow.IsOrdered(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31)).Should().BeTrue();
    }

    [Fact]
    public void IsOrdered_FromEqualsTo_IsTrue_BothBoundsInclusive()
    {
        var day = new DateOnly(2026, 7, 15);
        ReportWindow.IsOrdered(day, day).Should().BeTrue();
    }

    [Fact]
    public void IsOrdered_FromAfterTo_IsFalse()
    {
        ReportWindow.IsOrdered(new DateOnly(2026, 7, 31), new DateOnly(2026, 7, 1)).Should().BeFalse();
    }

    [Fact]
    public void WithinMaxSpan_ExactlyAtLimit_IsTrue()
    {
        var from = new DateOnly(2026, 1, 1);
        var to = from.AddDays(30);

        ReportWindow.WithinMaxSpan(from, to, maxWindowDays: 30).Should().BeTrue();
    }

    [Fact]
    public void WithinMaxSpan_OneDayOverLimit_IsFalse()
    {
        var from = new DateOnly(2026, 1, 1);
        var to = from.AddDays(31);

        ReportWindow.WithinMaxSpan(from, to, maxWindowDays: 30).Should().BeFalse();
    }

    [Fact]
    public void WithinMaxSpan_SameDay_IsAlwaysTrue()
    {
        var day = new DateOnly(2026, 1, 1);
        ReportWindow.WithinMaxSpan(day, day, maxWindowDays: 0).Should().BeTrue();
    }
}
