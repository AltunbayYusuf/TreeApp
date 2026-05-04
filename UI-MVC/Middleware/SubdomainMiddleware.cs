using System.Net;

namespace IntegratieProject.UI.MVC.Middleware;

public class SubdomainMiddleware
{
    private readonly RequestDelegate _next;

    public SubdomainMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;
        var slug = ExtractSubdomain(host)
                   ?? context.Request.Query["subplatform"].ToString().NullIfWhitespace();

        if (slug != null)
            context.Items["subplatform"] = slug;

        await _next(context);
    }

    private static string? ExtractSubdomain(string host)
    {
        if (IPAddress.TryParse(host, out _))
            return null;

        var parts = host.Split('.');
        if (parts.Length < 2)
            return null;

        return parts[0].NullIfWhitespace();
    }
}

internal static class StringExtensions
{
    public static string? NullIfWhitespace(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
