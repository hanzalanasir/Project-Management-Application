using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using ProjectManagementApp.Api.Controllers;
using ProjectManagementApp.Domain.Enums;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProjectManagementApp.Api.Common;

// `priority`/`status` stay plain strings end-to-end on the request side (matches
// ProjectStatusSchemaFilter/ChangeUserRoleRequestSchemaFilter's precedent and each handler's own
// Enum.TryParse checks), so an invalid value still produces the app's standard ValidationProblem
// rather than a raw model-binding failure. This filter only enriches the generated OpenAPI schema
// with the enum constraints docs/contracts/tasks.v1.yaml declares — it changes no runtime behavior.
public sealed class TaskRequestSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties is null)
        {
            return;
        }

        if (context.Type == typeof(TasksController.CreateTaskRequestBody) || context.Type == typeof(TasksController.UpdateTaskRequestBody))
        {
            if (schema.Properties.TryGetValue("priority", out var prioritySchema) && prioritySchema is OpenApiSchema concretePrioritySchema)
            {
                concretePrioritySchema.Enum = Enum.GetNames<TaskPriority>().Select(name => (JsonNode)name).ToList();
            }
        }

        if (context.Type == typeof(TasksController.UpdateTaskStatusRequestBody))
        {
            if (schema.Properties.TryGetValue("status", out var statusSchema) && statusSchema is OpenApiSchema concreteStatusSchema)
            {
                concreteStatusSchema.Enum = Enum.GetNames<ProjectManagementApp.Domain.Enums.TaskStatus>().Select(name => (JsonNode)name).ToList();
            }
        }
    }
}
