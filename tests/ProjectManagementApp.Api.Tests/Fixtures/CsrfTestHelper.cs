namespace ProjectManagementApp.Api.Tests.Fixtures;

// Angular's HttpClient automatically echoes the XSRF-TOKEN cookie back as X-XSRF-TOKEN for
// state-changing requests; plain HttpClient in tests must do this manually.
public static class CsrfTestHelper
{
    public static string ExtractXsrfToken(HttpResponseMessage response)
    {
        response.Headers.TryGetValues("Set-Cookie", out var cookies);
        var xsrfCookie = cookies?.SingleOrDefault(c => c.StartsWith("XSRF-TOKEN="))
            ?? throw new InvalidOperationException("Response did not set an XSRF-TOKEN cookie.");

        var pair = xsrfCookie.Split(';')[0];
        return Uri.UnescapeDataString(pair["XSRF-TOKEN=".Length..]);
    }

    public static HttpRequestMessage WithCsrfHeader(this HttpRequestMessage request, string xsrfToken)
    {
        request.Headers.Add("X-XSRF-TOKEN", xsrfToken);
        return request;
    }
}
