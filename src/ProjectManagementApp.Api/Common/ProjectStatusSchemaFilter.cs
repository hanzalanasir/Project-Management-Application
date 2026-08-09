using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using ProjectManagementApp.Application.Features.Projects.CreateProject;
using ProjectManagementApp.Api.Controllers;
using ProjectManagementApp.Domain.Enums;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProjectManagementApp.Api.Common;

// Status stays a plain string end-to-end on the request side (matches
// ChangeUserRoleRequestSchemaFilter's precedent and CreateProjectCommandHandler/
// UpdateProjectCommandHandler's own Enum.TryParse checks) so an invalid value still produces the
// app's standard ValidationProblem rather than a raw model-binding failure. This filter only
// enriches the generated OpenAPI schema with the enum constraint the contract declares
// (docs/contracts/projects.v1.yaml's ProjectStatus schema) — it changes no runtime behavior.
public sealed class ProjectStatusSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(CreateProjectCommand) && context.Type != typeof(ProjectsController.UpdateProjectRequestBody))
        {
            return;
        }

        if (schema.Properties is null)
        {
            return;
        }

        if (schema.Properties.TryGetValue("status", out var statusSchema) && statusSchema is OpenApiSchema concreteStatusSchema)
        {
            concreteStatusSchema.Enum = Enum.GetNames<ProjectStatus>().Select(name => (JsonNode)name).ToList();
        }
    }
}
