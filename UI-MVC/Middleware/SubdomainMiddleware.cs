using System.Net;
using Microsoft.Extensions.Configuration;

namespace IntegratieProject.UI.MVC.Middleware;

public class SubdomainMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string? _defaultSubplatform;
    private readonly string? _rootDomain;
    private readonly string? _appSubdomain;

    public SubdomainMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _defaultSubplatform = configuration["DefaultSubplatform"].NullIfWhitespace();
        _rootDomain = configuration["RootDomain"].NullIfWhitespace();
        _appSubdomain = configuration["AppSubdomain"].NullIfWhitespace();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;
        var fromSubdomain = ExtractSubdomain(host, _rootDomain, _appSubdomain);
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

    private static string? ExtractSubdomain(string host, string? rootDomain, string? appSubdomain)
    {
        if (IPAddress.TryParse(host, out _))
            return null;

        if (string.IsNullOrEmpty(rootDomain))
            return null;

        var suffix = "." + rootDomain;
        if (!host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            return null;

        var subdomain = host[..^suffix.Length];

        if (!string.IsNullOrEmpty(appSubdomain) &&
            subdomain.Equals(appSubdomain, StringComparison.OrdinalIgnoreCase))
            return null;

        return subdomain.NullIfWhitespace();
    }
}

internal static class StringExtensions
{
    public static string? NullIfWhitespace(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
