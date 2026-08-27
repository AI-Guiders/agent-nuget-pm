using Anpm.Core;
using Anpm.Core.Feed;
using Anpm.Core.Models;
using Anpm.Core.Sync;
using Anpm.View.Models;

namespace Anpm.View;

internal static class AnpmViewPresenter
{
    internal static FeedOverviewModel BuildOverview(IAnpmViewConfig config, string routePrefix, SyncFlashModel? flash = null)
    {
        if (!config.TryLoadStatus(out var report, out var v3BaseUrl, out var error))
        {
            return new FeedOverviewModel
            {
                Configured = false,
                ConfigError = error,
                RoutePrefix = routePrefix,
                Flash = flash,
            };
        }

        return new FeedOverviewModel
        {
            Configured = true,
            Status = report,
            V3BaseUrl = v3BaseUrl,
            RoutePrefix = routePrefix,
            Flash = flash,
        };
    }

    internal static PinMatrixModel BuildPinMatrix(IAnpmViewConfig config, string routePrefix, SyncFlashModel? flash = null)
    {
        if (!config.TryLoadStatus(out var report, out _, out var error))
        {
            return new PinMatrixModel
            {
                Configured = false,
                ConfigError = error,
                Rows = [],
                RoutePrefix = routePrefix,
                Flash = flash,
            };
        }

        var presentKeys = report!.Present
            .Select(p => Key(p.Id, p.Version))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rows = report.Pinned
            .Select(pin => new PinRowModel
            {
                Id = pin.Id,
                Version = pin.Version,
                State = presentKeys.Contains(Key(pin.Id, pin.Version)) ? "present" : "missing",
            })
            .Concat(report.Extra.Select(extra => new PinRowModel
            {
                Id = extra.Id,
                Version = extra.Version,
                State = "extra",
            }))
            .OrderBy(r => r.Id, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.Version, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new PinMatrixModel
        {
            Configured = true,
            Rows = rows,
            RoutePrefix = routePrefix,
            Flash = flash,
        };
    }

    internal static SyncFlashModel? TrySync(IAnpmViewConfig config, bool dryRun, bool rebuildIndex)
    {
        if (!config.TryLoadStatus(out var report, out var v3BaseUrl, out _))
            return null;

        var manifestPath = report!.ManifestPath;
        var manifest = ManifestLoader.Load(manifestPath);
        var sync = FeedSyncService.Sync(report.FeedRoot, manifest.Packages, dryRun);
        if (rebuildIndex && !dryRun && sync.Errors.Count == 0)
            V3IndexWriter.Rebuild(report.FeedRoot, v3BaseUrl ?? AnpmSettings.DefaultV3BaseUrl);

        return new SyncFlashModel
        {
            DryRun = dryRun,
            Downloaded = sync.Downloaded.Count,
            Skipped = sync.Skipped.Count,
            Errors = sync.Errors,
        };
    }

    private static string Key(string id, string version) => $"{id}|{version}";
}
