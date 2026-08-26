using AIGuiders.Cli;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Serialization;

namespace Anpm.Core.Config;

public static class AnpmConfigLoader
{
    public const string DefaultConfigRelativePath = "config/anpm.toml";

    public static AnpmLoadResult Load(string[] args)
    {
        if (HasHelp(args))
            return AnpmLoadResult.Help();

        var configPath = ResolveConfigPath(args, out var error);
        if (error is not null)
            return AnpmLoadResult.Fail(error);

        AnpmTomlValues? toml = null;
        if (!string.IsNullOrWhiteSpace(configPath))
        {
            var path = Path.GetFullPath(configPath.Trim());
            if (!File.Exists(path))
                return AnpmLoadResult.Fail($"Config file not found: {path}");

            try
            {
                toml = AnpmTomlParser.Parse(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                return AnpmLoadResult.Fail($"Invalid TOML config '{path}': {ex.Message}");
            }

            configPath = path;
        }

        var feed = ReadTable(toml?.Feed);
        var host = ReadTable(toml?.Host);
        var mcp = ReadTable(toml?.Mcp);

        var settings = new AnpmRuntimeSettings
        {
            ConfigPath = configPath,
            FeedRoot = FirstNonEmpty(
                Environment.GetEnvironmentVariable(AnpmSettings.FeedRootVar),
                ReadString(feed, "root")),
            ManifestPath = FirstNonEmpty(
                Environment.GetEnvironmentVariable(AnpmSettings.ManifestPathVar),
                ReadString(feed, "manifest_path")),
            V3BaseUrl = FirstNonEmpty(
                Environment.GetEnvironmentVariable(AnpmSettings.V3BaseUrlVar),
                ReadString(host, "v3_base_url")),
            HostUrls = FirstNonEmpty(
                Environment.GetEnvironmentVariable(AnpmSettings.HostUrlsVar),
                ReadString(host, "urls")),
            RebuildIndexOnStart = ReadBoolEnv(AnpmSettings.RebuildIndexOnStartVar)
                ?? ReadBool(host, "rebuild_index_on_start"),
            RepoRoot = FirstNonEmpty(
                Environment.GetEnvironmentVariable(AnpmSettings.RepoRootVar),
                ReadString(mcp, "repo_root")),
        };

        return AnpmLoadResult.Ok(settings);
    }

    private static string? ResolveConfigPath(string[] args, out string? error)
    {
        error = null;
        try
        {
            if (ConfigPathResolver.IsHelp(args))
                return null;

            var resolved = ConfigPathResolver.TryResolve(args);
            if (!string.IsNullOrWhiteSpace(resolved))
                return resolved;

            var defaultPath = Path.Combine(AppContext.BaseDirectory, DefaultConfigRelativePath);
            return File.Exists(defaultPath) ? defaultPath : null;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return null;
        }
    }

    private static bool HasHelp(string[] args) => ConfigPathResolver.IsHelp(args);

    private static TomlTable? ReadTable(TomlTable? table) => table;

    private static string? ReadString(TomlTable? table, string key)
    {
        if (table is null || !table.TryGetValue(key, out var value) || value is null)
            return null;

        return value switch
        {
            string s => s.Trim(),
            _ => value.ToString()?.Trim(),
        };
    }

    private static bool? ReadBool(TomlTable? table, string key)
    {
        if (table is null || !table.TryGetValue(key, out var value) || value is null)
            return null;

        return value switch
        {
            bool b => b,
            string s when bool.TryParse(s, out var parsed) => parsed,
            string s when s is "1" or "0" => s == "1",
            _ => throw new InvalidOperationException($"Expected boolean for '{key}'."),
        };
    }

    private static bool? ReadBoolEnv(string name)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        return raw.Trim() switch
        {
            "1" or "true" or "True" or "yes" or "YES" => true,
            "0" or "false" or "False" or "no" or "NO" => false,
            _ => bool.TryParse(raw, out var parsed) ? parsed : null,
        };
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }
}

public sealed class AnpmLoadResult
{
    public AnpmRuntimeSettings? Settings { get; init; }

    public string? Error { get; init; }

    public bool IsHelp { get; init; }

    public bool IsSuccess => Settings is not null && Error is null;

    public static AnpmLoadResult Ok(AnpmRuntimeSettings settings) => new() { Settings = settings };

    public static AnpmLoadResult Fail(string error) => new() { Error = error };

    public static AnpmLoadResult Help() => new() { IsHelp = true };
}

internal sealed class AnpmTomlValues
{
    public TomlTable? Feed { get; init; }

    public TomlTable? Host { get; init; }

    public TomlTable? Mcp { get; init; }
}

internal static class AnpmTomlParser
{
    internal const int SupportedVersion = 1;

    internal static AnpmTomlValues Parse(string text)
    {
        var model = TomlSerializer.Deserialize<TomlTable>(text)
            ?? throw new InvalidOperationException("Empty TOML document.");
        var version = ReadInt(model, "version");
        if (version is not null && version != SupportedVersion)
            throw new InvalidOperationException($"Unsupported version {version}; expected {SupportedVersion}.");

        return new AnpmTomlValues
        {
            Feed = ReadTable(model, "feed"),
            Host = ReadTable(model, "host"),
            Mcp = ReadTable(model, "mcp"),
        };
    }

    private static TomlTable? ReadTable(TomlTable model, string key) =>
        model.TryGetValue(key, out var value) && value is TomlTable table ? table : null;

    private static int? ReadInt(TomlTable model, string key)
    {
        if (!model.TryGetValue(key, out var value) || value is null)
            return null;

        return value switch
        {
            int i => i,
            long l => (int)l,
            string s when int.TryParse(s, out var parsed) => parsed,
            _ => throw new InvalidOperationException($"Expected integer for '{key}'."),
        };
    }
}
