using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using ProjectManagementApp.Api.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProjectManagementApp.Api.Common;

// `entityType` on GET /api/reports/activity and `groupBy` on GET /api/reports/task-completion are
// plain strings in the action signature, validated by GetActivityReportQueryValidator /
// GetTaskCompletionQueryValidator respectively (never bound to a C# enum type, so an invalid value
// still produces the app's standard ValidationProblem rather than a raw model-binding failure —
// mirrors ListProjectsOperationFilter/ListTasksOperationFilter's identical precedent). This filter
// only enriches the generated schema with the enum constraints docs/contracts/reports.v1.yaml
// declares — it changes no runtime validation.
public sealed class ReportsOperationFilter : IOperationFilter
{
    private static readonly string[] EntityTypeValues = ["User", "Project", "Task", "TeamMember", "Report"];
    private static readonly string[] GroupByValues = ["day", "week", "month"];

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null)
        {
            return;
        }

        var isActivity = context.MethodInfo.Name == nameof(ReportsController.GetActivity);
        var isTaskCompletion = context.MethodInfo.Name == nameof(ReportsController.GetTaskCompletion);
        if (!isActivity && !isTaskCompletion)
        {
            return;
        }

        foreach (var parameter in operation.Parameters)
        {
            if (parameter.Schema is not OpenApiSchema schema)
            {
                continue;
            }

            if (isActivity && parameter.Name == "entityType")
            {
                schema.Enum = EntityTypeValues.Select(v => (JsonNode)v).ToList();
            }

            if (isTaskCompletion && parameter.Name == "groupBy")
            {
                schema.Enum = GroupByValues.Select(v => (JsonNode)v).ToList();
            }
        }
    }
}
