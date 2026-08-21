using System.Text.Json;
using System.Text.Json.Serialization;
using Anpm.Core.Models;

namespace Anpm.Core.Feed;

public static class ManifestLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static AnpmManifest Load(string manifestPath)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
            throw new ArgumentException("manifest_path is required.", nameof(manifestPath));

        var fullPath = Path.GetFullPath(manifestPath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Manifest not found: {fullPath}", fullPath);

        var json = File.ReadAllText(fullPath);
        var dto = JsonSerializer.Deserialize<ManifestDto>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Manifest JSON is empty: {fullPath}");

        if (string.IsNullOrWhiteSpace(dto.Schema))
            throw new InvalidOperationException($"Manifest schema is required: {fullPath}");

        var pins = (dto.Packages ?? [])
            .Where(p => !string.IsNullOrWhiteSpace(p.Id) && !string.IsNullOrWhiteSpace(p.Version))
            .Select(p => new ManifestPackagePin
            {
                Id = p.Id!.Trim(),
                Version = p.Version!.Trim()
            })
            .ToList();

        return new AnpmManifest
        {
            Schema = dto.Schema.Trim(),
            FeedRoot = ExpandTokens(dto.FeedRoot),
            V3BaseUrl = dto.V3BaseUrl?.Trim(),
            Packages = pins
        };
    }

    public static string ResolveFeedRoot(AnpmManifest manifest, string? feedRootOverride)
    {
        var candidate = feedRootOverride?.Trim();
        if (string.IsNullOrWhiteSpace(candidate))
            candidate = Environment.GetEnvironmentVariable("ANPM_FEED_ROOT");

        if (string.IsNullOrWhiteSpace(candidate))
            candidate = manifest.FeedRoot;

        if (string.IsNullOrWhiteSpace(candidate))
            throw new ArgumentException("feed_root is required (argument, ANPM_FEED_ROOT, or manifest.feedRoot).");

        return Path.GetFullPath(ExpandTokens(candidate)!);
    }

    internal static string? ExpandTokens(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var expanded = Environment.ExpandEnvironmentVariables(value.Trim());
        if (expanded.StartsWith("${", StringComparison.Ordinal) && expanded.EndsWith('}'))
        {
            var name = expanded[2..^1];
            var fromEnv = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv;
        }

        return expanded;
    }

    private sealed class ManifestDto
    {
        public string? Schema { get; set; }

        public string? FeedRoot { get; set; }

        public string? V3BaseUrl { get; set; }

        public List<PackageDto>? Packages { get; set; }
    }

    private sealed class PackageDto
    {
        public string? Id { get; set; }

        public string? Version { get; set; }
    }
}
