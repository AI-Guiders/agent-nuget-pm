using Anpm.Core.Config;

namespace Anpm.Core;

public static class AnpmBootstrap
{
    static AnpmRuntimeSettings _current = AnpmRuntimeSettings.Empty;

    public static AnpmRuntimeSettings Current => _current;

    public static void Use(AnpmRuntimeSettings settings) =>
        _current = settings ?? AnpmRuntimeSettings.Empty;

    public static AnpmLoadResult Initialize(string[] args)
    {
        var load = AnpmConfigLoader.Load(args);
        if (load.IsSuccess && load.Settings is not null)
            Use(load.Settings);

        return load;
    }
}
