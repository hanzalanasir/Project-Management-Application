using System.Text.Json.Serialization;

namespace ProjectManagementApp.Application.Features.Dashboard;

/// <summary>
/// Matches the contract's <c>ProjectStatusCounts</c> exactly: five fixed, required properties —
/// never a free-form dictionary. A <c>Dictionary&lt;string,int&gt;</c> was tried first and works
/// correctly at runtime (dictionary keys bypass the API's global camelCase naming policy, so
/// "ToDo" serializes as-is), but Swashbuckle describes any <c>Dictionary&lt;,&gt;</c> as
/// `additionalProperties`, not fixed `required` properties — the drift gate (T003) caught this
/// immediately: it is precisely the "silently became a variable dictionary" failure mode the
/// contract's own comments warn about. <c>[JsonPropertyName]</c> on each property pins the exact
/// PascalCase key despite the app's camelCase policy, so this type gets both a correct schema AND
/// correct wire output.
/// </summary>
public sealed record ProjectStatusCountsDto(
    [property: JsonPropertyName("Planning")] int Planning,
    [property: JsonPropertyName("Active")] int Active,
    [property: JsonPropertyName("OnHold")] int OnHold,
    [property: JsonPropertyName("Completed")] int Completed,
    [property: JsonPropertyName("Cancelled")] int Cancelled);
