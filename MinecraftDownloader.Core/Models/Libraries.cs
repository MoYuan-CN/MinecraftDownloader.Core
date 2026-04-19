using System.Text.Json.Serialization;

namespace MinecraftDownloader.Core.Models;

public record Library(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("downloads")]
    LibraryDownloads? Downloads,
    [property: JsonPropertyName("url")] string? Url, // Fabric / Quilt repo URL
    [property: JsonPropertyName("rules")] IReadOnlyList<LibraryRule>? Rules
);

public record LibraryDownloads(
    [property: JsonPropertyName("artifact")]
    LibraryArtifact? Artifact
);

public record LibraryArtifact(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("sha1")] string Sha1,
    [property: JsonPropertyName("size")] long Size
);

public record LibraryRule(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("os")] OsCondition? Os
);

public record OsCondition(
    [property: JsonPropertyName("name")] string Name
);