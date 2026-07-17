# check-symbol-sync.ps1
# Verifies the two generated symbol-constant projections are in sync with the canonical
# symbol table src/core/go2cs/symbols.json:
#   - src/go2cs/symbols.go       (Go converter constants)
#   - src/core/go2cs/Symbols.cs  (class go2cs.Symbols, shared by golib and go2cs-gen)
# Re-runs the gensymbols generator, then fails (exit 1) if either committed projection
# differs from what the table produces - i.e., someone hand-edited a generated file or
# changed symbols.json without regenerating.

$ErrorActionPreference = "Stop"

$srcRoot = $PSScriptRoot
$converterDir = Join-Path $srcRoot "go2cs"

Write-Host "Regenerating symbol projections from core/go2cs/symbols.json..." -ForegroundColor Cyan

Push-Location $converterDir

try {
    go run ./internal/gensymbols

    if ($LASTEXITCODE -ne 0) {
        Write-Host "gensymbols FAILED (exit $LASTEXITCODE)." -ForegroundColor Red
        exit 1
    }
}
finally {
    Pop-Location
}

# Paths are relative to $srcRoot because git runs with -C $srcRoot.
git -C $srcRoot ls-files --error-unmatch go2cs/symbols.go core/go2cs/Symbols.cs | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "SYMBOL DRIFT: a generated projection is not tracked by git." -ForegroundColor Red
    exit 1
}

git -C $srcRoot diff --exit-code -- go2cs/symbols.go core/go2cs/Symbols.cs

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "SYMBOL DRIFT DETECTED: the committed symbols.go / Symbols.cs do not match" -ForegroundColor Red
    Write-Host "core/go2cs/symbols.json (diff above). If the symbols.json change is intended," -ForegroundColor Red
    Write-Host "commit the regenerated files this script just wrote; never hand-edit them." -ForegroundColor Red
    exit 1
}

Write-Host "Symbol projections are in sync with symbols.json." -ForegroundColor Green
exit 0
