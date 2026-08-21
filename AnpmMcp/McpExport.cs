using System.Text.Json;
using Anpm.Core;
using Anpm.Core.Config;

namespace AnpmMcp;

internal static class McpExport
{
    internal static string BuildManifestJson(string? commandPath = null, string? configPath = null)
    {
        configPath ??= Path.Combine(AppContext.BaseDirectory, AnpmConfigLoader.DefaultConfigRelativePath);
        var tools = ToolCatalog.Build();
        var payload = new
        {
            schema = "anpm/mcp-manifest/v1",
            server = new
            {
                name = "AnpmMcp",
                version = "0.1.0-m1",
                command = commandPath ?? "AnpmMcp.exe",
                args = new[] { "--config", configPath },
                env = new Dictionary<string, string>()
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
