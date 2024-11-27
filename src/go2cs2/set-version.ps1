$version = "0.1.4"
$fullVersion = "$version.0"

# Update version in exe resource file
.\tools\ReplaceInFiles.exe -x -e:UTF8 winres\winres.json "version\`"\: \`"((\*~!~\d+)\.)+(\*~!~\d+)\`" "version\`": \`"$fullVersion\`"

# Update version in csproj template
.\tools\ReplaceInFiles.exe -x -e:UTF8 csproj-template.xml "\<Version\>((\*~!~\d+)\.)+(\*~!~\d+)\<" "<Version>$version<"

# Rebuild resource file
go-winres make