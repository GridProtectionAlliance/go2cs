@echo off
rem Launcher for clean-bin.ps1 -- removes bin, obj and Generated folders under src.
pushd "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0clean-bin.ps1" %*
set "_ec=%ERRORLEVEL%"
popd
exit /b %_ec%
