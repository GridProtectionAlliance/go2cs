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
// </ImportedTypeAliases>

using go;
using static go.debug.dwarf_package;

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
[assembly: GoTypeAlias("LineReader", "ΔLineReader")]
[assembly: GoTypeAlias("Reader", "ΔReader")]
[assembly: GoTypeAlias("Type", "ΔType")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<(Type, bool), ΔType>]
[assembly: GoImplement<(Type, error), ΔType>]
[assembly: GoImplement<AddrType, ΔType>]
[assembly: GoImplement<ArrayType, ΔType>]
[assembly: GoImplement<BoolType, ΔType>]
[assembly: GoImplement<CharType, ΔType>]
[assembly: GoImplement<ComplexType, ΔType>]
[assembly: GoImplement<DecodeError, error>]
[assembly: GoImplement<DotDotDotType, ΔType>]
[assembly: GoImplement<Entry, ΔType>]
[assembly: GoImplement<EnumType, ΔType>]
[assembly: GoImplement<FloatType, ΔType>]
[assembly: GoImplement<FuncType, ΔType>]
[assembly: GoImplement<IntType, ΔType>]
[assembly: GoImplement<PtrType, ΔType>]
[assembly: GoImplement<QualType, ΔType>]
[assembly: GoImplement<StructType, ΔType>]
[assembly: GoImplement<TypedefType, ΔType>]
[assembly: GoImplement<UcharType, ΔType>]
[assembly: GoImplement<UintType, ΔType>]
[assembly: GoImplement<UnspecifiedType, ΔType>]
[assembly: GoImplement<UnsupportedType, ΔType>]
[assembly: GoImplement<VoidType, ΔType>]
[assembly: GoImplement<typeUnit, dataFormat>]
[assembly: GoImplement<typeUnitReader, typeReader>]
[assembly: GoImplement<unit, dataFormat>]
[assembly: GoImplement<unknownFormat, dataFormat>]
[assembly: GoImplement<ΔReader, typeReader>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Entry, ж<Entry>>(Indirect = true)]
[assembly: GoImplicitConv<typeUnit, ж<typeUnit>>(Indirect = true)]
[assembly: GoImplicitConv<unit, ж<unit>>(Indirect = true)]
// </ImplicitConversions>

namespace go.debug;

[GoPackage("dwarf")]
public static partial class dwarf_package
{
}
