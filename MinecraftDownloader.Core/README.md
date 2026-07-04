# MinecraftDownloader

一个用于下载和安装 Minecraft 的 .NET 库，支持 Vanilla、Forge、Fabric 和 Quilt。

## 特性

- 支持多种 Minecraft 加载器：Vanilla、Forge、Fabric、Quilt
- 并行下载，可配置并发数
- SHA1 文件校验确保下载完整性
- 支持跳过已存在的文件
- 进度回调支持
- 可取消的异步操作
- 依赖注入友好设计

## 安装

```bash
dotnet add package MinecraftDownloader.Core
```

## 快速开始

### 基础用法

```csharp
using MinecraftDownloader.Core.Models;
using MinecraftDownloader.Core.Orchestration;

// 创建安装器实例
var installer = MinecraftInstaller.CreateDefault();

// 创建安装请求
var request = new InstallRequest(
    GameVersion: "1.21.1",
    GameDirectory: @"C:\Games\.minecraft",
    Loader: LoaderKind.Vanilla,
    MaxParallelism: 128,
    SkipIfExists: true
);

// 执行安装
var result = await installer.InstallAsync(request);

if (result.Succeeded)
{
    Console.WriteLine($"安装成功！共下载 {result.Tasks.Count} 个文件，耗时 {result.Elapsed}");
}
else
{
    Console.WriteLine($"安装失败：{result.Error?.Message}");
}
```

### 安装 Fabric

```csharp
var request = new InstallRequest(
    GameVersion: "1.21.1",
    GameDirectory: @"C:\Games\.minecraft",
    Loader: LoaderKind.Fabric,
    LoaderVersion: null,  // null 表示使用最新稳定版
    MaxParallelism: 128
);
```

### 安装 Forge

```csharp
var request = new InstallRequest(
    GameVersion: "1.21.1",
    GameDirectory: @"C:\Games\.minecraft",
    Loader: LoaderKind.Forge,
    LoaderVersion: "52.0.0"
);
```

### 进度回调

```csharp
var progress = new Progress<DownloadProgress>(p =>
{
    Console.WriteLine($"[{p.Completed}/{p.Total}] {p.Kind}: {p.CurrentFile}");
});

var result = await installer.InstallAsync(request, progress);
```

### 取消操作

```csharp
using var cts = new CancellationTokenSource();

// 在需要时取消
// cts.Cancel();

var result = await installer.InstallAsync(request, progress, cts.Token);
```

## API 参考

### InstallRequest

| 属性           | 类型       | 说明                                     |
| -------------- | ---------- | ---------------------------------------- |
| GameVersion    | string     | Minecraft 版本，如 "1.21.1"              |
| GameDirectory  | string     | .minecraft 目录的绝对路径                |
| Loader         | LoaderKind | 加载器类型（Vanilla/Forge/Fabric/Quilt） |
| LoaderVersion  | string?    | 加载器版本，null 表示最新稳定版          |
| MaxParallelism | int        | 最大并发下载数，默认 8                   |
| SkipIfExists   | bool       | 是否跳过已存在的文件，默认 true          |

### LoaderKind

```csharp
public enum LoaderKind
{
    Vanilla,
    Forge,
    Fabric,
    Quilt
}
```

### InstallResult

| 属性      | 类型                        | 说明                 |
| --------- | --------------------------- | -------------------- |
| Succeeded | bool                        | 安装是否成功         |
| Request   | InstallRequest              | 原始请求             |
| Tasks     | IReadOnlyList<DownloadTask> | 下载任务列表         |
| Error     | Exception?                  | 错误信息（如果失败） |
| Elapsed   | TimeSpan                    | 总耗时               |

## 架构

项目采用分层架构设计：

- **Abstractions** - 核心接口定义
- **Models** - 数据模型
- **Manifest** - 版本清单解析（Mojang、Forge、Fabric）
- **Pipeline** - 下载任务构建管道
- **Download** - 并行下载执行器
- **Storage** - 本地存储提供者
- **Verify** - 文件校验（SHA1）
- **Orchestration** - 顶层 API 编排

## 依赖注入

库设计为 DI 友好，可以与 Microsoft.Extensions.DependencyInjection 等容器集成：

```csharp
services.AddSingleton<HttpClient>();
services.AddSingleton<IStorageProvider, LocalStorageProvider>();
services.AddSingleton<IFileVerifier, Sha1FileVerifier>();
services.AddSingleton<IManifestParser, MojangManifestParser>();
services.AddSingleton<IForgeMetaResolver, ForgeMetaResolver>();
services.AddSingleton<IFabricMetaResolver, FabricMetaResolver>();
services.AddSingleton<IFabricMetaResolver, QuiltMetaResolver>();
services.AddSingleton<IDownloadPipeline, DownloadPipeline>();
services.AddSingleton<IDownloadExecutor, ParallelDownloadExecutor>();
services.AddSingleton<MinecraftInstaller>();
```

## 许可证

[MIT License](LICENSE)

Copyright (c) 2026 墨渊
