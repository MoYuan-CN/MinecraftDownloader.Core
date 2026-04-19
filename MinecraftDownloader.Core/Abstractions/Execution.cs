using MinecraftDownloader.Core.Models;

namespace MinecraftDownloader.Core.Abstractions;

/// <summary>Executes a list of download tasks in parallel, bounded by a semaphore.</summary>
public interface IDownloadExecutor
{
    Task ExecuteAsync(
        IReadOnlyList<DownloadTask> tasks,
        int maxParallelism,
        bool skipIfExists,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken ct = default);
}