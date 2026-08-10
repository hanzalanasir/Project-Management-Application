using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Behaviors;
using ProjectManagementApp.Application.Common.Interfaces;

namespace ProjectManagementApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Order: Logging → Validation (research.md R-4).
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<IProjectAccessPolicy, ProjectAccessPolicy>();
        services.AddScoped<ITaskAccessPolicy, TaskAccessPolicy>();
        services.AddScoped<Features.Tasks.Common.AssigneeValidator>();
        services.AddScoped<ITeamAccessPolicy, TeamAccessPolicy>();

        return services;
    }
}
