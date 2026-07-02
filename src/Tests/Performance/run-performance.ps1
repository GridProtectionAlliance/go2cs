<#
.SYNOPSIS
    Build and run the standalone go2cs performance comparison runner.

.DESCRIPTION
    The runner (PerformanceRunner) builds each benchmark project three ways -- the original Go binary,
    the transpiled C# on the normal JIT runtime, and the transpiled C# as a Native AOT self-contained
    executable -- verifies all three produce identical output, then measures workload time (in-program,
    excludes startup), process wall time, and peak working set, and prints a markdown report.
    This wrapper just builds the runner (fast, dependency-free) and forwards all arguments to it.

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
exit $LASTEXITCODE
