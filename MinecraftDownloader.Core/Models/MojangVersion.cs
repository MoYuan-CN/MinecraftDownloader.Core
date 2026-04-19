using System.Text.Json.Serialization;

namespace MinecraftDownloader.Core.Models;

// ═══════════════════════════════════════════════════════════════════════════════
//  Mojang version manifest
// ═══════════════════════════════════════════════════════════════════════════════

public record VersionManifestIndex(
    [property: JsonPropertyName("latest")] LatestVersions Latest,
    [property: JsonPropertyName("versions")]
    IReadOnlyList<VersionEntry> Versions
);

public record LatestVersions(
    [property: JsonPropertyName("release")]
    string Release,
    [property: JsonPropertyName("snapshot")]
    string Snapshot
);

public record VersionEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("releaseTime")]
    DateTimeOffset ReleaseTime,
    [property: JsonPropertyName("sha1")] string Sha1
);