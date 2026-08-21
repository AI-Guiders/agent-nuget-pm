using System.Text.Json;
using ModelContextProtocol.Protocol;
using Tool = ModelContextProtocol.Protocol.Tool;

namespace AnpmMcp;

internal static class ToolCatalog
{
    private static JsonElement Schema(object schema) => JsonSerializer.SerializeToElement(schema);

    internal static List<Tool> Build() =>
    [
        new()
        {
            Name = "anpm_feed_status",
            Description =
                "Feed status: pinned manifest packages vs .nupkg present on feed_root. Returns missing/extra lists. DOI alias: anpm.feed.status.",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    feed_root = new { type = "string", description = "Feed directory (or ANPM_FEED_ROOT / manifest.feedRoot)." },
                    manifest_path = new { type = "string", description = "Pin manifest JSON (default manifest/pins.example.json when unset)." }
                }
            })
        },
        new()
        {
            Name = "anpm_pin_list",
            Description =
                "Org pin set from ANPM manifest (Directory.Packages.props SSOT mirror). DOI alias: anpm.pin.list.",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    feed_root = new { type = "string", description = "Optional resolved feed root for context." },
                    manifest_path = new { type = "string", description = "Pin manifest JSON path." }
                }
            })
        },
        new()
        {
            Name = "anpm_feed_sync",
            Description =
                "Download missing pinned packages into feed_root via dotnet nuget (sync host only). Optional rebuild_index. DOI alias: anpm.feed.sync.",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    feed_root = new { type = "string" },
                    manifest_path = new { type = "string" },
                    dry_run = new { type = "boolean", description = "Default false." },
                    rebuild_index = new { type = "boolean", description = "Rebuild .anpm/v3 index after sync (default true)." },
                    v3_base_url = new { type = "string", description = "Public v3 base URL for generated index." }
                }
            })
        },
        new()
        {
            Name = "anpm_feed_index",
            Description =
                "Rebuild flat feed scan + minimal v3 index files under feed_root/.anpm/v3 (static index; HTTP host M1 follow-up).",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    feed_root = new { type = "string" },
                    manifest_path = new { type = "string" },
                    v3_base_url = new { type = "string" }
                }
            })
        },
        new()
        {
            Name = "anpm_restore_verify",
            Description =
                "Dry-run dotnet restore for solution/project using feed_root source. DOI alias: anpm.restore.verify.",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    target_path = new { type = "string", description = "Path to .sln/.slnx/.csproj or directory containing one." },
                    feed_root = new { type = "string" },
                    manifest_path = new { type = "string" },
                    dry_run = new { type = "boolean", description = "Default true." }
                },
                required = new[] { "target_path" }
            })
        }
    ];
}
