using System.Reflection;
using FluentAssertions;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Architecture;

// Clean Architecture's inward dependency rule (Constitution II.2), enforced as a compile error
// via project references (research.md R-1) — this test proves the resulting assemblies actually
// have the graph we intend, not just that the solution happens to build today.
public class LayerDependencyTests
{
    [Fact]
    public void Domain_ReferencesNoOtherProjectAssembly()
    {
        var domainAssembly = typeof(ApplicationUser).Assembly;
        var referenced = domainAssembly.GetReferencedAssemblies().Select(a => a.Name).ToList();

        referenced.Should().NotContain(name =>
            name != null && name.StartsWith("ProjectManagementApp.") && name != "ProjectManagementApp.Domain");
    }

    [Fact]
    public void Application_DoesNotReferenceInfrastructureOrApi()
    {
        var applicationAssembly = typeof(Result).Assembly;
        var referenced = applicationAssembly.GetReferencedAssemblies().Select(a => a.Name).ToList();

        referenced.Should().NotContain("ProjectManagementApp.Infrastructure");
        referenced.Should().NotContain("ProjectManagementApp.Api");
    }
}
