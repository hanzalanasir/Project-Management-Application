using FluentValidation;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Options;

namespace ProjectManagementApp.Application.Features.Projects.CreateProject;

// Invoked automatically by ValidationBehavior — never called by the handler (ADR-0005).
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator(IOptions<ProjectsOptions> options)
    {
        var o = options.Value;

        RuleFor(c => c.Name).NotEmpty().MaximumLength(o.MaxNameLength);
        RuleFor(c => c.Description).MaximumLength(o.MaxDescriptionLength);

        // Cross-field date-order rule (ADR-0005) — only meaningful when EndDate is supplied
        // (an open-ended project with no EndDate is explicitly allowed, spec Edge cases).
        RuleFor(c => c.EndDate)
            .Must((command, endDate) => !endDate.HasValue || endDate.Value >= command.StartDate)
            .WithMessage("End date must not precede start date.")
            .When(c => c.EndDate.HasValue);
    }
}
