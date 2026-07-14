<#
.SYNOPSIS
    Solution-integrity guard: assert the set of behavioral test .csproj files on disk is exactly the
    set registered in go2cs.slnx. Catches a test (or sibling library sub-project) that was committed
    but never added to the solution.

.DESCRIPTION
    A behavioral test can pull in a sibling library project via <ProjectReference> -- e.g.
    GoNamespaceShadow references nsshadowlib\go.nsshadow.csproj. The BehavioralRunner / MSTest harness
    builds each test .csproj *by path* with $(SolutionDir) set, so a transitive ProjectReference
    resolves and the suite stays green EVEN WHEN the referenced project is missing from go2cs.slnx.
    The gap only bites when go2cs.slnx is opened/built in Visual Studio: the unregistered project is
    not a solution member, so VS builds it without the Debug+SolutionDir context and its $(go2csPath)
    references (core\golib, core\math, gen\go2cs-gen) fail to resolve (CS0246/CS0234). That is exactly
    how nsshadow slipped through (added in 96eff53cd, never registered until 53dd2497e).

    The invariant that prevents this: every .csproj physically under Tests\Behavioral is registered as
    a <Project Path="..."> in go2cs.slnx, and vice-versa. This script checks that equality both ways:
      - on disk but NOT in the solution  =>  forgot to register it (the build-breaking gap)
      - in the solution but NOT on disk  =>  dangling registration (renamed / deleted project)

    Pure static analysis over the file tree and the .slnx text: no build, no transpile, no run (<1s).
    Runs automatically as the preflight step of check-no-regression.ps1; also runnable standalone.

.EXAMPLE
    ./check-solution-integrity.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$behavioral = $PSScriptRoot
$srcRoot    = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$slnxPath   = (Resolve-Path (Join-Path $srcRoot "go2cs.slnx")).Path

# 1. Every .csproj physically under Tests\Behavioral. -Recurse descends into sibling library folders
#    (nsshadowlib\, netlike\, inner\, FsLike\, ...). Rendered as a src-relative forward-slash path so
#    it lines up with the go2cs.slnx <Project Path="..."> form. Includes the two tooling projects
#    (BehavioralTests, BehavioralRunner) -- they are legitimate solution members, so they appear on
#    both sides and cancel.
$onDisk = Get-ChildItem -Path $behavioral -Recurse -Filter *.csproj -File |
    ForEach-Object { $_.FullName.Substring($srcRoot.Length + 1).Replace('\', '/') } |
    Sort-Object

# 2. Every Tests/Behavioral/*.csproj registered in go2cs.slnx. Folder elements use Name="..." (not
#    Path=), so this Path-anchored pattern matches only <Project> entries.
$slnxText   = Get-Content -Raw -LiteralPath $slnxPath
$registered = [regex]::Matches($slnxText, 'Path="(Tests/Behavioral/[^"]+\.csproj)"') |
    ForEach-Object { $_.Groups[1].Value } |
    Sort-Object -Unique

# 3. Compare both directions.
$missing  = @($onDisk     | Where-Object { $_ -notin $registered })  # on disk, not in the solution
$dangling = @($registered | Where-Object { $_ -notin $onDisk })      # in the solution, not on disk

$ok = $true

if ($missing.Count -gt 0) {
    $ok = $false
    Write-Host "==> NOT REGISTERED in go2cs.slnx -- add a <Project Path=`"...`" /> line under" -ForegroundColor Red
    Write-Host "    the /tests/behavioral/target-projects/ folder for each:" -ForegroundColor Red
    $missing | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
}

if ($dangling.Count -gt 0) {
    $ok = $false
    Write-Host "==> DANGLING go2cs.slnx registration -- no such .csproj on disk (renamed/deleted?):" -ForegroundColor Red
    $dangling | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
}

if ($ok) {
    Write-Host "==> SOLUTION INTEGRITY OK: all $(@($onDisk).Count) behavioral projects are registered in go2cs.slnx." -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "Solution integrity check FAILED (see CLAUDE.md 'Adding a regression test' step 3)." -ForegroundColor Yellow
exit 1
