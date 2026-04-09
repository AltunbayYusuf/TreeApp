namespace IntergratieProject.UI.MVC.Routing;

public static class SubPlatformRouteHelper
{
    private static readonly Dictionary<string, string> PublicToCanonical = new(StringComparer.OrdinalIgnoreCase)
    {
        ["kdg"] = "kdg-hogeschool",
        ["ap"] = "ap-hogeschool"
    };

    private static readonly Dictionary<string, string> CanonicalToPublic =
        PublicToCanonical.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string subplatform)
    {
        if (string.IsNullOrWhiteSpace(subplatform))
        {
            return subplatform;
        }

        return PublicToCanonical.TryGetValue(subplatform, out var canonicalSlug)
            ? canonicalSlug
            : subplatform;
    }

    public static string ToPublicSlug(string subplatform)
    {
        if (string.IsNullOrWhiteSpace(subplatform))
        {
            return subplatform;
        }

        return CanonicalToPublic.TryGetValue(subplatform, out var publicSlug)
            ? publicSlug
            : subplatform;
    }
}
