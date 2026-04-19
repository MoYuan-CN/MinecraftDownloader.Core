namespace MinecraftDownloader.Core.Models;

public record InstallRequest(
    /// <summary>Vanilla game version, e.g. "1.21.1"</summary>
    string GameVersion,
    /// <summary>Absolute path to the .minecraft root directory.</summary>
    string GameDirectory,
    LoaderKind Loader = LoaderKind.Vanilla,
    /// <summary>Loader version string.  null = latest stable.</summary>
    string? LoaderVersion = null,
    /// <summary>Max concurrent HTTP downloads.</summary>
    int MaxParallelism = 8,
    bool SkipIfExists = true
);