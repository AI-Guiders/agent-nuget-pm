using Anpm.Core.Feed;
using Anpm.Core.Models;
using Xunit;

namespace AnpmMcp.Tests;

public class FeedScannerTests
{
    [Fact]
    public void TryParsePackage_parses_standard_file_name()
    {
        var ok = FeedScanner.TryParsePackage(
            "Contoso.Data.Access.2.1.0.nupkg",
            @"C:\feed\Contoso.Data.Access.2.1.0.nupkg",
            out var entry);

        Assert.True(ok);
        Assert.NotNull(entry);
        Assert.Equal("Contoso.Data.Access", entry!.Id);
        Assert.Equal("2.1.0", entry.Version);
    }

    [Fact]
    public void FeedStatusService_reports_missing_pins()
    {
        var temp = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "anpm-test-" + Guid.NewGuid().ToString("n")));
        try
        {
            var manifest = new AnpmManifest
            {
                Schema = "anpm/manifest/v1",
                Packages =
                [
                    new ManifestPackagePin { Id = "Contoso.A", Version = "1.0.0" },
                    new ManifestPackagePin { Id = "Contoso.B", Version = "2.0.0" }
                ]
            };

            File.WriteAllText(Path.Combine(temp.FullName, "Contoso.A.1.0.0.nupkg"), "fake");

            var report = FeedStatusService.Build(temp.FullName, "manifest.json", manifest);
            Assert.False(report.IsComplete);
            Assert.Single(report.Missing);
            Assert.Equal("Contoso.B", report.Missing[0].Id);
            Assert.Single(report.Present);
        }
        finally
        {
            temp.Delete(recursive: true);
        }
    }
}

public class ManifestLoaderTests
{
    [Fact]
    public void Load_reads_example_manifest()
    {
        var repoRoot = FindRepoRoot();
        var manifestPath = Path.Combine(repoRoot, "manifest", "pins.example.json");
        var manifest = ManifestLoader.Load(manifestPath);

        Assert.Equal("anpm/manifest/v1", manifest.Schema);
        Assert.Contains(manifest.Packages, p => p.Id == "Contoso.Library" && p.Version == "1.0.0");
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
