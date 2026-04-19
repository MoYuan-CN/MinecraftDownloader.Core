using MinecraftDownloader.Core.Models;

namespace MinecraftDownloader.Test.Models;

public class ModelTests
{
    public class DownloadTaskTests
    {
        [Fact]
        public void TwoInstances_WithSameProperties_ShouldBeEqual()
        {
            var task1 = new DownloadTask(
                Url: "https://example.com/file.jar",
                DestinationPath: "/path/to/file.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar
            );

            var task2 = new DownloadTask(
                Url: "https://example.com/file.jar",
                DestinationPath: "/path/to/file.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar
            );

            Assert.Equal(task1, task2);
            Assert.True(task1 == task2);
            Assert.False(task1 != task2);
            Assert.Equal(task1.GetHashCode(), task2.GetHashCode());
        }

        [Fact]
        public void TwoInstances_WithDifferentUrl_ShouldNotBeEqual()
        {
            var task1 = new DownloadTask(
                Url: "https://example.com/file1.jar",
                DestinationPath: "/path/to/file.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar
            );

            var task2 = new DownloadTask(
                Url: "https://example.com/file2.jar",
                DestinationPath: "/path/to/file.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar
            );

            Assert.NotEqual(task1, task2);
            Assert.False(task1 == task2);
            Assert.True(task1 != task2);
        }

        [Fact]
        public void TwoInstances_WithDifferentDestinationPath_ShouldNotBeEqual()
        {
            var task1 = new DownloadTask(
                Url: "https://example.com/file.jar",
                DestinationPath: "/path1/to/file.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar
            );

            var task2 = new DownloadTask(
                Url: "https://example.com/file.jar",
                DestinationPath: "/path2/to/file.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar
            );

            Assert.NotEqual(task1, task2);
        }

        [Fact]
        public void TwoInstances_WithDifferentSha1_ShouldNotBeEqual()
        {
            var task1 = new DownloadTask(
                Url: "https://example.com/file.jar",
                DestinationPath: "/path/to/file.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar
            );

            var task2 = new DownloadTask(
                Url: "https://example.com/file.jar",
                DestinationPath: "/path/to/file.jar",
                Sha1: "def456",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar
            );

            Assert.NotEqual(task1, task2);
        }

        [Fact]
        public void TwoInstances_WithDifferentSize_ShouldNotBeEqual()
        {
            var task1 = new DownloadTask(
                Url: "https://example.com/file.jar",
                DestinationPath: "/path/to/file.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar
            );

            var task2 = new DownloadTask(
                Url: "https://example.com/file.jar",
                DestinationPath: "/path/to/file.jar",
                Sha1: "abc123",
                Size: 2048,
                Kind: DownloadTaskKind.ClientJar
            );

            Assert.NotEqual(task1, task2);
        }

        [Fact]
        public void TwoInstances_WithDifferentKind_ShouldNotBeEqual()
        {
            var task1 = new DownloadTask(
                Url: "https://example.com/file.jar",
                DestinationPath: "/path/to/file.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.ClientJar
            );

            var task2 = new DownloadTask(
                Url: "https://example.com/file.jar",
                DestinationPath: "/path/to/file.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: DownloadTaskKind.Library
            );

            Assert.NotEqual(task1, task2);
        }

        [Theory]
        [InlineData(DownloadTaskKind.ClientJar)]
        [InlineData(DownloadTaskKind.Library)]
        [InlineData(DownloadTaskKind.Asset)]
        [InlineData(DownloadTaskKind.AssetIndex)]
        [InlineData(DownloadTaskKind.LogConfig)]
        [InlineData(DownloadTaskKind.ForgeUniversal)]
        [InlineData(DownloadTaskKind.ForgeInstaller)]
        [InlineData(DownloadTaskKind.FabricLibrary)]
        [InlineData(DownloadTaskKind.QuiltLibrary)]
        public void DownloadTask_ShouldHaveCorrectKind(DownloadTaskKind kind)
        {
            var task = new DownloadTask(
                Url: "https://example.com/file.jar",
                DestinationPath: "/path/to/file.jar",
                Sha1: "abc123",
                Size: 1024,
                Kind: kind
            );

            Assert.Equal(kind, task.Kind);
        }
    }

    public class DownloadProgressTests
    {
        [Fact]
        public void TwoInstances_WithSameProperties_ShouldBeEqual()
        {
            var progress1 = new DownloadProgress(
                Completed: 5,
                Total: 10,
                CurrentFile: "file.jar",
                Kind: DownloadTaskKind.ClientJar
            );

            var progress2 = new DownloadProgress(
                Completed: 5,
                Total: 10,
                CurrentFile: "file.jar",
                Kind: DownloadTaskKind.ClientJar
            );

            Assert.Equal(progress1, progress2);
            Assert.True(progress1 == progress2);
            Assert.False(progress1 != progress2);
            Assert.Equal(progress1.GetHashCode(), progress2.GetHashCode());
        }

        [Fact]
        public void TwoInstances_WithDifferentCompleted_ShouldNotBeEqual()
        {
            var progress1 = new DownloadProgress(
                Completed: 5,
                Total: 10,
                CurrentFile: "file.jar",
                Kind: DownloadTaskKind.ClientJar
            );

            var progress2 = new DownloadProgress(
                Completed: 6,
                Total: 10,
                CurrentFile: "file.jar",
                Kind: DownloadTaskKind.ClientJar
            );

            Assert.NotEqual(progress1, progress2);
        }

        [Fact]
        public void TwoInstances_WithDifferentTotal_ShouldNotBeEqual()
        {
            var progress1 = new DownloadProgress(
                Completed: 5,
                Total: 10,
                CurrentFile: "file.jar",
                Kind: DownloadTaskKind.ClientJar
            );

            var progress2 = new DownloadProgress(
                Completed: 5,
                Total: 20,
                CurrentFile: "file.jar",
                Kind: DownloadTaskKind.ClientJar
            );

            Assert.NotEqual(progress1, progress2);
        }

        [Fact]
        public void TwoInstances_WithDifferentCurrentFile_ShouldNotBeEqual()
        {
            var progress1 = new DownloadProgress(
                Completed: 5,
                Total: 10,
                CurrentFile: "file1.jar",
                Kind: DownloadTaskKind.ClientJar
            );

            var progress2 = new DownloadProgress(
                Completed: 5,
                Total: 10,
                CurrentFile: "file2.jar",
                Kind: DownloadTaskKind.ClientJar
            );

            Assert.NotEqual(progress1, progress2);
        }

        [Fact]
        public void TwoInstances_WithDifferentKind_ShouldNotBeEqual()
        {
            var progress1 = new DownloadProgress(
                Completed: 5,
                Total: 10,
                CurrentFile: "file.jar",
                Kind: DownloadTaskKind.ClientJar
            );

            var progress2 = new DownloadProgress(
                Completed: 5,
                Total: 10,
                CurrentFile: "file.jar",
                Kind: DownloadTaskKind.Library
            );

            Assert.NotEqual(progress1, progress2);
        }

        [Fact]
        public void DownloadProgress_ShouldHaveCorrectProperties()
        {
            var progress = new DownloadProgress(
                Completed: 3,
                Total: 7,
                CurrentFile: "client.jar",
                Kind: DownloadTaskKind.Asset
            );

            Assert.Equal(3, progress.Completed);
            Assert.Equal(7, progress.Total);
            Assert.Equal("client.jar", progress.CurrentFile);
            Assert.Equal(DownloadTaskKind.Asset, progress.Kind);
        }
    }

    public class InstallRequestTests
    {
        [Fact]
        public void WhenCreatedWithOnlyRequiredParameters_ShouldHaveDefaultValues()
        {
            var request = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft"
            );

            Assert.Equal("1.21.1", request.GameVersion);
            Assert.Equal("/path/to/.minecraft", request.GameDirectory);
            Assert.Equal(LoaderKind.Vanilla, request.Loader);
            Assert.Null(request.LoaderVersion);
            Assert.Equal(8, request.MaxParallelism);
            Assert.True(request.SkipIfExists);
        }

        [Fact]
        public void TwoInstances_WithSameProperties_ShouldBeEqual()
        {
            var request1 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                Loader: LoaderKind.Forge,
                LoaderVersion: "47.3.0",
                MaxParallelism: 16,
                SkipIfExists: false
            );

            var request2 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                Loader: LoaderKind.Forge,
                LoaderVersion: "47.3.0",
                MaxParallelism: 16,
                SkipIfExists: false
            );

            Assert.Equal(request1, request2);
            Assert.True(request1 == request2);
            Assert.False(request1 != request2);
            Assert.Equal(request1.GetHashCode(), request2.GetHashCode());
        }

        [Fact]
        public void TwoInstances_WithDifferentGameVersion_ShouldNotBeEqual()
        {
            var request1 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft"
            );

            var request2 = new InstallRequest(
                GameVersion: "1.20.4",
                GameDirectory: "/path/to/.minecraft"
            );

            Assert.NotEqual(request1, request2);
        }

        [Fact]
        public void TwoInstances_WithDifferentGameDirectory_ShouldNotBeEqual()
        {
            var request1 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path1/to/.minecraft"
            );

            var request2 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path2/to/.minecraft"
            );

            Assert.NotEqual(request1, request2);
        }

        [Fact]
        public void TwoInstances_WithDifferentLoader_ShouldNotBeEqual()
        {
            var request1 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                Loader: LoaderKind.Vanilla
            );

            var request2 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                Loader: LoaderKind.Forge
            );

            Assert.NotEqual(request1, request2);
        }

        [Fact]
        public void TwoInstances_WithDifferentLoaderVersion_ShouldNotBeEqual()
        {
            var request1 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                Loader: LoaderKind.Forge,
                LoaderVersion: "47.3.0"
            );

            var request2 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                Loader: LoaderKind.Forge,
                LoaderVersion: "47.3.1"
            );

            Assert.NotEqual(request1, request2);
        }

        [Fact]
        public void TwoInstances_WithDifferentMaxParallelism_ShouldNotBeEqual()
        {
            var request1 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                MaxParallelism: 8
            );

            var request2 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                MaxParallelism: 16
            );

            Assert.NotEqual(request1, request2);
        }

        [Fact]
        public void TwoInstances_WithDifferentSkipIfExists_ShouldNotBeEqual()
        {
            var request1 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                SkipIfExists: true
            );

            var request2 = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                SkipIfExists: false
            );

            Assert.NotEqual(request1, request2);
        }

        [Theory]
        [InlineData(LoaderKind.Vanilla)]
        [InlineData(LoaderKind.Forge)]
        [InlineData(LoaderKind.Fabric)]
        [InlineData(LoaderKind.Quilt)]
        public void InstallRequest_ShouldAcceptAllLoaderKinds(LoaderKind loader)
        {
            var request = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                Loader: loader
            );

            Assert.Equal(loader, request.Loader);
        }

        [Fact]
        public void InstallRequest_WithCustomMaxParallelism_ShouldOverrideDefault()
        {
            var request = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                MaxParallelism: 32
            );

            Assert.Equal(32, request.MaxParallelism);
        }

        [Fact]
        public void InstallRequest_WithLoaderVersion_ShouldStoreValue()
        {
            var request = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                Loader: LoaderKind.Fabric,
                LoaderVersion: "0.16.0"
            );

            Assert.Equal("0.16.0", request.LoaderVersion);
        }

        [Fact]
        public void InstallRequest_WithSkipIfExistsFalse_ShouldOverrideDefault()
        {
            var request = new InstallRequest(
                GameVersion: "1.21.1",
                GameDirectory: "/path/to/.minecraft",
                SkipIfExists: false
            );

            Assert.False(request.SkipIfExists);
        }
    }
}
