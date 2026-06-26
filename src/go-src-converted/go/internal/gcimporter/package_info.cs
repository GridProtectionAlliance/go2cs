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
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using constantꓸKind = go.go.constant_package.ΔKind;
global using execꓸError = go.os.exec_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using pkgbitsꓸCode = go.@internal.pkgbits_package.ΔCode;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
global using typesꓸError = go.go.types_package.ΔError;
global using typesꓸExpr = go.go.ast_package.Expr;
global using typesꓸInfo = go.go.types_package.ΔInfo;
global using typesꓸScope = go.go.types_package.ΔScope;
global using typesꓸSignature = go.go.types_package.ΔSignature;
global using typesꓸTerm = go.go.types_package.ΔTerm;
global using typesꓸType = go.go.types_package.ΔType;
// </ImportedTypeAliases>

using go;
using static go.go.@internal.gcimporter_package;

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
[assembly: GoImplement<anyType, go.types_package.ΔType>]
[assembly: GoImplement<bufio_package.Reader, io_package.ByteReader>]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>]
[assembly: GoImplement<bytes_package.Reader, io_package.ByteReader>]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>]
[assembly: GoImplement<constant.Value), go.types_package.ΔType>]
[assembly: GoImplement<go.types_package.Array, go.types_package.ΔType>]
[assembly: GoImplement<go.types_package.Basic, go.types_package.ΔType>]
[assembly: GoImplement<go.types_package.Chan, go.types_package.ΔType>]
[assembly: GoImplement<go.types_package.Const, go.types_package.Object>]
[assembly: GoImplement<go.types_package.Func, go.types_package.Object>]
[assembly: GoImplement<go.types_package.Interface, go.types_package.ΔType>]
[assembly: GoImplement<go.types_package.Map, go.types_package.ΔType>]
[assembly: GoImplement<go.types_package.Named, go.types_package.ΔType>]
[assembly: GoImplement<go.types_package.Pointer, go.types_package.ΔType>]
[assembly: GoImplement<go.types_package.Slice, go.types_package.ΔType>]
[assembly: GoImplement<go.types_package.Struct, go.types_package.ΔType>]
[assembly: GoImplement<go.types_package.TypeName, go.types_package.Object>]
[assembly: GoImplement<go.types_package.TypeParam, go.types_package.ΔType>]
[assembly: GoImplement<go.types_package.Union, go.types_package.ΔType>]
[assembly: GoImplement<go.types_package.Var, go.types_package.Object>]
[assembly: GoImplement<go.types_package.ΔSignature, go.types_package.ΔType>]
[assembly: GoImplement<intReader, io_package.Reader>]
[assembly: GoImplement<types.Type), go.types_package.Object>]
[assembly: GoImplement<types.Type, bool), go.types_package.ΔType>]
[assembly: GoImplement<types.Type, error), go.types_package.ΔType>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<readerDict, ж<readerDict>>]
[assembly: GoImplicitConv<types.Basic, ж<types.Basic>>(Indirect = true)]
[assembly: GoImplicitConv<types.Tuple, ж<types.Tuple>>(Indirect = true)]
// </ImplicitConversions>

namespace go.go.@internal;

[GoPackage("gcimporter")]
public static partial class gcimporter_package
{
}
