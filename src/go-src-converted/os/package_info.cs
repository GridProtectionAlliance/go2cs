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
global using runtimeꓸError = go.runtime_package.ΔError;
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.os_package;

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
[assembly: GoTypeAlias("DirEntry", "go.io.fs_package.DirEntry")]
[assembly: GoTypeAlias("FileInfo", "go.io.fs_package.FileInfo")]
[assembly: GoTypeAlias("FileMode", "go.io.fs_package.FileMode")]
[assembly: GoTypeAlias("Kill", "const:ΔKill")]
[assembly: GoTypeAlias("PathError", "go.io.fs_package.PathError")]
[assembly: GoTypeAlias("Signal", "ΔSignal")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<File, go.io.fs_package.File>(Pointer = true)]
[assembly: GoImplement<File, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<LinkError, error>(Pointer = true)]
[assembly: GoImplement<SyscallError, error>(Pointer = true)]
[assembly: GoImplement<dirEntry, DirEntry>]
[assembly: GoImplement<dirEntry, go.io.fs_package.DirEntry>]
[assembly: GoImplement<dirFS, go.io.fs_package.FS>]
[assembly: GoImplement<fileStat, FileInfo>(Pointer = true)]
[assembly: GoImplement<fileWithoutReadFrom, io_package.Writer>]
[assembly: GoImplement<fileWithoutWriteTo, io_package.Reader>]
[assembly: GoImplement<go.io.fs_package.File, io_package.Reader>]
[assembly: GoImplement<rawConn, syscall_package.RawConn>(Pointer = true)]
[assembly: GoImplement<syscall_package.ΔSignal, ΔSignal>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

namespace go;

[GoPackage("os")]
public static partial class os_package
{
}
