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
// </ImportedTypeAliases>

using go;
using static go.runtime_package;

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
[assembly: GoTypeAlias("Error", "ΔError")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<errorString, error>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<_type, ж<_type>>(Indirect = true)]
[assembly: GoImplicitConv<abi.RegArgs, ж<abi.RegArgs>>(Indirect = true)]
[assembly: GoImplicitConv<abi.Type, ж<abi.Type>>(Indirect = true)]
[assembly: GoImplicitConv<adjustinfo, ж<adjustinfo>>(Indirect = true)]
[assembly: GoImplicitConv<bmap, ж<bmap>>(Indirect = true)]
[assembly: GoImplicitConv<childInfo, ж<childInfo>>(Indirect = true)]
[assembly: GoImplicitConv<context, ж<context>>(Indirect = true)]
[assembly: GoImplicitConv<funcval, ж<funcval>>(Indirect = true)]
[assembly: GoImplicitConv<g, ж<g>>(Indirect = true)]
[assembly: GoImplicitConv<g, ж<g>>]
[assembly: GoImplicitConv<gQueue, ж<gQueue>>(Indirect = true)]
[assembly: GoImplicitConv<gcBits, ж<pinnerBits>>(Indirect = true)]
[assembly: GoImplicitConv<gcWork, ж<gcWork>>(Indirect = true)]
[assembly: GoImplicitConv<goroutineProfileStateHolder, ж<goroutineProfileStateHolder>>(Indirect = true)]
[assembly: GoImplicitConv<hiter, ж<hiter>>(Indirect = true)]
[assembly: GoImplicitConv<hmap, ж<hmap>>(Indirect = true)]
[assembly: GoImplicitConv<itab, ж<itab>>(Indirect = true)]
[assembly: GoImplicitConv<lfstack, Δhex>(Inverted = true, ValueType = "lfstack")]
[assembly: GoImplicitConv<limiterEventStamp, limiterEventType>(Inverted = true, ValueType = "limiterEventStamp")]
[assembly: GoImplicitConv<m, ж<m>>(Indirect = true)]
[assembly: GoImplicitConv<maptype, ж<maptype>>(Indirect = true)]
[assembly: GoImplicitConv<metricValue, ж<metricValue>>(Indirect = true)]
[assembly: GoImplicitConv<mspan, ж<mspan>>(Indirect = true)]
[assembly: GoImplicitConv<muintptr, Δhex>(Inverted = true, ValueType = "muintptr")]
[assembly: GoImplicitConv<nameOff, Δhex>(Inverted = true, ValueType = "nameOff")]
[assembly: GoImplicitConv<pinnerBits, ж<gcBits>>(Indirect = true)]
[assembly: GoImplicitConv<pollDesc, ж<pollDesc>>(Indirect = true)]
[assembly: GoImplicitConv<ptrtype, ж<ptrtype>>(Indirect = true)]
[assembly: GoImplicitConv<spanClass, traceArg>(Inverted = true, ValueType = "spanClass")]
[assembly: GoImplicitConv<stackScanState, ж<stackScanState>>(Indirect = true)]
[assembly: GoImplicitConv<stkframe, ж<stkframe>>]
[assembly: GoImplicitConv<sudog, ж<sudog>>(Indirect = true)]
[assembly: GoImplicitConv<sweepClass, spanClass>(Inverted = true, ValueType = "sweepClass")]
[assembly: GoImplicitConv<textOff, Δhex>(Inverted = true, ValueType = "textOff")]
[assembly: GoImplicitConv<traceGoStatus, traceArg>(Inverted = true, ValueType = "traceGoStatus")]
[assembly: GoImplicitConv<traceProcStatus, traceArg>(Inverted = true, ValueType = "traceProcStatus")]
[assembly: GoImplicitConv<typeOff, Δhex>(Inverted = true, ValueType = "typeOff")]
[assembly: GoImplicitConv<Δguintptr, Δhex>(Inverted = true, ValueType = "Δguintptr")]
[assembly: GoImplicitConv<Δp, ж<Δp>>(Indirect = true)]
[assembly: GoImplicitConv<Δslice, ж<Δslice>>(Indirect = true)]
// </ImplicitConversions>

namespace go;

[GoPackage("runtime")]
public static partial class runtime_package
{
}
