# PicShow

PicShow is a Windows 10+ image viewer planned around broad format support:

- Common raster images: JPG, PNG, BMP, TIFF, ICO, WebP
- Transparent backgrounds for PNG, PSD, SVG, and GIF
- Animated GIF playback
- SVG preview
- PSD composite preview
- PDF preview

## Prerequisites

- Windows 10 or later
- .NET 8 SDK
- WebView2 Runtime, normally already present on modern Windows systems

## Build

```powershell
dotnet restore
dotnet build
dotnet run --project src/PicShow.App
```

## First Milestone

The current milestone is the application shell: main window layout, supported format detection, and preview surface placeholders. Format renderers will be implemented step by step.
