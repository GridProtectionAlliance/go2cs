@echo off
rem Any parameter will cause a full-rebuild
if "%1"=="" goto build
rmdir /S /Q go-src-converted
mkdir go-src-converted
:build
powershell ".\go2cs\bin\Release\net5.0\publish\go2cs.exe -s -r -e "^.+_test\.go$" -g .\go-src-converted C:\Go\src\ | tee .\go-src-converted\build.log"
