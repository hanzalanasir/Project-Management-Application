using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Common.Interfaces;

// Shared-kernel interface; rules owned/implemented by 003, reused by 005, 006.
public interface ITaskAccessPolicy
{
    IQueryable<TaskItem> ApplyScope(IQueryable<TaskItem> source, CurrentUser caller);
    Task<AccessDecision> CanReadAsync(TaskItem task, CurrentUser caller, CancellationToken ct);
    Task<AccessDecision> CanMutateAsync(TaskItem task, TaskMutation mutation, CurrentUser caller, CancellationToken ct);
}
