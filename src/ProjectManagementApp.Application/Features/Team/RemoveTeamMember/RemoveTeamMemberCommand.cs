using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Team.RemoveTeamMember;

// Both ProjectId and UserId come from the route — no request body, no validator needed (same
// shape as DeleteTaskCommand).
public sealed record RemoveTeamMemberCommand(Guid ProjectId, Guid UserId) : IRequest<Result>;
