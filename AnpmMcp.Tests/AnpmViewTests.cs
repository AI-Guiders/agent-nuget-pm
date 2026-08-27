using Anpm.Core;
using Anpm.Core.Config;
using Anpm.Core.Feed;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AnpmMcp.Tests;

public class AnpmViewTests : IDisposable
{
    readonly string _feedRoot;
    readonly string _manifestPath;
    readonly WebApplicationFactory<Anpm.Host.Program> _factory;

    public AnpmViewTests()
    {
        AnpmBootstrap.Use(AnpmRuntimeSettings.Empty);
        _feedRoot = Path.Combine(Path.GetTempPath(), "anpm-view-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(_feedRoot);
        File.WriteAllText(Path.Combine(_feedRoot, "Contoso.A.1.0.0.nupkg"), "fake-nupkg");

        _manifestPath = Path.Combine(_feedRoot, "pins.json");
        File.WriteAllText(_manifestPath,
            """
            {
              "schema": "anpm/manifest/v1",
              "packages": [
                { "id": "Contoso.A", "version": "1.0.0" },
                { "id": "Contoso.B", "version": "2.0.0" }
              ]
            }
            """);

        Environment.SetEnvironmentVariable(AnpmSettings.FeedRootVar, _feedRoot);
        Environment.SetEnvironmentVariable(AnpmSettings.ManifestPathVar, _manifestPath);
        Environment.SetEnvironmentVariable(AnpmSettings.V3BaseUrlVar, "http://127.0.0.1/v3");
        Environment.SetEnvironmentVariable(AnpmSettings.RebuildIndexOnStartVar, "false");
        Environment.SetEnvironmentVariable(AnpmSettings.HostUrlsVar, "http://127.0.0.1:0");

        _factory = new WebApplicationFactory<Anpm.Host.Program>();
    }

    [Fact]
    public async Task Feed_overview_renders_feed_root()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/view/feed");
        Assert.Contains("ANPM feed overview", html, StringComparison.Ordinal);
        Assert.Contains(Path.GetFullPath(_feedRoot), html, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"anpm-feed-root\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Pin_matrix_lists_missing_pin()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/view/pins");
        Assert.Contains("Contoso.B", html, StringComparison.Ordinal);
        Assert.Contains("missing", html, StringComparison.Ordinal);
        Assert.Contains("present", html, StringComparison.Ordinal);
    }

    public void Dispose()
    {
        _factory.Dispose();
        Environment.SetEnvironmentVariable(AnpmSettings.FeedRootVar, null);
        Environment.SetEnvironmentVariable(AnpmSettings.ManifestPathVar, null);
        Environment.SetEnvironmentVariable(AnpmSettings.V3BaseUrlVar, null);
        Environment.SetEnvironmentVariable(AnpmSettings.RebuildIndexOnStartVar, null);
        Environment.SetEnvironmentVariable(AnpmSettings.HostUrlsVar, null);
        AnpmBootstrap.Use(AnpmRuntimeSettings.Empty);
        try { Directory.Delete(_feedRoot, recursive: true); } catch { /* temp */ }
    }
}
