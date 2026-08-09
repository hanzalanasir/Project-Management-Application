using FluentAssertions;
using ProjectManagementApp.Application.Features.Projects.ListProjects;

namespace ProjectManagementApp.Application.Tests.Features.Projects;

// research R-3: sort is a closed whitelist — an unrecognized value must be rejected, never
// string-interpolated into a query (the classic dynamic-OrderBy injection vector).
public class ProjectSortMapTests
{
    [Theory]
    [InlineData("name")]
    [InlineData("-name")]
    [InlineData("startDate")]
    [InlineData("-startDate")]
    [InlineData("endDate")]
    [InlineData("-endDate")]
    [InlineData("status")]
    [InlineData("-status")]
    [InlineData("createdAt")]
    [InlineData("-createdAt")]
    public void IsValid_EveryWhitelistedValue_ReturnsTrue(string sort)
    {
        ProjectSortMap.IsValid(sort).Should().BeTrue();
    }

    [Theory]
    [InlineData("DROP TABLE projects")]
    [InlineData("ownerId")]
    [InlineData("")]
    [InlineData("Name")]
    public void IsValid_UnrecognizedValue_ReturnsFalse(string sort)
    {
        ProjectSortMap.IsValid(sort).Should().BeFalse();
    }

    [Fact]
    public void Apply_UnrecognizedValue_Throws()
    {
        var act = () => ProjectSortMap.Apply(Array.Empty<ProjectManagementApp.Domain.Entities.Project>().AsQueryable(), "not-whitelisted");

        act.Should().Throw<ArgumentException>();
    }
}
