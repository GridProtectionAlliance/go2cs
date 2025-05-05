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
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.@internal.profile_package;

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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<(map<@string>string, error), error>]
[assembly: GoImplement<Function, message>]
[assembly: GoImplement<Label, message>]
[assembly: GoImplement<Line, message>]
[assembly: GoImplement<Location, message>]
[assembly: GoImplement<Mapping, message>]
[assembly: GoImplement<Profile, message>]
[assembly: GoImplement<Sample, message>]
[assembly: GoImplement<ValueType, message>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>]
[assembly: GoImplement<compress.gzip_package.Reader, io_package.Reader>]
[assembly: GoImplement<edgeList, sort_package.Interface>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Function, ж<Function>>(Indirect = true)]
[assembly: GoImplicitConv<Label, ж<Label>>(Indirect = true)]
[assembly: GoImplicitConv<Line, ж<Line>>(Indirect = true)]
[assembly: GoImplicitConv<Location, ж<Location>>(Indirect = true)]
[assembly: GoImplicitConv<Mapping, ж<Mapping>>(Indirect = true)]
[assembly: GoImplicitConv<Options, ж<Options>>(Indirect = true)]
[assembly: GoImplicitConv<Sample, ж<Sample>>(Indirect = true)]
[assembly: GoImplicitConv<ValueType, ж<ValueType>>(Indirect = true)]
[assembly: GoImplicitConv<struct{field int; typ int; u64 uint64; data <>byte; tmp <16>byte}, struct{field int; typ int; u64 uint64; data <>byte; tmp <16>byte}>(Inverted = true)]
// </ImplicitConversions>

namespace go.@internal;

[GoPackage("profile")]
public static partial class profile_package
{
}
