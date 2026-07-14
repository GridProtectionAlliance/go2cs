@echo off
setlocal
rem ============================================================================
rem  sign-nupkgs.bat -- author-sign every *.nupkg in a folder. Run this on the
rem  signing machine (the buildbot), where the code-signing certificate and
rem  nuget.exe live.
rem
rem  Requires environment variable NuGetCertFingerprint = the SHA-256 fingerprint
rem  of the code-signing certificate in this machine's certificate store.
rem
rem  Usage:  sign-nupkgs.bat [folder]
rem            folder - directory containing the *.nupkg (default: this .bat's folder,
rem                     so you can drop this file alongside the packages and run it).
rem ============================================================================

if "%NuGetCertFingerprint%"=="" (
    echo ERROR: environment variable NuGetCertFingerprint is not set.
    exit /b 1
)

if "%~1"=="" ( set "PkgDir=%~dp0" ) else ( set "PkgDir=%~1" )

pushd "%PkgDir%" 2>nul
if errorlevel 1 (
    echo ERROR: cannot open folder "%PkgDir%".
    exit /b 1
)

set /a Signed=0
set /a Failed=0
for %%F in (*.nupkg) do (
    echo Signing "%%~nxF" ...
    nuget sign "%%~fF" -CertificateFingerprint %NuGetCertFingerprint% -Timestamper http://timestamp.digicert.com
    if errorlevel 1 (
        echo   *** FAILED: %%~nxF
        set /a Failed+=1
    ) else (
        set /a Signed+=1
    )
)
popd

echo.
echo Done. Signed %Signed% package(s), %Failed% failure(s).
if %Failed% gtr 0 exit /b 1
exit /b 0
