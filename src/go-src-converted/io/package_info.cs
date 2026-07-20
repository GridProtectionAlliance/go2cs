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
using static go.io_package;

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
[assembly: GoImplement<LimitedReader, Reader>(Pointer = true)]
[assembly: GoImplement<OffsetWriter, Seeker>(Pointer = true)]
[assembly: GoImplement<OffsetWriter, WriteSeeker>(Pointer = true)]
[assembly: GoImplement<OffsetWriter, Writer>(Pointer = true)]
[assembly: GoImplement<OffsetWriter, WriterAt>(Pointer = true)]
[assembly: GoImplement<PipeReader, Closer>(Pointer = true)]
[assembly: GoImplement<PipeReader, ReadCloser>(Pointer = true)]
[assembly: GoImplement<PipeReader, Reader>(Pointer = true)]
[assembly: GoImplement<PipeWriter, Closer>(Pointer = true)]
[assembly: GoImplement<PipeWriter, WriteCloser>(Pointer = true)]
[assembly: GoImplement<PipeWriter, Writer>(Pointer = true)]
[assembly: GoImplement<SectionReader, ReadSeeker>(Pointer = true)]
[assembly: GoImplement<SectionReader, Reader>(Pointer = true)]
[assembly: GoImplement<SectionReader, ReaderAt>(Pointer = true)]
[assembly: GoImplement<SectionReader, Seeker>(Pointer = true)]
[assembly: GoImplement<discard, ReaderFrom>]
[assembly: GoImplement<discard, StringWriter>]
[assembly: GoImplement<discard, Writer>]
[assembly: GoImplement<eofReader, Reader>]
[assembly: GoImplement<multiReader, Reader>(Pointer = true)]
[assembly: GoImplement<multiReader, WriterTo>(Pointer = true)]
[assembly: GoImplement<multiWriter, StringWriter>(Pointer = true)]
[assembly: GoImplement<multiWriter, Writer>(Pointer = true)]
[assembly: GoImplement<nopCloser, ReadCloser>]
[assembly: GoImplement<nopCloser, Reader>(Promoted = true)]
[assembly: GoImplement<nopCloserWriterTo, ReadCloser>]
[assembly: GoImplement<nopCloserWriterTo, Reader>(Promoted = true)]
[assembly: GoImplement<nopCloserWriterTo, WriterTo>]
[assembly: GoImplement<teeReader, Reader>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

namespace go;

[GoPackage("io")]
public static partial class io_package
{
}
