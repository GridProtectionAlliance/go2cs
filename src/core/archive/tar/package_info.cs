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
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.archive.tar_package;

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
[assembly: GoImplement<(FileInfoNames, bool), FileInfoNames>]
[assembly: GoImplement<(io.ReadSeeker, bool), io_package.ReadSeeker>]
[assembly: GoImplement<(io.Seeker, bool), io_package.Seeker>]
[assembly: GoImplement<(io.WriteSeeker, bool), io_package.WriteSeeker>]
[assembly: GoImplement<ReadFrom_dst, io_package.Writer>]
[assembly: GoImplement<ReadFrom_dstᴛ1, io_package.Writer>]
[assembly: GoImplement<Reader, io_package.Reader>]
[assembly: GoImplement<WriteTo_src, io_package.Reader>]
[assembly: GoImplement<WriteTo_srcᴛ1, io_package.Reader>]
[assembly: GoImplement<Writer, io_package.Writer>]
[assembly: GoImplement<fileReader, io_package.Reader>]
[assembly: GoImplement<fileWriter, io_package.Writer>]
[assembly: GoImplement<fs.File, error), io.fs_package.File>]
[assembly: GoImplement<fs.FileInfo, error), io.fs_package.FileInfo>]
[assembly: GoImplement<headerError, error>]
[assembly: GoImplement<headerFileInfo, io.fs_package.FileInfo>]
[assembly: GoImplement<io.fs_package.File, io_package.Reader>]
[assembly: GoImplement<io_package.ReadSeeker, io_package.Reader>]
[assembly: GoImplement<io_package.WriteSeeker, io_package.Writer>]
[assembly: GoImplement<regFileReader, io_package.Reader>]
[assembly: GoImplement<regFileWriter, io_package.Writer>]
[assembly: GoImplement<sparseFileReader, io_package.Reader>]
[assembly: GoImplement<sparseFileWriter, io_package.Writer>]
[assembly: GoImplement<zeroReader, io_package.Reader>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

namespace go.archive;

[GoPackage("tar")]
public static partial class tar_package
{
}
