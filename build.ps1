param(
    [switch]$SelfContained
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $root "src\CodexLauncher.csproj"
$dist = Join-Path $root "dist"

New-Item -ItemType Directory -Force -Path $dist | Out-Null

$publishArgs = @(
    "publish",
    $project,
    "-c", "Release",
    "-r", "win-x64",
    "-p:PublishSingleFile=true",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-o", $dist
)

if ($SelfContained) {
    $publishArgs += "--self-contained"
    $publishArgs += "true"
    $publishArgs += "-p:EnableCompressionInSingleFile=true"
}
else {
    $publishArgs += "--self-contained"
    $publishArgs += "false"
}

dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$sourceExe = Join-Path $dist "CodexProxySwitcher.exe"
if (-not (Test-Path -LiteralPath $sourceExe)) {
    $sourceExe = Join-Path $dist "CodexLauncher.exe"
}

$targetExe = Join-Path $dist "Codex-Proxy-Switcher.exe"
if (Test-Path -LiteralPath $sourceExe) {
    Copy-Item -LiteralPath $sourceExe -Destination $targetExe -Force
}

Write-Host "Build completed: $targetExe"
