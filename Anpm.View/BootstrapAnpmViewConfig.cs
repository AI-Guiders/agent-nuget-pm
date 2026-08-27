using Anpm.Core;
using Anpm.Core.Feed;
using Anpm.Core.Models;

namespace Anpm.View;

public sealed class BootstrapAnpmViewConfig : IAnpmViewConfig
{
    public bool TryLoadStatus(out FeedStatusReport? report, out string? v3BaseUrl, out string? error)
    {
        report = null;
        v3BaseUrl = null;
        error = null;

        try
        {
            var manifestPath = AnpmSettings.ResolveManifestPath();
            if (string.IsNullOrWhiteSpace(manifestPath))
            {
                error = "manifest_path is not configured ([feed].manifest_path in anpm.toml or ANPM_MANIFEST_PATH).";
                return false;
            }

            var manifest = ManifestLoader.Load(manifestPath);
            var feedRoot = ManifestLoader.ResolveFeedRoot(manifest, null);
            report = FeedStatusService.Build(feedRoot, manifestPath, manifest);
            v3BaseUrl = AnpmSettings.ResolveV3BaseUrl(defaultValue: manifest.V3BaseUrl);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
