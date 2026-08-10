using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Team.AddTeamMember;

// Matches contracts/team.v1.yaml#/components/schemas/AddTeamMemberRequest. ProjectId comes from
// the ROUTE, never the body — the contract's AddTeamMemberRequest deliberately has no projectId
// property, so a membership cannot be smuggled into another project by a widened payload.
public sealed record AddTeamMemberCommand(Guid ProjectId, Guid UserId) : IRequest<Result<TeamMemberDto>>;
