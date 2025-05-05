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
global using elfꓸData = go.debug.elf_package.ΔData;
global using elfꓸSection = go.debug.elf_package.ΔSection;
global using machoꓸSection = go.debug.macho_package.ΔSection;
global using machoꓸSegment = go.debug.macho_package.ΔSegment;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using peꓸSection = go.debug.pe_package.ΔSection;
global using plan9objꓸSection = go.debug.plan9obj_package.ΔSection;
global using xcoffꓸSection = go.@internal.xcoff_package.ΔSection;
// </ImportedTypeAliases>

using go;
using static go.debug.buildinfo_package;

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
[assembly: GoTypeAlias("BuildInfo", "go.runtime.debug_package.BuildInfo")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<@internal.xcoff_package.ΔSection, io_package.ReaderAt>]
[assembly: GoImplement<debug.elf_package.Prog, io_package.ReaderAt>]
[assembly: GoImplement<debug.macho_package.ΔSegment, io_package.ReaderAt>]
[assembly: GoImplement<debug.pe_package.ΔSection, io_package.ReaderAt>]
[assembly: GoImplement<debug.plan9obj_package.ΔSection, io_package.ReaderAt>]
[assembly: GoImplement<elfExe, exe>]
[assembly: GoImplement<encoding.binary_package.bigEndian, encoding.binary_package.ByteOrder>]
[assembly: GoImplement<encoding.binary_package.littleEndian, encoding.binary_package.ByteOrder>]
[assembly: GoImplement<machoExe, exe>]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>]
[assembly: GoImplement<peExe, exe>]
[assembly: GoImplement<plan9objExe, exe>]
[assembly: GoImplement<xcoffExe, exe>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

namespace go.debug;

[GoPackage("buildinfo")]
public static partial class buildinfo_package
{
}
