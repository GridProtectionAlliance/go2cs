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
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using textprotoꓸError = go.net.textproto_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using urlꓸError = go.net.url_package.ΔError;
using bufio = go.bufio_package;
using http = go.net.http_package;
using url = go.net.url_package;
// </ImportedTypeAliases>

using go;
using static go.net.http.httputil_package;

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
[assembly: GoImplement<bufio_package.ReadWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<delegateReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<dumpConn, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<dumpConn, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<dumpConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<failureToReadBody, io_package.ReadCloser>]
[assembly: GoImplement<go.net.http_package.ResponseWriter, io_package.Writer>]
[assembly: GoImplement<io_package.PipeReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<io_package.PipeWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.ReadWriter>]
[assembly: GoImplement<io_package.WriteCloser, io_package.Writer>]
[assembly: GoImplement<maxLatencyWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriter>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
[assembly: GoImplement<neverEnding, io_package.Reader>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<http.Request, ж<http.Request>>(Indirect = true)]
[assembly: GoImplicitConv<url.URL, ж<url.URL>>(Indirect = true)]
// </ImplicitConversions>

namespace go.net.http;

[GoPackage("httputil")]
public static partial class httputil_package
{
}
