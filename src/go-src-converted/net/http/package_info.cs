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
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using textprotoꓸError = go.net.textproto_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tlsꓸConnectionState = go.crypto.tls_package.ΔConnectionState;
global using urlꓸError = go.net.url_package.ΔError;
// </ImportedTypeAliases>

using go;
using static go.net.http_package;

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
[assembly: GoTypeAlias("Cookie", "ΔCookie")]
[assembly: GoTypeAlias("Handler", "ΔHandler")]
[assembly: GoTypeAlias("Header", "ΔHeader")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<(File, error), File>]
[assembly: GoImplement<(Pusher, bool), Pusher>]
[assembly: GoImplement<(RoundTripper, bool), RoundTripper>]
[assembly: GoImplement<(canceler, bool), setRequestCancel_canceler>]
[assembly: GoImplement<(closeIdler, bool), CloseIdleConnections_closeIdler>]
[assembly: GoImplement<(closeWriter, bool), closeWriter>]
[assembly: GoImplement<(context.Context, context.CancelFunc), context_package.Context>]
[assembly: GoImplement<(ctx context.Context, cancel context.CancelCauseFunc), context_package.Context>]
[assembly: GoImplement<(ctx context.Context, cancel context.CancelFunc), context_package.Context>]
[assembly: GoImplement<(erringRoundTripper, bool), erringRoundTripper>]
[assembly: GoImplement<(h Handler, pattern string), ΔHandler>]
[assembly: GoImplement<(h2Transport, bool), h2Transport>]
[assembly: GoImplement<(io.ByteWriter, bool), io_package.ByteWriter>]
[assembly: GoImplement<(io.ReadCloser, bool), io_package.ReadCloser>]
[assembly: GoImplement<(io.ReadCloser, error), io_package.ReadCloser>]
[assembly: GoImplement<(io.ReaderFrom, bool), io_package.ReaderFrom>]
[assembly: GoImplement<(io.Seeker, bool), io_package.Seeker>]
[assembly: GoImplement<(io.StringWriter, bool), io_package.StringWriter>]
[assembly: GoImplement<(io.Writer, bool), io_package.Writer>]
[assembly: GoImplement<(io.Writer, error), io_package.Writer>]
[assembly: GoImplement<(net.Addr, error), net_package.ΔAddr>]
[assembly: GoImplement<(net.Conn, *bufio.ReadWriter, error), net_package.Conn>]
[assembly: GoImplement<(net.Conn, error), net_package.Conn>]
[assembly: GoImplement<(net.Error, bool), net_package.ΔError>]
[assembly: GoImplement<(net.Listener, error), net_package.Listener>]
[assembly: GoImplement<(requestTooLarger, bool), Read_requestTooLarger>]
[assembly: GoImplement<(rwc net.Conn, buf *bufio.ReadWriter, err error), net_package.Conn>]
[assembly: GoImplement<(underlyingReader io.Reader, isNopCloser bool), io_package.Reader>]
[assembly: GoImplement<@string, net_package.Conn>]
[assembly: GoImplement<Dir, FileSystem>]
[assembly: GoImplement<File, io_package.ReadSeeker>]
[assembly: GoImplement<HandlerFunc, ΔHandler>]
[assembly: GoImplement<ProtocolError, error>]
[assembly: GoImplement<ResponseWriter, io_package.Writer>]
[assembly: GoImplement<ServeMux, ΔHandler>]
[assembly: GoImplement<Transport, RoundTripper>]
[assembly: GoImplement<bodyLocked, io_package.Reader>]
[assembly: GoImplement<bufioFlushWriter, io_package.Writer>]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>]
[assembly: GoImplement<byteReader, io_package.Reader>]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>]
[assembly: GoImplement<checkConnErrorWriter, io_package.Writer>]
[assembly: GoImplement<chunkWriter, io_package.Writer>]
[assembly: GoImplement<connReader, io_package.Reader>]
[assembly: GoImplement<connectMethod, context_package.Context>]
[assembly: GoImplement<countingWriter, io_package.Writer>]
[assembly: GoImplement<dirEntryDirs, anyDirs>]
[assembly: GoImplement<exactSig, sniffSig>]
[assembly: GoImplement<fakeLocker, sync_package.Locker>]
[assembly: GoImplement<fileHandler, ΔHandler>]
[assembly: GoImplement<fileInfoDirs, anyDirs>]
[assembly: GoImplement<fileTransport, RoundTripper>]
[assembly: GoImplement<finishAsyncByteRead, io_package.Reader>]
[assembly: GoImplement<fs.File, error), io.fs_package.File>]
[assembly: GoImplement<fs.FileInfo, error), File>]
[assembly: GoImplement<fs.FileInfo, error), io.fs_package.FileInfo>]
[assembly: GoImplement<fs.ReadDirFile, bool), io.fs_package.ReadDirFile>]
[assembly: GoImplement<globalOptionsHandler, ΔHandler>]
[assembly: GoImplement<htmlSig, sniffSig>]
[assembly: GoImplement<http.pattern, matches <>string), ΔHandler>]
[assembly: GoImplement<initALPNRequest, ΔHandler>]
[assembly: GoImplement<ioFS, FileSystem>]
[assembly: GoImplement<ioFile, File>]
[assembly: GoImplement<io_package.PipeReader, io_package.Reader>]
[assembly: GoImplement<io_package.PipeWriter, io_package.Writer>]
[assembly: GoImplement<io_package.ReadCloser, io_package.Closer>]
[assembly: GoImplement<io_package.ReadCloser, io_package.Reader>]
[assembly: GoImplement<io_package.ReadSeeker, io_package.Reader>]
[assembly: GoImplement<io_package.WriteCloser, io_package.Writer>]
[assembly: GoImplement<loggingConn, net_package.Conn>]
[assembly: GoImplement<maskedSig, sniffSig>]
[assembly: GoImplement<maxBytesReader, io_package.ReadCloser>]
[assembly: GoImplement<mp4Sig, sniffSig>]
[assembly: GoImplement<multipart.File, error), mime.multipart_package.File>]
[assembly: GoImplement<net.http.internal_package.FlushAfterChunkWriter, io_package.Writer>]
[assembly: GoImplement<net.url_package.ΔError, error>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
[assembly: GoImplement<net_package.TCPConn, closeWriter>]
[assembly: GoImplement<noBody, io_package.ReadCloser>]
[assembly: GoImplement<noBody, io_package.WriterTo>]
[assembly: GoImplement<nothingWrittenError, error>(Promoted = true)]
[assembly: GoImplement<nothingWrittenError, error>]
[assembly: GoImplement<onceCloseListener, net_package.Listener>]
[assembly: GoImplement<os_package.File, File>]
[assembly: GoImplement<persistConn, io_package.Reader>]
[assembly: GoImplement<persistConnWriter, io_package.ReaderFrom>]
[assembly: GoImplement<persistConnWriter, io_package.Writer>]
[assembly: GoImplement<populateResponse, ResponseWriter>]
[assembly: GoImplement<readWriteCloserBody, io_package.ReadWriteCloser>]
[assembly: GoImplement<redirectHandler, ΔHandler>]
[assembly: GoImplement<requestBodyReadError, error>(Promoted = true)]
[assembly: GoImplement<requestBodyReadError, error>]
[assembly: GoImplement<response, ResponseWriter>]
[assembly: GoImplement<response, io_package.Writer>]
[assembly: GoImplement<rᴛ1, io_package.Reader>]
[assembly: GoImplement<statusError, error>]
[assembly: GoImplement<stringWriter, io_package.StringWriter>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>]
[assembly: GoImplement<sync_package.Mutex, sync_package.Locker>]
[assembly: GoImplement<textSig, sniffSig>]
[assembly: GoImplement<time_package.Duration, error>]
[assembly: GoImplement<timeoutError, error>]
[assembly: GoImplement<timeoutHandler, ΔHandler>]
[assembly: GoImplement<timeoutWriter, Pusher>]
[assembly: GoImplement<timeoutWriter, ResponseWriter>]
[assembly: GoImplement<transportReadFromServerError, error>]
[assembly: GoImplement<unsupportedTEError, error>]
[assembly: GoImplement<writerOnly, io_package.Writer>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Request, ж<Request>>(Indirect = true)]
[assembly: GoImplicitConv<bufio.Writer, ж<bufio.Writer>>(Indirect = true)]
[assembly: GoImplicitConv<http.pattern; matches <>string; otherValues map<string>string}, http.pattern; matches <>string; otherValues map<string>string}>(Inverted = true)]
[assembly: GoImplicitConv<http2Server, ж<http2Server>>(Indirect = true)]
[assembly: GoImplicitConv<httptrace.ClientTrace, ж<httptrace.ClientTrace>>]
[assembly: GoImplicitConv<struct{wall uint64; ext int64; loc *time.Location}, struct{wall uint64; ext int64; loc *time.Location}>(Inverted = true)]
[assembly: GoImplicitConv<tls.Conn, ж<tls.Conn>>(Indirect = true)]
// </ImplicitConversions>

namespace go.net;

[GoPackage("http")]
public static partial class http_package
{
}
