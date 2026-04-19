using MinecraftDownloader.Core.Abstractions;
using MinecraftDownloader.Core.Models;
using MinecraftDownloader.Core.Orchestration;
using Moq;

namespace MinecraftDownloader.Test.Orchestration;

public class MinecraftInstallerTests
{
    private readonly Mock<IDownloadPipeline> _pipelineMock;
    private readonly Mock<IDownloadExecutor> _executorMock;
    private readonly MinecraftInstaller _installer;

    public MinecraftInstallerTests()
    {
        _pipelineMock = new Mock<IDownloadPipeline>();
        _executorMock = new Mock<IDownloadExecutor>();
        _installer = new MinecraftInstaller(_pipelineMock.Object, _executorMock.Object);
    }

    [Fact]
    public async Task InstallAsync_SuccessFlow_ReturnsSuccessfulResultWithCorrectTasks()
    {
        var request = new InstallRequest(
            GameVersion: "1.21.1",
            GameDirectory: @"C:\minecraft",
            Loader: LoaderKind.Vanilla,
            LoaderVersion: null,
            MaxParallelism: 8,
            SkipIfExists: true);

        var tasks = new List<DownloadTask>
        {
            new(
                Url: "https://example.com/client.jar",
                DestinationPath: @"C:\minecraft\versions\1.21.1\client.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar),
            new(
                Url: "https://example.com/library.jar",
                DestinationPath: @"C:\minecraft\libraries\lib.jar",
                Sha1: "def456",
                Size: 512,
                Kind: DownloadTaskKind.Library),
            new(
                Url: "https://example.com/asset.png",
                DestinationPath: @"C:\minecraft\assets\asset.png",
                Sha1: "ghi789",
                Size: 256,
                Kind: DownloadTaskKind.Asset),
        };

        _pipelineMock
            .Setup(p => p.BuildTasksAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        _executorMock
            .Setup(e => e.ExecuteAsync(
                tasks,
                request.MaxParallelism,
                request.SkipIfExists,
                It.IsAny<IProgress<DownloadProgress>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _installer.InstallAsync(request);

        Assert.True(result.Succeeded);
        Assert.Equal(request, result.Request);
        Assert.Equal(tasks, result.Tasks);
        Assert.Null(result.Error);
        Assert.NotEqual(default, result.StartedAt);
        Assert.NotEqual(default, result.CompletedAt);
        Assert.True(result.CompletedAt >= result.StartedAt);

        _pipelineMock.Verify(p => p.BuildTasksAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _executorMock.Verify(e => e.ExecuteAsync(
            tasks,
            request.MaxParallelism,
            request.SkipIfExists,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InstallAsync_PipelineFailure_ReturnsFailedResultWithErrorAndEmptyTasks()
    {
        var request = new InstallRequest(
            GameVersion: "1.21.1",
            GameDirectory: @"C:\minecraft",
            Loader: LoaderKind.Vanilla,
            LoaderVersion: null,
            MaxParallelism: 8,
            SkipIfExists: true);

        var expectedException = new InvalidOperationException("Failed to build tasks");

        _pipelineMock
            .Setup(p => p.BuildTasksAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var result = await _installer.InstallAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(request, result.Request);
        Assert.Empty(result.Tasks);
        Assert.Equal(expectedException, result.Error);
        Assert.NotEqual(default, result.StartedAt);
        Assert.NotEqual(default, result.CompletedAt);
        Assert.True(result.CompletedAt >= result.StartedAt);

        _pipelineMock.Verify(p => p.BuildTasksAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _executorMock.Verify(e => e.ExecuteAsync(
            It.IsAny<IReadOnlyList<DownloadTask>>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<IProgress<DownloadProgress>?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InstallAsync_ExecutorFailure_ReturnsFailedResultWithErrorAndPartialTasks()
    {
        var request = new InstallRequest(
            GameVersion: "1.21.1",
            GameDirectory: @"C:\minecraft",
            Loader: LoaderKind.Vanilla,
            LoaderVersion: null,
            MaxParallelism: 8,
            SkipIfExists: true);

        var tasks = new List<DownloadTask>
        {
            new(
                Url: "https://example.com/client.jar",
                DestinationPath: @"C:\minecraft\versions\1.21.1\client.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar),
            new(
                Url: "https://example.com/library.jar",
                DestinationPath: @"C:\minecraft\libraries\lib.jar",
                Sha1: "def456",
                Size: 512,
                Kind: DownloadTaskKind.Library),
        };

        var expectedException = new HttpRequestException("Download failed");

        _pipelineMock
            .Setup(p => p.BuildTasksAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        _executorMock
            .Setup(e => e.ExecuteAsync(
                tasks,
                request.MaxParallelism,
                request.SkipIfExists,
                It.IsAny<IProgress<DownloadProgress>?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var result = await _installer.InstallAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(request, result.Request);
        Assert.Equal(tasks, result.Tasks);
        Assert.Equal(expectedException, result.Error);
        Assert.NotEqual(default, result.StartedAt);
        Assert.NotEqual(default, result.CompletedAt);
        Assert.True(result.CompletedAt >= result.StartedAt);

        _pipelineMock.Verify(p => p.BuildTasksAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _executorMock.Verify(e => e.ExecuteAsync(
            tasks,
            request.MaxParallelism,
            request.SkipIfExists,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InstallAsync_WithProgress_PassesProgressToExecutor()
    {
        var request = new InstallRequest(
            GameVersion: "1.21.1",
            GameDirectory: @"C:\minecraft",
            Loader: LoaderKind.Vanilla,
            LoaderVersion: null,
            MaxParallelism: 8,
            SkipIfExists: true);

        var tasks = new List<DownloadTask>
        {
            new(
                Url: "https://example.com/client.jar",
                DestinationPath: @"C:\minecraft\versions\1.21.1\client.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar),
        };

        var progressReports = new List<DownloadProgress>();
        var progress = new Progress<DownloadProgress>(p => progressReports.Add(p));

        _pipelineMock
            .Setup(p => p.BuildTasksAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        _executorMock
            .Setup(e => e.ExecuteAsync(
                tasks,
                request.MaxParallelism,
                request.SkipIfExists,
                It.IsAny<IProgress<DownloadProgress>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _installer.InstallAsync(request, progress);

        Assert.True(result.Succeeded);

        _executorMock.Verify(e => e.ExecuteAsync(
            tasks,
            request.MaxParallelism,
            request.SkipIfExists,
            progress,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InstallAsync_WithCancellationToken_PassesTokenToPipelineAndExecutor()
    {
        var request = new InstallRequest(
            GameVersion: "1.21.1",
            GameDirectory: @"C:\minecraft",
            Loader: LoaderKind.Vanilla,
            LoaderVersion: null,
            MaxParallelism: 8,
            SkipIfExists: true);

        var tasks = new List<DownloadTask>();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _pipelineMock
            .Setup(p => p.BuildTasksAsync(request, token))
            .ReturnsAsync(tasks);

        _executorMock
            .Setup(e => e.ExecuteAsync(
                tasks,
                request.MaxParallelism,
                request.SkipIfExists,
                It.IsAny<IProgress<DownloadProgress>?>(),
                token))
            .Returns(Task.CompletedTask);

        var result = await _installer.InstallAsync(request, null, token);

        Assert.True(result.Succeeded);

        _pipelineMock.Verify(p => p.BuildTasksAsync(request, token), Times.Once);
        _executorMock.Verify(e => e.ExecuteAsync(
            tasks,
            request.MaxParallelism,
            request.SkipIfExists,
            null,
            token), Times.Once);
    }

    [Fact]
    public async Task InstallAsync_PipelineCancellation_ReturnsFailedResultWithCancellationError()
    {
        var request = new InstallRequest(
            GameVersion: "1.21.1",
            GameDirectory: @"C:\minecraft",
            Loader: LoaderKind.Vanilla,
            LoaderVersion: null,
            MaxParallelism: 8,
            SkipIfExists: true);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var expectedException = new OperationCanceledException();

        _pipelineMock
            .Setup(p => p.BuildTasksAsync(request, cts.Token))
            .ThrowsAsync(expectedException);

        var result = await _installer.InstallAsync(request, null, cts.Token);

        Assert.False(result.Succeeded);
        Assert.Equal(request, result.Request);
        Assert.Empty(result.Tasks);
        Assert.Equal(expectedException, result.Error);
    }

    [Fact]
    public async Task InstallAsync_ExecutorCancellation_ReturnsFailedResultWithCancellationError()
    {
        var request = new InstallRequest(
            GameVersion: "1.21.1",
            GameDirectory: @"C:\minecraft",
            Loader: LoaderKind.Vanilla,
            LoaderVersion: null,
            MaxParallelism: 8,
            SkipIfExists: true);

        var tasks = new List<DownloadTask>
        {
            new(
                Url: "https://example.com/client.jar",
                DestinationPath: @"C:\minecraft\versions\1.21.1\client.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar),
        };

        using var cts = new CancellationTokenSource();

        var expectedException = new OperationCanceledException();

        _pipelineMock
            .Setup(p => p.BuildTasksAsync(request, cts.Token))
            .ReturnsAsync(tasks);

        _executorMock
            .Setup(e => e.ExecuteAsync(
                tasks,
                request.MaxParallelism,
                request.SkipIfExists,
                It.IsAny<IProgress<DownloadProgress>?>(),
                cts.Token))
            .ThrowsAsync(expectedException);

        var result = await _installer.InstallAsync(request, null, cts.Token);

        Assert.False(result.Succeeded);
        Assert.Equal(request, result.Request);
        Assert.Equal(tasks, result.Tasks);
        Assert.Equal(expectedException, result.Error);
    }
}
