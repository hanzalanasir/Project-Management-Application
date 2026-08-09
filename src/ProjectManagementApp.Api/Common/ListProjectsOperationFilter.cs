using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using ProjectManagementApp.Api.Controllers;
using ProjectManagementApp.Application.Features.Projects.ListProjects;
using ProjectManagementApp.Domain.Enums;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProjectManagementApp.Api.Common;

// `sort` and `status` on GET /api/projects are plain strings in the action signature, validated
// against ProjectSortMap's whitelist / ProjectStatus respectively (never bound to a C# enum type —
// ProjectSortMap's "-name"/"-startDate" values cannot BE C# enum members, since identifiers can't
// start with '-'; research R-3). This filter only enriches the generated schema with the enum
// constraints docs/contracts/projects.v1.yaml declares — it changes no runtime validation.
public sealed class ListProjectsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.Name != nameof(ProjectsController.ListProjects) || operation.Parameters is null)
        {
            return;
        }

        foreach (var parameter in operation.Parameters)
        {
            if (parameter.Schema is not OpenApiSchema schema)
            {
                continue;
            }

            switch (parameter.Name)
            {
                case "sort":
                    schema.Enum = ProjectSortValues().Select(v => (JsonNode)v).ToList();
                    schema.Default = ProjectSortMap.Default;
                    break;
                case "status":
                    // Deliberately no default here — an omitted filter means "every status", not
                    // "Planning only" (V6/V7 quickstart behavior). ProjectStatus's own schema default
                    // of "Planning" describes a NEW project's default status, a different context.
                    schema.Enum = Enum.GetNames<ProjectStatus>().Select(name => (JsonNode)name).ToList();
                    break;
            }
        }
    }

    private static IEnumerable<string> ProjectSortValues() =>
        ["name", "-name", "startDate", "-startDate", "endDate", "-endDate", "status", "-status", "createdAt", "-createdAt"];
}
