using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Api.Common;

// The only place the ErrorKind -> HTTP status mapping exists (shared-contracts.md §1, research.md R-8).
public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result, int onSuccess = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return new StatusCodeResult(onSuccess);
        }

        return ToProblemResult(result.Error!);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, int onSuccess = StatusCodes.Status200OK, string? location = null)
    {
        if (result.IsSuccess)
        {
            if (onSuccess == StatusCodes.Status201Created && location is not null)
            {
                return new CreatedResult(location, result.Value);
            }

            return new ObjectResult(result.Value) { StatusCode = onSuccess };
        }

        return ToProblemResult(result.Error!);
    }

    private static IActionResult ToProblemResult(Error error)
    {
        // Exhaustive switch, no default/discard arm for any named ErrorKind member — a missed
        // member fails the build (CS8524, and Directory.Build.props sets TreatWarningsAsErrors).
        // CS8524 also fires for a switch that DOES cover every named member, because C# enums are
        // open to arbitrary underlying int values; that residual case is suppressed below since
        // Error.Kind is always constructed from the enum type itself, never an arbitrary cast.
#pragma warning disable CS8524
        var (status, title) = error.Kind switch
        {
            ErrorKind.Validation => (StatusCodes.Status400BadRequest, "Validation failed"),
            ErrorKind.Unauthenticated => (StatusCodes.Status401Unauthorized, "Authentication required"),
            ErrorKind.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            ErrorKind.NotFound => (StatusCodes.Status404NotFound, error.Message),
            ErrorKind.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ErrorKind.UnprocessableContent => (StatusCodes.Status422UnprocessableEntity, "Unprocessable — result set too large"),
            ErrorKind.Unexpected => (StatusCodes.Status500InternalServerError, "Unexpected error"),
        };
#pragma warning restore CS8524

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = error.Kind == ErrorKind.Unexpected ? null : error.Message
        };

        if (error.Fields is { Count: > 0 })
        {
            problemDetails.Extensions["errors"] = error.Fields;
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" }
        };
    }
}
