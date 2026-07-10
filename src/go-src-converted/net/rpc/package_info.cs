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
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using templateꓸError = go.html.template_package.ΔError;
global using templateꓸFuncMap = go.text.template_package.FuncMap;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
using bufio = go.bufio_package;
using sync = go.sync_package;
using Δhttp = go.net.http_package;
// </ImportedTypeAliases>

using go;
using static go.net.rpc_package;

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
[assembly: GoTypeAlias("Call", "ΔCall")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<Server, go.net.http_package.ΔHandler>(Pointer = true)]
[assembly: GoImplement<ServerError, error>]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<debugHTTP, go.net.http_package.ΔHandler>]
[assembly: GoImplement<go.net.http_package.ResponseWriter, io_package.Writer>]
[assembly: GoImplement<gobClientCodec, ClientCodec>(Pointer = true)]
[assembly: GoImplement<gobServerCodec, ServerCodec>(Pointer = true)]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.Reader>]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.Writer>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Request, ж<Request>>(Indirect = true)]
[assembly: GoImplicitConv<methodType, ж<methodType>>(Indirect = true)]
[assembly: GoImplicitConv<sync.Mutex, ж<sync.Mutex>>(Indirect = true)]
[assembly: GoImplicitConv<sync.WaitGroup, ж<sync.WaitGroup>>(Indirect = true)]
[assembly: GoImplicitConv<Δhttp.Request, ж<Δhttp.Request>>(Indirect = true)]
// </ImplicitConversions>

namespace go.net;

[GoPackage("rpc")]
public static partial class rpc_package
{
}
