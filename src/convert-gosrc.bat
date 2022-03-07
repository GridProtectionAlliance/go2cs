@echo off
rem Any parameter will cause a full-rebuild
if "%1"=="" goto build
rmdir /S /Q go-src-converted
mkdir go-src-converted
:build
powershell ".\convert-gosrc.cmd 2>&1 | tee .\go-src-converted\build.log"
