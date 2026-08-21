namespace AnpmMcp;

internal static class AnpmEnvironment
{
    internal const string FeedRootVar = Anpm.Core.AnpmSettings.FeedRootVar;
    internal const string ManifestPathVar = Anpm.Core.AnpmSettings.ManifestPathVar;
    internal const string V3BaseUrlVar = Anpm.Core.AnpmSettings.V3BaseUrlVar;
    internal const string RepoRootVar = "ANPM_REPO_ROOT";

    internal static string ResolveManifestPath(string? manifestPath)
    {
        var candidate = manifestPath?.Trim();
        if (string.IsNullOrWhiteSpace(candidate))
            candidate = Environment.GetEnvironmentVariable(ManifestPathVar);

        if (string.IsNullOrWhiteSpace(candidate))
        {
            var repoRoot = ResolveRepoRoot();
            candidate = Path.Combine(repoRoot, "manifest", "pins.example.json");
        }

        return Path.GetFullPath(candidate);
    }

    internal static string ResolveRepoRoot()
    {
        var fromEnv = Environment.GetEnvironmentVariable(RepoRootVar);
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return Path.GetFullPath(fromEnv);

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "AnpmMcp.slnx"))
                || File.Exists(Path.Combine(dir.FullName, "manifest", "pins.example.json")))
                return dir.FullName;

            dir = dir.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
