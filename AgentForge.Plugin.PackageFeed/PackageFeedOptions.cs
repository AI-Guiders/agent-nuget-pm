using Anpm.Core;
using Anpm.Core.Feed;
using Anpm.Core.Models;
using Anpm.Core.Sync;
using Microsoft.Extensions.Configuration;

namespace AgentForge.Plugin.PackageFeed;

internal sealed class PackageFeedOptions(IConfiguration configuration)
{
    internal string ResolveManifestPath() =>
        configuration["ANPM_MANIFEST_PATH"]
        ?? Environment.GetEnvironmentVariable(AnpmSettings.ManifestPathVar)
        ?? throw new InvalidOperationException($"{AnpmSettings.ManifestPathVar} is required for package-feed plugin.");

    internal string ResolveFeedRoot(AnpmManifest manifest) =>
        ManifestLoader.ResolveFeedRoot(
            manifest,
            configuration["ANPM_FEED_ROOT"] ?? Environment.GetEnvironmentVariable(AnpmSettings.FeedRootVar));

    internal string? ResolveV3BaseUrl(AnpmManifest manifest) =>
        configuration["ANPM_V3_BASE_URL"]
        ?? Environment.GetEnvironmentVariable(AnpmSettings.V3BaseUrlVar)
        ?? manifest.V3BaseUrl;
}
