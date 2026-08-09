using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Features.Projects.ListProjects;

// Closed whitelist mapping ?sort= values to OrderBy expressions (research R-3). An unrecognized
// value is rejected at the validator (ListProjectsQueryValidator, T043) before this is ever
// called with untrusted input — Apply's exception is a defensive backstop, not the primary gate.
public static class ProjectSortMap
{
    private static readonly IReadOnlyDictionary<string, Func<IQueryable<Project>, IOrderedQueryable<Project>>> Map =
        new Dictionary<string, Func<IQueryable<Project>, IOrderedQueryable<Project>>>
        {
            ["name"] = q => q.OrderBy(p => p.Name),
            ["-name"] = q => q.OrderByDescending(p => p.Name),
            ["startDate"] = q => q.OrderBy(p => p.StartDate),
            ["-startDate"] = q => q.OrderByDescending(p => p.StartDate),
            ["endDate"] = q => q.OrderBy(p => p.EndDate),
            ["-endDate"] = q => q.OrderByDescending(p => p.EndDate),
            ["status"] = q => q.OrderBy(p => p.Status),
            ["-status"] = q => q.OrderByDescending(p => p.Status),
            ["createdAt"] = q => q.OrderBy(p => p.CreatedAt),
            ["-createdAt"] = q => q.OrderByDescending(p => p.CreatedAt),
        };

    public const string Default = "-createdAt";

    public static bool IsValid(string sort) => Map.ContainsKey(sort);

    public static IOrderedQueryable<Project> Apply(IQueryable<Project> query, string sort)
    {
        if (!Map.TryGetValue(sort, out var apply))
        {
            throw new ArgumentException($"Unrecognized sort value: '{sort}'.", nameof(sort));
        }

        return apply(query);
    }
}
