namespace Anpm.Core.Config;

public sealed class AnpmRuntimeSettings
{
    public static AnpmRuntimeSettings Empty { get; } = new();

    public string? ConfigPath { get; init; }

    public string? FeedRoot { get; init; }

    public string? ManifestPath { get; init; }

    public string? V3BaseUrl { get; init; }

    public string? HostUrls { get; init; }

    public bool? RebuildIndexOnStart { get; init; }

    public string? RepoRoot { get; init; }
}
