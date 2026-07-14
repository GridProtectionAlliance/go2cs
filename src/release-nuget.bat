@echo off
setlocal
rem ============================================================================
rem  release-nuget.bat -- publish a new SIGNED release of the go2cs NuGet packages
rem  when signing happens OFFLINE on a separate machine. Three steps:
rem
rem    Phase 1  bump the build number, then build + pack all Release packages.
rem    (manual) copy the packages to the signing machine, sign them
rem             (sign-nupkgs.bat, with NuGetCertFingerprint set), copy them back.
rem    Phase 2  push the signed packages.
rem
rem  The build number is bumped UP FRONT (Phase 1) so the packages that get built,
rem  signed and pushed all carry the SAME new version -- you cannot change a
rem  package's version after it is signed. Commit version.props after a success.
rem
rem  Requires environment variable NUGET_API_KEY for the push.
rem  NOTE: if you abort before Phase 2 (or Phase 1 fails), version.props was
rem  already bumped -- either re-run and let it advance, or reset it (git checkout
rem  src\version.props) before the next release.
rem ============================================================================

set "SRC=%~dp0"
set "OUTDIR=%SRC%artifacts\nupkg"

echo.
echo === Phase 1: bump version and pack Release packages ===
powershell -NoProfile -ExecutionPolicy Bypass -File "%SRC%push-nuget.ps1" -BumpBuild -OutDir "%OUTDIR%"
if errorlevel 1 (
    echo Phase 1 ^(pack^) FAILED.
    exit /b 1
)

echo.
echo ============================================================================
echo  Packages are in:  %OUTDIR%
echo.
echo  DO THIS NOW ^(offline signing^):
echo    1. Copy  %OUTDIR%\*.nupkg  to the signing machine.
echo    2. Run  sign-nupkgs.bat  there ^(with NuGetCertFingerprint set^).
echo    3. Copy the SIGNED *.nupkg back into  %OUTDIR%  ^(overwrite the originals^).
echo.
echo  Then press any key to push the signed packages -- or close this window to stop.
echo ============================================================================
pause

echo.
echo === Phase 2: push the signed packages ===
if "%NUGET_API_KEY%"=="" (
    echo ERROR: NUGET_API_KEY is not set.
    exit /b 1
)
dotnet nuget push "%OUTDIR%\*.nupkg" --source https://api.nuget.org/v3/index.json --api-key %NUGET_API_KEY% --skip-duplicate
if errorlevel 1 (
    echo Phase 2 ^(push^) FAILED -- the signed packages are still in %OUTDIR%;
    echo re-run just the push manually once resolved.
    exit /b 1
)

echo.
echo Done. Now COMMIT version.props to record the published build number.
exit /b 0
