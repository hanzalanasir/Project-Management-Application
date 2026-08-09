using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using ProjectManagementApp.Api.Controllers;
using ProjectManagementApp.Domain.Enums;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProjectManagementApp.Api.Common;

// Role stays a plain string end-to-end (matches UserDto/AdminUserSummary/AdminUserDetail and
// ChangeUserRoleCommandValidator's own Enum.TryParse check) so an invalid value still produces the
// app's standard ValidationProblem via FluentValidation rather than a raw model-binding failure.
// This filter only enriches the generated OpenAPI schema with the enum constraint the contract
// declares (docs/contracts/auth.v1.yaml's Role schema) — it changes no runtime behavior.
public sealed class ChangeUserRoleRequestSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(UsersController.ChangeUserRoleRequest) || schema.Properties is null)
        {
            return;
        }

        if (schema.Properties.TryGetValue("role", out var roleSchema) && roleSchema is OpenApiSchema concreteRoleSchema)
        {
            concreteRoleSchema.Enum = Enum.GetNames<Role>().Select(name => (JsonNode)name).ToList();
        }
    }
}
