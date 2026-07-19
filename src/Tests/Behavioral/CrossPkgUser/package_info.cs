// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
global using CrossPkgLibꓸGrade = go.CrossPkgLib_package.ΔGrade;
global using CrossPkgLibꓸMarker = go.CrossPkgLib_package.ΔMarker;
global using CrossPkgLibꓸStatus = go.CrossPkgLib_package.ΔStatus;
global using CrossPkgLibꓸTemperature = go.CrossPkgLib_package.Celsius;
global using CrossPkgLibꓸToken = object;
global using CrossPkgLibꓸΔToken = object;
using CrossPkgLib = go.CrossPkgLib_package;
// </ImportedTypeAliases>

using go;
using static go.main_package;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
[assembly: GoTypeAlias("Meter", "ΔMeter")]
[assembly: GoTypeAlias("Tagged", "go.CrossPkgLib_package.Labeled")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<CrossPkgLib_package.Sensor, Labeled>(Pointer = true)]
[assembly: GoImplement<badge, Labeled>]
[assembly: GoImplement<badge, Tagged>]
[assembly: GoImplement<cert, Labeled>]
[assembly: GoImplement<cert, certificate>]
[assembly: GoImplement<counter, ΔMeter>]
[assembly: GoImplement<emblem, Labeled>]
[assembly: GoImplement<emblem, namedLabel>]
[assembly: GoImplement<probe, Labeled>]
[assembly: GoImplement<relay, CrossPkgLib_package.Reporter>(Pointer = true)]
[assembly: GoImplement<seal, Labeled>]
[assembly: GoImplement<seal, stamped>]
[assembly: GoImplement<tagged, Labeled>]
[assembly: GoImplement<tallies, CrossPkgLib_package.Scored>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<localCelsius, CrossPkgLib.Celsius>(Inverted = true, ValueType = "localCelsius")]
// </ImplicitConversions>

namespace go;

[GoPackage("main")]
[GoTestMatchingConsoleOutput]
public static partial class main_package
{
}
