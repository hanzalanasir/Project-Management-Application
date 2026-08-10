using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;

namespace ProjectManagementApp.Application.Features.Team.RemoveTeamMember;

/// <summary>
/// Reads the shared <c>tasks</c> table directly via <see cref="IApplicationDbContext"/> — never
/// calls into a 003 handler, and never mutates a task (research R-5, ADR-0006 addendum). "Open"
/// is <c>status != Done</c>, not an enum allowlist, so a future status added to the workflow stays
/// correctly blocking without a code change here.
/// </summary>
public static class OpenAssignedTaskCheck
{
    public static Task<int> CountBlockingTasksAsync(IApplicationDbContext db, Guid projectId, Guid userId, CancellationToken ct) =>
        db.Tasks.CountAsync(
            t => t.ProjectId == projectId && t.AssigneeId == userId && t.Status != ProjectManagementApp.Domain.Enums.TaskStatus.Done,
            ct);
}
