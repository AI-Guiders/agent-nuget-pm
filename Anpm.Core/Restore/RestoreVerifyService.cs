using System.Diagnostics;
using Anpm.Core.Models;

namespace Anpm.Core.Restore;

public static class RestoreVerifyService
{
    public static RestoreVerifyReport Verify(string targetPath, string? feedRoot, bool dryRun = true)
    {
        if (string.IsNullOrWhiteSpace(targetPath))
            throw new ArgumentException("target_path is required.", nameof(targetPath));

        var fullPath = Path.GetFullPath(targetPath);
        if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
            throw new FileNotFoundException($"Target not found: {fullPath}", fullPath);

        var projectOrSln = File.Exists(fullPath)
            ? fullPath
            : Directory.EnumerateFiles(fullPath, "*.sln*", SearchOption.TopDirectoryOnly).FirstOrDefault()
              ?? Directory.EnumerateFiles(fullPath, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault()
              ?? throw new FileNotFoundException($"No .sln/.csproj under: {fullPath}");

        var args = new List<string> { "restore", Quote(projectOrSln), "--verbosity", "minimal" };
        if (dryRun)
            args.Add("--dry-run");

        if (!string.IsNullOrWhiteSpace(feedRoot))
            args.AddRange(["--source", Quote(Path.GetFullPath(feedRoot)), "--source", Quote("https://api.nuget.org/v3/index.json")]);

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = string.Join(' ', args),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(projectOrSln) ?? Environment.CurrentDirectory
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        var output = string.Join(Environment.NewLine, new[] { stdout, stderr }.Where(s => !string.IsNullOrWhiteSpace(s)));

        return new RestoreVerifyReport
        {
            TargetPath = projectOrSln,
            Success = process.ExitCode == 0,
            Output = output.Trim()
        };
    }

    private static string Quote(string value) => value.Contains('"') ? value : $"\"{value}\"";
}
