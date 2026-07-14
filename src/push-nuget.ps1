<#
.SYNOPSIS
    Packs the go2cs converted Go standard library (plus the go.lib runtime and go.gen source
    generator) as NuGet packages from a fresh Release build, and optionally pushes them to a feed.

.DESCRIPTION
    Targets ONLY src\go-src-converted.slnx -- the full-conversion solution, which contains the ~301
    converted stdlib libraries AND the two shared infrastructure projects core\golib (go.lib) and
    gen\go2cs-gen (go.gen). It deliberately never packs src\go2cs.slnx or the src\core baseline stub
    packages: those reuse the same go.<pkg> PackageIds (e.g. go.fmt) and would collide on the feed.

    All packages share one version, sourced from src\version.props: <GoStdLibVersion>.<GoBuildNumber>
    (base tracks the converted Go release, e.g. 1.23.1; the 4th part is the build/publish counter).
    -Push increments GoBuildNumber by default so every publish is a new version (commit version.props
    afterward to record the release). See -BumpBuild to force or suppress the bump.

    SAFETY: pushing to a public feed is an irreversible publish (a version can be unlisted, never
    deleted). This script therefore PACKS ONLY by default; it pushes nothing unless -Push is given,
    and -WhatIf reports each push without performing it. The API key is read from the NUGET_API_KEY
    environment variable (or -ApiKey) and is never written to disk.

.PARAMETER ApiKey
    NuGet API key for -Push. Defaults to the NUGET_API_KEY environment variable.

.PARAMETER Source
    NuGet push source. Defaults to https://api.nuget.org/v3/index.json. May be a local folder feed.

.PARAMETER Configuration
    Build configuration to pack. Defaults to Release.

.PARAMETER OutDir
    Directory to collect the .nupkg files. Defaults to src\artifacts\nupkg.

.PARAMETER SkipBuild
    Skip the Release build and pack the output already on disk (e.g. a build from Visual Studio).
    By default the script does a fresh Release build first and aborts if it fails.

.PARAMETER BumpBuild
    Force or suppress the GoBuildNumber increment in src\version.props (then commit it). Defaults to
    ON with -Push (each publish is a new version) and OFF for a pack-only run. Pass -BumpBuild:$false
    to publish the CURRENT version without bumping -- e.g. finishing a partially-failed push (with
    --skip-duplicate) or serial automation that manages the version itself.

.PARAMETER Push
    Actually push the packed .nupkg to the feed. Without this switch the script only packs.

.EXAMPLE
    .\push-nuget.ps1
    Pack every package to src\artifacts\nupkg (no push, no bump). Inspect the output, then push.

.EXAMPLE
    .\push-nuget.ps1 -Push
    The normal release: bump the build number, build + pack Release, and push the NEXT version to
    nuget.org (NUGET_API_KEY set).

.EXAMPLE
    .\push-nuget.ps1 -Push -BumpBuild:$false
    Re-push the CURRENT version without bumping -- e.g. to finish a partially-failed publish
    (--skip-duplicate skips the packages already on the feed).
#>
#Requires -Version 5.1
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$ApiKey = $env:NUGET_API_KEY,
    [string]$Source = 'https://api.nuget.org/v3/index.json',
    [string]$Configuration = 'Release',
    [string]$OutDir,
    [switch]$SkipBuild,
    [switch]$BumpBuild,
    [switch]$Push
)

$ErrorActionPreference = 'Stop'

$src = $PSScriptRoot
$slnx = Join-Path $src 'go-src-converted.slnx'
$versionProps = Join-Path $src 'version.props'
if (-not $OutDir) { $OutDir = Join-Path $src 'artifacts\nupkg' }

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

if (-not (Test-Path $slnx)) { throw "Solution not found: $slnx" }
if (-not (Test-Path $versionProps)) { throw "version.props not found: $versionProps" }

# --- Build-number bump (raw-text edit preserves comments/formatting) -----------------------------
# Each publish should be a NEW version, so the build number is bumped by default when -Push is given
# and left alone for a pack-only run. An explicit -BumpBuild / -BumpBuild:$false always wins -- use
# -BumpBuild:$false to re-push the CURRENT version (e.g. finishing a partially-failed push).
if ($PSBoundParameters.ContainsKey('BumpBuild')) { $doBump = [bool]$BumpBuild } else { $doBump = [bool]$Push }

$propsText = [System.IO.File]::ReadAllText($versionProps)
if ($propsText -notmatch '<GoBuildNumber>(\d+)</GoBuildNumber>') { throw "GoBuildNumber not found in $versionProps" }
$build = [int]$Matches[1]

if ($doBump) {
    $newBuild = $build + 1
    if ($PSCmdlet.ShouldProcess($versionProps, "bump GoBuildNumber $build -> $newBuild")) {
        $propsText = $propsText -replace '<GoBuildNumber>\d+</GoBuildNumber>', "<GoBuildNumber>$newBuild</GoBuildNumber>"
        [System.IO.File]::WriteAllText($versionProps, $propsText, $utf8NoBom)
        Write-Step "Bumped GoBuildNumber $build -> $newBuild (commit version.props to record the release)"
        $build = $newBuild
    }
}

if ($propsText -match '<GoStdLibVersion>([^<]+)</GoStdLibVersion>') { $baseVersion = $Matches[1] } else { $baseVersion = '?' }
$fullVersion = "$baseVersion.$build"
Write-Step "Package version: $fullVersion   (solution: go-src-converted.slnx)"

# --- Fresh Release pack -------------------------------------------------------------------------
New-Item -ItemType Directory -Force $OutDir | Out-Null
Get-ChildItem $OutDir -Filter *.nupkg -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue

# Fresh Release build first (default) so a compile error fails the run BEFORE anything is packed or
# pushed. -SkipBuild packs whatever is already built on disk (e.g. a Release build from Visual Studio).
# GeneratePackageOnBuild=false keeps the build from also scattering .nupkg into every project's bin\.
if ($SkipBuild) {
    Write-Step "Skipping build (-SkipBuild): packing existing $Configuration output"
} else {
    Write-Step "Building $Configuration (compiles the whole stdlib; several minutes)"
    & dotnet build $slnx -c $Configuration -p:GeneratePackageOnBuild=false --nologo -v m
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed ($LASTEXITCODE) -- fix build errors before packing" }
}

# Pack from the built output. --no-build honors -SkipBuild and avoids a redundant second build;
# GeneratePackageOnBuild=false leaves packaging to 'dotnet pack' -> $OutDir only (mirrors deploy-core.ps1).
Write-Step "Packing $Configuration -> $OutDir"
& dotnet pack $slnx -c $Configuration -o $OutDir -p:GeneratePackageOnBuild=false --no-build --nologo -v m
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed ($LASTEXITCODE)" }

$pkgs = @(Get-ChildItem $OutDir -Filter *.nupkg)
Write-Step "Packed $($pkgs.Count) package(s)"
if ($pkgs.Count -eq 0) { throw "No .nupkg produced in $OutDir" }

# --- Push gate ----------------------------------------------------------------------------------
if (-not $Push) {
    Write-Host ""
    Write-Host "Pack-only (default). Inspect $OutDir, then re-run with -Push to publish." -ForegroundColor Yellow
    exit 0
}

if (-not $ApiKey) { throw "-Push requires an API key: pass -ApiKey or set `$env:NUGET_API_KEY." }

# Publish go.lib and go.gen first (dependencies of every stdlib package). --skip-duplicate makes a
# re-run idempotent; nuget.org indexes asynchronously so strict ordering is a nicety, not required.
$deps = @($pkgs | Where-Object { $_.Name -match '^go\.(lib|gen)\.' })
$rest = @($pkgs | Where-Object { $_.Name -notmatch '^go\.(lib|gen)\.' })
$ordered = $deps + $rest

Write-Step "Pushing $($ordered.Count) package(s) to $Source"
$pushed = 0
foreach ($p in $ordered) {
    if ($PSCmdlet.ShouldProcess($p.Name, "nuget push -> $Source")) {
        & dotnet nuget push $p.FullName --source $Source --api-key $ApiKey --skip-duplicate
        if ($LASTEXITCODE -ne 0) { throw "push failed for $($p.Name) ($LASTEXITCODE)" }
        $pushed++
    }
}
Write-Step "Done. Pushed $pushed package(s) at version $fullVersion."
