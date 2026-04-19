using System.Net;
using MinecraftDownloader.Core.Abstractions;
using MinecraftDownloader.Core.Download;
using MinecraftDownloader.Core.Models;
using Moq;
using Moq.Protected;
using Xunit;

namespace MinecraftDownloader.Test.Download;

public class ParallelDownloadExecutorTests
{
    private readonly Mock<IFileVerifier> _verifierMock;
    private readonly Mock<IStorageProvider> _storageMock;

    public ParallelDownloadExecutorTests()
    {
        _verifierMock = new Mock<IFileVerifier>();
        _storageMock = new Mock<IStorageProvider>();
    }

    private ParallelDownloadExecutor CreateExecutor(HttpClient? http = null)
    {
        return new ParallelDownloadExecutor(
            http ?? new HttpClient(),
            _verifierMock.Object,
            _storageMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyTaskList_ShouldCompleteImmediatelyWithoutError()
    {
        var executor = CreateExecutor();
        var emptyTasks = new List<DownloadTask>();

        await executor.ExecuteAsync(emptyTasks, maxParallelism: 4, skipIfExists: false);

        _verifierMock.Verify(v => v.VerifyAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _storageMock.Verify(s => s.FileExists(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithProgressReporting_ShouldReportProgressForEachCompletedTask()
    {
        var progressReports = new List<DownloadProgress>();
        var progressMock = new Mock<IProgress<DownloadProgress>>();
        progressMock
            .Setup(p => p.Report(It.IsAny<DownloadProgress>()))
            .Callback<DownloadProgress>(progressReports.Add);

        _verifierMock
            .Setup(v => v.VerifyAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _storageMock
            .Setup(s => s.FileExists(It.IsAny<string>()))
            .Returns(true);

        var executor = CreateExecutor();
        var tasks = new List<DownloadTask>
        {
            new("http://example.com/file1.jar", "/path/file1.jar", "sha1", 100, DownloadTaskKind.Library),
            new("http://example.com/file2.jar", "/path/file2.jar", "sha2", 200, DownloadTaskKind.Asset)
        };

        await executor.ExecuteAsync(tasks, maxParallelism: 2, skipIfExists: true, progress: progressMock.Object);

        Assert.Equal(2, progressReports.Count);
        Assert.Contains(progressReports, p => p.CurrentFile == "file1.jar" && p.Kind == DownloadTaskKind.Library);
        Assert.Contains(progressReports, p => p.CurrentFile == "file2.jar" && p.Kind == DownloadTaskKind.Asset);
        Assert.All(progressReports, p => Assert.Equal(2, p.Total));
        Assert.Equal(1, progressReports[0].Completed);
        Assert.Equal(2, progressReports[1].Completed);
    }

    [Fact]
    public async Task ExecuteAsync_WithSkipIfExistsAndValidFile_ShouldSkipDownload()
    {
        var progressReports = new List<DownloadProgress>();
        var progressMock = new Mock<IProgress<DownloadProgress>>();
        progressMock
            .Setup(p => p.Report(It.IsAny<DownloadProgress>()))
            .Callback<DownloadProgress>(progressReports.Add);

        var httpHandlerMock = new Mock<HttpMessageHandler>();
        httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)))
            .Verifiable();

        var httpClient = new HttpClient(httpHandlerMock.Object);

        _verifierMock
            .Setup(v => v.VerifyAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _storageMock
            .Setup(s => s.FileExists(It.IsAny<string>()))
            .Returns(true);

        var executor = CreateExecutor(httpClient);
        var tasks = new List<DownloadTask>
        {
            new("http://example.com/file.jar", "/path/file.jar", "sha1", 100, DownloadTaskKind.Library)
        };

        await executor.ExecuteAsync(tasks, maxParallelism: 1, skipIfExists: true, progress: progressMock.Object);

        _verifierMock.Verify(v => v.VerifyAsync(
            "/path/file.jar",
            "sha1",
            100,
            It.IsAny<CancellationToken>()), Times.Once);

        httpHandlerMock
            .Protected()
            .Verify("SendAsync", Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        Assert.Single(progressReports);
        Assert.Equal(1, progressReports[0].Completed);
        Assert.Equal(1, progressReports[0].Total);
        Assert.Equal("file.jar", progressReports[0].CurrentFile);
    }

    [Fact]
    public async Task ExecuteAsync_WithSkipIfExistsAndMissingSha1_ShouldOnlyCheckFileExists()
    {
        var progressReports = new List<DownloadProgress>();
        var progressMock = new Mock<IProgress<DownloadProgress>>();
        progressMock
            .Setup(p => p.Report(It.IsAny<DownloadProgress>()))
            .Callback<DownloadProgress>(progressReports.Add);

        var httpClient = new HttpClient();

        _storageMock
            .Setup(s => s.FileExists(It.IsAny<string>()))
            .Returns(true);

        var executor = CreateExecutor(httpClient);
        var tasks = new List<DownloadTask>
        {
            new("http://example.com/file.jar", "/path/file.jar", "", -1, DownloadTaskKind.Library)
        };

        await executor.ExecuteAsync(tasks, maxParallelism: 1, skipIfExists: true, progress: progressMock.Object);

        _storageMock.Verify(s => s.FileExists("/path/file.jar"), Times.Once);

        _verifierMock.Verify(v => v.VerifyAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);

        Assert.Single(progressReports);
        Assert.Equal(1, progressReports[0].Completed);
    }

    [Fact]
    public async Task ExecuteAsync_WithSkipIfExistsAndInvalidFile_ShouldAttemptDownload()
    {
        var httpHandlerMock = new Mock<HttpMessageHandler>();
        httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
            }));

        var httpClient = new HttpClient(httpHandlerMock.Object);

        _verifierMock
            .Setup(v => v.VerifyAsync(
                It.Is<string>(path => !path.EndsWith(".tmp")),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _storageMock
            .Setup(s => s.OpenWrite(It.IsAny<string>()))
            .Returns(() => new MemoryStream());

        _storageMock
            .Setup(s => s.FileExists(It.IsAny<string>()))
            .Returns(false);

        var executor = CreateExecutor(httpClient);
        var tasks = new List<DownloadTask>
        {
            new("http://example.com/file.jar", "/path/file.jar", "sha1", 3, DownloadTaskKind.Library)
        };

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            executor.ExecuteAsync(tasks, maxParallelism: 1, skipIfExists: true));

        Assert.Contains("Hash/size mismatch", exception.Message);
        Assert.Contains("http://example.com/file.jar", exception.Message);

        _verifierMock.Verify(v => v.VerifyAsync(
            "/path/file.jar",
            "sha1",
            3,
            It.IsAny<CancellationToken>()), Times.Once);

        httpHandlerMock
            .Protected()
            .Verify("SendAsync", Times.AtLeastOnce(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var executor = CreateExecutor();
        var tasks = new List<DownloadTask>
        {
            new("http://example.com/file.jar", "/path/file.jar", "sha1", 100, DownloadTaskKind.Library)
        };

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            executor.ExecuteAsync(tasks, maxParallelism: 1, skipIfExists: false, ct: cts.Token));
    }
}
