@echo off
rem Launcher for deploy-core.ps1 -- forwards all arguments to PowerShell so the script can be
rem run from cmd without the ExecutionPolicy / -File noise. Examples:
rem   deploy-core stub      deploy the runnable baseline core to %GOPATH%\src\go2cs
rem   deploy-core stdlib    deploy the compilable full standard library to %GOPATH%\src\go2cs
pushd "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0deploy-core.ps1" %*
set "_ec=%ERRORLEVEL%"
popd
exit /b %_ec%
