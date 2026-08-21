using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anpm.Core;

public static class AnpmJson
{
    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, Options);

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
