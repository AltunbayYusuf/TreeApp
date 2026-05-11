using System.Net;
using Microsoft.Extensions.Configuration;

namespace IntegratieProject.UI.MVC.Middleware;

public class SubdomainMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string? _defaultSubplatform;

    public SubdomainMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _defaultSubplatform = configuration["DefaultSubplatform"].NullIfWhitespace();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;
        var fromSubdomain = ExtractSubdomain(host);
        var fromRoute = context.GetRouteValue("subplatform")?.ToString().NullIfWhitespace();
        var fromQuery = context.Request.Query["subplatform"].ToString().NullIfWhitespace();
        var fromCookie = context.Request.Cookies["dev_subplatform"].NullIfWhitespace();

        var slug = fromSubdomain ?? fromRoute ?? fromQuery ?? fromCookie ?? _defaultSubplatform;

        if (fromSubdomain == null && (fromRoute ?? fromQuery) is { } devSlug && devSlug != fromCookie)
            context.Response.Cookies.Append("dev_subplatform", devSlug, new CookieOptions { SameSite = SameSiteMode.Lax });

        if (slug != null)
            context.Items["subplatform"] = slug;

        await _next(context);
    }

    private static string? ExtractSubdomain(string host)
    {
        if (IPAddress.TryParse(host, out _))
            return null;

        var parts = host.Split('.');
        if (parts.Length < 4)
            return null;

        return parts[0].NullIfWhitespace();
    }
}

internal static class StringExtensions
{
    public static string? NullIfWhitespace(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
