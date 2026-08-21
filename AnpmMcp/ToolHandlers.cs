using System.Text.Json;
using Anpm.Core;
using Anpm.Core.Feed;
using Anpm.Core.Models;
using Anpm.Core.Restore;
using Anpm.Core.Sync;

namespace AnpmMcp;

internal static class ToolHandlers
{
    internal static string Handle(string name, IReadOnlyDictionary<string, JsonElement> args) =>
        name switch
        {
            "anpm_feed_status" => FeedStatus(args),
            "anpm_pin_list" => PinList(args),
            "anpm_feed_sync" => FeedSync(args),
            "anpm_feed_index" => FeedIndex(args),
            "anpm_restore_verify" => RestoreVerify(args),
            "anpm_mcp_export" => ExportMcpManifest(args),
            _ => throw new ArgumentException($"Unknown tool: {name}")
        };

    private static string FeedStatus(IReadOnlyDictionary<string, JsonElement> args)
    {
        var manifestPath = AnpmEnvironment.ResolveManifestPath(GetOptionalString(args, "manifest_path"));
        var manifest = ManifestLoader.Load(manifestPath);
        var feedRoot = ManifestLoader.ResolveFeedRoot(manifest, GetOptionalString(args, "feed_root"));
        var report = FeedStatusService.Build(feedRoot, manifestPath, manifest);
        return AnpmJson.Serialize(report);
    }

    private static string PinList(IReadOnlyDictionary<string, JsonElement> args)
    {
        var manifestPath = AnpmEnvironment.ResolveManifestPath(GetOptionalString(args, "manifest_path"));
        var manifest = ManifestLoader.Load(manifestPath);
        return AnpmJson.Serialize(new
        {
            schema = manifest.Schema,
            manifestPath,
            feedRoot = ManifestLoader.ResolveFeedRoot(manifest, GetOptionalString(args, "feed_root")),
            packages = manifest.Packages
        });
    }

    private static string FeedSync(IReadOnlyDictionary<string, JsonElement> args)
    {
        var manifestPath = AnpmEnvironment.ResolveManifestPath(GetOptionalString(args, "manifest_path"));
        var manifest = ManifestLoader.Load(manifestPath);
        var feedRoot = ManifestLoader.ResolveFeedRoot(manifest, GetOptionalString(args, "feed_root"));
        var dryRun = GetOptionalBool(args, "dry_run") ?? false;
        var rebuildIndex = GetOptionalBool(args, "rebuild_index") ?? true;

        var sync = FeedSyncService.Sync(feedRoot, manifest.Packages, dryRun);
        IndexRebuildReport? index = null;
        if (rebuildIndex && !dryRun && sync.Errors.Count == 0)
        {
            var v3Base = GetOptionalString(args, "v3_base_url")
                ?? Environment.GetEnvironmentVariable(AnpmEnvironment.V3BaseUrlVar)
                ?? manifest.V3BaseUrl;
            index = V3IndexWriter.Rebuild(feedRoot, v3Base);
        }

        return AnpmJson.Serialize(new { sync, index });
    }

    private static string FeedIndex(IReadOnlyDictionary<string, JsonElement> args)
    {
        var manifestPath = AnpmEnvironment.ResolveManifestPath(GetOptionalString(args, "manifest_path"));
        var manifest = ManifestLoader.Load(manifestPath);
        var feedRoot = ManifestLoader.ResolveFeedRoot(manifest, GetOptionalString(args, "feed_root"));
        var v3Base = GetOptionalString(args, "v3_base_url")
            ?? Environment.GetEnvironmentVariable(AnpmEnvironment.V3BaseUrlVar)
            ?? manifest.V3BaseUrl;
        var report = V3IndexWriter.Rebuild(feedRoot, v3Base);
        return AnpmJson.Serialize(report);
    }

    private static string ExportMcpManifest(IReadOnlyDictionary<string, JsonElement> args)
    {
        var commandPath = GetOptionalString(args, "command_path");
        return McpExport.BuildManifestJson(commandPath);
    }

    private static string RestoreVerify(IReadOnlyDictionary<string, JsonElement> args)
    {
        var targetPath = GetRequiredString(args, "target_path");
        var manifestPath = AnpmEnvironment.ResolveManifestPath(GetOptionalString(args, "manifest_path"));
        var manifest = ManifestLoader.Load(manifestPath);
        var feedRoot = ManifestLoader.ResolveFeedRoot(manifest, GetOptionalString(args, "feed_root"));
        var dryRun = GetOptionalBool(args, "dry_run") ?? true;
        var report = RestoreVerifyService.Verify(targetPath, feedRoot, dryRun);
        return AnpmJson.Serialize(report);
    }

    private static string? GetOptionalString(IReadOnlyDictionary<string, JsonElement> args, string name) =>
        args.TryGetValue(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string GetRequiredString(IReadOnlyDictionary<string, JsonElement> args, string name)
    {
        var value = GetOptionalString(args, name);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} is required.");
        return value;
    }

    private static bool? GetOptionalBool(IReadOnlyDictionary<string, JsonElement> args, string name)
    {
        if (!args.TryGetValue(name, out var value))
            return null;

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(value.GetString(), out var parsed) ? parsed : null,
            _ => null
        };
    }
}
