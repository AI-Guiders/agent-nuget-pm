using Anpm.Core;

namespace Anpm.Host;

public class Program
{
    public static void Main(string[] args)
    {
        var feedRoot = AnpmSettings.RequireFeedRoot();
        var v3BaseUrl = AnpmSettings.ResolveV3BaseUrl();
        var rebuildIndex = AnpmSettings.RebuildIndexOnStart();
        var runtime = AnpmHostRuntime.Create(feedRoot, v3BaseUrl, rebuildIndex);

        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls(Environment.GetEnvironmentVariable(AnpmSettings.HostUrlsVar) ?? "http://127.0.0.1:5088");

        var app = builder.Build();
        app.MapFeedV3(runtime);
        app.Run();
    }
}
