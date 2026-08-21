using Anpm.Core;
using Anpm.Core.Config;
using Xunit;

namespace AnpmMcp.Tests;

public class AnpmConfigLoaderTests
{
    [Fact]
    public void Load_reads_feed_and_host_from_toml()
    {
        Environment.SetEnvironmentVariable(AnpmSettings.FeedRootVar, null);
        Environment.SetEnvironmentVariable(AnpmSettings.ManifestPathVar, null);
        Environment.SetEnvironmentVariable(AnpmSettings.V3BaseUrlVar, null);
        Environment.SetEnvironmentVariable(AnpmSettings.HostUrlsVar, null);
        Environment.SetEnvironmentVariable(AnpmSettings.RebuildIndexOnStartVar, null);

        var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "anpm-cfg-" + Guid.NewGuid().ToString("n")));
        var configPath = Path.Combine(dir.FullName, "anpm.toml");
        File.WriteAllText(configPath, """
            version = 1

            [feed]
            root = "C:/feed"
            manifest_path = "C:/pins.json"

            [host]
            urls = "http://127.0.0.1:5099"
            v3_base_url = "http://127.0.0.1:5099/v3"
            rebuild_index_on_start = false
            """);

        var load = AnpmConfigLoader.Load(["--config", configPath]);
        Assert.True(load.IsSuccess);
        Assert.NotNull(load.Settings);
        Assert.Equal(Path.GetFullPath(configPath), load.Settings!.ConfigPath);
        Assert.Equal("C:/feed", load.Settings.FeedRoot);
        Assert.Equal("C:/pins.json", load.Settings.ManifestPath);
        Assert.Equal("http://127.0.0.1:5099/v3", load.Settings.V3BaseUrl);
        Assert.Equal("http://127.0.0.1:5099", load.Settings.HostUrls);
        Assert.False(load.Settings.RebuildIndexOnStart);

        try { Directory.Delete(dir.FullName, recursive: true); } catch { /* temp */ }
    }

    [Fact]
    public void Load_env_overrides_toml()
    {
        var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "anpm-cfg-" + Guid.NewGuid().ToString("n")));
        var configPath = Path.Combine(dir.FullName, "anpm.toml");
        File.WriteAllText(configPath, """
            version = 1
            [feed]
            root = "C:/from-toml"
            """);

        Environment.SetEnvironmentVariable(AnpmSettings.FeedRootVar, "C:/from-env");
        try
        {
            var load = AnpmConfigLoader.Load(["--config", configPath]);
            Assert.True(load.IsSuccess);
            Assert.Equal("C:/from-env", load.Settings!.FeedRoot);
        }
        finally
        {
            Environment.SetEnvironmentVariable(AnpmSettings.FeedRootVar, null);
            try { Directory.Delete(dir.FullName, recursive: true); } catch { /* temp */ }
        }
    }

    [Fact]
    public void Bootstrap_wires_runtime_settings()
    {
        Environment.SetEnvironmentVariable(AnpmSettings.FeedRootVar, null);
        AnpmBootstrap.Use(AnpmRuntimeSettings.Empty);
        var load = AnpmBootstrap.Initialize(["--config", Path.Combine(FindRepoRoot(), "config", "anpm.toml.example")]);
        Assert.True(load.IsSuccess);
        Assert.Equal("C:/local/nuget-feed", AnpmBootstrap.Current.FeedRoot);
        AnpmBootstrap.Use(AnpmRuntimeSettings.Empty);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "AnpmMcp.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Repo root not found.");
    }
}
