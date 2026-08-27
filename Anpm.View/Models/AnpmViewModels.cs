using Anpm.Core.Models;

namespace Anpm.View.Models;

public sealed class FeedOverviewModel
{
    public bool Configured { get; init; }

    public string? ConfigError { get; init; }

    public FeedStatusReport? Status { get; init; }

    public string? V3BaseUrl { get; init; }

    public string RoutePrefix { get; init; } = "/view";

    public SyncFlashModel? Flash { get; init; }
}

public sealed class PinMatrixModel
{
    public bool Configured { get; init; }

    public string? ConfigError { get; init; }

    public required IReadOnlyList<PinRowModel> Rows { get; init; }

    public string RoutePrefix { get; init; } = "/view";

    public SyncFlashModel? Flash { get; init; }
}

public sealed class PinRowModel
{
    public required string Id { get; init; }

    public required string Version { get; init; }

    public required string State { get; init; }
}

public sealed class SyncFlashModel
{
    public required bool DryRun { get; init; }

    public required int Downloaded { get; init; }

    public required int Skipped { get; init; }

    public required IReadOnlyList<string> Errors { get; init; }
}
