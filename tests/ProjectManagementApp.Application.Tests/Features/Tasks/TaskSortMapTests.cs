using FluentAssertions;
using ProjectManagementApp.Application.Features.Tasks.ListTasks;

namespace ProjectManagementApp.Application.Tests.Features.Tasks;

public class TaskSortMapTests
{
    [Theory]
    [InlineData("dueDate")]
    [InlineData("-dueDate")]
    [InlineData("priority")]
    [InlineData("-priority")]
    [InlineData("status")]
    [InlineData("-status")]
    [InlineData("title")]
    [InlineData("-title")]
    [InlineData("createdAt")]
    [InlineData("-createdAt")]
    public void IsValid_WhitelistedValues_ReturnsTrue(string sort)
    {
        TaskSortMap.IsValid(sort).Should().BeTrue();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("assigneeId")]
    [InlineData("")]
    [InlineData("DROP TABLE tasks;")]
    public void IsValid_UnrecognizedValues_ReturnsFalse(string sort)
    {
        TaskSortMap.IsValid(sort).Should().BeFalse();
    }

    [Fact]
    public void Default_IsDueDateAscending()
    {
        TaskSortMap.Default.Should().Be("dueDate");
    }

    [Fact]
    public void Apply_UnrecognizedValue_Throws()
    {
        var act = () => TaskSortMap.Apply(new List<ProjectManagementApp.Domain.Entities.TaskItem>().AsQueryable(), "not-a-real-sort");
        act.Should().Throw<ArgumentException>();
    }
}
