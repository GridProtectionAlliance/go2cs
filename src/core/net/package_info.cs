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
global using netipꓸAddr = go.net.netip_package.ΔAddr;
global using netipꓸPrefix = go.net.netip_package.ΔPrefix;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.net_package;

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
[assembly: GoTypeAlias("Addr", "ΔAddr")]
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
[assembly: GoImplement<(*IPConn, error), Conn>]
[assembly: GoImplement<(*IPConn, error), PacketConn>]
[assembly: GoImplement<(*TCPConn, error), Conn>]
[assembly: GoImplement<(*TCPListener, error), Listener>]
[assembly: GoImplement<(*UDPConn, error), Conn>]
[assembly: GoImplement<(*UDPConn, error), PacketConn>]
[assembly: GoImplement<(*UnixConn, error), Conn>]
[assembly: GoImplement<(*UnixConn, error), PacketConn>]
[assembly: GoImplement<(*UnixListener, error), Listener>]
[assembly: GoImplement<(Conn, error), Conn>]
[assembly: GoImplement<(Error, bool), ΔError>]
[assembly: GoImplement<(Listener, error), Listener>]
[assembly: GoImplement<(PacketConn, error), PacketConn>]
[assembly: GoImplement<(buffersWriter, bool), buffersWriter>]
[assembly: GoImplement<(c Conn, err error), Conn>]
[assembly: GoImplement<(context.Context, context.CancelFunc), context_package.Context>]
[assembly: GoImplement<(ctx context.Context, cancel context.CancelFunc), context_package.Context>]
[assembly: GoImplement<(os.FileInfo, error), osꓸFileInfo>]
[assembly: GoImplement<(syscall.Sockaddr, error), syscall_package.ΔSockaddr>]
[assembly: GoImplement<(temporary, bool), temporary>]
[assembly: GoImplement<(timeout, bool), timeout>]
[assembly: GoImplement<@internal.poll_package.errNetClosing, error>]
[assembly: GoImplement<AddrError, error>]
[assembly: GoImplement<Buffers, io_package.Reader>]
[assembly: GoImplement<Buffers, io_package.WriterTo>]
[assembly: GoImplement<Conn, error>]
[assembly: GoImplement<DNSError, error>]
[assembly: GoImplement<IPAddr, ΔAddr>]
[assembly: GoImplement<IPNet, ΔAddr>]
[assembly: GoImplement<OpError, error>]
[assembly: GoImplement<ParseError, error>]
[assembly: GoImplement<TCPAddr, ΔAddr>]
[assembly: GoImplement<TCPConn, Conn>]
[assembly: GoImplement<UDPAddr, ΔAddr>]
[assembly: GoImplement<UnixAddr, ΔAddr>]
[assembly: GoImplement<UnixConn, Conn>]
[assembly: GoImplement<UnknownNetworkError, error>]
[assembly: GoImplement<byRFC6724, sort_package.Interface>]
[assembly: GoImplement<canceledError, error>]
[assembly: GoImplement<dialParallel_dialResult, Conn>(Promoted = true)]
[assembly: GoImplement<dialParallel_dialResult, error>(Promoted = true)]
[assembly: GoImplement<notFoundError, error>]
[assembly: GoImplement<onlyValuesCtx, context_package.Context>]
[assembly: GoImplement<os_package.File, io_package.Reader>]
[assembly: GoImplement<pipe, Conn>]
[assembly: GoImplement<pipeAddr, ΔAddr>]
[assembly: GoImplement<rawConn, syscall_package.RawConn>]
[assembly: GoImplement<rawListener, syscall_package.RawConn>]
[assembly: GoImplement<syscall_package.Errno, error>]
[assembly: GoImplement<syscall_package.SockaddrInet4, syscall_package.ΔSockaddr>]
[assembly: GoImplement<syscall_package.SockaddrInet6, syscall_package.ΔSockaddr>]
[assembly: GoImplement<tcpConnWithoutReadFrom, io_package.Writer>]
[assembly: GoImplement<tcpConnWithoutWriteTo, io_package.Reader>]
[assembly: GoImplement<timeoutError, error>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<struct{wall uint64; ext int64; loc *time.Location}, struct{wall uint64; ext int64; loc *time.Location}>(Inverted = true)]
// </ImplicitConversions>

namespace go;

[GoPackage("net")]
public static partial class net_package
{
}
