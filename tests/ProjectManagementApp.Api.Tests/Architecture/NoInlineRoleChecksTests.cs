using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApp.Api.Controllers;

namespace ProjectManagementApp.Api.Tests.Architecture;

// Roles must be attribute-declared only (Constitution V.2). Two complementary checks:
// (1) reflection — every controller action carries an [Authorize]/[AllowAnonymous] declaration
//     (at the method or the controller level), never a body that decides access itself;
// (2) source scan — no handler/controller source file contains a literal role-string comparison
//     (the kind of ad-hoc check an attribute is supposed to replace).
public class NoInlineRoleChecksTests
{
    [Fact]
    public void EveryControllerAction_IsCoveredByAnAuthorizeOrAllowAnonymousAttribute()
    {
        var controllerTypes = typeof(AuthController).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var controllerType in controllerTypes)
        {
            var hasControllerLevelAttribute =
                controllerType.GetCustomAttribute<AuthorizeAttribute>() is not null ||
                controllerType.GetCustomAttribute<AllowAnonymousAttribute>() is not null;

            var actions = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName);

            foreach (var action in actions)
            {
                var hasActionLevelAttribute =
                    action.GetCustomAttribute<AuthorizeAttribute>() is not null ||
                    action.GetCustomAttribute<AllowAnonymousAttribute>() is not null;

                (hasControllerLevelAttribute || hasActionLevelAttribute).Should().BeTrue(
                    $"{controllerType.Name}.{action.Name} must declare [Authorize] or [AllowAnonymous] " +
                    "— the global fallback policy covers anything undeclared, but every endpoint's intent must be explicit");
            }
        }
    }

    [Fact]
    public void NoSourceFile_ContainsALiteralRoleStringComparison()
    {
        var repoRoot = FindRepoRoot();
        var suspiciousPattern = new Regex(
            @"(Role|role)\s*(==|!=)\s*""(Admin|ProjectManager|TeamMember)""|""(Admin|ProjectManager|TeamMember)""\s*(==|!=)\s*(Role|role)",
            RegexOptions.Compiled);

        var offendingFiles = new List<string>();
        foreach (var dir in new[] { "src/ProjectManagementApp.Api/Controllers", "src/ProjectManagementApp.Application/Features" })
        {
            var fullDir = Path.Combine(repoRoot, dir);
            if (!Directory.Exists(fullDir)) continue;

            foreach (var file in Directory.EnumerateFiles(fullDir, "*.cs", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(file);
                if (suspiciousPattern.IsMatch(content))
                {
                    offendingFiles.Add(file);
                }
            }
        }

        offendingFiles.Should().BeEmpty("role checks must be attribute-declared, not ad-hoc string comparisons in a handler or controller");
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
