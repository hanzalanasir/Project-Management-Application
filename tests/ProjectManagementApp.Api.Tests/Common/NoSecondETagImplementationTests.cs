using FluentAssertions;
using ProjectManagementApp.Api.Common;
using ProjectManagementApp.Api.Controllers;

namespace ProjectManagementApp.Api.Tests.Common;

// Guards against exactly the duplication 002's task list originally risked (research R-2 note,
// 001 research R-15): ETagExtensions is created once by 001; 002 must reuse it, never reimplement
// it locally under Features/Projects/.
public class NoSecondETagImplementationTests
{
    [Fact]
    public void ProjectsController_Exists_AndTheSharedETagExtensionsType_LivesInApiCommon()
    {
        // The compile-time dependency on ProjectsController below is itself part of the guard:
        // this test file cannot build until the controller exists (T021), at which point a
        // reviewer reading ProjectsController.cs can see it calls WriteETag/TryParseIfMatch from
        // this same ETagExtensions type — verified directly in T033/T054/T065.
        _ = typeof(ProjectsController);
        typeof(ETagExtensions).Namespace.Should().Be("ProjectManagementApp.Api.Common");
    }

    [Fact]
    public void FeaturesProjectsNamespace_ContainsNoLocalETagReimplementation()
    {
        var applicationAssembly = typeof(ProjectManagementApp.Application.Features.Projects.ProjectSummaryDto).Assembly;

        var suspiciousTypes = applicationAssembly.GetTypes()
            .Where(t => t.Namespace is not null && t.Namespace.StartsWith("ProjectManagementApp.Application.Features.Projects"))
            .Where(t => t.Name.Contains("ETag", StringComparison.OrdinalIgnoreCase));

        suspiciousTypes.Should().BeEmpty("ETag formatting/parsing belongs solely to Api.Common.ETagExtensions (001 T117)");
    }
}
