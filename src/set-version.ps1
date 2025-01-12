$version = "0.1.4"
$fullVersion = "$version.0"

# Update version in exe resource file
.\Utilities\ReplaceInFiles\ReplaceInFiles.exe -x -e:UTF8 .\go2cs\winres\winres.json "version\`"\: \`"((\*~!~\d+)\.)+(\*~!~\d+)\`" "version\`": \`"$fullVersion\`"

# Update version in csproj template
.\Utilities\ReplaceInFiles\ReplaceInFiles.exe -x -e:UTF8 .\go2cs\csproj-template.xml "\<Version\>((\*~!~\d+)\.)+(\*~!~\d+)\<" "<Version>$version<"

# Update version in go2cs code generator
.\Utilities\ReplaceInFiles\ReplaceInFiles.exe -x -e:UTF8 .\go2cs-gen\go2cs-gen.csproj "\<Version\>((\*~!~\d+)\.)+(\*~!~\d+)\<" "<Version>$version<"

# Rebuild resource file
cd go2cs
go install github.com/tc-hib/go-winres@latest
go-winres make
cd ..

cmd /c "pause"
