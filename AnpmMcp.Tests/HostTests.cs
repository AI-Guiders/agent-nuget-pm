using Anpm.Core;
using Anpm.Core.Config;
using Anpm.Core.Feed;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace AnpmMcp.Tests;

public class AnpmHostTests : IDisposable
{
    readonly string _feedRoot;
    readonly WebApplicationFactory<Anpm.Host.Program> _factory;

    public AnpmHostTests()
    {
        AnpmBootstrap.Use(AnpmRuntimeSettings.Empty);
        _feedRoot = Path.Combine(Path.GetTempPath(), "anpm-host-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(_feedRoot);
        File.WriteAllText(Path.Combine(_feedRoot, "Contoso.A.1.0.0.nupkg"), "fake-nupkg");

        Environment.SetEnvironmentVariable(AnpmSettings.FeedRootVar, _feedRoot);
        Environment.SetEnvironmentVariable(AnpmSettings.V3BaseUrlVar, "http://127.0.0.1/v3");
        Environment.SetEnvironmentVariable(AnpmSettings.RebuildIndexOnStartVar, "true");
        Environment.SetEnvironmentVariable(AnpmSettings.HostUrlsVar, "http://127.0.0.1:0");

        _factory = new WebApplicationFactory<Anpm.Host.Program>();
    }

    [Fact]
    public async Task Health_reports_feed_root()
    {
        var client = _factory.CreateClient();
        var json = await client.GetStringAsync("/health");
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal(Path.GetFullPath(_feedRoot), doc.RootElement.GetProperty("feedRoot").GetString());
    }

    [Fact]
    public async Task V3_index_and_package_bytes_are_served()
    {
        V3IndexWriter.Rebuild(_feedRoot, "http://127.0.0.1/v3");

        var client = _factory.CreateClient();

        var indexResponse = await client.GetAsync("/v3/index.json");
        Assert.Equal(HttpStatusCode.OK, indexResponse.StatusCode);
        var indexJson = await indexResponse.Content.ReadAsStringAsync();
        Assert.Contains("PackageBaseAddress/3.0.0", indexJson, StringComparison.Ordinal);

        var packageResponse = await client.GetAsync("/v3/package/Contoso.A/1.0.0/Contoso.A.1.0.0.nupkg");
        Assert.Equal(HttpStatusCode.OK, packageResponse.StatusCode);
        var bytes = await packageResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal("fake-nupkg", System.Text.Encoding.UTF8.GetString(bytes));
    }

    public void Dispose()
    {
        _factory.Dispose();
        Environment.SetEnvironmentVariable(AnpmSettings.FeedRootVar, null);
        Environment.SetEnvironmentVariable(AnpmSettings.V3BaseUrlVar, null);
        Environment.SetEnvironmentVariable(AnpmSettings.RebuildIndexOnStartVar, null);
        Environment.SetEnvironmentVariable(AnpmSettings.HostUrlsVar, null);
        AnpmBootstrap.Use(AnpmRuntimeSettings.Empty);
        try { Directory.Delete(_feedRoot, recursive: true); } catch { /* temp */ }
    }
}
