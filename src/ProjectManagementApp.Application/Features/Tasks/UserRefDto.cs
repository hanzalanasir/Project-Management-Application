namespace ProjectManagementApp.Application.Features.Tasks;

// Matches contracts/tasks.v1.yaml#/components/schemas/UserRef. isActive is surfaced (not hidden)
// so a deactivated assignee is still shown, flagged — never silently omitted (same convention as
// 002's OwnerRefDto, spec US-002-03 equivalent).
public sealed record UserRefDto(Guid Id, string FullName, bool IsActive);
