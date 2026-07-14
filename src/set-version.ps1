# set-version.ps1 -- sets the go2cs CONVERTER TOOL version (the go2cs.exe Windows resource).
#
# This is the go2cs *tool* version and is INDEPENDENT of the published NuGet package version:
# the converted stdlib packages, go.lib and go.gen take their version from src\version.props
# (base = converted Go release e.g. 1.23.1, build number bumped per publish by push-nuget.ps1).
# The csproj template and go2cs-gen.csproj no longer carry a <Version> element -- they inherit
# $(Version) from version.props -- so this script only stamps the winres resource.

$version = "0.1.4"
$fullVersion = "$version.0"

# Update version in exe resource file
.\Utilities\ReplaceInFiles\ReplaceInFiles.exe -x -e:UTF8 .\go2cs\winres\winres.json "version\`"\: \`"((\*~!~\d+)\.)+(\*~!~\d+)\`" "version\`": \`"$fullVersion\`"

# Rebuild resource file
cd go2cs
go install github.com/tc-hib/go-winres@latest
go-winres make
cd ..

cmd /c "pause"
