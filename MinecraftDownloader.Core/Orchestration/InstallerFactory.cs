using MinecraftDownloader.Core.Abstractions;
using MinecraftDownloader.Core.Download;
using MinecraftDownloader.Core.Manifest;
using MinecraftDownloader.Core.Models;
using MinecraftDownloader.Core.Pipeline;
using MinecraftDownloader.Core.Storage;
using MinecraftDownloader.Core.Verify;

namespace MinecraftDownloader.Core.Orchestration;

// ─────────────────────────────────────────────────────────────────────────────
//  Install result
// ─────────────────────────────────────────────────────────────────────────────

public sealed class InstallResult
{
    public bool Succeeded { get; private init; }
    public InstallRequest Request { get; private init; } = default!;
    public IReadOnlyList<DownloadTask> Tasks { get; private init; } = [];
    public Exception? Error { get; private init; }
    public DateTimeOffset StartedAt { get; private init; }
    public DateTimeOffset CompletedAt { get; private init; }
    public TimeSpan Elapsed => CompletedAt - StartedAt;

    public static InstallResult Success(
        InstallRequest request,
        IReadOnlyList<DownloadTask> tasks,
        DateTimeOffset started) => new()
    {
        Succeeded = true,
        Request = request,
        Tasks = tasks,
        StartedAt = started,
        CompletedAt = DateTimeOffset.UtcNow,
    };

    public static InstallResult Failure(
        InstallRequest request,
        IReadOnlyList<DownloadTask> tasks,
        Exception error,
        DateTimeOffset started) => new()
    {
        Succeeded = false,
        Request = request,
        Tasks = tasks,
        Error = error,
        StartedAt = started,
        CompletedAt = DateTimeOffset.UtcNow,
    };
}

// ─────────────────────────────────────────────────────────────────────────────
//  Factory  (pure DI wiring — no framework dependency)
// ─────────────────────────────────────────────────────────────────────────────

public static class InstallerFactory
{
    /// <summary>
    ///   Wires all default implementations together.
    ///   Pass a pre-configured <see cref="HttpClient"/> (e.g. one with a
    ///   custom timeout, retry policy, or DelegatingHandler for mirrors).
    /// </summary>
    public static MinecraftInstaller Create(HttpClient http)
    {
        IStorageProvider storage = new LocalStorageProvider();
        IFileVerifier verifier = new Sha1FileVerifier(storage);

        IManifestParser mojang = new MojangManifestParser(http);
        IForgeMetaResolver forge = new ForgeMetaResolver(http);
        IFabricMetaResolver fabric = new FabricMetaResolver(http);
        IFabricMetaResolver quilt = new QuiltMetaResolver(http);

        IDownloadPipeline pipeline = new DownloadPipeline(mojang, forge, fabric, quilt, storage);
        IDownloadExecutor executor = new ParallelDownloadExecutor(http, verifier, storage);

        return new MinecraftInstaller(pipeline, executor);
    }
}