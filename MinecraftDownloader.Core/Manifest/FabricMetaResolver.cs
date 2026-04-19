using System.Text.Json;
using System.Text.Json.Serialization;
using MinecraftDownloader.Core.Abstractions;
using MinecraftDownloader.Core.Models;

namespace MinecraftDownloader.Core.Manifest;

// ─────────────────────────────────────────────────────────────────────────────
//  Shared base
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
///   Base class for Fabric and Quilt meta resolvers.  Both meta APIs share
///   the same REST shape; only the base URL differs.
/// </summary>
public abstract class FabricCompatMetaResolver : IFabricMetaResolver
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    private readonly HttpClient _http;

    /// <summary>e.g. "https://meta.fabricmc.net" or "https://meta.quiltmc.org"</summary>
    protected abstract string MetaBaseUrl { get; }

    protected FabricCompatMetaResolver(HttpClient http) => _http = http;

    public async Task<FabricProfileMeta> ResolveAsync(
        string gameVersion,
        string? loaderVersion,
        CancellationToken ct = default)
    {
        var resolved = loaderVersion ?? await ResolveLatestLoaderAsync(gameVersion, ct);

        // GET /v2/versions/loader/{game}/{loader}/profile/json
        var url = $"{MetaBaseUrl}/v2/versions/loader/{gameVersion}/{resolved}/profile/json";
        var json = await _http.GetStringAsync(url, ct);

        return JsonSerializer.Deserialize<FabricProfileMeta>(json, JsonOpts)
               ?? throw new InvalidDataException(
                   $"Null result parsing {GetType().Name} profile for {gameVersion}/{resolved}.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> ResolveLatestLoaderAsync(string gameVersion, CancellationToken ct)
    {
        // GET /v2/versions/loader/{game}  → array of { loader: { version, stable } }
        var url = $"{MetaBaseUrl}/v2/versions/loader/{gameVersion}";
        var json = await _http.GetStringAsync(url, ct);
        var items = JsonSerializer.Deserialize<IReadOnlyList<LoaderListEntry>>(json, JsonOpts)
                    ?? throw new InvalidDataException("Empty loader list.");

        var stable = items.FirstOrDefault(l => l.Loader.Stable)
                     ?? items.FirstOrDefault()
                     ?? throw new InvalidOperationException(
                         $"No {GetType().Name} loader found for Minecraft {gameVersion}.");

        return stable.Loader.Version;
    }

    // Local DTO
    private record LoaderListEntry(
        [property: JsonPropertyName("loader")] LoaderRef Loader
    );

    private record LoaderRef(
        [property: JsonPropertyName("version")]
        string Version,
        [property: JsonPropertyName("stable")] bool Stable
    );
}

// ─────────────────────────────────────────────────────────────────────────────
//  Fabric
// ─────────────────────────────────────────────────────────────────────────────

public sealed class FabricMetaResolver : FabricCompatMetaResolver
{
    protected override string MetaBaseUrl => "https://meta.fabricmc.net";

    public FabricMetaResolver(HttpClient http) : base(http)
    {
    }
}

// ─────────────────────────────────────────────────────────────────────────────
//  Quilt
// ─────────────────────────────────────────────────────────────────────────────

public sealed class QuiltMetaResolver : FabricCompatMetaResolver
{
    protected override string MetaBaseUrl => "https://meta.quiltmc.org";

    public QuiltMetaResolver(HttpClient http) : base(http)
    {
    }
}