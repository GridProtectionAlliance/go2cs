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
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸMapType = go.@internal.abi_package.ΔMapType;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static go.reflect_package;

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
[assembly: GoTypeAlias("Bool", "const:ΔBool")]
[assembly: GoTypeAlias("ChanDir", "ΔChanDir")]
[assembly: GoTypeAlias("Int", "const:ΔInt")]
[assembly: GoTypeAlias("Interface", "const:ΔInterface")]
[assembly: GoTypeAlias("Kind", "ΔKind")]
[assembly: GoTypeAlias("Method", "ΔMethod")]
[assembly: GoTypeAlias("Pointer", "const:ΔPointer")]
[assembly: GoTypeAlias("Slice", "const:ΔSlice")]
[assembly: GoTypeAlias("String", "const:ΔString")]
[assembly: GoTypeAlias("Type", "ΔType")]
[assembly: GoTypeAlias("Uint", "const:ΔUint")]
[assembly: GoTypeAlias("UnsafePointer", "const:ΔUnsafePointer")]
[assembly: GoTypeAlias("Value", "ΔValue")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<rtype, ΔType>]
[assembly: GoImplement<uintptr, ΔType>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<abi.RegArgs, ж<abi.RegArgs>>(Indirect = true)]
[assembly: GoImplicitConv<abi.Type, ж<abi.Type>>(Indirect = true)]
[assembly: GoImplicitConv<flag, abiꓸKind>(Inverted = false, ValueType = "abiꓸKind")]
[assembly: GoImplicitConv<flag, ΔKind>(Inverted = true, ValueType = "flag")]
[assembly: GoImplicitConv<hiter, ж<hiter>>(Indirect = true)]
[assembly: GoImplicitConv<ΔChanDir, abiꓸChanDir>(Inverted = false, ValueType = "abiꓸChanDir")]
[assembly: GoImplicitConv<ΔKind, abiꓸKind>(Inverted = false, ValueType = "abiꓸKind")]
[assembly: GoImplicitConv<ΔKind, flag>(Inverted = true, ValueType = "ΔKind")]
// </ImplicitConversions>

namespace go;

[GoPackage("reflect")]
public static partial class reflect_package
{
}
