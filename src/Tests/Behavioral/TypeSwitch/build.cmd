mkdir bin\win-x64
bflat build .\TypeSwitch.cs ..\..\..\gocore\GlobalUsings.cs -o:bin\win-x64\TypeSwitch.exe --os:windows --arch:x64 --stdlib:DotNet -Ot --no-globalization --langversion:latest -r:..\..\..\gocore\golib\bin\Debug\net8.0\golib.dll -r:..\..\..\gocore\fmt\bin\Debug\net8.0\fmt_package.dll 
