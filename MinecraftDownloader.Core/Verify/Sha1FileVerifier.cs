using System.Security.Cryptography;
using MinecraftDownloader.Core.Abstractions;

namespace MinecraftDownloader.Core.Verify;

public sealed class Sha1FileVerifier : IFileVerifier
{
    private readonly IStorageProvider _storage;

    public Sha1FileVerifier(IStorageProvider storage) => _storage = storage;

    public async Task<bool> VerifyAsync(
        string filePath,
        string expectedSha1,
        long expectedSize,
        CancellationToken ct = default)
    {
        if (!_storage.FileExists(filePath))
            return false;

        // Fast size check first – avoids hashing obviously wrong files
        if (_storage.GetFileSize(filePath) != expectedSize)
            return false;

        var actualHash = await ComputeSha1Async(filePath, ct);
        return string.Equals(actualHash, expectedSha1, StringComparison.OrdinalIgnoreCase);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> ComputeSha1Async(string filePath, CancellationToken ct)
    {
        await using var stream = _storage.OpenRead(filePath);
        var hash = await SHA1.HashDataAsync(stream, ct);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}