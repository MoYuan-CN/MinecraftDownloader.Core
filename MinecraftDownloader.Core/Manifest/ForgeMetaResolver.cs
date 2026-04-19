using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using MinecraftDownloader.Core.Abstractions;
using MinecraftDownloader.Core.Models;

namespace MinecraftDownloader.Core.Manifest;

/// <summary>
///   Downloads the Forge installer JAR for the requested version, extracts
///   <c>install_profile.json</c> and <c>version.json</c> from it, then parses both.
/// </summary>
public sealed class ForgeMetaResolver : IForgeMetaResolver
{
    // Maven URL template for the Forge installer JAR
    private const string ForgeInstallerTemplate =
        "https://maven.minecraftforge.net/net/minecraftforge/forge/{0}/forge-{0}-installer.jar";

    // Forge promotions JSON (stable / latest per MC version)
    private const string ForgePromotionsUrl =
        "https://files.minecraftforge.net/net/minecraftforge/forge/promotions_slim.json";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    private readonly HttpClient _http;

    public ForgeMetaResolver(HttpClient http) => _http = http;

    public async Task<(ForgeInstallProfile Profile, VersionMeta VersionJson)> ResolveAsync(
        string gameVersion,
        string? forgeVersion,
        CancellationToken ct = default)
    {
        var resolvedForge = forgeVersion
                            ?? await ResolveLatestForgeVersionAsync(gameVersion, ct);

        var combinedVersion = $"{gameVersion}-{resolvedForge}";
        var jarUrl = string.Format(ForgeInstallerTemplate, combinedVersion);

        using var jarStream = await _http.GetStreamAsync(jarUrl, ct);
        using var zip = new ZipArchive(jarStream, ZipArchiveMode.Read, leaveOpen: false);

        var profile = ReadZipEntry<ForgeInstallProfile>(zip, "install_profile.json");
        var versionJson = ReadZipEntry<VersionMeta>(zip, "version.json");

        return (profile, versionJson);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> ResolveLatestForgeVersionAsync(string gameVersion, CancellationToken ct)
    {
        var json = await _http.GetStringAsync(ForgePromotionsUrl, ct);
        var promotions = JsonSerializer.Deserialize<ForgePromotions>(json, JsonOpts)
                         ?? throw new InvalidDataException("Failed to parse Forge promotions JSON.");

        // Prefer "recommended", fall back to "latest"
        var key = $"{gameVersion}-recommended";
        if (promotions.Promos.TryGetValue(key, out var version))
            return version;

        key = $"{gameVersion}-latest";
        if (promotions.Promos.TryGetValue(key, out version))
            return version;

        throw new InvalidOperationException(
            $"No Forge build found for Minecraft {gameVersion}. " +
            $"Specify a forgeVersion explicitly.");
    }

    private static T ReadZipEntry<T>(ZipArchive zip, string entryName)
    {
        var entry = zip.GetEntry(entryName)
                    ?? throw new FileNotFoundException(
                        $"Entry '{entryName}' not found in Forge installer JAR.");

        using var stream = entry.Open();
        return JsonSerializer.Deserialize<T>(stream, JsonOpts)
               ?? throw new InvalidDataException($"Null result deserialising '{entryName}'.");
    }

    // Local DTO for the promotions endpoint
    private record ForgePromotions(
        [property: JsonPropertyName("promos")] IReadOnlyDictionary<string, string> Promos
    );
}