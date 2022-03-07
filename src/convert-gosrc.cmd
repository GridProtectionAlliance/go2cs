@echo off
.\go2cs\bin\Release\net6.0\publish\go2cs.exe -s -r -e "^.+_test\.go$" -g .\go-src-converted "%GOROOT%\src