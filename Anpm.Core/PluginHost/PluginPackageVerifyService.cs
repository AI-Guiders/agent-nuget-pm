using AIGuiders.PluginHost.Runtime;

namespace Anpm.Core.PluginHost;

public static class PluginPackageVerifyService
{
    public static object Verify(string packageRoot)
    {
        var result = PluginPackageVerifier.VerifyDirectory(packageRoot);
        return new
        {
            ok = result.Ok,
            errors = result.Errors,
            manifest = result.Manifest is null
                ? null
                : new { id = result.Manifest.Id, version = result.Manifest.Version, schema = result.Manifest.Schema }
        };
    }
}
