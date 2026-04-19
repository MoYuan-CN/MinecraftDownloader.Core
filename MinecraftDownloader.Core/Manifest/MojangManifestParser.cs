using System.Text.Json;
using MinecraftDownloader.Core.Abstractions;
using MinecraftDownloader.Core.Models;

namespace MinecraftDownloader.Core.Manifest;

/// <summary>Fetches and parses JSON from Mojang's launcher meta service.</summary>
public sealed class MojangManifestParser : IManifestParser
{
    private const string ManifestIndexUrl =
        "https://launchermeta.mojang.com/mc/game/version_manifest_v2.json";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    private readonly HttpClient _http;

    public MojangManifestParser(HttpClient http) => _http = http;

    public async Task<VersionManifestIndex> GetVersionManifestIndexAsync(CancellationToken ct = default)
    {
        var json = await _http.GetStringAsync(ManifestIndexUrl, ct);
        return Deserialize<VersionManifestIndex>(json, "version manifest index");
    }

    public async Task<VersionMetaResult> GetVersionMetaAsync(string versionId, CancellationToken ct = default)
    {
        var index = await GetVersionManifestIndexAsync(ct);
        var entry = index.Versions.FirstOrDefault(v => v.Id == versionId)
                    ?? throw new InvalidOperationException($"Version '{versionId}' not found in Mojang manifest.");

        var json = await _http.GetStringAsync(entry.Url, ct);
        var meta = Deserialize<VersionMeta>(json, $"version meta '{versionId}'");
        return new VersionMetaResult(meta, entry.Url, entry.Sha1);
    }

    public async Task<AssetIndex> GetAssetIndexAsync(AssetIndexRef assetRef, CancellationToken ct = default)
    {
        var json = await _http.GetStringAsync(assetRef.Url, ct);
        return Deserialize<AssetIndex>(json, $"asset index '{assetRef.Id}'");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static T Deserialize<T>(string json, string context)
        => JsonSerializer.Deserialize<T>(json, JsonOpts)
           ?? throw new InvalidDataException($"Null result deserialising {context}.");
}