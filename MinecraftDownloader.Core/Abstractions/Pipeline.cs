using MinecraftDownloader.Core.Models;

namespace MinecraftDownloader.Core.Abstractions;

/// <summary>Builds a flat, ordered list of <see cref="DownloadTask"/> for a given install request.</summary>
public interface IDownloadPipeline
{
    Task<IReadOnlyList<DownloadTask>> BuildTasksAsync(
        InstallRequest request,
        CancellationToken ct = default);
}