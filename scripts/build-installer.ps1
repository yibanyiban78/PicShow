param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SelfContained
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src\PicShow.App\PicShow.App.csproj"
$publishDir = Join-Path $repoRoot "publish\$Runtime"
$installerScript = Join-Path $repoRoot "installer\PicShow.iss"

$dotnet = Join-Path $env:ProgramFiles "dotnet\dotnet.exe"
if (-not (Test-Path $dotnet)) {
    $dotnet = "dotnet"
}

Write-Host "Publishing PicShow..."
$publishArgs = @(
    "publish",
    $project,
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained:$($SelfContained.IsPresent.ToString().ToLowerInvariant())",
    "-p:PublishSingleFile=true",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-o", $publishDir
)

if ($SelfContained) {
    $publishArgs += "-p:EnableCompressionInSingleFile=true"
}

& $dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$isccCommand = Get-Command iscc.exe -ErrorAction SilentlyContinue
$isccPath = $null
if ($isccCommand) {
    $isccPath = $isccCommand.Source
}
if (-not $isccPath) {
    $defaultPaths = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    )

    foreach ($path in $defaultPaths) {
        if (Test-Path $path) {
            $isccPath = $path
            break
        }
    }
}

if (-not $isccPath) {
    throw "ISCC.exe was not found. Please install Inno Setup 6, then run this script again."
}

Write-Host "Building installer..."
& $isccPath $installerScript
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup compiler failed with exit code $LASTEXITCODE."
}

Write-Host "Done."
Write-Host (Join-Path $repoRoot "publish\installer\PicShow-v1.1.1-setup-win-x64.exe")
