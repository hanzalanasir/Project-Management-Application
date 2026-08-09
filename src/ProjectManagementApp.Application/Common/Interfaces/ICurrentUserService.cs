using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Common.Interfaces;

// Materialized from the validated JWT. Never populated from a request body or query string.
/// <summary>Resolves the caller's identity from the validated access token — the only source of truth for "who is calling".</summary>
public interface ICurrentUserService
{
    /// <summary>The authenticated caller. Throws if there is no authenticated request in scope.</summary>
    CurrentUser Current { get; }
}
