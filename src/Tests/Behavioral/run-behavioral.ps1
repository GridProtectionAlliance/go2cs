<#
.SYNOPSIS
    Build and run the standalone go2cs behavioral runner (Tier 3) -- no MSTest, no testhost.

.DESCRIPTION
    The runner (BehavioralRunner) executes the same four phases as the MSTest suite
    (Transpile -> Compile -> TargetComparison -> OutputComparison) as a plain console app that is not
    hosted in testhost.exe, so it cannot wedge on the testhost/MSB3027 self-lock. It also collapses the
    180 per-project "dotnet build" calls into one parallel MSBuild invocation. This wrapper just builds
    the runner (fast, dependency-free) and forwards all arguments to it.

.PARAMETER RunnerArgs
    Forwarded verbatim to BehavioralRunner. Examples:
      --filter <substr>     only matching projects
      --phase <list>        transpile,compile,target,output,all
      --update-targets      regenerate .cs.target goldens, then stop
      --list                list matched projects

.EXAMPLE
    ./run-behavioral.ps1
    ./run-behavioral.ps1 --filter Atomic
    ./run-behavioral.ps1 --phase transpile,target
#>
[CmdletBinding()]
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $RunnerArgs
)

$ErrorActionPreference = "Stop"

$runnerProj = Join-Path $PSScriptRoot "BehavioralRunner\BehavioralRunner.csproj"
$runnerExe  = Join-Path $PSScriptRoot "BehavioralRunner\bin\Debug\net9.0\BehavioralRunner.exe"

Write-Host "==> building BehavioralRunner..." -ForegroundColor Cyan
& dotnet build $runnerProj -c Debug -clp:ErrorsOnly --nologo | Out-Null
if ($LASTEXITCODE -ne 0) { throw "BehavioralRunner build failed ($LASTEXITCODE)" }

& $runnerExe @RunnerArgs
exit $LASTEXITCODE
