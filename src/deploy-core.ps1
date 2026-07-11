<#
.SYNOPSIS
    Deploys the go2cs core libraries to the standard root %GOPATH%\src\go2cs so that
    converted projects (and, later, recursively converted end-user apps that target the
    same root) can lean on relative $(go2csPath) project references.

.DESCRIPTION
    Two modes, deploying to the SAME root (they are alternatives, never mixed):

      stub    Deploy the hand-finished baseline core (src\core) -- a small, *runnable*
              standard-library subset. Best for simple conversions / behavioral-style apps.

      stdlib  Deploy the full auto-converted standard library (src\go-src-converted) -- the
              *compilable* 302-package tree. Best for real apps that import arbitrary stdlib
              packages.

    Both modes land the stdlib at <root>\core\<pkg>, the shared runtime at <root>\core\golib,
    and the source-generator/analyzer at <root>\gen\go2cs-gen, then write a Directory.Build.props
    at <root> pinning $(go2csPath) to that root. Because every generated .csproj references its
    dependencies as $(go2csPath)core\<pkg> / $(go2csPath)gen\go2cs-gen, that single props file
    makes the whole deployed tree -- and anything a recursive conversion later drops under the
    same root -- resolve without any per-build -p:go2csPath flag.

    The full stdlib's inter-package references are written as $(go2csPath)go-src-converted\<pkg>
    (a historical relocation); stdlib mode rewrites them to $(go2csPath)core\<pkg> on deploy so
    both modes present the standard library uniformly at core\<pkg>.

.PARAMETER Mode
    'stub' or 'stdlib' (see above).

.PARAMETER Target
    Deploy root. Defaults to <GOPATH>\src\go2cs (GOPATH from `go env GOPATH`).

.PARAMETER Configuration
    Build configuration used by the verify build. Defaults to Debug.

.PARAMETER NoBuild
    Copy + wire up only; skip the verify build.

.EXAMPLE
    .\deploy-core.ps1 stub
.EXAMPLE
    .\deploy-core.ps1 stdlib
#>
#Requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet('stub', 'stdlib')]
    [string]$Mode,

    [string]$Target,
    [string]$Configuration = 'Debug',
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'

# Repo src\ directory (this script's home) -- all sources are resolved from here, so the
# script works regardless of the caller's current directory.
$srcRoot = $PSScriptRoot

function Invoke-Robocopy {
    param(
        [Parameter(Mandatory)] [string]$From,
        [Parameter(Mandatory)] [string]$To,
        [string[]]$ExtraArgs = @()
    )

    # /E copy subdirs incl. empty; quiet flags; retry once. Build artifacts are excluded so
    # only source is deployed. robocopy returns a bitmask: < 8 is success (files copied /
    # extra / mismatch), >= 8 is a real failure.
    $args = @($From, $To, '/E', '/NFL', '/NDL', '/NJH', '/NJS', '/NP', '/R:1', '/W:1',
        '/XD', 'bin', 'obj', 'Generated', '.vs') + $ExtraArgs
    & robocopy @args | Out-Null

    if ($LASTEXITCODE -ge 8) {
        throw "robocopy failed ($LASTEXITCODE) copying `"$From`" -> `"$To`""
    }

    # robocopy uses a success BITMASK (1 = files copied, 2 = extras, 3 = both, ... all < 8 are
    # success). Reset $LASTEXITCODE so those non-zero success codes do not leak out as the
    # script's own exit code (e.g. on the -NoBuild path, where no later command resets it).
    $global:LASTEXITCODE = 0
}

# ---- Resolve the deploy root ------------------------------------------------------------
if (-not $Target) {
    $goPath = (& go env GOPATH 2>$null)
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($goPath)) {
        throw "Could not resolve GOPATH (is Go on PATH?). Pass -Target explicitly."
    }
    $Target = Join-Path $goPath.Trim() 'src\go2cs'
}

# Normalize to a full path with no trailing separator (used for relative-path math below).
[System.IO.Directory]::CreateDirectory($Target) | Out-Null
$Target = [System.IO.Path]::GetFullPath($Target).TrimEnd('\')

$coreDst = Join-Path $Target 'core'
$genRoot = Join-Path $Target 'gen'
$genDst  = Join-Path $genRoot 'go2cs-gen'

Write-Host "deploy-core: mode=$Mode -> $Target" -ForegroundColor Cyan

# ---- Clean the pieces this script owns (leave sibling app dirs untouched) ----------------
foreach ($dir in @($coreDst, $genRoot)) {
    if (Test-Path $dir) {
        Write-Host "  cleaning $dir" -ForegroundColor DarkGray
        Remove-Item -Recurse -Force $dir
    }
}

# ---- Deploy the analyzer (both modes) ----------------------------------------------------
Write-Host "  deploying analyzer  -> gen\go2cs-gen" -ForegroundColor Yellow
Invoke-Robocopy (Join-Path $srcRoot 'gen\go2cs-gen') $genDst

# ---- Deploy the core per mode ------------------------------------------------------------
if ($Mode -eq 'stub') {
    Write-Host "  deploying baseline stub -> core" -ForegroundColor Yellow
    Invoke-Robocopy (Join-Path $srcRoot 'core') $coreDst
}
else {
    Write-Host "  deploying full stdlib   -> core" -ForegroundColor Yellow
    # Exclude go-src-converted's own Directory.Build.props: the root props written below is
    # the authoritative one; a nested copy would shadow it for everything under core\.
    Invoke-Robocopy (Join-Path $srcRoot 'go-src-converted') $coreDst @('/XF', 'Directory.Build.props')

    # go-src-converted references the shared runtime + shared project it does not itself carry.
    # Overlay ONLY those (golib, the go2cs shared projitems dir, and the core-root icons) -- NOT
    # the baseline package dirs, which would shadow the full-stdlib packages of the same name.
    Write-Host "  overlaying golib + shared project + icons" -ForegroundColor Yellow
    Invoke-Robocopy (Join-Path $srcRoot 'core\golib') (Join-Path $coreDst 'golib')
    Invoke-Robocopy (Join-Path $srcRoot 'core\go2cs') (Join-Path $coreDst 'go2cs')
    Copy-Item (Join-Path $srcRoot 'core\go2cs.ico') (Join-Path $coreDst 'go2cs.ico') -Force
    Copy-Item (Join-Path $srcRoot 'core\go2cs.png') (Join-Path $coreDst 'go2cs.png') -Force

    # Rewrite inter-package references go-src-converted\ -> core\ so the deployed stdlib presents
    # uniformly at core\<pkg> (matching the stub layout and what fresh conversions reference).
    Write-Host "  rewriting go-src-converted\ -> core\ references" -ForegroundColor Yellow
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $rewritten = 0
    Get-ChildItem -Path $coreDst -Recurse -Filter *.csproj | ForEach-Object {
        $text = [System.IO.File]::ReadAllText($_.FullName)
        if ($text.Contains('$(go2csPath)go-src-converted\')) {
            $text = $text.Replace('$(go2csPath)go-src-converted\', '$(go2csPath)core\')
            [System.IO.File]::WriteAllText($_.FullName, $text, $utf8NoBom)
            $rewritten++
        }
    }
    Write-Host "    rewrote $rewritten project file(s)" -ForegroundColor DarkGray
}

# ---- Write the root Directory.Build.props ------------------------------------------------
# Pins $(go2csPath) to this deploy root for every project beneath it, so all
# $(go2csPath)core\... / $(go2csPath)gen\... references resolve with no per-build flag.
$propsContent = @'
<Project>

  <!-- Written by deploy-core.ps1. Pins $(go2csPath) to this deploy root so every project
       below it (the converted standard library, golib, the go2cs-gen analyzer, and any
       recursively converted end-user app placed under this root) resolves its
       $(go2csPath)core\... and $(go2csPath)gen\... ProjectReferences without needing a
       -p:go2csPath flag on the build. MSBuildThisFileDirectory ends with a separator. -->
  <PropertyGroup>
    <go2csPath>$(MSBuildThisFileDirectory)</go2csPath>
  </PropertyGroup>

</Project>
'@
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText((Join-Path $Target 'Directory.Build.props'), $propsContent, $utf8NoBom)

# ---- Generate a solution over everything deployed ----------------------------------------
$projectLines = Get-ChildItem -Path $Target -Recurse -Filter *.csproj | ForEach-Object {
    $rel = $_.FullName.Substring($Target.Length).TrimStart('\', '/').Replace('\', '/')
    "  <Project Path=`"$rel`" />"
}
$slnx = @("<Solution>",
    "  <Configurations>",
    "    <Platform Name=`"Any CPU`" />",
    "    <Platform Name=`"x64`" />",
    "    <Platform Name=`"x86`" />",
    "  </Configurations>") + $projectLines + @("</Solution>")
$slnxText = ($slnx -join "`r`n") + "`r`n"
$slnxPath = Join-Path $Target 'go2cs-core.slnx'
[System.IO.File]::WriteAllText($slnxPath, $slnxText, $utf8NoBom)

Write-Host "  deployed $($projectLines.Count) project(s); solution: $slnxPath" -ForegroundColor Green

# ---- Verify build ------------------------------------------------------------------------
if ($NoBuild) {
    Write-Host "deploy-core: copy complete (-NoBuild); skipped verify build." -ForegroundColor Cyan
    exit 0
}

Write-Host "Building $slnxPath ($Configuration)..." -ForegroundColor Cyan
# GeneratePackageOnBuild is on per-project (library packaging) -- suppress it for the verify
# build so we do not emit hundreds of .nupkg files just to confirm the tree compiles.
& dotnet build $slnxPath -c $Configuration -p:GeneratePackageOnBuild=false -v m
if ($LASTEXITCODE -ne 0) {
    throw "Verify build FAILED ($LASTEXITCODE)."
}

$state = if ($Mode -eq 'stub') { 'runnable' } else { 'compilable' }
Write-Host "deploy-core: $Mode deployment verified ($state) at $Target" -ForegroundColor Green
exit 0
