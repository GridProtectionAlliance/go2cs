@ECHO OFF
:: Copy go2cs core libraries source to go2cs folder in %GOPATH%\src
XCOPY gocore\*.* %GOPATH%\src\go2cs /S /C /I /Y
:: Build debug and release builds of go2cs core libraries in %GOPATH%\src
FOR /D %%i IN (%GOPATH%\src\go2cs\*) DO (dotnet build -v q -c Debug "%%i") 
FOR /D %%i IN (%GOPATH%\src\go2cs\*) DO (dotnet build -v q -c Release "%%i") 
