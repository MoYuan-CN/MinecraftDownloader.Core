using System.Runtime.InteropServices;
using MinecraftDownloader.Core.Storage;

namespace MinecraftDownloader.Test.Storage;

public class LocalStorageProviderTests
{
    private readonly LocalStorageProvider _provider = new();

    [Fact]
    public void GetLibraryPath_ShouldReturnCorrectPath_WithNormalizedSeparators()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var mavenPath = "net/minecraft/client/1.21.1/client-1.21.1.jar";

        var result = _provider.GetLibraryPath(gameDir, mavenPath);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\libraries\\net\\minecraft\\client\\1.21.1\\client-1.21.1.jar"
            : "C:/Games/Minecraft/libraries/net/minecraft/client/1.21.1/client-1.21.1.jar";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetLibraryPath_ShouldHandleAlreadyNormalizedPath()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var mavenPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "net\\minecraft\\client\\1.21.1\\client-1.21.1.jar"
            : "net/minecraft/client/1.21.1/client-1.21.1.jar";

        var result = _provider.GetLibraryPath(gameDir, mavenPath);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\libraries\\net\\minecraft\\client\\1.21.1\\client-1.21.1.jar"
            : "C:/Games/Minecraft/libraries/net/minecraft/client/1.21.1/client-1.21.1.jar";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetLibraryPath_ShouldHandleMixedSeparators()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var mavenPath = "net/minecraft\\client/1.21.1\\client-1.21.1.jar";

        var result = _provider.GetLibraryPath(gameDir, mavenPath);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\libraries\\net\\minecraft\\client\\1.21.1\\client-1.21.1.jar"
            : "C:/Games/Minecraft/libraries/net/minecraft/client/1.21.1/client-1.21.1.jar";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAssetObjectPath_ShouldReturnCorrectTwoLevelHashPrefixPath()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var hash = "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6";

        var result = _provider.GetAssetObjectPath(gameDir, hash);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\assets\\objects\\a1\\a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"
            : "C:/Games/Minecraft/assets/objects/a1/a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAssetObjectPath_ShouldHandleShortHash()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var hash = "ab1234";

        var result = _provider.GetAssetObjectPath(gameDir, hash);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\assets\\objects\\ab\\ab1234"
            : "C:/Games/Minecraft/assets/objects/ab/ab1234";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAssetIndexPath_ShouldReturnCorrectPath_WithIndexId()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var indexId = "1.21";

        var result = _provider.GetAssetIndexPath(gameDir, indexId);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\assets\\indexes\\1.21.json"
            : "C:/Games/Minecraft/assets/indexes/1.21.json";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAssetIndexPath_ShouldHandlePreReleaseIndexId()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var indexId = "1.21-pre1";

        var result = _provider.GetAssetIndexPath(gameDir, indexId);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\assets\\indexes\\1.21-pre1.json"
            : "C:/Games/Minecraft/assets/indexes/1.21-pre1.json";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetClientJarPath_ShouldReturnCorrectVersionJarPath()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var versionId = "1.21.1";

        var result = _provider.GetClientJarPath(gameDir, versionId);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\versions\\1.21.1\\1.21.1.jar"
            : "C:/Games/Minecraft/versions/1.21.1/1.21.1.jar";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetClientJarPath_ShouldHandleSnapshotVersion()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var versionId = "24w33a";

        var result = _provider.GetClientJarPath(gameDir, versionId);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\versions\\24w33a\\24w33a.jar"
            : "C:/Games/Minecraft/versions/24w33a/24w33a.jar";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetLogConfigPath_ShouldReturnCorrectLogConfigPath()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var fileName = "client-1.21.1.xml";

        var result = _provider.GetLogConfigPath(gameDir, fileName);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\assets\\log_configs\\client-1.21.1.xml"
            : "C:/Games/Minecraft/assets/log_configs/client-1.21.1.xml";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetLogConfigPath_ShouldHandleJsonConfigFile()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var fileName = "client-1.21.json";

        var result = _provider.GetLogConfigPath(gameDir, fileName);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\assets\\log_configs\\client-1.21.json"
            : "C:/Games/Minecraft/assets/log_configs/client-1.21.json";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetVersionJsonPath_ShouldReturnCorrectVersionJsonPath()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var versionId = "1.21.1";

        var result = _provider.GetVersionJsonPath(gameDir, versionId);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\versions\\1.21.1\\1.21.1.json"
            : "C:/Games/Minecraft/versions/1.21.1/1.21.1.json";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetVersionJsonPath_ShouldHandleSnapshotVersion()
    {
        var gameDir = "C:\\Games\\Minecraft";
        var versionId = "24w33a";

        var result = _provider.GetVersionJsonPath(gameDir, versionId);

        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "C:\\Games\\Minecraft\\versions\\24w33a\\24w33a.json"
            : "C:/Games/Minecraft/versions/24w33a/24w33a.json";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void MavenCoordinateToPath_ShouldConvertValidCoordinate_WithThreeParts()
    {
        var coordinate = "com.example:artifact:1.0.0";

        var result = LocalStorageProvider.MavenCoordinateToPath(coordinate);

        Assert.Equal("com/example/artifact/1.0.0/artifact-1.0.0.jar", result);
    }

    [Fact]
    public void MavenCoordinateToPath_ShouldConvertValidCoordinate_WithGroupIdHavingMultipleParts()
    {
        var coordinate = "net.minecraft.client:client:1.21.1";

        var result = LocalStorageProvider.MavenCoordinateToPath(coordinate);

        Assert.Equal("net/minecraft/client/client/1.21.1/client-1.21.1.jar", result);
    }

    [Fact]
    public void MavenCoordinateToPath_ShouldConvertValidCoordinate_WithClassifier()
    {
        var coordinate = "com.example:artifact:1.0.0:sources";

        var result = LocalStorageProvider.MavenCoordinateToPath(coordinate);

        Assert.Equal("com/example/artifact/1.0.0/artifact-1.0.0-sources.jar", result);
    }

    [Fact]
    public void MavenCoordinateToPath_ShouldConvertValidCoordinate_WithCustomExtension()
    {
        var coordinate = "com.example:artifact:1.0.0@natives";

        var result = LocalStorageProvider.MavenCoordinateToPath(coordinate);

        Assert.Equal("com/example/artifact/1.0.0/artifact-1.0.0.natives", result);
    }

    [Fact]
    public void MavenCoordinateToPath_ShouldConvertValidCoordinate_WithClassifierAndCustomExtension()
    {
        var coordinate = "com.example:artifact:1.0.0:sources@zip";

        var result = LocalStorageProvider.MavenCoordinateToPath(coordinate);

        Assert.Equal("com/example/artifact/1.0.0/artifact-1.0.0-sources.zip", result);
    }

    [Fact]
    public void MavenCoordinateToPath_ShouldConvertRealMinecraftCoordinate()
    {
        var coordinate = "com.mojang:authlib:3.11.50";

        var result = LocalStorageProvider.MavenCoordinateToPath(coordinate);

        Assert.Equal("com/mojang/authlib/3.11.50/authlib-3.11.50.jar", result);
    }

    [Fact]
    public void MavenCoordinateToPath_ShouldConvertNativeLibraryCoordinate()
    {
        var coordinate = "org.lwjgl:lwjgl-glfw:3.3.3:natives-windows@jar";

        var result = LocalStorageProvider.MavenCoordinateToPath(coordinate);

        Assert.Equal("org/lwjgl/lwjgl-glfw/3.3.3/lwjgl-glfw-3.3.3-natives-windows.jar", result);
    }

    [Fact]
    public void MavenCoordinateToPath_ShouldThrowArgumentException_ForInvalidCoordinate_WithTwoParts()
    {
        var coordinate = "com.example:artifact";

        var exception = Assert.Throws<ArgumentException>(
            () => LocalStorageProvider.MavenCoordinateToPath(coordinate));

        Assert.Contains("Invalid Maven coordinate", exception.Message);
        Assert.Equal("coordinate", exception.ParamName);
    }

    [Fact]
    public void MavenCoordinateToPath_ShouldThrowArgumentException_ForInvalidCoordinate_WithOnePart()
    {
        var coordinate = "com.example";

        var exception = Assert.Throws<ArgumentException>(
            () => LocalStorageProvider.MavenCoordinateToPath(coordinate));

        Assert.Contains("Invalid Maven coordinate", exception.Message);
        Assert.Equal("coordinate", exception.ParamName);
    }

    [Fact]
    public void MavenCoordinateToPath_ShouldThrowArgumentException_ForEmptyCoordinate()
    {
        var coordinate = "";

        var exception = Assert.Throws<ArgumentException>(
            () => LocalStorageProvider.MavenCoordinateToPath(coordinate));

        Assert.Contains("Invalid Maven coordinate", exception.Message);
        Assert.Equal("coordinate", exception.ParamName);
    }

    [Fact]
    public void MavenCoordinateToPath_ShouldThrowArgumentException_ForCoordinateWithOnlySeparator()
    {
        var coordinate = ":";

        var exception = Assert.Throws<ArgumentException>(
            () => LocalStorageProvider.MavenCoordinateToPath(coordinate));

        Assert.Contains("Invalid Maven coordinate", exception.Message);
        Assert.Equal("coordinate", exception.ParamName);
    }
}
