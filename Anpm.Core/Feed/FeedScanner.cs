using System.IO.Compression;
using System.Text.RegularExpressions;
using Anpm.Core.Models;

namespace Anpm.Core.Feed;

public static partial class FeedScanner
{
    public static IReadOnlyList<FeedPackageEntry> Scan(string feedRoot)
    {
        if (string.IsNullOrWhiteSpace(feedRoot))
            throw new ArgumentException("feed_root is required.", nameof(feedRoot));

        var root = Path.GetFullPath(feedRoot);
        if (!Directory.Exists(root))
            return [];

        var entries = new List<FeedPackageEntry>();
        foreach (var path in Directory.EnumerateFiles(root, "*.nupkg", SearchOption.TopDirectoryOnly))
        {
            var fileName = Path.GetFileName(path);
            FeedPackageEntry? entry = null;

            if (TryParsePackage(fileName, path, out var parsed))
                entry = parsed;
            else if (TryReadNuspecIdentity(path, out var id, out var version))
            {
                entry = new FeedPackageEntry
                {
                    Id = id,
                    Version = version,
                    FileName = fileName,
                    FullPath = path,
                    SizeBytes = new FileInfo(path).Length
                };
            }

            if (entry is not null)
                entries.Add(entry);
        }

        return entries
            .OrderBy(e => e.Id, StringComparer.OrdinalIgnoreCase)
            .ThenBy(e => e.Version, StringComparer.Ordinal)
            .ToList();
    }

    public static bool TryParsePackage(string fileName, string fullPath, out FeedPackageEntry? entry)
    {
        entry = null;
        if (!TrySplitPackageFileName(fileName, out var id, out var version))
            return false;

        var size = File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0;
        entry = new FeedPackageEntry
        {
            Id = id,
            Version = version,
            FileName = fileName,
            FullPath = fullPath,
            SizeBytes = size
        };
        return true;
    }

    internal static bool TrySplitPackageFileName(string fileName, out string id, out string version)
    {
        id = "";
        version = "";
        if (!fileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
            return false;

        var stem = fileName[..^".nupkg".Length];
        var match = VersionSuffixRegex().Match(stem);
        if (!match.Success)
            return false;

        version = match.Groups["version"].Value;
        id = stem[..match.Index].TrimEnd('.');
        return id.Length > 0 && version.Length > 0;
    }

    private static bool TryReadNuspecIdentity(string nupkgPath, out string id, out string version)
    {
        id = "";
        version = "";
        try
        {
            using var archive = ZipFile.OpenRead(nupkgPath);
            var nuspec = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
            if (nuspec is null)
                return false;

            using var stream = nuspec.Open();
            using var reader = new StreamReader(stream);
            var xml = reader.ReadToEnd();
            var idMatch = Regex.Match(xml, "<id>(?<id>[^<]+)</id>", RegexOptions.IgnoreCase);
            var versionMatch = Regex.Match(xml, "<version>(?<version>[^<]+)</version>", RegexOptions.IgnoreCase);
            if (!idMatch.Success || !versionMatch.Success)
                return false;

            id = idMatch.Groups["id"].Value.Trim();
            version = versionMatch.Groups["version"].Value.Trim();
            return id.Length > 0 && version.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    [GeneratedRegex(@"\.(?<version>\d+(?:\.\d+){0,3}(?:-[\w\.\-]+)?)$", RegexOptions.CultureInvariant)]
    private static partial Regex VersionSuffixRegex();
}
