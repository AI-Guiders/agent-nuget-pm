using System.Collections.Frozen;
using System.Text.Json;
using AnpmMcp;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Tool = ModelContextProtocol.Protocol.Tool;

if (TryRunCli(args, out var exitCode))
    return exitCode;

var toolsList = ToolCatalog.Build();

var options = new McpServerOptions
{
    ServerInfo = new Implementation { Name = "AnpmMcp", Version = "0.1.0-m1" },
    ProtocolVersion = "2024-11-05",
    Capabilities = new ServerCapabilities { Tools = new ToolsCapability { ListChanged = false } },
    Handlers = new McpServerHandlers
    {
        ListToolsHandler = (_, _) => ValueTask.FromResult(new ListToolsResult { Tools = toolsList }),
        CallToolHandler = (request, cancellationToken) =>
        {
            _ = cancellationToken;
            var name = request.Params?.Name ?? "";
            var argsDict = request.Params?.Arguments is IReadOnlyDictionary<string, JsonElement> a
                ? a
                : FrozenDictionary<string, JsonElement>.Empty;
            return ValueTask.FromResult(InvokeTool(name, argsDict));
        }
    }
};

var transport = new StdioServerTransport("AnpmMcp");
await using var server = McpServer.Create(transport, options);
await server.RunAsync();
return 0;

static CallToolResult InvokeTool(string name, IReadOnlyDictionary<string, JsonElement> args)
{
    try
    {
        var text = ToolHandlers.Handle(name, args);
        return new CallToolResult
        {
            Content = [new TextContentBlock { Text = text }],
            IsError = false
        };
    }
    catch (ArgumentException ex)
    {
        return new CallToolResult
        {
            Content = [new TextContentBlock { Text = $"Error: {ex.Message}" }],
            IsError = true
        };
    }
    catch (Exception ex)
    {
        return new CallToolResult
        {
            Content = [new TextContentBlock { Text = "Error: " + ex.Message }],
            IsError = true
        };
    }
}

static bool TryRunCli(string[] args, out int exitCode)
{
    exitCode = 0;
    if (args.Length == 0 || !string.Equals(args[0], "--invoke", StringComparison.Ordinal))
        return false;

    if (args.Length < 2)
        throw new ArgumentException("Usage: AnpmMcp --invoke <tool> [--key value ...]");

    var tool = args[1];
    var toolArgs = ParseCliArgs(args.AsSpan(2));
    var result = InvokeTool(tool, toolArgs);
    var text = result.Content.FirstOrDefault() is TextContentBlock block ? block.Text : string.Empty;
    Console.WriteLine(text);
    exitCode = result.IsError == true ? 1 : 0;
    return true;
}

static FrozenDictionary<string, JsonElement> ParseCliArgs(ReadOnlySpan<string> args)
{
    var map = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < args.Length; i++)
    {
        var token = args[i];
        if (!token.StartsWith("--", StringComparison.Ordinal))
            continue;

        var key = token[2..];
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Missing value for --{key}");

        var value = args[++i];
        map[key] = JsonSerializer.SerializeToElement(ParseCliValue(value));
    }

    return map.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}

static object ParseCliValue(string value) =>
    value switch
    {
        "true" => true,
        "false" => false,
        _ when int.TryParse(value, out var n) => n,
        _ => value
    };
