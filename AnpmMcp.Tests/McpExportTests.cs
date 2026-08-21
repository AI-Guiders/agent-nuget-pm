using AnpmMcp;
using Xunit;

namespace AnpmMcp.Tests;

public class McpExportTests
{
    [Fact]
    public void BuildManifestJson_lists_all_tools()
    {
        var json = McpExport.BuildManifestJson("D:/tools/AnpmMcp.exe");
        Assert.Contains("anpm_feed_status", json, StringComparison.Ordinal);
        Assert.Contains("anpm_mcp_export", json, StringComparison.Ordinal);
        Assert.Contains("D:/tools/AnpmMcp.exe", json, StringComparison.Ordinal);
        Assert.Contains("--config", json, StringComparison.Ordinal);
    }
}
