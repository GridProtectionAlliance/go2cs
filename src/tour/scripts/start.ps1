[CmdletBinding()]
param(
    [string]$ListenAddress = "127.0.0.1:4000",
    [ValidateSet("core", "deployed", "nuget")]
    [string]$Runtime = "core",
    [switch]$NoOpen
)

$ErrorActionPreference = "Stop"
$tourRoot = Split-Path -Parent $PSScriptRoot
$repoRoot = (Resolve-Path (Join-Path $tourRoot "..\..")).Path

Write-Host "Checking Go..."
& go version

Write-Host "Checking .NET..."
& dotnet --version

$goPath = (& go env GOPATH).Trim()
$tourBinary = Join-Path $goPath "bin\tour.exe"
if (-not (Test-Path -LiteralPath $tourBinary)) {
    Write-Host "Installing the official offline Tour of Go..."
    & go install golang.org/x/website/tour@latest
}

$env:GO_TOUR_BIN = $tourBinary

Push-Location $tourRoot
try {
    $arguments = @("run", ".", "-addr=$ListenAddress", "-repo=$repoRoot", "-runtime=$Runtime")
    if ($NoOpen) {
        $arguments += "-no-open"
    }
    & go @arguments
}
finally {
    Pop-Location
}
