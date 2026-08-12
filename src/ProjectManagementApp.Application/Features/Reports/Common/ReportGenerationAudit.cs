using System.Text.Json;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Reports.Common;

/// <summary>
/// The one deliberate write this read-side feature performs (research R-1, FR-011): exactly one
/// <c>ReportGenerated</c> row per successful report <b>data</b> request, via 001's
/// <see cref="IActivityLogService.LogAsync"/>. Shared by all four report handlers (Project Progress,
/// Task Completion, Team Performance, Activity) so no user story edits another's file, and by
/// design the catalog endpoint never calls this — metadata is not a generation.
/// </summary>
/// <remarks>
/// <c>entityId</c> is a freshly generated run id, not any domain entity's id — <c>entity_id</c> is
/// <c>varchar(64)</c> precisely so a logical target like <c>Report</c> fits (data-model.md §2).
/// <c>projectId</c> is left <c>null</c>: a report can span several (or zero) projects, so it has no
/// single owning project to stamp — matching 001's convention for project-less events (e.g. `User`
/// actions), which are then Admin-only visible through the scoped read. Gated by
/// <see cref="ReportsOptions.AuditOnGeneration"/> (plan.md Follow-up 3 — should stay <c>true</c> in
/// every deployment; the flag exists for the rare deployment that wants only exports audited, not
/// data production itself).
/// </remarks>
public static class ReportGenerationAudit
{
    public static Task RecordAsync(
        IActivityLogService activityLogService,
        ReportsOptions options,
        CurrentUser caller,
        string reportType,
        object parameters,
        CancellationToken ct)
    {
        if (!options.AuditOnGeneration)
        {
            return Task.CompletedTask;
        }

        var runId = Guid.NewGuid().ToString();
        var changeSummary = $"{reportType} report generated with parameters {JsonSerializer.Serialize(parameters)}";

        return activityLogService.LogAsync(
            actorId: caller.UserId,
            action: nameof(AuditAction.ReportGenerated),
            entityType: "Report",
            entityId: runId,
            changeSummary: changeSummary,
            ct);
    }
}
