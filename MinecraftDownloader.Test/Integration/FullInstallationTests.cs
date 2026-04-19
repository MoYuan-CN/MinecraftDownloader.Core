using System.Diagnostics;
using MinecraftDownloader.Core.Models;
using MinecraftDownloader.Core.Orchestration;

namespace MinecraftDownloader.Test.Integration;

public class FullInstallationTests : IAsyncLifetime
{
    private readonly string _testGameDir = Path.Combine(Path.GetTempPath(), $"MinecraftDownloader_Test_{Guid.NewGuid():N}");
    private readonly MinecraftInstaller _installer = MinecraftInstaller.CreateDefault();

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(_testGameDir);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_testGameDir))
        {
            try
            {
                Directory.Delete(_testGameDir, recursive: true);
            }
            catch
            {
                // ignored
            }
        }

        return Task.CompletedTask;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullInstall_Vanilla_LatestVersion_InstallsSuccessfully()
    {
        var request = new InstallRequest(
            GameVersion: "1.21.1",
            GameDirectory: _testGameDir,
            Loader: LoaderKind.Vanilla,
            MaxParallelism: 128,
            SkipIfExists: true);

        var progressReports = new List<DownloadProgress>();
        var stopwatch = Stopwatch.StartNew();
        var lastReportTime = stopwatch.ElapsedMilliseconds;
        var progress = new Progress<DownloadProgress>(p =>
        {
            progressReports.Add(p);
            var now = stopwatch.ElapsedMilliseconds;
            if (now - lastReportTime >= 500 || p.Completed == p.Total)
            {
                lastReportTime = now;
                Console.WriteLine($"[{stopwatch.Elapsed:ss\\.ff}] {p.Completed}/{p.Total} ({100.0 * p.Completed / p.Total:F1}%) - {p.Kind}: {p.CurrentFile}");
            }
        });

        var result = await _installer.InstallAsync(request, progress);
        stopwatch.Stop();

        Assert.True(result.Succeeded, $"Installation failed: {result.Error?.Message}");
        Assert.NotNull(result.Tasks);
        Assert.NotEmpty(result.Tasks);

        Assert.True(progressReports.Count > 0, "Expected progress reports during installation");

        Assert.True(Directory.Exists(Path.Combine(_testGameDir, "versions")),
            "Expected 'versions' directory to exist");
        Assert.True(Directory.Exists(Path.Combine(_testGameDir, "libraries")),
            "Expected 'libraries' directory to exist");
        Assert.True(Directory.Exists(Path.Combine(_testGameDir, "assets")),
            "Expected 'assets' directory to exist");

        var clientJar = Path.Combine(_testGameDir, "versions", "1.21.1", "1.21.1.jar");
        Assert.True(File.Exists(clientJar), $"Expected client JAR at {clientJar}");

        var versionJson = Path.Combine(_testGameDir, "versions", "1.21.1", "1.21.1.json");
        Assert.True(File.Exists(versionJson), $"Expected version JSON at {versionJson}");

        OutputProgressSummary(progressReports, stopwatch.Elapsed);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullInstall_Fabric_LatestVersion_InstallsSuccessfully()
    {
        var request = new InstallRequest(
            GameVersion: "1.21.1",
            GameDirectory: _testGameDir,
            Loader: LoaderKind.Fabric,
            LoaderVersion: null,
            MaxParallelism: 128,
            SkipIfExists: true);

        var progressReports = new List<DownloadProgress>();
        var stopwatch = Stopwatch.StartNew();
        var lastReportTime = stopwatch.ElapsedMilliseconds;
        var progress = new Progress<DownloadProgress>(p =>
        {
            progressReports.Add(p);
            var now = stopwatch.ElapsedMilliseconds;
            if (now - lastReportTime >= 500 || p.Completed == p.Total)
            {
                lastReportTime = now;
                Console.WriteLine($"[{stopwatch.Elapsed:ss\\.ff}] {p.Completed}/{p.Total} ({100.0 * p.Completed / p.Total:F1}%) - {p.Kind}: {p.CurrentFile}");
            }
        });

        var result = await _installer.InstallAsync(request, progress);
        stopwatch.Stop();

        Assert.True(result.Succeeded, $"Installation failed: {result.Error?.Message}");
        Assert.NotNull(result.Tasks);
        Assert.NotEmpty(result.Tasks);

        var fabricLibraries = result.Tasks!.Where(t => t.Kind == DownloadTaskKind.FabricLibrary).ToList();
        Assert.True(fabricLibraries.Count > 0, "Expected Fabric libraries to be downloaded");

        OutputProgressSummary(progressReports, stopwatch.Elapsed);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullInstall_WithCustomInstance_InstallsSuccessfully()
    {
        var instanceInfo = new
        {
            GameVersion = "1.20.4",
            GameDirectory = _testGameDir,
            Loader = LoaderKind.Vanilla,
            MaxParallelism = 128
        };

        var request = new InstallRequest(
            GameVersion: instanceInfo.GameVersion,
            GameDirectory: instanceInfo.GameDirectory,
            Loader: instanceInfo.Loader,
            MaxParallelism: instanceInfo.MaxParallelism,
            SkipIfExists: true);

        var stopwatch = Stopwatch.StartNew();
        var progress = new Progress<DownloadProgress>(p =>
        {
            Console.WriteLine($"[{stopwatch.Elapsed:ss\\.ff}] {p.Completed}/{p.Total} ({100.0 * p.Completed / p.Total:F1}%) - {p.Kind}: {p.CurrentFile}");
        });

        var result = await _installer.InstallAsync(request, progress);
        stopwatch.Stop();

        Assert.True(result.Succeeded, $"Installation failed: {result.Error?.Message}");

        var clientJar = Path.Combine(_testGameDir, "versions", instanceInfo.GameVersion, $"{instanceInfo.GameVersion}.jar");
        Assert.True(File.Exists(clientJar), $"Expected client JAR at {clientJar}");

        Console.WriteLine($"Total time: {stopwatch.Elapsed}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullInstall_SkipExistingFiles_DoesNotRedownload()
    {
        var request = new InstallRequest(
            GameVersion: "1.21.1",
            GameDirectory: _testGameDir,
            Loader: LoaderKind.Vanilla,
            MaxParallelism: 128,
            SkipIfExists: true);

        var stopwatch = Stopwatch.StartNew();
        var progress = new Progress<DownloadProgress>(p =>
        {
            Console.WriteLine($"[{stopwatch.Elapsed:ss\\.ff}] {p.Completed}/{p.Total} ({100.0 * p.Completed / p.Total:F1}%) - {p.Kind}: {p.CurrentFile}");
        });

        var firstResult = await _installer.InstallAsync(request, progress);
        stopwatch.Stop();
        Console.WriteLine($"First install time: {stopwatch.Elapsed}");
        Assert.True(firstResult.Succeeded, $"First installation failed: {firstResult.Error?.Message}");

        stopwatch.Restart();
        var secondResult = await _installer.InstallAsync(request, progress);
        stopwatch.Stop();
        Console.WriteLine($"Second install time: {stopwatch.Elapsed}");
        Assert.True(secondResult.Succeeded, $"Second installation failed: {secondResult.Error?.Message}");

        Assert.Equal(firstResult.Tasks!.Count, secondResult.Tasks!.Count);
    }

    private static void OutputProgressSummary(List<DownloadProgress> reports, TimeSpan elapsed)
    {
        if (reports.Count == 0) return;

        var byKind = reports.GroupBy(r => r.Kind)
            .Select(g => $"{g.Key}: {g.Count()} files")
            .ToList();

        Console.WriteLine();
        Console.WriteLine($"=== Download Summary ===");
        Console.WriteLine($"Total time: {elapsed}");
        Console.WriteLine($"Total files: {reports.Count}");
        foreach (var summary in byKind)
        {
            Console.WriteLine($"  - {summary}");
        }
    }
}
