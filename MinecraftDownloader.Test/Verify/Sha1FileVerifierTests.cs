using System.Text;
using MinecraftDownloader.Core.Abstractions;
using MinecraftDownloader.Core.Verify;
using Moq;
using Xunit;

namespace MinecraftDownloader.Test.Verify;

public class Sha1FileVerifierTests
{
    private readonly Mock<IStorageProvider> _storageMock;
    private readonly Sha1FileVerifier _verifier;

    public Sha1FileVerifierTests()
    {
        _storageMock = new Mock<IStorageProvider>(MockBehavior.Strict);
        _verifier = new Sha1FileVerifier(_storageMock.Object);
    }

    [Fact]
    public async Task VerifyAsync_WhenFileDoesNotExist_ReturnsFalse()
    {
        const string filePath = "/path/to/file.jar";
        const string expectedSha1 = "abc123";
        const long expectedSize = 1024;

        _storageMock
            .Setup(s => s.FileExists(filePath))
            .Returns(false);

        var result = await _verifier.VerifyAsync(filePath, expectedSha1, expectedSize);

        Assert.False(result);
        _storageMock.Verify(s => s.FileExists(filePath), Times.Once);
        _storageMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task VerifyAsync_WhenSizeMismatches_ReturnsFalseWithoutComputingHash()
    {
        const string filePath = "/path/to/file.jar";
        const string expectedSha1 = "abc123";
        const long expectedSize = 1024;
        const long actualSize = 512;

        _storageMock
            .Setup(s => s.FileExists(filePath))
            .Returns(true);
        _storageMock
            .Setup(s => s.GetFileSize(filePath))
            .Returns(actualSize);

        var result = await _verifier.VerifyAsync(filePath, expectedSha1, expectedSize);

        Assert.False(result);
        _storageMock.Verify(s => s.FileExists(filePath), Times.Once);
        _storageMock.Verify(s => s.GetFileSize(filePath), Times.Once);
        _storageMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task VerifyAsync_WhenHashMismatches_ReturnsFalse()
    {
        const string filePath = "/path/to/file.jar";
        const string expectedSha1 = "abc123def456";
        const long expectedSize = 4;
        var fileContent = Encoding.UTF8.GetBytes("test");
        using var memoryStream = new MemoryStream(fileContent);

        _storageMock
            .Setup(s => s.FileExists(filePath))
            .Returns(true);
        _storageMock
            .Setup(s => s.GetFileSize(filePath))
            .Returns(expectedSize);
        _storageMock
            .Setup(s => s.OpenRead(filePath))
            .Returns(memoryStream);

        var result = await _verifier.VerifyAsync(filePath, expectedSha1, expectedSize);

        Assert.False(result);
        _storageMock.Verify(s => s.FileExists(filePath), Times.Once);
        _storageMock.Verify(s => s.GetFileSize(filePath), Times.Once);
        _storageMock.Verify(s => s.OpenRead(filePath), Times.Once);
    }

    [Fact]
    public async Task VerifyAsync_WhenVerificationSucceeds_ReturnsTrue()
    {
        const string filePath = "/path/to/file.jar";
        const long expectedSize = 4;
        var fileContent = Encoding.UTF8.GetBytes("test");
        using var memoryStream = new MemoryStream(fileContent);
        var expectedSha1 = ComputeSha1(fileContent);

        _storageMock
            .Setup(s => s.FileExists(filePath))
            .Returns(true);
        _storageMock
            .Setup(s => s.GetFileSize(filePath))
            .Returns(expectedSize);
        _storageMock
            .Setup(s => s.OpenRead(filePath))
            .Returns(memoryStream);

        var result = await _verifier.VerifyAsync(filePath, expectedSha1, expectedSize);

        Assert.True(result);
        _storageMock.Verify(s => s.FileExists(filePath), Times.Once);
        _storageMock.Verify(s => s.GetFileSize(filePath), Times.Once);
        _storageMock.Verify(s => s.OpenRead(filePath), Times.Once);
    }

    [Fact]
    public async Task VerifyAsync_WhenHashMatchesWithDifferentCase_ReturnsTrue()
    {
        const string filePath = "/path/to/file.jar";
        const long expectedSize = 4;
        var fileContent = Encoding.UTF8.GetBytes("test");
        using var memoryStream = new MemoryStream(fileContent);
        var expectedSha1 = ComputeSha1(fileContent).ToUpperInvariant();

        _storageMock
            .Setup(s => s.FileExists(filePath))
            .Returns(true);
        _storageMock
            .Setup(s => s.GetFileSize(filePath))
            .Returns(expectedSize);
        _storageMock
            .Setup(s => s.OpenRead(filePath))
            .Returns(memoryStream);

        var result = await _verifier.VerifyAsync(filePath, expectedSha1, expectedSize);

        Assert.True(result);
    }

    [Fact]
    public async Task VerifyAsync_WhenCancellationTokenIsCancelled_ThrowsTaskCanceledException()
    {
        const string filePath = "/path/to/file.jar";
        const string expectedSha1 = "abc123";
        const long expectedSize = 1024;
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _storageMock
            .Setup(s => s.FileExists(filePath))
            .Returns(true);
        _storageMock
            .Setup(s => s.GetFileSize(filePath))
            .Returns(expectedSize);
        _storageMock
            .Setup(s => s.OpenRead(filePath))
            .Returns(new MemoryStream(new byte[1024]));

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _verifier.VerifyAsync(filePath, expectedSha1, expectedSize, cts.Token));
    }

    private static string ComputeSha1(byte[] data)
    {
        var hash = System.Security.Cryptography.SHA1.HashData(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
