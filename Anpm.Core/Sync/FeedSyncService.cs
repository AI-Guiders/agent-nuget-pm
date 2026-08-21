using System.Diagnostics;
using Anpm.Core.Feed;
using Anpm.Core.Models;

namespace Anpm.Core.Sync;

public static class FeedSyncService
{
    public static FeedSyncReport Sync(
        string feedRoot,
        IReadOnlyList<ManifestPackagePin> pins,
        bool dryRun,
        string nugetSource = "https://api.nuget.org/v3/index.json")
    {
        Directory.CreateDirectory(feedRoot);
        var present = FeedScanner.Scan(feedRoot)
            .Select(p => $"{p.Id}|{p.Version}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var downloaded = new List<ManifestPackagePin>();
        var skipped = new List<ManifestPackagePin>();
        var errors = new List<string>();

        foreach (var pin in pins)
        {
            var key = $"{pin.Id}|{pin.Version}";
            if (present.Contains(key))
            {
                skipped.Add(pin);
                continue;
            }

            if (dryRun)
            {
                downloaded.Add(pin);
                continue;
            }

            var targetFile = Path.Combine(feedRoot, $"{pin.Id}.{pin.Version}.nupkg");
            var args = $"add package {pin.Id} --version {pin.Version} --source {Quote(nugetSource)} --package-directory {Quote(feedRoot)} --no-cache";
            var (exitCode, stdout, stderr) = RunDotnetNuget(args);
            if (exitCode != 0)
            {
                errors.Add($"{pin.Id} {pin.Version}: {stderr.Trim()} {stdout.Trim()}".Trim());
                continue;
            }

            if (!File.Exists(targetFile))
            {
                var fallback = Directory.EnumerateFiles(feedRoot, $"{pin.Id}.*.nupkg", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(f => f.Contains(pin.Version, StringComparison.OrdinalIgnoreCase));
                if (fallback is null)
                {
                    errors.Add($"{pin.Id} {pin.Version}: download finished but .nupkg not found in feed root.");
                    continue;
                }
            }

            downloaded.Add(pin);
            present.Add(key);
        }

        return new FeedSyncReport
        {
            FeedRoot = feedRoot,
            Requested = pins,
            Downloaded = downloaded,
            Skipped = skipped,
            Errors = errors,
            DryRun = dryRun
        };
    }

    private static (int ExitCode, string StdOut, string StdErr) RunDotnetNuget(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"nuget {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, stdout, stderr);
    }

    private static string Quote(string value) => value.Contains('"') ? value : $"\"{value}\"";
}
