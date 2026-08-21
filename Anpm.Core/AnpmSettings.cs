namespace Anpm.Core;

public static class AnpmSettings
{
    public const string FeedRootVar = "ANPM_FEED_ROOT";
    public const string ManifestPathVar = "ANPM_MANIFEST_PATH";
    public const string V3BaseUrlVar = "ANPM_V3_BASE_URL";
    public const string HostUrlsVar = "ANPM_HOST_URLS";
    public const string RebuildIndexOnStartVar = "ANPM_REBUILD_INDEX_ON_START";

    public static string RequireFeedRoot(string? overrideValue = null)
    {
        var candidate = overrideValue?.Trim();
        if (string.IsNullOrWhiteSpace(candidate))
            candidate = Environment.GetEnvironmentVariable(FeedRootVar);

        if (string.IsNullOrWhiteSpace(candidate))
            throw new InvalidOperationException($"{FeedRootVar} is required.");

        return Path.GetFullPath(candidate);
    }

    public static string ResolveV3BaseUrl(string? overrideValue = null, string defaultValue = "http://127.0.0.1:5088/v3")
    {
        var candidate = overrideValue?.Trim();
        if (string.IsNullOrWhiteSpace(candidate))
            candidate = Environment.GetEnvironmentVariable(V3BaseUrlVar);

        if (string.IsNullOrWhiteSpace(candidate))
            return defaultValue;

        candidate = candidate.Trim().TrimEnd('/');
        return candidate.Length == 0 ? defaultValue : candidate;
    }

    public static bool RebuildIndexOnStart(bool defaultValue = true)
    {
        var raw = Environment.GetEnvironmentVariable(RebuildIndexOnStartVar);
        if (string.IsNullOrWhiteSpace(raw))
            return defaultValue;

        return bool.TryParse(raw, out var parsed) ? parsed : defaultValue;
    }
}
