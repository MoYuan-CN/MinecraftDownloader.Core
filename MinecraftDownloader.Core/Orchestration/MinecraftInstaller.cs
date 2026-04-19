using MinecraftDownloader.Core.Abstractions;
using MinecraftDownloader.Core.Models;

namespace MinecraftDownloader.Core.Orchestration;

/// <summary>
///   Top-level public API.  Call <see cref="InstallAsync"/> to download and
///   verify a complete Minecraft installation (Vanilla, Forge, Fabric, or Quilt).
/// </summary>
public sealed class MinecraftInstaller(
    IDownloadPipeline pipeline,
    IDownloadExecutor executor)
{
    // ─────────────────────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///   Installs the requested Minecraft version (including loader libraries and
    ///   assets) into <see cref="InstallRequest.GameDirectory"/>.
    /// </summary>
    /// <param name="request">What to install and where.</param>
    /// <param name="progress">Optional progress callback; called after each file completes.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    ///   An <see cref="InstallResult"/> describing what was downloaded and whether
    ///   everything succeeded.
    /// </returns>
    public async Task<InstallResult> InstallAsync(
        InstallRequest request,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken ct = default)
    {
        var started = DateTimeOffset.UtcNow;

        // Step 1 — build the task list
        IReadOnlyList<DownloadTask> tasks;
        try
        {
            tasks = await pipeline.BuildTasksAsync(request, ct);
        }
        catch (Exception ex)
        {
            return InstallResult.Failure(request, [], ex, started);
        }

        // Step 2 — execute downloads
        try
        {
            await executor.ExecuteAsync(
                tasks,
                request.MaxParallelism,
                request.SkipIfExists,
                progress,
                ct);
        }
        catch (Exception ex)
        {
            return InstallResult.Failure(request, tasks, ex, started);
        }

        return InstallResult.Success(request, tasks, started);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Factory  (convenience — avoids forcing callers to wire DI manually)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///   Creates a fully-wired <see cref="MinecraftInstaller"/> using default
    ///   implementations.  For production use, prefer registering services via
    ///   your DI container and injecting <see cref="MinecraftInstaller"/> directly.
    /// </summary>
    public static MinecraftInstaller CreateDefault()
    {
        var http = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5),
            DefaultRequestHeaders = { { "User-Agent", "MinecraftDownloader.Core/1.0" } },
        };

        return InstallerFactory.Create(http);
    }
}