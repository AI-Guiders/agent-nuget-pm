using AgentForge.Abstractions;
using AgentForge.Plugin.PackageFeed.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentForge.Plugin.PackageFeed;

public sealed class PackageFeedForgePlugin : IForgePlugin
{
    public string Id => "package-feed";

    public string DisplayName => "ANPM package feed";

    public string Tier => "zoo";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration) =>
        services.AddSingleton(new PackageFeedOptions(configuration));

    public void MapEndpoints(WebApplication app)
    {
        var api = app.MapGroup("/api/v1");
        api.MapPackageFeedEndpoints();
    }

    public void RegisterFeatures(ForgeFeatureRegistry registry) => registry.Add("anpm_package_feed");

    public void RegisterCommands(ForgeCommandRegistry registry) => PackageFeedCommandRegistration.Register(registry);
}
