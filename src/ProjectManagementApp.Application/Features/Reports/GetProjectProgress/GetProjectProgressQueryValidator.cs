using FluentValidation;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.Common;

namespace ProjectManagementApp.Application.Features.Reports.GetProjectProgress;

// "required" is owned here, not by ASP.NET's default non-nullable-query-param 400 — from/to are
// bound as nullable DateOnly? in the controller specifically so this validator's message (not a
// generic model-binding error) is what the caller sees (research/plan convention shared by every
// 006 report validator).
public sealed class GetProjectProgressQueryValidator : AbstractValidator<GetProjectProgressQuery>
{
    public GetProjectProgressQueryValidator(IOptions<ReportsOptions> options)
    {
        var maxWindowDays = options.Value.MaxWindowDays;

        RuleFor(q => q.From).NotNull().WithMessage("'from' is required.");
        RuleFor(q => q.To).NotNull().WithMessage("'to' is required.");

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
