@echo off
rem Any parameter will cause a full-rebuild
if "%1"=="" goto build
rmdir /S /Q go-src-converted
mkdir go-src-converted
:build
copy ".\go2cs\bin\Release\net6.0\publish\win-x64\go2cs.exe" ".\go2cs\bin\Release\net6.0\publish\"
powershell ".\convert-gosrc.cmd 2>&1 | tee .\go-src-converted\build.log"
