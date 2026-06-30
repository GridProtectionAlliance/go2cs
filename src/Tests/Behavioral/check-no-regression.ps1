<#
.SYNOPSIS
    Fast converter no-regression check: re-transpile every behavioral project and report any change
    to the generated C#. No compile, no run, no testhost.

.DESCRIPTION
    Per CLAUDE.md: byte-identical generated .cs  =>  identical compile+run  =>  identical results.
    So after a converter change, the cheapest regression signal is simply to re-transpile every
    Tests\Behavioral\* project and `git status` the generated .cs files. If nothing changed, the
    converter change is provably output-neutral for the behavioral corpus with zero build/run cost.
    If files changed, this prints them so you can inspect (intended new goldens) vs. revert (regression).

    This regenerates the .cs in-place. That IS the check: identical output leaves the tree clean.

.PARAMETER Revert
    After reporting, `git checkout` any changed .cs back to HEAD (use when you only wanted the check,
    not to keep regenerated output).

.EXAMPLE
    ./check-no-regression.ps1
    ./check-no-regression.ps1 -Revert
#>
[CmdletBinding()]
param(
    [switch] $Revert
)

$ErrorActionPreference = "Stop"

$repoRoot     = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path
$converterSrc = Join-Path $repoRoot "src\go2cs"
$go2csExe     = Join-Path $converterSrc "bin\go2cs.exe"
$behavioral   = $PSScriptRoot

# 1. Build a current go2cs.exe (cheap; only relinks if the Go sources changed).
Write-Host "==> go build -o bin\go2cs.exe" -ForegroundColor Cyan
Push-Location $converterSrc
try {
    & go build -o $go2csExe
    if ($LASTEXITCODE -ne 0) { throw "go build failed ($LASTEXITCODE)" }
}
finally { Pop-Location }

# 2. Re-transpile every behavioral project (a project dir is one that has a .csproj, excluding the
#    BehavioralTests runner itself).
$projects = Get-ChildItem -Path $behavioral -Directory |
    Where-Object { $_.Name -ne "BehavioralTests" -and (Get-ChildItem $_.FullName -Filter *.csproj -File) }

Write-Host "==> transpiling $($projects.Count) behavioral projects..." -ForegroundColor Cyan
foreach ($proj in $projects) {
    & $go2csExe $proj.FullName | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Host "    [transpile FAILED] $($proj.Name)" -ForegroundColor Red }
}

# 3. Report any changed generated .cs under the behavioral tree.
$changed = & git -C $repoRoot status --short -- "src/Tests/Behavioral/*.cs" |
    Where-Object { $_ -notmatch "BehavioralTests/" }

if (-not $changed) {
    Write-Host "==> NO REGRESSION: generated C# is byte-identical across all behavioral projects." -ForegroundColor Green
    exit 0
}

Write-Host "==> CHANGED generated C# (inspect: intended new golden vs. regression):" -ForegroundColor Yellow
$changed | ForEach-Object { Write-Host "    $_" }

if ($Revert) {
    Write-Host "==> -Revert: restoring changed .cs to HEAD" -ForegroundColor Cyan
    & git -C $repoRoot checkout -- "src/Tests/Behavioral/*.cs"
}

exit 1
