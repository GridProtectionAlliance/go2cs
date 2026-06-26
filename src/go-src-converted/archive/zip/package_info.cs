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
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.archive.zip_package;

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
[assembly: GoImplement<(fileInfoDirEntry, error), fileInfoDirEntry>]
[assembly: GoImplement<(fileInfoDirEntry, error), io.fs_package.FileInfo>]
[assembly: GoImplement<(io.ReadCloser, bool), io_package.ReadCloser>]
[assembly: GoImplement<(io.ReadCloser, error), io_package.ReadCloser>]
[assembly: GoImplement<(io.Reader, error), io_package.Reader>]
[assembly: GoImplement<(io.Writer, error), io_package.Writer>]
[assembly: GoImplement<(os.FileInfo, error), osꓸFileInfo>]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>]
[assembly: GoImplement<checksumReader, io_package.ReadCloser>]
[assembly: GoImplement<countWriter, io_package.Writer>]
[assembly: GoImplement<dirReader, io_package.ReadCloser>]
[assembly: GoImplement<dirWriter, io_package.Writer>]
[assembly: GoImplement<fileListEntry, fileInfoDirEntry>]
[assembly: GoImplement<fileListEntry, io.fs_package.DirEntry>]
[assembly: GoImplement<fileListEntry, io.fs_package.FileInfo>]
[assembly: GoImplement<fileWriter, io_package.Writer>]
[assembly: GoImplement<fs.File, error), io.fs_package.File>]
[assembly: GoImplement<fs.FileInfo, error), io.fs_package.FileInfo>]
[assembly: GoImplement<headerFileInfo, fileInfoDirEntry>]
[assembly: GoImplement<headerFileInfo, io.fs_package.FileInfo>]
[assembly: GoImplement<io.fs_package.File, io_package.Reader>]
[assembly: GoImplement<io.fs_package.PathError, error>]
[assembly: GoImplement<io_package.SectionReader, io_package.Reader>]
[assembly: GoImplement<openDir, io.fs_package.File>]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>]
[assembly: GoImplement<pooledFlateReader, io_package.ReadCloser>]
[assembly: GoImplement<pooledFlateWriter, io_package.WriteCloser>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<bufio.Reader, ж<bufio.Reader>>(Indirect = true)]
[assembly: GoImplicitConv<io.SectionReader, ж<io.SectionReader>>(Indirect = true)]
// </ImplicitConversions>

namespace go.archive;

[GoPackage("zip")]
public static partial class zip_package
{
}
