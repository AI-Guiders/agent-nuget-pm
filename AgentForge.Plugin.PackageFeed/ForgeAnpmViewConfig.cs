using Anpm.Core;
using Anpm.Core.Feed;
using Anpm.Core.Models;

namespace AgentForge.Plugin.PackageFeed;

internal sealed class ForgeAnpmViewConfig(PackageFeedOptions options) : Anpm.View.IAnpmViewConfig
{
    public bool TryLoadStatus(out FeedStatusReport? report, out string? v3BaseUrl, out string? error)
    {
        report = null;
        v3BaseUrl = null;
        error = null;

        try
        {
            var manifestPath = options.ResolveManifestPath();
            var manifest = ManifestLoader.Load(manifestPath);
            var feedRoot = options.ResolveFeedRoot(manifest);
            report = FeedStatusService.Build(feedRoot, manifestPath, manifest);
            v3BaseUrl = options.ResolveV3BaseUrl(manifest) ?? AnpmSettings.DefaultV3BaseUrl;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
