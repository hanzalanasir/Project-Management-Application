using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Dashboard.Common;

/// <summary>
/// The one scope chain every 005 aggregate composes against (data-model.md §2). Reuses 002's
/// <see cref="IProjectAccessPolicy.ApplyScope"/> — 005 defines no scope logic of its own.
/// </summary>
public static class VisibleProjectScope
{
    /// <summary>
    /// Returns the caller's visible-project ids as an <b>un-materialized</b> <see cref="IQueryable{T}"/>.
    /// Never call <c>ToListAsync()</c> on the result before composing it into another query — for an
    /// Admin that round-trips the entire projects table and turns every aggregate that consumes it
    /// into a fetch-then-filter (research R-4, tasks.md T008).
    /// </summary>
    public static IQueryable<Guid> Resolve(IApplicationDbContext db, IProjectAccessPolicy accessPolicy, CurrentUser caller) =>
        accessPolicy.ApplyScope(db.Projects, caller).Select(p => p.Id);
}
