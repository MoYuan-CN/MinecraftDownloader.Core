using MinecraftDownloader.Core.Models;

namespace MinecraftDownloader.Core.Abstractions;

public record VersionMetaResult(
    VersionMeta Meta,
    string Url,
    string Sha1
);

/// <summary>Fetches and parses Mojang's version manifest JSON.</summary>
public interface IManifestParser
{
    Task<VersionManifestIndex> GetVersionManifestIndexAsync(CancellationToken ct = default);
    Task<VersionMetaResult> GetVersionMetaAsync(string versionId, CancellationToken ct = default);
    Task<AssetIndex> GetAssetIndexAsync(AssetIndexRef assetRef, CancellationToken ct = default);
}

/// <summary>Resolves Forge install-profile metadata from a local installer jar or remote URL.</summary>
public interface IForgeMetaResolver
{
    /// <param name="gameVersion">e.g. "1.21.1"</param>
    /// <param name="forgeVersion">e.g. "47.3.0" — null to use latest.</param>
    Task<(ForgeInstallProfile Profile, VersionMeta VersionJson)> ResolveAsync(
        string gameVersion,
        string? forgeVersion,
        CancellationToken ct = default);
}

/// <summary>Resolves Fabric or Quilt loader metadata from their respective meta APIs.</summary>
public interface IFabricMetaResolver
{
    Task<FabricProfileMeta> ResolveAsync(
        string gameVersion,
        string? loaderVersion,
        CancellationToken ct = default);
}