using Microsoft.AspNetCore.Http;

namespace ProjectManagementApp.Api.Common;

// Shared xmin<->ETag/If-Match helper (ADR-0007 §3). Created here in 001 (research.md R-15) —
// promoted from 002's original plan since 001 needed it first for Admin user management.
// 002's T017/T018 verify and reuse this file instead of creating a second one.
public static class ETagExtensions
{
    public static string FormatETag(uint version) => $"\"{version}\"";

    public static bool TryParseETag(string? raw, out uint version)
    {
        version = 0;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var trimmed = raw.Trim().Trim('"');
        return uint.TryParse(trimmed, out version);
    }

    public static void WriteETag(this HttpResponse response, uint version)
    {
        response.Headers.ETag = FormatETag(version);
    }

    public static bool TryParseIfMatch(this HttpRequest request, out uint version)
    {
        var header = request.Headers.IfMatch.ToString();
        return TryParseETag(header, out version);
    }
}
