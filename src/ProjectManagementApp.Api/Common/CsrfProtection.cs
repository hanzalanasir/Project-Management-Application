using System.Security.Cryptography;
using ProjectManagementApp.Api.Configuration;

namespace ProjectManagementApp.Api.Common;

// A deliberately simple double-submit-cookie check for /refresh and /logout (FR-016) — matches
// exactly what Angular's built-in HttpClient XSRF support does: read a readable cookie, echo it
// back as a header. ASP.NET Core's IAntiforgery was tried first, but its cookie and header values
// are cryptographically linked, not equal strings, which does not match Angular's simpler
// synchronizer-token assumption. SameSite=Strict on the auth cookies is the primary defense;
// this is defense-in-depth for the two endpoints that mutate server-side session state.
public static class CsrfProtection
{
    public const string CookieName = "XSRF-TOKEN";

    public static void IssueToken(HttpResponse response, CsrfOptions options)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = false, // must be JS-readable so the client can echo it back as a header
            Secure = options.Secure,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
    }

    public static bool IsValid(HttpRequest request, CsrfOptions options)
    {
        var cookieValue = request.Cookies[CookieName];
        var headerValue = request.Headers[options.HeaderName].ToString();

        return !string.IsNullOrEmpty(cookieValue)
            && !string.IsNullOrEmpty(headerValue)
            && cookieValue == headerValue;
    }
}

public class CsrfOptions
{
    public string HeaderName { get; set; } = "X-XSRF-TOKEN";
    public bool Secure { get; set; } = true;
}
