using MinecraftDownloader.Core.Abstractions;
using MinecraftDownloader.Core.Models;

namespace MinecraftDownloader.Core.Download;

/// <summary>
///   Downloads a list of <see cref="DownloadTask"/> objects in parallel,
///   bounded by a <see cref="SemaphoreSlim"/> to limit concurrent HTTP connections.
/// </summary>
public sealed class ParallelDownloadExecutor(
    HttpClient http,
    IFileVerifier verifier,
    IStorageProvider storage)
    : IDownloadExecutor
{
    public async Task ExecuteAsync(
        IReadOnlyList<DownloadTask>  tasks,
        int                          maxParallelism,
        bool                         skipIfExists,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken            ct       = default)
    {
        if (tasks.Count == 0) return;

        var semaphore = new SemaphoreSlim(Math.Max(1, maxParallelism));
        // int[] acts as a heap-allocated counter that async lambdas can capture
        // and Interlocked can operate on via ref array[0].
        var counter   = new int[1];
        var total     = tasks.Count;

        var downloadTasks = tasks.Select(task =>
            DownloadOneAsync(task, semaphore, skipIfExists, progress, counter, total, ct));

        await Task.WhenAll(downloadTasks);
    }

    // ── Per-file logic ────────────────────────────────────────────────────────

    private async Task DownloadOneAsync(
        DownloadTask                 task,
        SemaphoreSlim                semaphore,
        bool                         skipIfExists,
        IProgress<DownloadProgress>? progress,
        int[]                        counter,
        int                          total,
        CancellationToken            ct)
    {
        await semaphore.WaitAsync(ct);
        try
        {
            if (skipIfExists && await IsAlreadyValidAsync(task, ct))
            {
                ReportProgress(progress, counter, total, task);
                return;
            }

            await DownloadWithRetryAsync(task, ct);
            ReportProgress(progress, counter, total, task);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<bool> IsAlreadyValidAsync(DownloadTask task, CancellationToken ct)
    {
        // Skip hash/size verification when the manifest didn't provide them
        if (task.Size < 0 || string.IsNullOrEmpty(task.Sha1))
            return storage.FileExists(task.DestinationPath);

        return await verifier.VerifyAsync(task.DestinationPath, task.Sha1, task.Size, ct);
    }

    private async Task DownloadWithRetryAsync(DownloadTask task, CancellationToken ct,
        int maxRetries = 3)
    {
        Exception? lastEx = null;

        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                await DownloadCoreAsync(task, ct);
                return;
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException && attempt < maxRetries - 1)
            {
                lastEx = ex;
                // Exponential back-off: 500 ms, 1 s, 2 s …
                var delay = TimeSpan.FromMilliseconds(500 * Math.Pow(2, attempt));
                await Task.Delay(delay, ct);
            }
        }

        throw new IOException(
            $"Failed to download '{task.Url}' after {maxRetries} attempts.", lastEx);
    }

    private async Task DownloadCoreAsync(DownloadTask task, CancellationToken ct)
    {
        storage.EnsureDirectoryExists(task.DestinationPath);

        using var response = await http.GetAsync(task.Url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        // Write to a temp file; rename on success to keep destination atomic
        var tmp = task.DestinationPath + ".tmp";
        try
        {
            await using (var src  = await response.Content.ReadAsStreamAsync(ct))
            await using (var dst  = storage.OpenWrite(tmp))
            {
                await src.CopyToAsync(dst, ct);
            }

            // Verify before committing
            if (task.Size >= 0 && !string.IsNullOrEmpty(task.Sha1))
            {
                var ok = await verifier.VerifyAsync(tmp, task.Sha1, task.Size, ct);
                if (!ok)
                    throw new InvalidDataException(
                        $"Hash/size mismatch for '{task.Url}' → '{task.DestinationPath}'.");
            }

            if (storage.FileExists(task.DestinationPath))
                storage.DeleteFile(task.DestinationPath);

            File.Move(tmp, task.DestinationPath);
        }
        catch
        {
            if (storage.FileExists(tmp))
                storage.DeleteFile(tmp);
            throw;
        }
    }

    // ── Progress ──────────────────────────────────────────────────────────────

    private static void ReportProgress(
        IProgress<DownloadProgress>? progress,
        int[]                        counter,
        int                          total,
        DownloadTask                 task)
    {
        var c = Interlocked.Increment(ref counter[0]);
        progress?.Report(new DownloadProgress(c, total, Path.GetFileName(task.DestinationPath), task.Kind));
    }
}