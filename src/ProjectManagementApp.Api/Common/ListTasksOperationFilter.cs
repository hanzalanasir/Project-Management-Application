using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using ProjectManagementApp.Api.Controllers;
using ProjectManagementApp.Application.Features.Tasks.ListTasks;
using ProjectManagementApp.Domain.Enums;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProjectManagementApp.Api.Common;

// `sort` and `status` on both GET /api/projects/{projectId}/tasks and GET /api/tasks are plain
// strings in the action signature, validated against TaskSortMap's whitelist / TaskStatus
// respectively (never bound to a C# enum type — TaskSortMap's "-dueDate"/"-priority" values cannot
// BE C# enum members; mirrors ListProjectsOperationFilter's identical precedent, research R-3).
// This filter only enriches the generated schema with the enum constraints
// docs/contracts/tasks.v1.yaml declares — it changes no runtime validation.
public sealed class ListTasksOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var isListAction = context.MethodInfo.Name is nameof(TasksController.ListProjectTasks) or nameof(TasksController.ListTasks);
        if (!isListAction || operation.Parameters is null)
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
                    schema.Enum = TaskSortValues().Select(v => (JsonNode)v).ToList();
                    schema.Default = TaskSortMap.Default;
                    break;
                case "status":
                    // Deliberately no default here — an omitted filter means "every status", not
                    // "ToDo only". TaskStatus's own schema default of "ToDo" describes a NEW task's
                    // default status, a different context (same distinction 002 draws for Projects).
                    schema.Enum = Enum.GetNames<ProjectManagementApp.Domain.Enums.TaskStatus>().Select(name => (JsonNode)name).ToList();
                    break;
            }
        }
    }

    private static IEnumerable<string> TaskSortValues() =>
        ["dueDate", "-dueDate", "priority", "-priority", "status", "-status", "title", "-title", "createdAt", "-createdAt"];
}
