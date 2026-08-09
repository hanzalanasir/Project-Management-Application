using Microsoft.AspNetCore.Http;
using ProjectManagementApp.Api.Configuration;

namespace ProjectManagementApp.Api.Common;

// Shared write/clear for the refresh-token cookie — used by Login, Refresh, and Logout so the
// attributes (HttpOnly, Secure, SameSite, Path) are declared in exactly one place.
public static class RefreshCookieExtensions
{
    public static void SetRefreshCookie(this HttpResponse response, string rawRefreshToken, DateTimeOffset expiresAt, RefreshCookieOptions options)
    {
        response.Cookies.Append(options.Name, rawRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = options.Secure,
            SameSite = ParseSameSite(options.SameSite),
            Path = options.Path,
            Expires = expiresAt
        });
    }

    public static void ClearRefreshCookie(this HttpResponse response, RefreshCookieOptions options)
    {
        response.Cookies.Append(options.Name, string.Empty, new CookieOptions
        {
            HttpOnly = true,
            Secure = options.Secure,
            SameSite = ParseSameSite(options.SameSite),
            Path = options.Path,
            Expires = DateTimeOffset.UnixEpoch
        });
    }

    private static SameSiteMode ParseSameSite(string value) => value switch
    {
        "Strict" => SameSiteMode.Strict,
        "Lax" => SameSiteMode.Lax,
        "None" => SameSiteMode.None,
        _ => SameSiteMode.Strict
    };
}
