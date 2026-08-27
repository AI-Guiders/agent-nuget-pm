using Anpm.Core;
using Anpm.Core.Config;
using Anpm.View;

namespace Anpm.Host;

public class Program
{
    public static void Main(string[] args)
    {
        var bootstrap = AnpmBootstrap.Initialize(args);
        if (bootstrap.IsHelp)
        {
            PrintUsage();
            return;
        }

        if (!bootstrap.IsSuccess)
            throw new InvalidOperationException(bootstrap.Error ?? "Failed to load ANPM config.");

        var feedRoot = AnpmSettings.RequireFeedRoot();
        var v3BaseUrl = AnpmSettings.ResolveV3BaseUrl();
        var rebuildIndex = AnpmSettings.RebuildIndexOnStart();
        var runtime = AnpmHostRuntime.Create(feedRoot, v3BaseUrl, rebuildIndex);

        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls(AnpmSettings.ResolveHostUrls());
        builder.Services.AddSingleton<IAnpmViewConfig, BootstrapAnpmViewConfig>();
        builder.Services.AddAnpmView();

        var app = builder.Build();
        app.MapFeedV3(runtime);
        app.MapAnpmView();
        app.Run();
    }

    private static void PrintUsage()
    {
        Console.Error.WriteLine("""
            Anpm.Host [--config|-c PATH]

            Config SSOT: anpm.toml ([feed], [host]). ANPM_* env overrides TOML.
            See config/anpm.toml.example.
            """);
    }
}
