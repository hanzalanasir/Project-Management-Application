using FluentValidation;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Options;

namespace ProjectManagementApp.Application.Features.Tasks.UpdateTask;

// Invoked automatically by ValidationBehavior — never called by the handler (ADR-0005).
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator(IOptions<TasksOptions> options)
    {
        var o = options.Value;

        RuleFor(c => c.Title).NotEmpty().MaximumLength(o.MaxTitleLength);
        RuleFor(c => c.Description).MaximumLength(o.MaxDescriptionLength);
        RuleFor(c => c.Priority).NotEmpty();
    }
}
