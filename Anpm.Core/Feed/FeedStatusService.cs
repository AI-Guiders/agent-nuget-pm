using Anpm.Core.Models;

namespace Anpm.Core.Feed;

public static class FeedStatusService
{
    public static FeedStatusReport Build(string feedRoot, string manifestPath, AnpmManifest manifest)
    {
        var present = FeedScanner.Scan(feedRoot);
        var pinned = manifest.Packages;

        var presentKeys = present
            .Select(p => Key(p.Id, p.Version))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var pinnedKeys = pinned
            .Select(p => Key(p.Id, p.Version))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = pinned
            .Where(p => !presentKeys.Contains(Key(p.Id, p.Version)))
            .ToList();

        var extra = present
            .Where(p => !pinnedKeys.Contains(Key(p.Id, p.Version)))
            .ToList();

        return new FeedStatusReport
        {
            FeedRoot = feedRoot,
            ManifestPath = Path.GetFullPath(manifestPath),
            Pinned = pinned,
            Present = present,
            Missing = missing,
            Extra = extra
        };
    }

    private static string Key(string id, string version) => $"{id}|{version}";
}
