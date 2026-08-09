using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;

namespace ProjectManagementApp.Application.Features.Tasks.Common;

// Shared by CreateTask and ReassignTask (research R-6). Reads the shared team_members entity
// read-only — never calls a 004 handler and never writes it.
public class AssigneeValidator
{
    private readonly IApplicationDbContext _db;

    public AssigneeValidator(IApplicationDbContext db)
    {
        _db = db;
    }

    // A null assigneeId (unassign) is always legal — only a supplied candidate is checked against
    // the project's team pool and activity.
    public async Task<bool> IsEligibleAsync(Guid projectId, Guid? assigneeId, CancellationToken ct)
    {
        if (assigneeId is null)
        {
            return true;
        }

        return await _db.TeamMembers.AnyAsync(
            tm => tm.ProjectId == projectId && tm.UserId == assigneeId && tm.User.IsActive, ct);
    }
}
