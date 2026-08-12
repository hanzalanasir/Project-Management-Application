using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Reports.Common;

/// <summary>
/// Resolves a report's <c>projectScope</c> query parameter into the caller's actual visible-project
/// set. Reuses 002's <see cref="IProjectAccessPolicy.ApplyScope"/> exactly as 005's
/// <c>VisibleProjectScope</c> does — 006 defines no scope logic of its own (data-model.md §3).
/// </summary>
/// <remarks>
/// <b>Parameters narrow within scope; they never widen it</b> (data-model.md §3). Three branches:
/// <list type="bullet">
/// <item><c>null</c>/<c>"all"</c> — silently narrowed to the caller's visible set, never a 403,
/// because nothing was named (research R-7).</item>
/// <item>a comma-separated list where <b>every</b> id is in scope — succeeds, narrowed to exactly
/// those ids.</item>
/// <item>a list where <b>any</b> id is out of scope — the whole request 403s. Silently dropping the
/// out-of-scope id and returning the rest would misrepresent the report as covering everything
/// asked for (FR-004).</item>
/// </list>
/// A malformed id or an empty list is a 400, not a 403 — it never reached the scope check.
/// </remarks>
public static class ReportScope
{
    public static async Task<Result<IQueryable<Guid>>> ResolveAsync(
        IApplicationDbContext db,
        IProjectAccessPolicy accessPolicy,
        CurrentUser caller,
        string? projectScope,
        CancellationToken ct)
    {
        // Un-materialized — never ToListAsync() this for "all"; an Admin's visible set can be the
        // whole projects table, and every aggregate that composes it as a subquery instead of a
        // materialized list is what keeps filter-at-source true (005 T008's identical reasoning).
        var visibleProjectIds = accessPolicy.ApplyScope(db.Projects, caller).Select(p => p.Id);

        if (string.IsNullOrWhiteSpace(projectScope) || projectScope == "all")
        {
            return Result<IQueryable<Guid>>.Success(visibleProjectIds);
        }

        var rawIds = projectScope.Split(',', StringSplitOptions.TrimEntries);
        var namedIds = new List<Guid>(rawIds.Length);

        foreach (var raw in rawIds)
        {
            if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var id))
            {
                return Result<IQueryable<Guid>>.Failure(new Error(
                    ErrorKind.Validation, "Validation failed",
                    new Dictionary<string, string[]>
                    {
                        ["projectScope"] = ["'projectScope' must be 'all' or a comma-separated list of project ids."],
                    }));
            }

            namedIds.Add(id);
        }

        // Filtering the visible set down to the named ids, rather than trusting namedIds outright,
        // is what makes this a single EF-translatable query — and (via the count check below)
        // proves every named id really is in scope, not just well-formed.
        var namedInScope = visibleProjectIds.Where(id => namedIds.Contains(id));
        var inScopeCount = await namedInScope.CountAsync(ct);

        if (inScopeCount != namedIds.Count)
        {
            return Result<IQueryable<Guid>>.Failure(new Error(
                ErrorKind.Forbidden,
                "One or more of the named projects are outside your visible scope."));
        }

        // Reuses the same EF-backed subquery rather than namedIds.AsQueryable() — the latter is a
        // LINQ-to-Objects queryable, not IAsyncEnumerable, and breaks the moment a handler composes
        // it into a further EF query or calls ToListAsync() on it directly.
        return Result<IQueryable<Guid>>.Success(namedInScope);
    }
}
