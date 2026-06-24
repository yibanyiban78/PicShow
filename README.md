# PicShow 鍥剧墖绉€

![PicShow 鎴浘](docs/images/screenshot.png)

## 涓枃鐗?
PicShow 鍥剧墖绉€鏄竴涓潰鍚?Windows 10 鍙婁互涓婄増鏈殑杞婚噺绾у浘鐗囨煡鐪嬪櫒锛屼笓娉ㄤ簬骞插噣銆佸揩閫熴€佸皯鎵撴壈鐨勭湅鍥句綋楠屻€?
鍏嶈垂寮€婧愶紝娆㈣繋浼犳挱鍏变韩銆傚仛瀹冪殑鍘熷洜寰堢畝鍗曪細灏辨槸涓嶇埥鏌愪簺鐪嬪浘杞欢鐨勫脊绐楀箍鍛娿€?
### 鍔熻兘鐗规€?
- 鏃犺竟妗嗗浘鐗囬瑙堢獥鍙ｏ紝鎵撳紑鍥剧墖鍚庝細鏍规嵁鍥剧墖鍜屽睆骞曞昂瀵歌嚜閫傚簲銆?- 鏀寔榧犳爣婊氳疆缂╂斁锛涘浘鐗囧ぇ浜庣獥鍙ｆ椂鍙嫋鍔ㄦ煡鐪嬨€?- 鏀寔閿洏宸﹀彸鏂瑰悜閿垏鎹笂涓€寮?/ 涓嬩竴寮犮€?- 鏀寔 PNG 绛夊甫閫忔槑閫氶亾鍥剧墖鐨勯€忔槑鑳屾櫙鏄剧ず銆?- 鍚姩椤垫敮鎸佹嫋鍏ュ浘鐗囧拰 `Ctrl+O` 鎵撳紑鍥剧墖銆?- 璁剧疆涓哄浘鐗囨墦寮€绋嬪簭鍚庯紝鏀寔鍙屽嚮鍥剧墖鐩存帴鐢?PicShow 鎵撳紑銆?- 鎻愪緵 Windows 瀹夎鍖咃紝瀹夎鐣岄潰鏀寔绠€浣撲腑鏂囥€?
### 褰撳墠鏍煎紡鏀寔

褰撳墠宸插疄鐜帮細

- JPG / JPEG
- PNG
- BMP
- TIFF / TIF
- ICO
- WebP

璁″垝鏀寔锛?
- 鍔ㄦ€?GIF 鎾斁
- SVG 棰勮
- PSD 鍚堟垚鍥鹃瑙?- PDF 棰勮

### 涓嬭浇

鍙粠 GitHub Release 涓嬭浇鏈€鏂扮増鏈細

[PicShow v1.1.1 Release](https://github.com/yibanyiban78/PicShow/releases/tag/v1.1.1)

鎺ㄨ崘涓嬭浇锛?
- `PicShow-v1.1.1-setup-win-x64.exe`锛氳嚜鍖呭惈 Windows 瀹夎鍖咃紝鐢ㄦ埛鏃犻渶棰濆瀹夎 .NET 杩愯鏃躲€?
渚挎惡鐗堬細

- `PicShow-v1.1.1-win-x64.zip`锛氫究鎼虹増鍙墽琛屾枃浠跺帇缂╁寘銆?
### 杩愯瑕佹眰

鑷寘鍚畨瑁呭寘锛?
- Windows 10 鎴栨洿楂樼増鏈?
寮€鍙戠幆澧冿細

- Windows 10 鎴栨洿楂樼増鏈?- .NET 8 SDK
- Inno Setup 6锛屼粎鍦ㄩ渶瑕佺敓鎴愬畨瑁呭寘鏃朵娇鐢?
### 浠庢簮鐮佹瀯寤?
杩樺師骞舵瀯寤猴細

```powershell
dotnet restore
dotnet build
```

杩愯绋嬪簭锛?
```powershell
dotnet run --project src/PicShow.App
```

鍙戝竷鍗曟枃浠跺彲鎵ц绋嬪簭锛?
```powershell
dotnet publish .\src\PicShow.App\PicShow.App.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish\win-x64
```

### 鐢熸垚瀹夎鍖?
璇峰厛瀹夎 Inno Setup 6锛?
[https://jrsoftware.org/isdl.php](https://jrsoftware.org/isdl.php)

鐢熸垚鏅€氬畨瑁呭寘锛?
```powershell
.\scripts\build-installer.ps1
```

鐢熸垚鑷寘鍚畨瑁呭寘锛?
```powershell
.\scripts\build-installer.ps1 -SelfContained
```

杈撳嚭鏂囦欢锛?
```text
publish\installer\PicShow-v1.1.1-setup-win-x64.exe
```

### 鏂囦欢鍏宠仈璇存槑

瀹夎鍖呬細鎶?PicShow 娉ㄥ唽鍒板彈鏀寔鍥剧墖鏍煎紡鐨勨€滄墦寮€鏂瑰紡鈥濆拰鈥滈粯璁ゅ簲鐢ㄢ€濆垪琛ㄤ腑銆傜幇浠?Windows 浼氫繚鎶ら粯璁ゅ簲鐢ㄨ缃紝鍥犳瀹夎绋嬪簭涓嶈兘鍙潬鍦板己鍒舵妸 PicShow 璁句负榛樿鍥剧墖鏌ョ湅鍣紝閫氬父浠嶉渶瑕佺敤鎴风‘璁ゃ€?
灏?PicShow 璁剧疆涓洪粯璁ゅ浘鐗囨煡鐪嬪櫒鍚庯紝鍙屽嚮鍥剧墖浼氫互绫讳技鏂瑰紡鍚姩锛?
```powershell
PicShow.exe "C:\Path\To\Image.png"
```

PicShow 宸叉敮鎸佹鍚姩鍙傛暟锛屼細鐩存帴鎵撳紑瀵瑰簲鍥剧墖銆?
### 浣滆€?
杞欢浣滆€咃細澹逛即澹逛即

娆㈣繋閫氳繃鎶栭煶鑱旂郴銆?
### 璁稿彲璇?
鏈」鐩紑婧愬厤璐癸紝娆㈣繋浼犳挱鍏变韩銆?
---

## English Version

PicShow is a lightweight Windows image viewer for Windows 10 and later, focused on a clean, fast, distraction-free viewing experience.

It is free and open source. The reason for building it is simple: pop-up ads in image viewers are annoying.

### Features

- Borderless image preview window with adaptive sizing.
- Mouse wheel zoom, drag-to-pan when the image is larger than the view.
- Use the Up arrow for the previous image and the Down arrow for the next image.
- Press Esc to exit PicShow.
- Transparent-background display for PNG and other alpha-capable images.
- Startup welcome page with drag-and-drop and `Ctrl+O` open flow.
- Double-click image opening after registering PicShow as an image viewer.
- Windows installer with Simplified Chinese setup UI.

### Current Format Support

Currently implemented:

- JPG / JPEG
- PNG
- BMP
- TIFF / TIF
- ICO
- WebP

Planned:

- Animated GIF playback
- SVG preview
- PSD composite preview
- PDF preview

### Download

Download the latest release from GitHub:

[PicShow v1.1.1 Release](https://github.com/yibanyiban78/PicShow/releases/tag/v1.1.1)

Recommended download:

- `PicShow-v1.1.1-setup-win-x64.exe`: self-contained Windows installer, no extra .NET runtime required.

Portable option:

- `PicShow-v1.1.1-win-x64.zip`: portable executable package.

### Requirements

For the self-contained installer:

- Windows 10 or later

For development:

- Windows 10 or later
- .NET 8 SDK
- Inno Setup 6, only needed for building the installer

### Build From Source

Restore and build:

```powershell
dotnet restore
dotnet build
```

Run the app:

```powershell
dotnet run --project src/PicShow.App
```

Publish a single-file executable:

```powershell
dotnet publish .\src\PicShow.App\PicShow.App.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish\win-x64
```

### Build Installer

Install Inno Setup 6 first:

[https://jrsoftware.org/isdl.php](https://jrsoftware.org/isdl.php)

Build the normal installer:

```powershell
.\scripts\build-installer.ps1
```

Build the self-contained installer:

```powershell
.\scripts\build-installer.ps1 -SelfContained
```

Output:

```text
publish\installer\PicShow-v1.1.1-setup-win-x64.exe
```

### File Association Notes

The installer registers PicShow in the Windows "Open with" and default-apps list for supported image formats. Modern Windows protects default app choices, so installers cannot reliably force PicShow as the default image viewer without user confirmation.

After setting PicShow as the default image viewer, double-clicking an image launches:

```powershell
PicShow.exe "C:\Path\To\Image.png"
```

PicShow supports this startup path and opens the image directly.

### Author

Software author: 澹逛即澹逛即

Welcome to contact me on Douyin.

### License

This project is open source and free to share.

