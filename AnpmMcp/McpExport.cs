using System.Text.Json;
using Anpm.Core;

namespace AnpmMcp;

internal static class McpExport
{
    internal static string BuildManifestJson(string? commandPath = null)
    {
        var tools = ToolCatalog.Build();
        var payload = new
        {
            schema = "anpm/mcp-manifest/v1",
            server = new
            {
                name = "AnpmMcp",
                version = "0.1.0-m1",
                command = commandPath ?? "AnpmMcp.exe",
                args = Array.Empty<string>(),
                env = new Dictionary<string, string>
                {
                    [AnpmSettings.FeedRootVar] = "${ANPM_FEED_ROOT}",
                    [AnpmSettings.ManifestPathVar] = "${ANPM_MANIFEST_PATH}",
                    [AnpmSettings.V3BaseUrlVar] = "http://127.0.0.1:5088/v3"
                }
            },
            tools = tools.Select(t => new
            {
                name = t.Name,
                description = t.Description,
                inputSchema = t.InputSchema
            })
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }
}
