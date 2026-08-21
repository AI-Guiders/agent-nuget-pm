using AgentForge.Abstractions;

namespace AgentForge.Plugin.PackageFeed;

internal static class PackageFeedCommandRegistration
{
    internal static void Register(ForgeCommandRegistry registry)
    {
        registry.Add(new ForgeCommandDescriptor
        {
            Domain = "anpm",
            Object = "feed",
            Intent = "status",
            CommandId = "anpm.feed.status",
            Path = "/package feed status",
            PathAliases = ["/anpm feed status"],
            Help = "Pinned manifest packages vs .nupkg on ANPM feed_root.",
            Category = "Package feed",
            Surfaces = ["command-bar", "global"],
            RequiredCapabilities = ["read"],
            Bindings = new ForgeCommandBindings
            {
                View = "/api/v1/package-feed/status",
            },
        });

        registry.Add(new ForgeCommandDescriptor
        {
            Domain = "anpm",
            Object = "feed",
            Intent = "sync",
            CommandId = "anpm.feed.sync",
            Path = "/package feed sync",
            PathAliases = ["/anpm feed sync"],
            Help = "Download missing pinned packages into feed_root (sync host; inet required).",
            Category = "Package feed",
            Surfaces = ["command-bar", "global"],
            RequiredCapabilities = ["write"],
            Bindings = new ForgeCommandBindings
            {
                View = "/api/v1/package-feed/sync",
            },
        });
    }
}
