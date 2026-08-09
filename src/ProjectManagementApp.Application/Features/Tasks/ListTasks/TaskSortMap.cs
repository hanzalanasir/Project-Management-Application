using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Features.Tasks.ListTasks;

// Closed whitelist mapping ?sort= values to OrderBy expressions (data-model.md §5, mirrors 002's
// ProjectSortMap). An unrecognized value is rejected at the validator before this is ever called
// with untrusted input — Apply's exception is a defensive backstop, not the primary gate.
public static class TaskSortMap
{
    private static readonly IReadOnlyDictionary<string, Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>> Map =
        new Dictionary<string, Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>>
        {
            ["dueDate"] = q => q.OrderBy(t => t.DueDate),
            ["-dueDate"] = q => q.OrderByDescending(t => t.DueDate),
            ["priority"] = q => q.OrderBy(t => t.Priority),
            ["-priority"] = q => q.OrderByDescending(t => t.Priority),
            ["status"] = q => q.OrderBy(t => t.Status),
            ["-status"] = q => q.OrderByDescending(t => t.Status),
            ["title"] = q => q.OrderBy(t => t.Title),
            ["-title"] = q => q.OrderByDescending(t => t.Title),
            ["createdAt"] = q => q.OrderBy(t => t.CreatedAt),
            ["-createdAt"] = q => q.OrderByDescending(t => t.CreatedAt),
        };

    public const string Default = "dueDate";

    public static bool IsValid(string sort) => Map.ContainsKey(sort);

    public static IOrderedQueryable<TaskItem> Apply(IQueryable<TaskItem> query, string sort)
    {
        if (!Map.TryGetValue(sort, out var apply))
        {
            throw new ArgumentException($"Unrecognized sort value: '{sort}'.", nameof(sort));
        }

        return apply(query);
    }
}
