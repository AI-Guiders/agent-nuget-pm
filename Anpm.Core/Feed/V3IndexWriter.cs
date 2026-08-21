using System.Text.Json;
using System.Text.Json.Serialization;
using Anpm.Core.Models;

namespace Anpm.Core.Feed;

public static class V3IndexWriter
{
    public static IndexRebuildReport Rebuild(string feedRoot, string? v3BaseUrl)
    {
        var root = Path.GetFullPath(feedRoot);
        Directory.CreateDirectory(root);

        var packages = FeedScanner.Scan(root);
        var indexRoot = Path.Combine(root, ".anpm", "v3");
        var baseUrl = NormalizeBaseUrl(v3BaseUrl) ?? "http://127.0.0.1:5088/v3";

        if (Directory.Exists(indexRoot))
            Directory.Delete(indexRoot, recursive: true);

        Directory.CreateDirectory(indexRoot);

        var written = new List<string>();
        var serviceIndexPath = Path.Combine(indexRoot, "index.json");
        WriteJson(serviceIndexPath, BuildServiceIndex(baseUrl));
        written.Add(serviceIndexPath);

        var registrationRoot = Path.Combine(indexRoot, "registration");
        Directory.CreateDirectory(registrationRoot);

        foreach (var group in packages.GroupBy(p => p.Id.ToLowerInvariant()))
        {
            var pagePath = Path.Combine(registrationRoot, $"{group.Key}/index.json");
            Directory.CreateDirectory(Path.GetDirectoryName(pagePath)!);
            WriteJson(pagePath, BuildRegistrationPage(baseUrl, group.Key, group.OrderBy(p => p.Version).ToList()));
            written.Add(pagePath);
        }

        var flatManifestPath = Path.Combine(root, ".anpm", "feed-index.json");
        WriteJson(flatManifestPath, new
        {
            schema = "anpm/feed-index/v1",
            generatedUtc = DateTime.UtcNow,
            packageCount = packages.Count,
            packages = packages.Select(p => new { p.Id, p.Version, p.FileName, p.SizeBytes })
        });
        written.Add(flatManifestPath);

        return new IndexRebuildReport
        {
            FeedRoot = root,
            V3IndexRoot = indexRoot,
            PackageCount = packages.Count,
            WrittenFiles = written
        };
    }

    private static object BuildServiceIndex(string baseUrl) => new
    {
        version = "3.0.0",
        resources = new object[]
        {
            new { type = "PackagePublish/2.0.0", id = $"{baseUrl}/package" },
            new { type = "RegistrationsBaseUrl/3.0.0-beta", id = $"{baseUrl}/registration" },
            new { type = "RegistrationsBaseUrl/3.6.0", id = $"{baseUrl}/registration" },
            new { type = "PackageBaseAddress/3.0.0", id = $"{baseUrl}/package" },
            new { type = "SearchQueryService/3.0.0-beta", id = $"{baseUrl}/search/query" }
        }
    };

    private static object BuildRegistrationPage(string baseUrl, string idLower, IReadOnlyList<FeedPackageEntry> versions)
    {
        var id = versions[0].Id;
        return new
        {
            count = versions.Count,
            items = versions.Select(v => new
            {
                id = $"{baseUrl}/registration/{idLower}/index.json",
                registration = $"{baseUrl}/registration/{idLower}/index.json",
                packageContent = $"{baseUrl}/package/{v.Id}/{v.Version}/{v.FileName}",
                catalogEntry = new
                {
                    id,
                    version = v.Version,
                    listed = true
                }
            })
        };
    }

    private static void WriteJson(string path, object payload)
    {
        var json = JsonSerializer.Serialize(payload, JsonWriteOptions);
        File.WriteAllText(path, json);
    }

    private static string? NormalizeBaseUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim().TrimEnd('/');
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static readonly JsonSerializerOptions JsonWriteOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
