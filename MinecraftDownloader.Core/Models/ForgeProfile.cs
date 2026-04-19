using System.Text.Json.Serialization;

namespace MinecraftDownloader.Core.Models;

public record ForgeInstallProfile(
    [property: JsonPropertyName("version")]
    string Version,
    [property: JsonPropertyName("minecraft")]
    string Minecraft,
    [property: JsonPropertyName("json")] string JsonPath, // path inside installer jar
    [property: JsonPropertyName("path")] string? Path, // maven path of forge universal
    [property: JsonPropertyName("libraries")]
    IReadOnlyList<Library> Libraries,
    [property: JsonPropertyName("data")] IReadOnlyDictionary<string, ForgeDataValue>? Data,
    [property: JsonPropertyName("processors")]
    IReadOnlyList<ForgeProcessor>? Processors
);

public record ForgeDataValue(
    [property: JsonPropertyName("client")] string Client,
    [property: JsonPropertyName("server")] string Server
);

public record ForgeProcessor(
    [property: JsonPropertyName("jar")] string Jar,
    [property: JsonPropertyName("classpath")]
    IReadOnlyList<string> Classpath,
    [property: JsonPropertyName("args")] IReadOnlyList<string> Args,
    [property: JsonPropertyName("outputs")]
    IReadOnlyDictionary<string, string>? Outputs,
    [property: JsonPropertyName("sides")] IReadOnlyList<string>? Sides
);