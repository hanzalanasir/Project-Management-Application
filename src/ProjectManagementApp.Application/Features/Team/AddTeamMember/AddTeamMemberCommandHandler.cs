using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Team.AddTeamMember;

/// <summary>
/// Fixed order (data-model.md §5): load the parent project (404) → <c>CanManageTeamAsync</c> (403)
/// → target user exists (404) and is active (400, the only add-time gate, FR-016) → friendly
/// not-already-a-member pre-check (409) → insert + <c>TeamMemberAdded</c> audit in one
/// <c>SaveChangesAsync</c>, wrapped to also catch a genuine SQLSTATE 23505 race and map it to the
/// same 409 (research R-3). The pre-check alone would 500 on the losing side of a real race —
/// ADR-0003 forbids an expected outcome escaping as an unhandled exception. The catch here
/// duplicates <c>Infrastructure.Persistence.UniqueViolationMapper</c>'s one-line check rather than
/// referencing it: Application must not reference Infrastructure (Constitution II.2), and
/// <c>Npgsql.EntityFrameworkCore.PostgreSQL</c> is already a direct Application package for
/// <c>EF.Functions.ILike</c>, so the type is available here without crossing the layer boundary.
/// </summary>
public class AddTeamMemberCommandHandler : IRequestHandler<AddTeamMemberCommand, Result<TeamMemberDto>>
{
    private const string UniqueViolationSqlState = "23505";

    private readonly IApplicationDbContext _db;
    private readonly ITeamAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLog;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TeamOptions _options;

    public AddTeamMemberCommandHandler(
        IApplicationDbContext db,
        ITeamAccessPolicy accessPolicy,
        ICurrentUserService currentUserService,
        IActivityLogService activityLog,
        UserManager<ApplicationUser> userManager,
        IOptions<TeamOptions> options)
    {
        _db = db;
        _accessPolicy = accessPolicy;
        _currentUserService = currentUserService;
        _activityLog = activityLog;
        _userManager = userManager;
        _options = options.Value;
    }

    public async Task<Result<TeamMemberDto>> Handle(AddTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        var project = await _db.Projects.SingleOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Result<TeamMemberDto>.Failure(new Error(ErrorKind.NotFound, "No project exists with that id."));
        }

        var decision = await _accessPolicy.CanManageTeamAsync(project, caller, cancellationToken);
        if (!decision.Allowed)
        {
            return Result<TeamMemberDto>.Failure(new Error(ErrorKind.Forbidden, decision.Reason ?? "You do not have access to manage this project's team."));
        }

        var targetUser = await _db.Users.SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (targetUser is null)
        {
            return Result<TeamMemberDto>.Failure(new Error(ErrorKind.NotFound, "No user exists with that id."));
        }

        if (!targetUser.IsActive && !_options.AllowAddInactiveUser)
        {
            return Result<TeamMemberDto>.Failure(new Error(
                ErrorKind.Validation, "Validation failed",
                new Dictionary<string, string[]> { ["userId"] = new[] { "User is deactivated and cannot be added to a team." } }));
        }

        var alreadyMember = await _db.TeamMembers.AnyAsync(m => m.ProjectId == project.Id && m.UserId == request.UserId, cancellationToken);
        if (alreadyMember)
        {
            return Result<TeamMemberDto>.Failure(new Error(ErrorKind.Conflict, "User is already a member of this project."));
        }

        var member = new TeamMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Project = project,
            UserId = targetUser.Id,
            User = targetUser,
            AddedBy = caller.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.TeamMembers.Add(member);

        await _activityLog.LogAsync(
            actorId: caller.UserId,
            action: nameof(AuditAction.TeamMemberAdded),
            entityType: "TeamMember",
            entityId: member.Id.ToString(),
            changeSummary: $"User '{targetUser.Email}' added to project '{project.Name}'",
            cancellationToken, projectId: project.Id);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: UniqueViolationSqlState })
        {
            // The database, not this handler's pre-check above, is what makes the concurrency
            // guarantee true (research R-2/R-3) — the losing side of a genuine race lands here.
            return Result<TeamMemberDto>.Failure(new Error(ErrorKind.Conflict, "User is already a member of this project."));
        }

        var role = (await _userManager.GetRolesAsync(targetUser)).Single();
        return Result<TeamMemberDto>.Success(member.ToDto(role));
    }
}
