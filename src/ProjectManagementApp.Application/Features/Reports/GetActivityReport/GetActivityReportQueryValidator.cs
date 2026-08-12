using FluentValidation;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.Common;

namespace ProjectManagementApp.Application.Features.Reports.GetActivityReport;

// pageSize is deliberately NOT validated — oversized values are clamped by the handler, never
// rejected (Constitution VI.4, same convention as 002-005's list endpoints and T034's Dashboard
// activity validator). page IS validated: page=-1 must 400, not silently clamp to 1.
public sealed class GetActivityReportQueryValidator : AbstractValidator<GetActivityReportQuery>
{
    private static readonly string[] AllowedEntityTypes = ["User", "Project", "Task", "TeamMember", "Report"];

    public GetActivityReportQueryValidator(IOptions<ReportsOptions> options)
    {
        var maxWindowDays = options.Value.MaxWindowDays;

        RuleFor(q => q.From).NotNull().WithMessage("'from' is required.");
        RuleFor(q => q.To).NotNull().WithMessage("'to' is required.");
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);

        RuleFor(q => q.EntityType)
            .Must(t => t is null || AllowedEntityTypes.Contains(t))
            .WithMessage("'entityType' must be one of: User, Project, Task, TeamMember, Report.");

        RuleFor(q => q)
            .Must(q => q.From is null || q.To is null || ReportWindow.IsOrdered(q.From.Value, q.To.Value))
            .WithMessage("'from' must be on or before 'to'.")
            .WithName("from");

        RuleFor(q => q)
            .Must(q => q.From is null || q.To is null || ReportWindow.WithinMaxSpan(q.From.Value, q.To.Value, maxWindowDays))
            .WithMessage($"The window may not exceed {maxWindowDays} days.")
            .WithName("to");
    }
}
