using System.Text.Json.Serialization;

namespace MinecraftDownloader.Core.Models;

public record VersionMeta(
    [property: JsonPropertyName("id")]          string                   Id,
    [property: JsonPropertyName("type")]        string                   Type,
    [property: JsonPropertyName("mainClass")]   string                   MainClass,
    [property: JsonPropertyName("downloads")]   VersionDownloads         Downloads,
    [property: JsonPropertyName("assetIndex")]  AssetIndexRef            AssetIndex,
    [property: JsonPropertyName("assets")]      string                   Assets,
    [property: JsonPropertyName("libraries")]   IReadOnlyList<Library>   Libraries,
    [property: JsonPropertyName("logging")]     LoggingConfig?           Logging
);

public record VersionDownloads(
    [property: JsonPropertyName("client")]          DownloadEntry  Client,
    [property: JsonPropertyName("client_mappings")] DownloadEntry? ClientMappings,
    [property: JsonPropertyName("server")]          DownloadEntry? Server
);

public record DownloadEntry(
    [property: JsonPropertyName("url")]  string Url,
    [property: JsonPropertyName("sha1")] string Sha1,
    [property: JsonPropertyName("size")] long   Size
);

public record AssetIndexRef(
    [property: JsonPropertyName("id")]        string Id,
    [property: JsonPropertyName("url")]       string Url,
    [property: JsonPropertyName("sha1")]      string Sha1,
    [property: JsonPropertyName("size")]      long   Size,
    [property: JsonPropertyName("totalSize")] long   TotalSize
);