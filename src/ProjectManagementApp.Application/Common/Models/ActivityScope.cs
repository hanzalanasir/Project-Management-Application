namespace ProjectManagementApp.Application.Common.Models;

public sealed record ActivityScope(IReadOnlyCollection<Guid> VisibleProjectIds, bool Unscoped);
