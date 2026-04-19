using System.Text.Json.Serialization;

namespace MinecraftDownloader.Core.Models;

public record LoggingConfig(
    [property: JsonPropertyName("client")] LoggingClient? Client
);

public record LoggingClient(
    [property: JsonPropertyName("file")] DownloadEntry File,
    [property: JsonPropertyName("argument")]
    string Argument,
    [property: JsonPropertyName("type")] string Type
);