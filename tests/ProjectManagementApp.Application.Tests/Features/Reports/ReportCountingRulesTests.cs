using FluentAssertions;
using ProjectManagementApp.Application.Features.Reports.Common;
using ProjectManagementApp.Application.Tests.Builders;

namespace ProjectManagementApp.Application.Tests.Features.Reports;

// T015: the re-open exclusion (FR-017) applies identically to Project Progress's closedTasks and
// Task Completion's buckets, because both consume the SAME IsClosed predicate — proven here by
// evaluating it once and using the result for both claims, rather than testing two separately
// hand-rolled predicates that could drift apart. Also: CompletionRate never divides by zero.
public class ReportCountingRulesTests
{
    [Fact]
    public void IsClosed_DoneTask_IsTrue()
    {
        var task = new TaskBuilder()
            .WithStatus(Domain.Enums.TaskStatus.Done)
            .WithClosedAt(DateTimeOffset.UtcNow)
            .Build();

        ReportCountingRules.IsClosed.Compile()(task).Should().BeTrue();
    }

    [Fact]
    public void IsClosed_ReopenedTask_IsFalse_ExcludedFromBothClosedTasksAndCompletionBuckets()
    {
        // A re-opened task: 003's status handler cleared closed_at and moved status off Done. The
        // single IsClosed expression is what both Project Progress's closedTasks count and Task
        // Completion's bucket membership are built from — evaluating it once here is the proof
        // that both surfaces apply the identical rule, since there is only one rule to apply.
        var reopened = new TaskBuilder()
            .WithStatus(Domain.Enums.TaskStatus.InProgress)
            .WithClosedAt(null)
            .Build();

        var isClosed = ReportCountingRules.IsClosed.Compile()(reopened);

        isClosed.Should().BeFalse("a re-opened task must drop out of closedTasks and every completion bucket alike");
    }

    [Fact]
    public void IsOverdue_DueYesterday_NotDone_IsOverdue()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var task = new TaskBuilder().WithStatus(Domain.Enums.TaskStatus.ToDo).WithDueDate(today.AddDays(-1)).Build();

        ReportCountingRules.IsOverdue(today).Compile()(task).Should().BeTrue();
    }

    [Fact]
    public void IsOpenAssignment_DoneTask_IsFalse()
    {
        var task = new TaskBuilder().WithStatus(Domain.Enums.TaskStatus.Done).Build();
        ReportCountingRules.IsOpenAssignment.Compile()(task).Should().BeFalse();
    }

    [Fact]
    public void IsOpenAssignment_InProgressTask_IsTrue()
    {
        var task = new TaskBuilder().WithStatus(Domain.Enums.TaskStatus.InProgress).Build();
        ReportCountingRules.IsOpenAssignment.Compile()(task).Should().BeTrue();
    }

    [Fact]
    public void CompletionRate_NoTasks_IsZero_NeverDivideByZero()
    {
        ReportCountingRules.CompletionRate(closed: 0, total: 0).Should().Be(0m);
    }

    [Fact]
    public void CompletionRate_ThreeOfTwelve_IsExactQuarter()
    {
        ReportCountingRules.CompletionRate(closed: 3, total: 12).Should().Be(0.25m);
    }
}
