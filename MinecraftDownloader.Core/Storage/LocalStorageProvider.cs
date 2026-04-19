using MinecraftDownloader.Core.Abstractions;

namespace MinecraftDownloader.Core.Storage;

/// <summary>Default implementation backed by <see cref="System.IO"/>.</summary>
public sealed class LocalStorageProvider : IStorageProvider
{
    public bool FileExists(string path)  => File.Exists(path);
    public long GetFileSize(string path) => new FileInfo(path).Length;

    public void EnsureDirectoryExists(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }

    public Stream OpenRead(string path)   => File.OpenRead(path);
    public Stream OpenWrite(string path)  => File.OpenWrite(path);
    public void   DeleteFile(string path) => File.Delete(path);

    // ── Canonical path helpers ────────────────────────────────────────────────

    /// <param name="mavenPath">
    ///   Maven artifact path, e.g.
    ///   <c>net/minecraft/client/1.21.1/client-1.21.1.jar</c>
    /// </param>
    public string GetLibraryPath(string gameDir, string mavenPath) =>
        Path.Combine(gameDir, "libraries", NormaliseSeparators(mavenPath));

    /// <summary>
    ///   Assets are stored in a two-level hash-prefix layout:
    ///   <c>assets/objects/&lt;hash[..2]&gt;/&lt;hash&gt;</c>
    /// </summary>
    public string GetAssetObjectPath(string gameDir, string hash) =>
        Path.Combine(gameDir, "assets", "objects", hash[..2], hash);

    public string GetAssetIndexPath(string gameDir, string indexId) =>
        Path.Combine(gameDir, "assets", "indexes", $"{indexId}.json");

    public string GetClientJarPath(string gameDir, string versionId) =>
        Path.Combine(gameDir, "versions", versionId, $"{versionId}.jar");

    public string GetLogConfigPath(string gameDir, string fileName) =>
        Path.Combine(gameDir, "assets", "log_configs", fileName);

    public string GetVersionJsonPath(string gameDir, string versionId) =>
        Path.Combine(gameDir, "versions", versionId, $"{versionId}.json");

    // ── Private ───────────────────────────────────────────────────────────────

    private static string NormaliseSeparators(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);

    // ── Maven coordinate helper ───────────────────────────────────────────────

    /// <summary>
    ///   Converts a Maven coordinate string such as
    ///   <c>com.example:artifact:1.0.0</c> into the canonical path
    ///   <c>com/example/artifact/1.0.0/artifact-1.0.0.jar</c>.
    /// </summary>
    public static string MavenCoordinateToPath(string coordinate)
    {
        // Format:  groupId:artifactId:version[:classifier][@ext]
        var atIdx = coordinate.IndexOf('@');
        var ext   = atIdx >= 0 ? coordinate[(atIdx + 1)..] : "jar";
        var core  = atIdx >= 0 ? coordinate[..atIdx] : coordinate;

        var parts = core.Split(':');
        if (parts.Length < 3)
            throw new ArgumentException(
                $"Invalid Maven coordinate: '{coordinate}'.", nameof(coordinate));

        var groupPath  = parts[0].Replace('.', '/');
        var artifactId = parts[1];
        var version    = parts[2];
        var classifier = parts.Length >= 4 ? $"-{parts[3]}" : string.Empty;

        return $"{groupPath}/{artifactId}/{version}/{artifactId}-{version}{classifier}.{ext}";
    }
}