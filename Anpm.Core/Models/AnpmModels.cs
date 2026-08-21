namespace Anpm.Core.Models;

public sealed record AnpmManifest
{
    public required string Schema { get; init; }

    public string? FeedRoot { get; init; }

    public string? V3BaseUrl { get; init; }

    public required IReadOnlyList<ManifestPackagePin> Packages { get; init; }
}

public sealed record ManifestPackagePin
{
    public required string Id { get; init; }

    public required string Version { get; init; }
}

public sealed record FeedPackageEntry
{
    public required string Id { get; init; }

    public required string Version { get; init; }

    public required string FileName { get; init; }

    public required string FullPath { get; init; }

    public long SizeBytes { get; init; }
}

public sealed record FeedStatusReport
{
    public required string FeedRoot { get; init; }

    public required string ManifestPath { get; init; }

    public required IReadOnlyList<ManifestPackagePin> Pinned { get; init; }

    public required IReadOnlyList<FeedPackageEntry> Present { get; init; }

    public required IReadOnlyList<ManifestPackagePin> Missing { get; init; }

    public required IReadOnlyList<FeedPackageEntry> Extra { get; init; }

    public bool IsComplete => Missing.Count == 0;
}

public sealed record IndexRebuildReport
{
    public required string FeedRoot { get; init; }

    public required string V3IndexRoot { get; init; }

    public required int PackageCount { get; init; }

    public required IReadOnlyList<string> WrittenFiles { get; init; }
}

public sealed record RestoreVerifyReport
{
    public required string TargetPath { get; init; }

    public required bool Success { get; init; }

    public required string Output { get; init; }
}

public sealed record FeedSyncReport
{
    public required string FeedRoot { get; init; }

    public required IReadOnlyList<ManifestPackagePin> Requested { get; init; }

    public required IReadOnlyList<ManifestPackagePin> Downloaded { get; init; }

    public required IReadOnlyList<ManifestPackagePin> Skipped { get; init; }

    public required IReadOnlyList<string> Errors { get; init; }

    public bool DryRun { get; init; }
}
