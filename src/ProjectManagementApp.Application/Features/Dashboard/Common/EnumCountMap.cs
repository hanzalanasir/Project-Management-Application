using ProjectManagementApp.Application.Features.Dashboard;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Dashboard.Common;

/// <summary>
/// Builds an enum-keyed count map pre-populated with <b>every</b> value of <typeparamref name="TEnum"/>
/// at zero, then merges query results in. This is what makes the summary payload a stable typed
/// contract rather than a variable dictionary (data-model.md §4, contract's <c>additionalProperties:
/// false</c>) — a status with no rows appears as <c>0</c>, never omitted.
/// </summary>
public static class EnumCountMap
{
    public static Dictionary<TEnum, int> Build<TEnum>(IEnumerable<(TEnum Status, int Count)> counts)
        where TEnum : struct, Enum
    {
        var seeded = Enum.GetValues<TEnum>().ToDictionary(value => value, _ => 0);

        foreach (var (status, count) in counts)
        {
            seeded[status] = count;
        }

        return seeded;
    }

    /// <summary>
    /// Converts the typed enum-keyed map to the fixed-shape <see cref="ProjectStatusCountsDto"/> at
    /// the DTO boundary. A plain <c>Dictionary&lt;,&gt;</c> was tried first — see that type's
    /// remarks for why Swashbuckle describes it as a free-form map rather than the contract's fixed
    /// `required` properties, which fails the drift gate (T003).
    /// </summary>
    public static ProjectStatusCountsDto ToProjectStatusCountsDto(Dictionary<ProjectStatus, int> map) =>
        new(map[ProjectStatus.Planning], map[ProjectStatus.Active], map[ProjectStatus.OnHold], map[ProjectStatus.Completed], map[ProjectStatus.Cancelled]);

    /// <summary>Converts the typed enum-keyed map to the fixed-shape <see cref="TaskStatusCountsDto"/> — see <see cref="ToProjectStatusCountsDto"/>.</summary>
    public static TaskStatusCountsDto ToTaskStatusCountsDto(Dictionary<ProjectManagementApp.Domain.Enums.TaskStatus, int> map) =>
        new(map[ProjectManagementApp.Domain.Enums.TaskStatus.ToDo], map[ProjectManagementApp.Domain.Enums.TaskStatus.InProgress],
            map[ProjectManagementApp.Domain.Enums.TaskStatus.InReview], map[ProjectManagementApp.Domain.Enums.TaskStatus.Done],
            map[ProjectManagementApp.Domain.Enums.TaskStatus.Blocked]);
}
