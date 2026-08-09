using FluentValidation;

namespace ProjectManagementApp.Application.Features.Projects.ListProjects;

// PageSize is deliberately NOT validated here — oversized values are clamped by the handler, never
// rejected (contract: "Values above the configured maximum are clamped, not rejected", VI.4).
public class ListProjectsQueryValidator : AbstractValidator<ListProjectsQuery>
{
    public ListProjectsQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.Search).MaximumLength(200);

        // Closed whitelist (research R-3) — an unrecognized value is 400, never string-interpolated.
        RuleFor(q => q.Sort)
            .Must(sort => string.IsNullOrEmpty(sort) || ProjectSortMap.IsValid(sort))
            .WithMessage(q => $"'{q.Sort}' is not a recognized sort value.");
    }
}
