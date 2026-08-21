using Anpm.Core;
using Anpm.Core.Feed;
using Anpm.Core.Models;
using Anpm.Core.Sync;
using Microsoft.Extensions.Configuration;

namespace AgentForge.Plugin.PackageFeed;

internal sealed class PackageFeedOptions(IConfiguration configuration)
{
    private IConfigurationSection Anpm => configuration.GetSection("anpm");

    internal string ResolveManifestPath() =>
        AnpmSettings.FirstNonEmpty(
            Anpm["manifest_path"],
            AnpmSettings.ResolveManifestPath())
        ?? throw new InvalidOperationException(
            "anpm.manifest_path is required (appsettings [anpm] or ANPM_* env override).");

    internal string ResolveFeedRoot(AnpmManifest manifest) =>
        ManifestLoader.ResolveFeedRoot(
            manifest,
            AnpmSettings.FirstNonEmpty(Anpm["feed_root"], AnpmBootstrap.Current.FeedRoot));

    internal string? ResolveV3BaseUrl(AnpmManifest manifest) =>
        AnpmSettings.FirstNonEmpty(
            Anpm["v3_base_url"],
            AnpmSettings.ResolveV3BaseUrl(),
            manifest.V3BaseUrl);
}
