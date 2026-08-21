using Anpm.Core;
using Anpm.Core.Feed;
using Anpm.Core.Models;
using Anpm.Core.Sync;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AgentForge.Plugin.PackageFeed.Endpoints;

internal static class PackageFeedEndpoints
{
    internal static RouteGroupBuilder MapPackageFeedEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/package-feed/status", GetStatus);
        api.MapPost("/package-feed/sync", PostSync);
        return api;
    }

    private static IResult GetStatus(PackageFeedOptions options)
    {
        var manifestPath = options.ResolveManifestPath();
        var manifest = ManifestLoader.Load(manifestPath);
        var feedRoot = options.ResolveFeedRoot(manifest);
        var report = FeedStatusService.Build(feedRoot, manifestPath, manifest);
        return Results.Json(report);
    }

    private static IResult PostSync(PackageFeedOptions options, bool dryRun = false, bool rebuildIndex = true)
    {
        var manifestPath = options.ResolveManifestPath();
        var manifest = ManifestLoader.Load(manifestPath);
        var feedRoot = options.ResolveFeedRoot(manifest);
        var sync = FeedSyncService.Sync(feedRoot, manifest.Packages, dryRun);
        IndexRebuildReport? index = null;
        if (rebuildIndex && !dryRun && sync.Errors.Count == 0)
            index = V3IndexWriter.Rebuild(feedRoot, options.ResolveV3BaseUrl(manifest));

        return Results.Json(new { sync, index });
    }
}
