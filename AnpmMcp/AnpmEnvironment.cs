namespace AnpmMcp;

internal static class AnpmEnvironment
{
    internal static string ResolveManifestPath(string? manifestPath)
    {
        var candidate = Anpm.Core.AnpmSettings.ResolveManifestPath(manifestPath);
        if (string.IsNullOrWhiteSpace(candidate))
        {
            var repoRoot = Anpm.Core.AnpmSettings.ResolveRepoRoot();
            candidate = Path.Combine(repoRoot, "manifest", "pins.example.json");
        }

        return Path.GetFullPath(candidate);
    }
}
