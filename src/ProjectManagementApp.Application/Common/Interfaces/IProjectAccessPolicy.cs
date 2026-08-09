using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Common.Interfaces;

// Shared-kernel interface; rules owned/implemented by 002, reused by 005, 006.
public interface IProjectAccessPolicy
{
    IQueryable<Project> ApplyScope(IQueryable<Project> source, CurrentUser caller);
    Task<AccessDecision> CanReadAsync(Project project, CurrentUser caller, CancellationToken ct);
    Task<AccessDecision> CanMutateAsync(Project project, CurrentUser caller, CancellationToken ct);
}
