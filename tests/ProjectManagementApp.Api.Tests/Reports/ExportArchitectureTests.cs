using System.Reflection;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApp.Api.Controllers;

namespace ProjectManagementApp.Api.Tests.Reports;

// T082 🎯 — makes Constitution III's jsPDF lock and the client-side export architecture (research
// R-3) a proven fact rather than a claim. Turns stage 4's manual T080 inspection into a permanent
// automated assertion, and adds the one thing manual inspection can't reliably catch: that no
// server-side PDF/CSV package snuck into any backend .csproj.
public class ExportArchitectureTests
{
    [Fact]
    public void ReportsController_DeclaresNoFormatParameterOnAnyAction()
    {
        var offendingParameters = typeof(ReportsController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(m => m.GetParameters())
            .Where(p => string.Equals(p.Name, "format", StringComparison.OrdinalIgnoreCase))
            .ToList();

        offendingParameters.Should().BeEmpty(
            "no report endpoint may accept a ?format query parameter — export is a client-only concern (research R-3)");
    }

    [Fact]
    public void ReportsController_DeclaresNoExportRoute()
    {
        var routeTemplates = typeof(ReportsController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(m => m.GetCustomAttributes<HttpGetAttribute>())
            .Select(a => a.Template ?? string.Empty)
            .ToList();

        routeTemplates.Should().NotBeEmpty();
        routeTemplates.Should().NotContain(t => t.Contains("export", StringComparison.OrdinalIgnoreCase),
            "PDF/CSV rendering happens entirely client-side (ReportExportService) — there is no /export route to hit");
    }

    [Fact]
    public void ReportsController_DeclaresNoNonGetVerbAnywhere()
    {
        var mutationAttributes = new[] { typeof(HttpPostAttribute), typeof(HttpPutAttribute), typeof(HttpDeleteAttribute), typeof(HttpPatchAttribute) };

        var offendingMethods = typeof(ReportsController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttributes(inherit: true).Any(a => mutationAttributes.Contains(a.GetType())))
            .Select(m => m.Name)
            .ToList();

        offendingMethods.Should().BeEmpty("no POST/PUT/DELETE/PATCH route may exist under /api/reports — every action is a read");
    }

    [Fact]
    public void ReportsController_ExposesExactlyFiveGetActions()
    {
        var getActions = typeof(ReportsController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttributes<HttpGetAttribute>().Any())
            .Select(m => m.Name)
            .ToList();

        getActions.Should().BeEquivalentTo(
            ["GetCatalog", "GetProjectProgress", "GetTaskCompletion", "GetTeamPerformance", "GetActivity"]);
    }

    [Theory]
    [InlineData("ProjectManagementApp.Api")]
    [InlineData("ProjectManagementApp.Application")]
    [InlineData("ProjectManagementApp.Infrastructure")]
    [InlineData("ProjectManagementApp.Domain")]
    public void BackendProject_ReferencesNoServerSidePdfOrCsvPackage(string projectName)
    {
        var repoRoot = FindRepoRoot();
        var csprojPath = Path.Combine(repoRoot, "src", projectName, $"{projectName}.csproj");
        File.Exists(csprojPath).Should().BeTrue($"expected to find {csprojPath}");

        var doc = XDocument.Load(csprojPath);
        var packageIds = doc.Descendants("PackageReference")
            .Select(e => e.Attribute("Include")?.Value ?? string.Empty)
            .ToList();

        // Names of the usual server-side PDF/CSV generation libraries in the .NET ecosystem. jsPDF
        // and papaparse (Constitution III's locked choice) are npm packages consumed only by the
        // Angular frontend — a backend .csproj can never reference them, so this list is about
        // catching the *substitutes* a well-meaning "just add server export" PR might reach for.
        var forbiddenSubstrings = new[] { "itextsharp", "itext", "questpdf", "syncfusion", "aspose", "csvhelper", "wkhtmltopdf", "puppeteersharp" };

        var offending = packageIds
            .Where(id => forbiddenSubstrings.Any(f => id.Contains(f, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        offending.Should().BeEmpty(
            $"{projectName} must not reference any server-side PDF/CSV generation package — export is exclusively client-side (jsPDF + papaparse)");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ProjectManagementApp.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Could not locate repo root (ProjectManagementApp.slnx not found).");
    }
}
