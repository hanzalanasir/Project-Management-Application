using FluentValidation;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Options;

namespace ProjectManagementApp.Application.Features.Projects.UpdateProject;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator(IOptions<ProjectsOptions> options)
    {
        var o = options.Value;

        RuleFor(c => c.Name).NotEmpty().MaximumLength(o.MaxNameLength);
        RuleFor(c => c.Description).MaximumLength(o.MaxDescriptionLength);

        RuleFor(c => c.EndDate)
            .Must((command, endDate) => !endDate.HasValue || endDate.Value >= command.StartDate)
            .WithMessage("End date must not precede start date.")
            .When(c => c.EndDate.HasValue);
    }
}
