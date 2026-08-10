using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Features.Team.ListTeam;

/// <summary>
/// Fixed order (data-model.md §5): load the parent project (404) → <c>CanViewTeamAsync</c> (403) —
/// run BEFORE any roster row is materialized, never as a post-filter — then a single bounded read
/// of this project's <c>team_members</c> joined to <c>Users</c> via <c>Include</c> (no per-row
/// lookup for name/email). Role is read-only and GLOBAL: <see cref="UserManager{TUser}"/>'s
/// <c>BuildRoleLookupAsync</c> (T014) costs exactly 3 queries regardless of roster size, never one
/// per row. Returns <see cref="IReadOnlyList{T}"/> directly — not <c>PagedResult&lt;T&gt;</c>
/// (research R-4): a project team is human-scale, so the &gt;50-item paging trigger never fires.
/// </summary>
public class ListTeamQueryHandler : IRequestHandler<ListTeamQuery, Result<IReadOnlyList<TeamMemberDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITeamAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TeamOptions _options;

    public ListTeamQueryHandler(
        IApplicationDbContext db,
        ITeamAccessPolicy accessPolicy,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager,
        IOptions<TeamOptions> options)
    {
        _db = db;
        _accessPolicy = accessPolicy;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _options = options.Value;
    }

    public async Task<Result<IReadOnlyList<TeamMemberDto>>> Handle(ListTeamQuery request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        var project = await _db.Projects.SingleOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Result<IReadOnlyList<TeamMemberDto>>.Failure(new Error(ErrorKind.NotFound, "No project exists with that id."));
        }

        var decision = await _accessPolicy.CanViewTeamAsync(project, caller, cancellationToken);
        if (!decision.Allowed)
        {
            return Result<IReadOnlyList<TeamMemberDto>>.Failure(new Error(ErrorKind.Forbidden, decision.Reason ?? "You do not have access to view this project's team."));
        }

        IQueryable<TeamMember> query = _db.TeamMembers.AsNoTracking()
            .Include(m => m.User)
            .Where(m => m.ProjectId == project.Id);

        if (!_options.IncludeInactiveMembersInRoster)
        {
            query = query.Where(m => m.User.IsActive);
        }

        var members = await query.OrderBy(m => m.CreatedAt).ToListAsync(cancellationToken);

        var roleLookup = await _userManager.BuildRoleLookupAsync(cancellationToken);

        var items = members
            .Select(m => m.ToDto(roleLookup.TryGetValue(m.UserId, out var role) ? role : "Unknown"))
            .ToList();

        return Result<IReadOnlyList<TeamMemberDto>>.Success(items);
    }
}
