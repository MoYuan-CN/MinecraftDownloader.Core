namespace MinecraftDownloader.Core.Abstractions;

/// <summary>Verifies a downloaded file against its expected SHA-1 hash and byte size.</summary>
public interface IFileVerifier
{
    /// <returns><c>true</c> if the file passes all checks.</returns>
    Task<bool> VerifyAsync(
        string filePath,
        string expectedSha1,
        long expectedSize,
        CancellationToken ct = default);
}