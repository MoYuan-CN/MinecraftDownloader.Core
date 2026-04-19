using System.Text.Json;
using MinecraftDownloader.Core.Models;

namespace MinecraftDownloader.Test.Models;

public class JsonDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    public class VersionManifestIndexTests
    {
        [Fact]
        public void Deserialize_CompleteJson_ReturnsCorrectObject()
        {
            var json = """
                {
                    "latest": {
                        "release": "1.21",
                        "snapshot": "24w33a"
                    },
                    "versions": [
                        {
                            "id": "1.21",
                            "type": "release",
                            "url": "https://launchermeta.mojang.com/v1/versions/1.21.json",
                            "releaseTime": "2024-06-13T10:15:00Z",
                            "sha1": "abc123def456"
                        }
                    ]
                }
                """;

            var result = JsonSerializer.Deserialize<VersionManifestIndex>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal("1.21", result.Latest.Release);
            Assert.Equal("24w33a", result.Latest.Snapshot);
            Assert.Single(result.Versions);
            Assert.Equal("1.21", result.Versions[0].Id);
            Assert.Equal("release", result.Versions[0].Type);
            Assert.Equal("https://launchermeta.mojang.com/v1/versions/1.21.json", result.Versions[0].Url);
            Assert.Equal("abc123def456", result.Versions[0].Sha1);
        }

        [Fact]
        public void Deserialize_MultipleVersions_ReturnsAllVersions()
        {
            var json = """
                {
                    "latest": {
                        "release": "1.21.1",
                        "snapshot": "24w35a"
                    },
                    "versions": [
                        {
                            "id": "1.21.1",
                            "type": "release",
                            "url": "https://example.com/1.21.1.json",
                            "releaseTime": "2024-08-08T10:15:00Z",
                            "sha1": "sha1_1"
                        },
                        {
                            "id": "1.20.4",
                            "type": "release",
                            "url": "https://example.com/1.20.4.json",
                            "releaseTime": "2023-12-14T10:15:00Z",
                            "sha1": "sha1_2"
                        },
                        {
                            "id": "24w35a",
                            "type": "snapshot",
                            "url": "https://example.com/24w35a.json",
                            "releaseTime": "2024-08-20T10:15:00Z",
                            "sha1": "sha1_3"
                        }
                    ]
                }
                """;

            var result = JsonSerializer.Deserialize<VersionManifestIndex>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal(3, result.Versions.Count);
            Assert.Equal("1.21.1", result.Versions[0].Id);
            Assert.Equal("release", result.Versions[0].Type);
            Assert.Equal("1.20.4", result.Versions[1].Id);
            Assert.Equal("snapshot", result.Versions[2].Type);
        }

        [Fact]
        public void Deserialize_ReleaseTime_ParsesCorrectly()
        {
            var json = """
                {
                    "latest": {"release": "1.21", "snapshot": "24w33a"},
                    "versions": [{
                        "id": "1.21",
                        "type": "release",
                        "url": "https://example.com",
                        "releaseTime": "2024-06-13T10:15:00+00:00",
                        "sha1": "abc"
                    }]
                }
                """;

            var result = JsonSerializer.Deserialize<VersionManifestIndex>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal(new DateTimeOffset(2024, 6, 13, 10, 15, 0, TimeSpan.Zero), result.Versions[0].ReleaseTime);
        }

        [Fact]
        public void Deserialize_WithTrailingCommas_Succeeds()
        {
            var json = """
                {
                    "latest": {
                        "release": "1.21",
                        "snapshot": "24w33a",
                    },
                    "versions": [
                        {
                            "id": "1.21",
                            "type": "release",
                            "url": "https://example.com",
                            "releaseTime": "2024-06-13T10:15:00Z",
                            "sha1": "abc",
                        },
                    ],
                }
                """;

            var result = JsonSerializer.Deserialize<VersionManifestIndex>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal("1.21", result.Latest.Release);
        }
    }

    public class VersionMetaTests
    {
        [Fact]
        public void Deserialize_CompleteJson_ReturnsCorrectObject()
        {
            var json = """
                {
                    "id": "1.21.1",
                    "type": "release",
                    "mainClass": "net.minecraft.client.Main",
                    "downloads": {
                        "client": {
                            "url": "https://example.com/client.jar",
                            "sha1": "abc123",
                            "size": 12345
                        }
                    },
                    "assetIndex": {
                        "id": "1.21",
                        "url": "https://example.com/assets.json",
                        "sha1": "def456",
                        "size": 1000,
                        "totalSize": 50000
                    },
                    "assets": "1.21",
                    "libraries": [],
                    "logging": null
                }
                """;

            var result = JsonSerializer.Deserialize<VersionMeta>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal("1.21.1", result.Id);
            Assert.Equal("release", result.Type);
            Assert.Equal("net.minecraft.client.Main", result.MainClass);
            Assert.Equal("https://example.com/client.jar", result.Downloads.Client.Url);
            Assert.Equal("abc123", result.Downloads.Client.Sha1);
            Assert.Equal(12345, result.Downloads.Client.Size);
            Assert.Equal("1.21", result.AssetIndex.Id);
            Assert.Equal("def456", result.AssetIndex.Sha1);
            Assert.Equal(1000, result.AssetIndex.Size);
            Assert.Equal(50000, result.AssetIndex.TotalSize);
            Assert.Equal("1.21", result.Assets);
            Assert.Empty(result.Libraries);
            Assert.Null(result.Logging);
        }

        [Fact]
        public void Deserialize_WithAllDownloads_ReturnsAllDownloadEntries()
        {
            var json = """
                {
                    "id": "1.21.1",
                    "type": "release",
                    "mainClass": "net.minecraft.client.Main",
                    "downloads": {
                        "client": {
                            "url": "https://example.com/client.jar",
                            "sha1": "client_sha",
                            "size": 10000
                        },
                        "client_mappings": {
                            "url": "https://example.com/client_mappings.txt",
                            "sha1": "mappings_sha",
                            "size": 5000
                        },
                        "server": {
                            "url": "https://example.com/server.jar",
                            "sha1": "server_sha",
                            "size": 15000
                        }
                    },
                    "assetIndex": {
                        "id": "1.21",
                        "url": "https://example.com/assets.json",
                        "sha1": "assets_sha",
                        "size": 1000,
                        "totalSize": 50000
                    },
                    "assets": "1.21",
                    "libraries": [],
                    "logging": null
                }
                """;

            var result = JsonSerializer.Deserialize<VersionMeta>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.NotNull(result.Downloads.Client);
            Assert.NotNull(result.Downloads.ClientMappings);
            Assert.NotNull(result.Downloads.Server);
            Assert.Equal("client_sha", result.Downloads.Client.Sha1);
            Assert.Equal("mappings_sha", result.Downloads.ClientMappings!.Sha1);
            Assert.Equal("server_sha", result.Downloads.Server!.Sha1);
        }

        [Fact]
        public void Deserialize_WithLibraries_ReturnsLibraryList()
        {
            var json = """
                {
                    "id": "1.21.1",
                    "type": "release",
                    "mainClass": "net.minecraft.client.Main",
                    "downloads": {
                        "client": {"url": "https://example.com/client.jar", "sha1": "abc", "size": 12345}
                    },
                    "assetIndex": {
                        "id": "1.21",
                        "url": "https://example.com/assets.json",
                        "sha1": "def",
                        "size": 1000,
                        "totalSize": 50000
                    },
                    "assets": "1.21",
                    "libraries": [
                        {
                            "name": "com.google.code.findbugs:jsr305:3.0.2",
                            "downloads": {
                                "artifact": {
                                    "path": "com/google/code/findbugs/jsr305/3.0.2/jsr305-3.0.2.jar",
                                    "url": "https://example.com/jsr305.jar",
                                    "sha1": "lib_sha",
                                    "size": 2000
                                }
                            }
                        }
                    ],
                    "logging": null
                }
                """;

            var result = JsonSerializer.Deserialize<VersionMeta>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Single(result.Libraries);
            Assert.Equal("com.google.code.findbugs:jsr305:3.0.2", result.Libraries[0].Name);
            Assert.NotNull(result.Libraries[0].Downloads);
            Assert.NotNull(result.Libraries[0].Downloads!.Artifact);
            Assert.Equal("com/google/code/findbugs/jsr305/3.0.2/jsr305-3.0.2.jar", result.Libraries[0].Downloads!.Artifact!.Path);
        }

        [Fact]
        public void Deserialize_WithLogging_ReturnsLoggingConfig()
        {
            var json = """
                {
                    "id": "1.21.1",
                    "type": "release",
                    "mainClass": "net.minecraft.client.Main",
                    "downloads": {
                        "client": {"url": "https://example.com/client.jar", "sha1": "abc", "size": 12345}
                    },
                    "assetIndex": {
                        "id": "1.21",
                        "url": "https://example.com/assets.json",
                        "sha1": "def",
                        "size": 1000,
                        "totalSize": 50000
                    },
                    "assets": "1.21",
                    "libraries": [],
                    "logging": {
                        "client": {
                            "file": {
                                "url": "https://example.com/log4j2.xml",
                                "sha1": "log_sha",
                                "size": 500
                            },
                            "argument": "-Dlog4j.configurationFile=${path}",
                            "type": "log4j2-xml"
                        }
                    }
                }
                """;

            var result = JsonSerializer.Deserialize<VersionMeta>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.NotNull(result.Logging);
            Assert.NotNull(result.Logging!.Client);
            Assert.Equal("https://example.com/log4j2.xml", result.Logging.Client.File.Url);
            Assert.Equal("log_sha", result.Logging.Client.File.Sha1);
            Assert.Equal("-Dlog4j.configurationFile=${path}", result.Logging.Client.Argument);
            Assert.Equal("log4j2-xml", result.Logging.Client.Type);
        }
    }

    public class AssetIndexTests
    {
        [Fact]
        public void Deserialize_CompleteJson_ReturnsCorrectObject()
        {
            var json = """
                {
                    "objects": {
                        "minecraft/textures/block/stone.png": {
                            "hash": "abc123def456",
                            "size": 1234
                        }
                    },
                    "virtual": false,
                    "map_to_resources": false
                }
                """;

            var result = JsonSerializer.Deserialize<AssetIndex>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.False(result.Virtual);
            Assert.False(result.MapToResources);
            Assert.Single(result.Objects);
            Assert.True(result.Objects.ContainsKey("minecraft/textures/block/stone.png"));
            Assert.Equal("abc123def456", result.Objects["minecraft/textures/block/stone.png"].Hash);
            Assert.Equal(1234, result.Objects["minecraft/textures/block/stone.png"].Size);
        }

        [Fact]
        public void Deserialize_MultipleAssets_ReturnsAllObjects()
        {
            var json = """
                {
                    "objects": {
                        "minecraft/textures/block/stone.png": {
                            "hash": "hash_stone",
                            "size": 1234
                        },
                        "minecraft/textures/block/dirt.png": {
                            "hash": "hash_dirt",
                            "size": 2345
                        },
                        "minecraft/sounds/ambient/cave.ogg": {
                            "hash": "hash_sound",
                            "size": 50000
                        }
                    },
                    "virtual": false,
                    "map_to_resources": false
                }
                """;

            var result = JsonSerializer.Deserialize<AssetIndex>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal(3, result.Objects.Count);
            Assert.Equal("hash_stone", result.Objects["minecraft/textures/block/stone.png"].Hash);
            Assert.Equal("hash_dirt", result.Objects["minecraft/textures/block/dirt.png"].Hash);
            Assert.Equal("hash_sound", result.Objects["minecraft/sounds/ambient/cave.ogg"].Hash);
        }

        [Fact]
        public void Deserialize_VirtualTrue_ReturnsTrue()
        {
            var json = """
                {
                    "objects": {},
                    "virtual": true,
                    "map_to_resources": false
                }
                """;

            var result = JsonSerializer.Deserialize<AssetIndex>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.True(result.Virtual);
            Assert.False(result.MapToResources);
        }

        [Fact]
        public void Deserialize_MapToResourcesTrue_ReturnsTrue()
        {
            var json = """
                {
                    "objects": {},
                    "virtual": false,
                    "map_to_resources": true
                }
                """;

            var result = JsonSerializer.Deserialize<AssetIndex>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.False(result.Virtual);
            Assert.True(result.MapToResources);
        }

        [Fact]
        public void Deserialize_EmptyObjects_ReturnsEmptyDictionary()
        {
            var json = """
                {
                    "objects": {},
                    "virtual": false,
                    "map_to_resources": false
                }
                """;

            var result = JsonSerializer.Deserialize<AssetIndex>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Empty(result.Objects);
        }
    }

    public class LibraryTests
    {
        [Fact]
        public void Deserialize_CompleteJson_ReturnsCorrectObject()
        {
            var json = """
                {
                    "name": "com.google.code.findbugs:jsr305:3.0.2",
                    "downloads": {
                        "artifact": {
                            "path": "com/google/code/findbugs/jsr305/3.0.2/jsr305-3.0.2.jar",
                            "url": "https://repo1.maven.org/maven2/com/google/code/findbugs/jsr305/3.0.2/jsr305-3.0.2.jar",
                            "sha1": "abc123def456",
                            "size": 12345
                        }
                    }
                }
                """;

            var result = JsonSerializer.Deserialize<Library>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal("com.google.code.findbugs:jsr305:3.0.2", result.Name);
            Assert.NotNull(result.Downloads);
            Assert.NotNull(result.Downloads.Artifact);
            Assert.Equal("com/google/code/findbugs/jsr305/3.0.2/jsr305-3.0.2.jar", result.Downloads.Artifact!.Path);
            Assert.Equal("https://repo1.maven.org/maven2/com/google/code/findbugs/jsr305/3.0.2/jsr305-3.0.2.jar", result.Downloads.Artifact.Url);
            Assert.Equal("abc123def456", result.Downloads.Artifact.Sha1);
            Assert.Equal(12345, result.Downloads.Artifact.Size);
            Assert.Null(result.Rules);
        }

        [Fact]
        public void Deserialize_WithRules_ReturnsRulesArray()
        {
            var json = """
                {
                    "name": "com.google.code.findbugs:jsr305:3.0.2",
                    "downloads": {
                        "artifact": {
                            "path": "com/google/code/findbugs/jsr305/3.0.2/jsr305-3.0.2.jar",
                            "url": "https://example.com/jsr305.jar",
                            "sha1": "abc",
                            "size": 1234
                        }
                    },
                    "rules": [
                        {
                            "action": "allow",
                            "os": {
                                "name": "windows"
                            }
                        }
                    ]
                }
                """;

            var result = JsonSerializer.Deserialize<Library>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.NotNull(result.Rules);
            Assert.Single(result.Rules!);
            Assert.Equal("allow", result.Rules![0].Action);
            Assert.NotNull(result.Rules[0].Os);
            Assert.Equal("windows", result.Rules[0].Os!.Name);
        }

        [Fact]
        public void Deserialize_WithMultipleRules_ReturnsAllRules()
        {
            var json = """
                {
                    "name": "com.example:library:1.0",
                    "downloads": {
                        "artifact": {
                            "path": "com/example/library/1.0/library-1.0.jar",
                            "url": "https://example.com/lib.jar",
                            "sha1": "sha",
                            "size": 1000
                        }
                    },
                    "rules": [
                        {
                            "action": "allow",
                            "os": {"name": "windows"}
                        },
                        {
                            "action": "disallow",
                            "os": {"name": "linux"}
                        }
                    ]
                }
                """;

            var result = JsonSerializer.Deserialize<Library>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.NotNull(result.Rules);
            Assert.Equal(2, result.Rules!.Count);
            Assert.Equal("allow", result.Rules[0].Action);
            Assert.Equal("windows", result.Rules[0].Os!.Name);
            Assert.Equal("disallow", result.Rules[1].Action);
            Assert.Equal("linux", result.Rules[1].Os!.Name);
        }

        [Fact]
        public void Deserialize_WithOptionalUrl_ReturnsUrl()
        {
            var json = """
                {
                    "name": "net.fabricmc:sponge-mixin:0.12.5+mixin.0.8.5",
                    "url": "https://maven.fabricmc.net/",
                    "downloads": {
                        "artifact": {
                            "path": "net/fabricmc/sponge-mixin/0.12.5+mixin.0.8.5/sponge-mixin-0.12.5+mixin.0.8.5.jar",
                            "url": "https://maven.fabricmc.net/net/fabricmc/sponge-mixin/0.12.5+mixin.0.8.5/sponge-mixin-0.12.5+mixin.0.8.5.jar",
                            "sha1": "abc",
                            "size": 5000
                        }
                    }
                }
                """;

            var result = JsonSerializer.Deserialize<Library>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal("https://maven.fabricmc.net/", result.Url);
        }

        [Fact]
        public void Deserialize_RuleWithoutOs_ReturnsNullOs()
        {
            var json = """
                {
                    "name": "com.example:library:1.0",
                    "downloads": {
                        "artifact": {
                            "path": "com/example/library/1.0/library-1.0.jar",
                            "url": "https://example.com/lib.jar",
                            "sha1": "sha",
                            "size": 1000
                        }
                    },
                    "rules": [
                        {
                            "action": "allow"
                        }
                    ]
                }
                """;

            var result = JsonSerializer.Deserialize<Library>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.NotNull(result.Rules);
            Assert.Single(result.Rules!);
            Assert.Equal("allow", result.Rules![0].Action);
            Assert.Null(result.Rules[0].Os);
        }

        [Fact]
        public void Deserialize_WithoutDownloads_ReturnsNullDownloads()
        {
            var json = """
                {
                    "name": "com.example:library:1.0"
                }
                """;

            var result = JsonSerializer.Deserialize<Library>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal("com.example:library:1.0", result.Name);
            Assert.Null(result.Downloads);
            Assert.Null(result.Rules);
        }
    }

    public class LatestVersionsTests
    {
        [Fact]
        public void Deserialize_CompleteJson_ReturnsCorrectObject()
        {
            var json = """
                {
                    "release": "1.21.1",
                    "snapshot": "24w35a"
                }
                """;

            var result = JsonSerializer.Deserialize<LatestVersions>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal("1.21.1", result.Release);
            Assert.Equal("24w35a", result.Snapshot);
        }
    }

    public class VersionEntryTests
    {
        [Fact]
        public void Deserialize_CompleteJson_ReturnsCorrectObject()
        {
            var json = """
                {
                    "id": "1.21.1",
                    "type": "release",
                    "url": "https://launchermeta.mojang.com/v1/versions/1.21.1.json",
                    "releaseTime": "2024-08-08T10:15:00Z",
                    "sha1": "abc123def456789"
                }
                """;

            var result = JsonSerializer.Deserialize<VersionEntry>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal("1.21.1", result.Id);
            Assert.Equal("release", result.Type);
            Assert.Equal("https://launchermeta.mojang.com/v1/versions/1.21.1.json", result.Url);
            Assert.Equal("abc123def456789", result.Sha1);
        }
    }

    public class DownloadEntryTests
    {
        [Fact]
        public void Deserialize_CompleteJson_ReturnsCorrectObject()
        {
            var json = """
                {
                    "url": "https://example.com/file.jar",
                    "sha1": "deadbeef12345678",
                    "size": 9876543
                }
                """;

            var result = JsonSerializer.Deserialize<DownloadEntry>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal("https://example.com/file.jar", result.Url);
            Assert.Equal("deadbeef12345678", result.Sha1);
            Assert.Equal(9876543, result.Size);
        }
    }

    public class AssetIndexRefTests
    {
        [Fact]
        public void Deserialize_CompleteJson_ReturnsCorrectObject()
        {
            var json = """
                {
                    "id": "1.21",
                    "url": "https://launchermeta.mojang.com/v1/assets/1.21.json",
                    "sha1": "asset_sha_123",
                    "size": 1500,
                    "totalSize": 75000
                }
                """;

            var result = JsonSerializer.Deserialize<AssetIndexRef>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal("1.21", result.Id);
            Assert.Equal("https://launchermeta.mojang.com/v1/assets/1.21.json", result.Url);
            Assert.Equal("asset_sha_123", result.Sha1);
            Assert.Equal(1500, result.Size);
            Assert.Equal(75000, result.TotalSize);
        }
    }

    public class LibraryArtifactTests
    {
        [Fact]
        public void Deserialize_CompleteJson_ReturnsCorrectObject()
        {
            var json = """
                {
                    "path": "com/google/code/findbugs/jsr305/3.0.2/jsr305-3.0.2.jar",
                    "url": "https://repo1.maven.org/maven2/com/google/code/findbugs/jsr305/3.0.2/jsr305-3.0.2.jar",
                    "sha1": "abc123",
                    "size": 45000
                }
                """;

            var result = JsonSerializer.Deserialize<LibraryArtifact>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.Equal("com/google/code/findbugs/jsr305/3.0.2/jsr305-3.0.2.jar", result.Path);
            Assert.Equal("https://repo1.maven.org/maven2/com/google/code/findbugs/jsr305/3.0.2/jsr305-3.0.2.jar", result.Url);
            Assert.Equal("abc123", result.Sha1);
            Assert.Equal(45000, result.Size);
        }
    }

    public class LoggingConfigTests
    {
        [Fact]
        public void Deserialize_CompleteJson_ReturnsCorrectObject()
        {
            var json = """
                {
                    "client": {
                        "file": {
                            "url": "https://example.com/log4j2.xml",
                            "sha1": "log_sha",
                            "size": 1024
                        },
                        "argument": "-Dlog4j.configurationFile=${path}",
                        "type": "log4j2-xml"
                    }
                }
                """;

            var result = JsonSerializer.Deserialize<LoggingConfig>(json, JsonOptions);

            Assert.NotNull(result);
            Assert.NotNull(result.Client);
            Assert.Equal("https://example.com/log4j2.xml", result.Client.File.Url);
            Assert.Equal("log_sha", result.Client.File.Sha1);
            Assert.Equal(1024, result.Client.File.Size);
            Assert.Equal("-Dlog4j.configurationFile=${path}", result.Client.Argument);
            Assert.Equal("log4j2-xml", result.Client.Type);
        }
    }
}
