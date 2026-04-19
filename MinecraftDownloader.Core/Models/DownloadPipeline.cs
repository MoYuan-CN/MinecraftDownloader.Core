namespace MinecraftDownloader.Core.Models;

public record DownloadTask(
    string Url,
    string DestinationPath,
    string Sha1,
    long Size,
    DownloadTaskKind Kind
);

public enum DownloadTaskKind
{
    VersionJson,
    ClientJar,
    Library,
    Asset,
    AssetIndex,
    LogConfig,
    ForgeUniversal,
    ForgeInstaller,
    FabricLibrary,
    QuiltLibrary,
}

public record DownloadProgress(
    int Completed,
    int Total,
    string CurrentFile,
    DownloadTaskKind Kind
);