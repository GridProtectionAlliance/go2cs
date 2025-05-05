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
global using oldtraceꓸSTWReason = go.@internal.trace.@internal.oldtrace_package.ΔSTWReason;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.@internal.trace_package;

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
[assembly: GoTypeAlias("Event", "ΔEvent")]
[assembly: GoTypeAlias("Label", "ΔLabel")]
[assembly: GoTypeAlias("Log", "ΔLog")]
[assembly: GoTypeAlias("Metric", "ΔMetric")]
[assembly: GoTypeAlias("Range", "ΔRange")]
[assembly: GoTypeAlias("Region", "ΔRegion")]
[assembly: GoTypeAlias("Stack", "ΔStack")]
[assembly: GoTypeAlias("StateTransition", "ΔStateTransition")]
[assembly: GoTypeAlias("Task", "ΔTask")]
[assembly: GoTypeAlias("Time", "ΔTime")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<(schedCtx, bool, error), error>]
[assembly: GoImplement<bandUtilHeap, container.heap_package.Interface>]
[assembly: GoImplement<bool, error>]
[assembly: GoImplement<bufio_package.Reader, interface{io.Reader; io.ByteReader}>]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>]
[assembly: GoImplement<bytes_package.Reader, io_package.ByteReader>]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>]
[assembly: GoImplement<readBatch_r, io_package.ByteReader>]
[assembly: GoImplement<readBatch_r, io_package.Reader>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>]
[assembly: GoImplement<utilHeap, container.heap_package.Interface>]
[assembly: GoImplement<utilHeap, sort_package.Interface>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<evTable, ж<evTable>>(Indirect = true)]
[assembly: GoImplicitConv<spilledBatch, ж<spilledBatch>>(Indirect = true)]
[assembly: GoImplicitConv<trace.stringID, string>, ж<trace.stringID, string>>>(Indirect = true)]
[assembly: GoImplicitConv<ΔTime, oldtrace.Timestamp>(Inverted = false, ValueType = "oldtrace.Timestamp")]
[assembly: GoImplicitConv<ΔTime, timestamp>(Inverted = true, ValueType = "ΔTime")]
// </ImplicitConversions>

namespace go.@internal;

[GoPackage("trace")]
public static partial class trace_package
{
}
