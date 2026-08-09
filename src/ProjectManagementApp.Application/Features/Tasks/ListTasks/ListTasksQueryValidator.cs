using FluentValidation;

namespace ProjectManagementApp.Application.Features.Tasks.ListTasks;

// PageSize is deliberately NOT validated here — oversized values are clamped by the handler, never
// rejected (Constitution VI.4, mirrors 002's ListProjectsQueryValidator).
public class ListTasksQueryValidator : AbstractValidator<ListTasksQuery>
{
    public ListTasksQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.Search).MaximumLength(200);

        // Closed whitelist — an unrecognized value is 400, never string-interpolated.
        RuleFor(q => q.Sort)
            .Must(sort => string.IsNullOrEmpty(sort) || TaskSortMap.IsValid(sort))
            .WithMessage(q => $"'{q.Sort}' is not a recognized sort value.");
    }
}
