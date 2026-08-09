using FluentAssertions;
using ProjectManagementApp.Api.Common;

namespace ProjectManagementApp.Api.Tests.Common;

public class ETagExtensionsTests
{
    [Fact]
    public void FormatThenParse_RoundTrips_ToTheSameVersion()
    {
        var formatted = ETagExtensions.FormatETag(4212);

        ETagExtensions.TryParseETag(formatted, out var parsed).Should().BeTrue();
        parsed.Should().Be(4212u);
    }

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("\"not-a-number\"")]
    [InlineData("12.5")]
    public void TryParseETag_MalformedValue_ReturnsFalse(string malformed)
    {
        ETagExtensions.TryParseETag(malformed, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParseETag_AbsentOrEmpty_ReturnsFalse(string? absent)
    {
        ETagExtensions.TryParseETag(absent, out _).Should().BeFalse();
    }
}
