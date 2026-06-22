# PicShow / 图片秀

PicShow is a lightweight Windows image viewer built for a clean, distraction-free viewing experience.

PicShow 是一个轻量级 Windows 图片查看器，目标是提供干净、快速、少打扰的看图体验。

> Free and open source. Built because pop-up ads in image viewers are annoying.
>
> 免费开源。开发它的原因很简单：就是不爽某些看图软件的弹窗广告。

## Features / 功能特性

- Borderless image preview window with adaptive sizing.
- 无边框图片预览窗口，打开图片后会根据图片和屏幕尺寸自适应。
- Mouse wheel zoom, drag-to-pan when the image is larger than the view.
- 支持鼠标滚轮缩放；图片大于窗口时可拖动查看。
- Keyboard navigation with left and right arrow keys.
- 支持键盘左右方向键切换上一张 / 下一张。
- Transparent-background display for PNG and other alpha-capable images.
- 支持 PNG 等带透明通道图片的透明背景显示。
- Startup welcome page with drag-and-drop and Ctrl+O open flow.
- 启动页支持拖入图片和 `Ctrl+O` 打开图片。
- Double-click image opening after registering PicShow as an image viewer.
- 设置为图片打开程序后，支持双击图片直接用 PicShow 打开。
- Windows installer with Simplified Chinese setup UI.
- 提供 Windows 安装包，安装界面支持简体中文。

## Current Format Support / 当前格式支持

Currently implemented:

当前已实现：

- JPG / JPEG
- PNG
- BMP
- TIFF / TIF
- ICO
- WebP

Planned:

计划支持：

- Animated GIF playback / 动态 GIF 播放
- SVG preview / SVG 预览
- PSD composite preview / PSD 合成图预览
- PDF preview / PDF 预览

## Download / 下载

Download the latest release from GitHub:

可从 GitHub Release 下载最新版本：

[PicShow v1.0.0 Release](https://github.com/yibanyiban78/PicShow/releases/tag/v1.0.0)

Recommended download:

推荐下载：

- `PicShow-v1.0.0-setup-win-x64.exe`: self-contained Windows installer, no extra .NET runtime required.
- `PicShow-v1.0.0-setup-win-x64.exe`：自包含 Windows 安装包，无需用户额外安装 .NET 运行时。

Portable option:

便携版：

- `PicShow-v1.0.0-win-x64.zip`: portable executable package.
- `PicShow-v1.0.0-win-x64.zip`：便携版可执行文件压缩包。

## Requirements / 运行要求

For the self-contained installer:

自包含安装包：

- Windows 10 or later
- Windows 10 或更高版本

For development:

开发环境：

- Windows 10 or later
- Windows 10 或更高版本
- .NET 8 SDK
- .NET 8 SDK
- Inno Setup 6, only needed for building the installer
- Inno Setup 6，仅在需要生成安装包时使用

## Build From Source / 从源码构建

Restore and build:

还原并构建：

```powershell
dotnet restore
dotnet build
```

Run the app:

运行程序：

```powershell
dotnet run --project src/PicShow.App
```

Publish a single-file executable:

发布单文件可执行程序：

```powershell
dotnet publish .\src\PicShow.App\PicShow.App.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish\win-x64
```

## Build Installer / 生成安装包

Install Inno Setup 6 first:

请先安装 Inno Setup 6：

[https://jrsoftware.org/isdl.php](https://jrsoftware.org/isdl.php)

Build the normal installer:

生成普通安装包：

```powershell
.\scripts\build-installer.ps1
```

Build the self-contained installer:

生成自包含安装包：

```powershell
.\scripts\build-installer.ps1 -SelfContained
```

Output:

输出文件：

```text
publish\installer\PicShow-v1.0.0-setup-win-x64.exe
```

## File Association Notes / 文件关联说明

The installer registers PicShow in the Windows "Open with" and default-apps list for supported image formats. Modern Windows protects default app choices, so installers cannot reliably force PicShow as the default image viewer without user confirmation.

安装包会把 PicShow 注册到受支持图片格式的“打开方式”和“默认应用”列表中。现代 Windows 会保护默认应用设置，因此安装程序不能可靠地强制把 PicShow 设为默认图片查看器，通常仍需要用户确认。

After setting PicShow as the default image viewer, double-clicking an image launches:

将 PicShow 设置为默认图片查看器后，双击图片会以类似方式启动：

```powershell
PicShow.exe "C:\Path\To\Image.png"
```

PicShow supports this startup path and opens the image directly.

PicShow 已支持此启动参数，会直接打开对应图片。

## Author / 作者

Software author: 壹伴壹伴

软件作者：壹伴壹伴

Welcome to contact me on Douyin.

欢迎通过抖音联系。

## License / 许可证

This project is open source and free to share.

本项目开源免费，欢迎传播共享。
