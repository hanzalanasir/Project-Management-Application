using FluentAssertions;
using ProjectManagementApp.Application.Features.Dashboard.Common;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Tests.Features.Dashboard;

// Proves T010's claim: the map is a stable typed contract, not a variable dictionary — every enum
// value is present at 0 when there are no matching rows, and is never omitted (data-model.md §4).
public class EnumCountMapTests
{
    [Fact]
    public void Build_WithNoRows_EveryEnumValuePresentAtZero()
    {
        var map = EnumCountMap.Build<ProjectStatus>([]);

        map.Should().HaveCount(Enum.GetValues<ProjectStatus>().Length);
        map.Values.Should().OnlyContain(count => count == 0);
        map.Keys.Should().BeEquivalentTo(Enum.GetValues<ProjectStatus>());
    }

    [Fact]
    public void Build_MergesQueryResults_WithoutOmittingUnmatchedKeys()
    {
        var map = EnumCountMap.Build<ProjectStatus>([(ProjectStatus.Active, 3), (ProjectStatus.Completed, 1)]);

        map[ProjectStatus.Active].Should().Be(3);
        map[ProjectStatus.Completed].Should().Be(1);
        map[ProjectStatus.Planning].Should().Be(0);
        map[ProjectStatus.OnHold].Should().Be(0);
        map[ProjectStatus.Cancelled].Should().Be(0);
        map.Should().ContainKey(ProjectStatus.Planning);
    }

    [Fact]
    public void Build_WorksForTaskStatus_AllFiveKeysPresent()
    {
        var map = EnumCountMap.Build<ProjectManagementApp.Domain.Enums.TaskStatus>(
            [(ProjectManagementApp.Domain.Enums.TaskStatus.Done, 5)]);

        map.Should().HaveCount(5);
        map[ProjectManagementApp.Domain.Enums.TaskStatus.Blocked].Should().Be(0);
    }
}
