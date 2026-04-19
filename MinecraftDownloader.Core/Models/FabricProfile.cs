using System.Text.Json.Serialization;

namespace MinecraftDownloader.Core.Models;

public record FabricLoaderVersion(
    [property: JsonPropertyName("separator")]
    string Separator,
    [property: JsonPropertyName("build")] int Build,
    [property: JsonPropertyName("maven")] string Maven,
    [property: JsonPropertyName("version")]
    string Version,
    [property: JsonPropertyName("stable")] bool Stable
);

public record FabricLaunchWrapper(
    [property: JsonPropertyName("version")]
    string Version,
    [property: JsonPropertyName("maven")] string Maven,
    [property: JsonPropertyName("stable")] bool Stable
);

/// <summary>
///   Response from  GET /v2/versions/loader/{game}/{loader}/profile/json
///   (same shape for both Fabric and Quilt).
/// </summary>
public record FabricProfileMeta(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("inheritsFrom")]
    string InheritsFrom,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("mainClass")]
    string MainClass,
    [property: JsonPropertyName("arguments")]
    FabricArguments? Arguments,
    [property: JsonPropertyName("libraries")]
    IReadOnlyList<Library> Libraries
);

public record FabricArguments(
    [property: JsonPropertyName("game")] IReadOnlyList<string>? Game,
    [property: JsonPropertyName("jvm")] IReadOnlyList<string>? Jvm
);