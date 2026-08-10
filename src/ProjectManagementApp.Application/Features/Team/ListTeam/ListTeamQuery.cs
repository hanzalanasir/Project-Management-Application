using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Team.ListTeam;

// No Page/PageSize — research R-4: a project team is human-scale, so the query never accepts
// paging parameters at all (not even ones the handler ignores).
public sealed record ListTeamQuery(Guid ProjectId) : IRequest<Result<IReadOnlyList<TeamMemberDto>>>;
