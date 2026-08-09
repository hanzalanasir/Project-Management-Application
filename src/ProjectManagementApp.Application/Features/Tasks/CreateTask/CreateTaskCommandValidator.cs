using FluentValidation;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Options;

namespace ProjectManagementApp.Application.Features.Tasks.CreateTask;

// Invoked automatically by ValidationBehavior — never called by the handler (ADR-0005). Database-
// dependent rules (assignee pool, due-date window) live in the handler instead, since they need
// the loaded parent Project (data-model.md, research R-6).
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator(IOptions<TasksOptions> options)
    {
        var o = options.Value;

        RuleFor(c => c.Title).NotEmpty().MaximumLength(o.MaxTitleLength);
        RuleFor(c => c.Description).MaximumLength(o.MaxDescriptionLength);
    }
}
