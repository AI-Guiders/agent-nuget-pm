using Anpm.Core;
using Anpm.Core.Feed;
using Anpm.Core.Models;
using Microsoft.Extensions.FileProviders;

namespace Anpm.Host;

internal static class FeedV3Endpoints
{
    internal static void MapFeedV3(this WebApplication app, AnpmHostRuntime runtime)
    {
        app.MapGet("/health", () => Results.Json(new
        {
            schema = "anpm/host/v1",
            ok = true,
            feedRoot = runtime.FeedRoot,
            v3BaseUrl = runtime.V3BaseUrl,
            packageCount = runtime.PackageCount
        }));

        app.MapGet("/v3/package/{id}/{version}/{fileName}", (string id, string version, string fileName) =>
        {
            var match = runtime.FindPackage(id, version, fileName);
            return match is null
                ? Results.NotFound()
                : Results.File(match.FullPath, "application/octet-stream", match.FileName);
        });

        if (!Directory.Exists(runtime.V3IndexRoot))
            return;

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(runtime.V3IndexRoot),
            RequestPath = "/v3"
        });
    }
}

internal sealed class AnpmHostRuntime
{
    public required string FeedRoot { get; init; }
    public required string V3BaseUrl { get; init; }
    public required string V3IndexRoot { get; init; }
    public required IReadOnlyList<FeedPackageEntry> Packages { get; init; }

    public int PackageCount => Packages.Count;

    public FeedPackageEntry? FindPackage(string id, string version, string fileName) =>
        Packages.FirstOrDefault(p =>
            string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)
            && string.Equals(p.Version, version, StringComparison.Ordinal)
            && string.Equals(p.FileName, fileName, StringComparison.OrdinalIgnoreCase));

    public static AnpmHostRuntime Create(string feedRoot, string v3BaseUrl, bool rebuildIndex)
    {
        var root = Path.GetFullPath(feedRoot);
        Directory.CreateDirectory(root);

        if (rebuildIndex)
            V3IndexWriter.Rebuild(root, v3BaseUrl);

        return new AnpmHostRuntime
        {
            FeedRoot = root,
            V3BaseUrl = v3BaseUrl,
            V3IndexRoot = Path.Combine(root, ".anpm", "v3"),
            Packages = FeedScanner.Scan(root)
        };
    }
}
