using FluentAssertions;
using ProjectManagementApp.Application.Common.Metrics;
using ProjectManagementApp.Application.Tests.Builders;

namespace ProjectManagementApp.Application.Tests.Common.Metrics;

// Lives beside the shared-kernel type, not under Features/Dashboard/ — 006 depends on this
// identical behaviour (006 NFR-002). Proves the overdue boundary and the divide-by-zero guard
// (data-model.md §3, research R-2).
public class MetricDefinitionsTests
{
    [Fact]
    public void IsOverdue_DueToday_IsNotOverdue()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var task = new TaskBuilder().WithStatus(Domain.Enums.TaskStatus.ToDo).WithDueDate(today).Build();

        var isOverdue = MetricDefinitions.IsOverdue(today).Compile()(task);

        isOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_DueYesterday_IsOverdue()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var task = new TaskBuilder().WithStatus(Domain.Enums.TaskStatus.ToDo).WithDueDate(today.AddDays(-1)).Build();

        var isOverdue = MetricDefinitions.IsOverdue(today).Compile()(task);

        isOverdue.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_DoneTask_IsNeverOverdue_RegardlessOfDueDate()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var task = new TaskBuilder().WithStatus(Domain.Enums.TaskStatus.Done).WithDueDate(today.AddDays(-30)).Build();

        var isOverdue = MetricDefinitions.IsOverdue(today).Compile()(task);

        isOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_NoDueDate_IsNeverOverdue()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var task = new TaskBuilder().WithStatus(Domain.Enums.TaskStatus.ToDo).WithDueDate(null).Build();

        var isOverdue = MetricDefinitions.IsOverdue(today).Compile()(task);

        isOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsClosed_MatchesOnlyDoneStatus()
    {
        var doneTask = new TaskBuilder().WithStatus(Domain.Enums.TaskStatus.Done).Build();
        var todoTask = new TaskBuilder().WithStatus(Domain.Enums.TaskStatus.ToDo).Build();
        var blockedTask = new TaskBuilder().WithStatus(Domain.Enums.TaskStatus.Blocked).Build();

        var isClosed = MetricDefinitions.IsClosed.Compile();

        isClosed(doneTask).Should().BeTrue();
        isClosed(todoTask).Should().BeFalse();
        isClosed(blockedTask).Should().BeFalse();
    }

    [Fact]
    public void CompletionRate_EmptySet_ReturnsZero_NeverDivideByZero()
    {
        MetricDefinitions.CompletionRate(0, 0).Should().Be(0m);
    }

    [Fact]
    public void CompletionRate_PartialCompletion_ReturnsExactFraction()
    {
        MetricDefinitions.CompletionRate(3, 12).Should().Be(0.25m);
    }

    [Fact]
    public void CompletionRate_AllClosed_ReturnsOne()
    {
        MetricDefinitions.CompletionRate(5, 5).Should().Be(1m);
    }
}
