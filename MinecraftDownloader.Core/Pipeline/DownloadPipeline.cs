using MinecraftDownloader.Core.Abstractions;
using MinecraftDownloader.Core.Models;
using MinecraftDownloader.Core.Storage;

namespace MinecraftDownloader.Core.Pipeline;

/// <summary>
///   Resolves all manifest data and builds the complete list of
///   <see cref="DownloadTask"/> objects for a given <see cref="InstallRequest"/>.
/// </summary>
public sealed class DownloadPipeline : IDownloadPipeline
{
    private readonly IManifestParser _mojang;
    private readonly IForgeMetaResolver _forge;
    private readonly IFabricMetaResolver _fabric;
    private readonly IFabricMetaResolver _quilt;
    private readonly IStorageProvider _storage;

    public DownloadPipeline(
        IManifestParser mojang,
        IForgeMetaResolver forge,
        IFabricMetaResolver fabric,
        IFabricMetaResolver quilt,
        IStorageProvider storage)
    {
        _mojang = mojang;
        _forge = forge;
        _fabric = fabric;
        _quilt = quilt;
        _storage = storage;
    }

    public async Task<IReadOnlyList<DownloadTask>> BuildTasksAsync(
        InstallRequest request,
        CancellationToken ct = default)
    {
        var tasks = new List<DownloadTask>(256);

        // 1. Always resolve vanilla meta first
        var metaResult = await _mojang.GetVersionMetaAsync(request.GameVersion, ct);
        var vanillaMeta = metaResult.Meta;

        // 2. Version JSON
        tasks.Add(new DownloadTask(
            metaResult.Url,
            _storage.GetVersionJsonPath(request.GameDirectory, vanillaMeta.Id),
            metaResult.Sha1,
            -1,
            DownloadTaskKind.VersionJson));

        // 3. Client JAR
        tasks.Add(new DownloadTask(
            vanillaMeta.Downloads.Client.Url,
            _storage.GetClientJarPath(request.GameDirectory, vanillaMeta.Id),
            vanillaMeta.Downloads.Client.Sha1,
            vanillaMeta.Downloads.Client.Size,
            DownloadTaskKind.ClientJar));

        // 4. Vanilla libraries
        AddLibraryTasks(tasks, vanillaMeta.Libraries, request.GameDirectory, DownloadTaskKind.Library);

        // 5. Asset index + assets
        await AddAssetTasksAsync(tasks, vanillaMeta, request.GameDirectory, ct);

        // 6. Logging config
        if (vanillaMeta.Logging?.Client is { } log)
            tasks.Add(new DownloadTask(
                log.File.Url,
                _storage.GetLogConfigPath(request.GameDirectory, Path.GetFileName(log.File.Url)),
                log.File.Sha1,
                log.File.Size,
                DownloadTaskKind.LogConfig));

        // 7. Loader-specific tasks
        switch (request.Loader)
        {
            case LoaderKind.Forge:
                await AddForgeTasksAsync(tasks, request, ct);
                break;

            case LoaderKind.Fabric:
                await AddFabricTasksAsync(tasks, request, _fabric, DownloadTaskKind.FabricLibrary, ct);
                break;

            case LoaderKind.Quilt:
                await AddFabricTasksAsync(tasks, request, _quilt, DownloadTaskKind.QuiltLibrary, ct);
                break;

            case LoaderKind.Vanilla:
            default:
                break;
        }

        // 8. Deduplicate by destination path (keep first occurrence)
        return tasks
            .GroupBy(t => t.DestinationPath, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }

    // ── Vanilla assets ────────────────────────────────────────────────────────

    private async Task AddAssetTasksAsync(
        List<DownloadTask> tasks,
        VersionMeta meta,
        string gameDir,
        CancellationToken ct)
    {
        var indexRef = meta.AssetIndex;
        var indexPath = _storage.GetAssetIndexPath(gameDir, indexRef.Id);

        // Asset index JSON itself
        tasks.Add(new DownloadTask(
            indexRef.Url,
            indexPath,
            indexRef.Sha1,
            indexRef.Size,
            DownloadTaskKind.AssetIndex));

        // Individual asset objects
        var assetIndex = await _mojang.GetAssetIndexAsync(indexRef, ct);

        foreach (var (_, obj) in assetIndex.Objects)
        {
            var dest = _storage.GetAssetObjectPath(gameDir, obj.Hash);
            var url = $"https://resources.download.minecraft.net/{obj.Hash[..2]}/{obj.Hash}";

            tasks.Add(new DownloadTask(url, dest, obj.Hash, obj.Size, DownloadTaskKind.Asset));
        }
    }

    // ── Library helpers ───────────────────────────────────────────────────────

    private void AddLibraryTasks(
        List<DownloadTask> tasks,
        IReadOnlyList<Library> libraries,
        string gameDir,
        DownloadTaskKind kind)
    {
        foreach (var lib in libraries)
        {
            if (!PlatformRuleEvaluator.IsAllowed(lib.Rules))
                continue;

            // Mojang-style: downloads.artifact is present
            if (lib.Downloads?.Artifact is { } artifact)
            {
                tasks.Add(new DownloadTask(
                    artifact.Url,
                    _storage.GetLibraryPath(gameDir, artifact.Path),
                    artifact.Sha1,
                    artifact.Size,
                    kind));
                continue;
            }

            // Fabric/Quilt-style: only "name" + optional "url" are present
            if (lib.Url is not null || kind is DownloadTaskKind.FabricLibrary or DownloadTaskKind.QuiltLibrary)
            {
                TryAddMavenLibrary(tasks, lib, gameDir, kind);
            }
        }
    }

    private void TryAddMavenLibrary(
        List<DownloadTask> tasks,
        Library lib,
        string gameDir,
        DownloadTaskKind kind)
    {
        try
        {
            var path = LocalStorageProvider.MavenCoordinateToPath(lib.Name);
            var repoBase = (lib.Url ?? "https://libraries.minecraft.net/").TrimEnd('/');
            var url = $"{repoBase}/{path}";
            var dest = _storage.GetLibraryPath(gameDir, path);

            // Size / sha1 unknown from this format — use empty sentinel; verifier will skip hash check
            tasks.Add(new DownloadTask(url, dest, string.Empty, -1, kind));
        }
        catch (ArgumentException)
        {
            // Malformed coordinate — skip silently
        }
    }

    // ── Forge ─────────────────────────────────────────────────────────────────

    private async Task AddForgeTasksAsync(
        List<DownloadTask> tasks,
        InstallRequest request,
        CancellationToken ct)
    {
        var (profile, versionJson) = await _forge.ResolveAsync(
            request.GameVersion, request.LoaderVersion, ct);

        // Forge install-profile libraries (e.g. MCP, SRG mappings, ForgeGradle deps)
        AddLibraryTasks(tasks, profile.Libraries, request.GameDirectory, DownloadTaskKind.Library);

        // Forge version.json libraries (the actual mod loader classes)
        AddLibraryTasks(tasks, versionJson.Libraries, request.GameDirectory, DownloadTaskKind.Library);

        // Forge universal JAR (if referenced by path)
        if (profile.Path is not null)
        {
            var mavenPath = LocalStorageProvider.MavenCoordinateToPath(profile.Path);
            var repoBase = "https://maven.minecraftforge.net/";
            var url = $"{repoBase}{mavenPath}";
            var dest = _storage.GetLibraryPath(request.GameDirectory, mavenPath);

            tasks.Add(new DownloadTask(url, dest, string.Empty, -1, DownloadTaskKind.ForgeUniversal));
        }
    }

    // ── Fabric / Quilt ────────────────────────────────────────────────────────

    private async Task AddFabricTasksAsync(
        List<DownloadTask> tasks,
        InstallRequest request,
        IFabricMetaResolver resolver,
        DownloadTaskKind kind,
        CancellationToken ct)
    {
        var profile = await resolver.ResolveAsync(
            request.GameVersion, request.LoaderVersion, ct);

        AddLibraryTasks(tasks, profile.Libraries, request.GameDirectory, kind);
    }
}