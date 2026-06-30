<#
.SYNOPSIS
    Clean-slate runner for the go2cs behavioral test suite.

.DESCRIPTION
    The behavioral suite shells out to dotnet build / go build / the transpiled .exe from inside an
    MSTest assembly hosted in testhost.exe. A stale testhost / vstest.console / MSBuild worker node
    left resident by a prior run (or by Visual Studio) can hold a file lock on BehavioralTests.dll
    or on a target's bin\obj, making the *next* build fail with MSB3027 or hang indefinitely.

    This script clears those stale lock-holders BEFORE the build (the only place that can be done,
    since the lock manifests at build time, before testhost loads), then runs the suite with
    --blame-hang so a wedged testhost is force-killed and reported instead of left resident.

.PARAMETER Filter
    Optional VSTest filter expression, e.g. "FullyQualifiedName~AtomicField". Omit to run all tests.

.PARAMETER NoBuild
    Pass --no-build to dotnet test (valid only when no *Tests.cs changed since the last build).

.PARAMETER HangTimeout
    Per-test hang timeout handed to --blame-hang-timeout. Default 180s (matches the ~3 min cap).

.EXAMPLE
    ./run-behavioral-tests.ps1
    ./run-behavioral-tests.ps1 -Filter "FullyQualifiedName~AtomicField" -NoBuild
#>
[CmdletBinding()]
param(
    [string] $Filter,
    [switch] $NoBuild,
    [string] $HangTimeout = "180s"
)

$ErrorActionPreference = "Stop"

# Repo root is three levels up from src\Tests\Behavioral.
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path
$slnx     = Join-Path $repoRoot "src\go2cs.slnx"

Write-Host "==> Clearing stale test/build processes (prior-run lock-holders)..." -ForegroundColor Cyan

# Stale testhost/vstest from a PRIOR run hold the lock that breaks THIS build. Safe to kill here:
# this script runs in a normal shell, not inside testhost, so we are not killing ourselves.
foreach ($name in @("testhost", "vstest.console", "MSBuild")) {
    $procs = Get-Process -Name $name -ErrorAction SilentlyContinue
    if ($procs) {
        Write-Host "    killing $($procs.Count) x $name" -ForegroundColor DarkYellow
        $procs | Stop-Process -Force -ErrorAction SilentlyContinue
    }
}

# Release MSBuild build-server nodes so they stop holding bin\obj handles.
Write-Host "==> dotnet build-server shutdown" -ForegroundColor Cyan
& dotnet build-server shutdown | Out-Null

# Assemble the dotnet test invocation.
$testArgs = @(
    "test", $slnx,
    "-c", "Debug",
    "--blame-hang",
    "--blame-hang-timeout", $HangTimeout,
    "--blame-hang-dump-type", "none"
)

if ($NoBuild)              { $testArgs += "--no-build" }
if ($Filter)              { $testArgs += @("--filter", $Filter) }

Write-Host "==> dotnet $($testArgs -join ' ')" -ForegroundColor Cyan
& dotnet @testArgs
$exit = $LASTEXITCODE

# Tear down any nodes our run spawned so the next invocation starts clean.
& dotnet build-server shutdown | Out-Null

exit $exit
