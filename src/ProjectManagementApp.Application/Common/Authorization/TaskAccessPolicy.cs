using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Common.Authorization;

/// <summary>
/// The graduated authorization model (data-model.md §3) — the core of 003. <see cref="CanMutateAsync"/>
/// resolves the 5 x 3 <see cref="TaskMutation"/> x role matrix in one place: a TeamMember passes
/// only <see cref="TaskMutation.StatusChange"/>, and only when they are the task's assignee.
/// Lives in Application, not Infrastructure — authorization rules are business rules (002 R-1).
/// </summary>
public sealed class TaskAccessPolicy : ITaskAccessPolicy
{
    private readonly IApplicationDbContext _db;

    public TaskAccessPolicy(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    /// <remarks>
    /// TeamMember scope is by ASSIGNMENT, not membership (data-model.md §3) — a TeamMember on a
    /// project's team sees only tasks assigned to them, not every task on that project.
    /// </remarks>
    public IQueryable<TaskItem> ApplyScope(IQueryable<TaskItem> source, CurrentUser caller) => caller.Role switch
    {
        nameof(Role.Admin) => source,
        nameof(Role.ProjectManager) => source.Where(t => t.Project.OwnerId == caller.UserId),
        _ => source.Where(t => t.AssigneeId == caller.UserId),
    };

    /// <inheritdoc />
    public async Task<AccessDecision> CanReadAsync(TaskItem task, CurrentUser caller, CancellationToken ct)
    {
        await Task.CompletedTask;

        return caller.Role switch
        {
            nameof(Role.Admin) => new AccessDecision(true),
            nameof(Role.ProjectManager) => new AccessDecision(task.Project.OwnerId == caller.UserId),
            _ => new AccessDecision(task.AssigneeId == caller.UserId),
        };
    }

    /// <inheritdoc />
    /// <remarks>
    /// data-model.md §3's mutation matrix: Admin and an owning ProjectManager may perform every
    /// mutation kind; a TeamMember may perform ONLY <see cref="TaskMutation.StatusChange"/>, and
    /// only on a task assigned to them. This is the graduated cell — the same caller and the same
    /// row are denied every other mutation kind.
    /// </remarks>
    public async Task<AccessDecision> CanMutateAsync(TaskItem task, TaskMutation mutation, CurrentUser caller, CancellationToken ct)
    {
        await Task.CompletedTask;

        if (caller.Role == nameof(Role.Admin))
        {
            return new AccessDecision(true);
        }

        if (caller.Role == nameof(Role.ProjectManager))
        {
            return new AccessDecision(task.Project.OwnerId == caller.UserId);
        }

        // TeamMember: only StatusChange, and only as the assignee — every other mutation kind is
        // denied even for the assignee (spec's "belt-and-braces" model, structural + behavioural).
        var allowed = mutation == TaskMutation.StatusChange && task.AssigneeId == caller.UserId;
        if (allowed)
        {
            return new AccessDecision(true);
        }

        // The "narrower right" message (quickstart V2) is specific to being refused FullEdit — it
        // names the StatusChange right the assignee DOES have. That phrasing is wrong for every
        // other denied mutation (Create/Reassign/Delete, or StatusChange by a non-assignee): a
        // TeamMember refused Delete does not have a narrower "status" right on this call, and
        // telling them so is actively misleading (T103 finding — surfaced when Delete/Reassign
        // started routing through this policy instead of an attribute-only gate). Leave Reason null
        // for every other case so each handler's own contextual fallback message applies instead.
        var reason = mutation == TaskMutation.FullEdit
            ? "You may update the status of this task, but not its details."
            : null;
        return new AccessDecision(false, reason);
    }
}
