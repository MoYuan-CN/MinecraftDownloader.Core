using System.Text.Json.Serialization;

namespace MinecraftDownloader.Core.Models;

public record AssetIndex(
    [property: JsonPropertyName("objects")]
    IReadOnlyDictionary<string, AssetObject> Objects,
    [property: JsonPropertyName("virtual")]
    bool Virtual,
    [property: JsonPropertyName("map_to_resources")]
    bool MapToResources
);

public record AssetObject(
    [property: JsonPropertyName("hash")] string Hash,
    [property: JsonPropertyName("size")] long Size
);