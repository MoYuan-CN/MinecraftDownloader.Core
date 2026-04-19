namespace MinecraftDownloader.Core.Abstractions;

/// <summary>
///   Abstracts all file-system operations so tests can use an in-memory implementation.
/// </summary>
public interface IStorageProvider
{
    bool FileExists(string path);
    long GetFileSize(string path);
    void EnsureDirectoryExists(string path);
    Stream OpenRead(string path);
    Stream OpenWrite(string path);
    void DeleteFile(string path);

    // ── Canonical path helpers ────────────────────────────────────────────────
    string GetLibraryPath(string gameDir, string mavenPath);
    string GetAssetObjectPath(string gameDir, string hash);
    string GetAssetIndexPath(string gameDir, string indexId);
    string GetClientJarPath(string gameDir, string versionId);
    string GetLogConfigPath(string gameDir, string fileName);
    string GetVersionJsonPath(string gameDir, string versionId);
}