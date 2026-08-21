namespace Anpm.Core;

public static class AnpmSettings
{
    public const string FeedRootVar = "ANPM_FEED_ROOT";
    public const string ManifestPathVar = "ANPM_MANIFEST_PATH";
    public const string V3BaseUrlVar = "ANPM_V3_BASE_URL";
    public const string HostUrlsVar = "ANPM_HOST_URLS";
    public const string RebuildIndexOnStartVar = "ANPM_REBUILD_INDEX_ON_START";
    public const string RepoRootVar = "ANPM_REPO_ROOT";

    public const string DefaultV3BaseUrl = "http://127.0.0.1:5088/v3";
    public const string DefaultHostUrls = "http://127.0.0.1:5088";

    public static string RequireFeedRoot(string? overrideValue = null)
    {
        var candidate = FirstNonEmpty(overrideValue, AnpmBootstrap.Current.FeedRoot);
        if (string.IsNullOrWhiteSpace(candidate))
            throw new InvalidOperationException(
                $"{FeedRootVar} is required (tool arg, env override, or [feed].root in anpm.toml).");

        return Path.GetFullPath(candidate);
    }

    public static string ResolveV3BaseUrl(string? overrideValue = null, string? defaultValue = null)
    {
        defaultValue ??= DefaultV3BaseUrl;
        var candidate = FirstNonEmpty(overrideValue, AnpmBootstrap.Current.V3BaseUrl);
        if (string.IsNullOrWhiteSpace(candidate))
            return defaultValue;

        candidate = candidate.Trim().TrimEnd('/');
        return candidate.Length == 0 ? defaultValue : candidate;
    }

    public static string ResolveHostUrls(string? overrideValue = null, string? defaultValue = null)
    {
        defaultValue ??= DefaultHostUrls;
        var candidate = FirstNonEmpty(overrideValue, AnpmBootstrap.Current.HostUrls);
        return string.IsNullOrWhiteSpace(candidate) ? defaultValue : candidate.Trim();
    }

    public static bool RebuildIndexOnStart(bool defaultValue = true) =>
        AnpmBootstrap.Current.RebuildIndexOnStart ?? defaultValue;

    public static string? ResolveManifestPath(string? overrideValue = null) =>
        FirstNonEmpty(overrideValue, AnpmBootstrap.Current.ManifestPath);

    public static string ResolveRepoRoot(string? overrideValue = null)
    {
        var fromConfig = FirstNonEmpty(overrideValue, AnpmBootstrap.Current.RepoRoot);
        if (!string.IsNullOrWhiteSpace(fromConfig))
            return Path.GetFullPath(fromConfig);

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "AnpmMcp.slnx"))
                || File.Exists(Path.Combine(dir.FullName, "manifest", "pins.example.json")))
                return dir.FullName;

            dir = dir.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    public static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }
}
