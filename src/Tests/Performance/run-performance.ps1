<#
.SYNOPSIS
    Build and run the standalone go2cs performance comparison runner.

.DESCRIPTION
    The runner (PerformanceRunner) builds each benchmark project three ways -- the original Go binary,
    the transpiled C# on the normal JIT runtime, and the transpiled C# as a Native AOT self-contained
    executable -- verifies all three produce identical output, then measures workload time (in-program,
    excludes startup), process wall time, and peak working set, and prints a markdown report.
    This wrapper builds the runner (fast, dependency-free), forwards all arguments to it, and on
    success mirrors README.md to docs/Performance.md so the GitHub Pages site (which only publishes
    the docs folder) can link the performance comparison. README.md here is the master copy.

    A full run including the Native AOT publishes takes several minutes; pass --no-aot while iterating.

.PARAMETER RunnerArgs
    Forwarded verbatim to PerformanceRunner. Examples:
      --filter <substr>     only matching projects
      --phase <list>        transpile,build,verify,measure,all
      --runs <n>            measured runs per variant (default 5)
      --no-aot              skip the Native AOT column (much faster)
      --update-readme       rewrite the results block in README.md
      --list                list matched projects

.EXAMPLE
    ./run-performance.ps1
    ./run-performance.ps1 --filter Fib --no-aot
    ./run-performance.ps1 --runs 10 --update-readme
#>
[CmdletBinding()]
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $RunnerArgs
)

$ErrorActionPreference = "Stop"

$runnerProj = Join-Path $PSScriptRoot "PerformanceRunner\PerformanceRunner.csproj"
$runnerExe  = Join-Path $PSScriptRoot "PerformanceRunner\bin\Debug\net9.0\PerformanceRunner.exe"

Write-Host "==> building PerformanceRunner..." -ForegroundColor Cyan
& dotnet build $runnerProj -c Debug -clp:ErrorsOnly --nologo | Out-Null
if ($LASTEXITCODE -ne 0) { throw "PerformanceRunner build failed ($LASTEXITCODE)" }

& $runnerExe @RunnerArgs
$runnerExit = $LASTEXITCODE

# Mirror the README into docs/ so it is reachable from the GitHub Pages site, which only
# publishes the docs folder. This README.md is the master; docs/Performance.md is the copy.
if ($runnerExit -eq 0) {
    $readme   = Join-Path $PSScriptRoot "README.md"
    $docsCopy = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..\..\docs")) "Performance.md"
    $banner   = "<!-- AUTO-COPIED from src/Tests/Performance/README.md by run-performance.ps1 -- edit that file, not this one. -->`r`n`r`n"
    [IO.File]::WriteAllText($docsCopy, $banner + [IO.File]::ReadAllText($readme))
    Write-Host "==> mirrored README.md to docs/Performance.md" -ForegroundColor Cyan
}
exit $runnerExit
