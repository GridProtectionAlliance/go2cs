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
using bufio = go.bufio_package;
using httptrace = go.net.http.httptrace_package;
using tls = go.crypto.tls_package;
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
[assembly: GoImplement<Client, h2Transport>(Pointer = true)]
[assembly: GoImplement<Dir, FileSystem>]
[assembly: GoImplement<File, io_package.ReadSeeker>]
[assembly: GoImplement<HandlerFunc, ΔHandler>]
[assembly: GoImplement<MaxBytesError, error>(Pointer = true)]
[assembly: GoImplement<ProtocolError, error>(Pointer = true)]
[assembly: GoImplement<ResponseController, Hijacker>(Pointer = true)]
[assembly: GoImplement<ResponseWriter, io_package.Writer>]
[assembly: GoImplement<ServeMux, ΔHandler>(Pointer = true)]
[assembly: GoImplement<Transport, CloseIdleConnections_closeIdler>(Pointer = true)]
[assembly: GoImplement<Transport, RoundTripper>(Pointer = true)]
[assembly: GoImplement<Transport, h2Transport>(Pointer = true)]
[assembly: GoImplement<Transport, setRequestCancel_canceler>(Pointer = true)]
[assembly: GoImplement<Write_type, io_package.Closer>(Promoted = true)]
[assembly: GoImplement<Write_type, io_package.ReadCloser>]
[assembly: GoImplement<Write_type, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<body, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<bodyEOFSignal, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<bodyEOFSignal, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bodyLocked, io_package.Reader>]
[assembly: GoImplement<bufioFlushWriter, io_package.Writer>]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<byteReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<cancelTimerBody, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<checkConnErrorWriter, io_package.Writer>]
[assembly: GoImplement<chunkWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<connReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<countingWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<dirEntryDirs, anyDirs>]
[assembly: GoImplement<errorReader, io_package.Reader>]
[assembly: GoImplement<exactSig, sniffSig>(Pointer = true)]
[assembly: GoImplement<expectContinueReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<fakeLocker, sync_package.Locker>]
[assembly: GoImplement<fileHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<fileInfoDirs, anyDirs>]
[assembly: GoImplement<fileTransport, RoundTripper>]
[assembly: GoImplement<finishAsyncByteRead, io_package.Reader>]
[assembly: GoImplement<globalOptionsHandler, ΔHandler>]
[assembly: GoImplement<go.net.http.internal_package.FlushAfterChunkWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.net.url_package.ΔError, error>(Pointer = true)]
[assembly: GoImplement<gzipReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<htmlSig, sniffSig>]
[assembly: GoImplement<http2ClientConn, RoundTripper>(Pointer = true)]
[assembly: GoImplement<http2ConnectionError, error>]
[assembly: GoImplement<http2ContinuationFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2ContinuationFrame, http2headersEnder>(Pointer = true)]
[assembly: GoImplement<http2ContinuationFrame, http2headersOrContinuation>(Pointer = true)]
[assembly: GoImplement<http2DataFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2DataFrame, http2streamEnder>(Pointer = true)]
[assembly: GoImplement<http2FrameHeader, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2GoAwayError, error>]
[assembly: GoImplement<http2GoAwayFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2HeadersFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2HeadersFrame, http2headersEnder>(Pointer = true)]
[assembly: GoImplement<http2HeadersFrame, http2headersOrContinuation>(Pointer = true)]
[assembly: GoImplement<http2HeadersFrame, http2streamEnder>(Pointer = true)]
[assembly: GoImplement<http2MetaHeadersFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2MetaHeadersFrame, http2headersOrContinuation>]
[assembly: GoImplement<http2MetaHeadersFrame, http2streamEnder>]
[assembly: GoImplement<http2PingFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2PriorityFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2PushPromiseFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2PushPromiseFrame, http2headersEnder>(Pointer = true)]
[assembly: GoImplement<http2PushPromiseFrame, http2headersOrContinuation>(Pointer = true)]
[assembly: GoImplement<http2RSTStreamFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2SettingsFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2StreamError, error>]
[assembly: GoImplement<http2StreamError, http2writeFramer>]
[assembly: GoImplement<http2Transport, CloseIdleConnections_closeIdler>(Pointer = true)]
[assembly: GoImplement<http2Transport, RoundTripper>(Pointer = true)]
[assembly: GoImplement<http2Transport, h2Transport>(Pointer = true)]
[assembly: GoImplement<http2UnknownFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2WindowUpdateFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2bufferedWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<http2chunkWriter, io_package.Writer>]
[assembly: GoImplement<http2clientConnPool, http2ClientConnPool>(Pointer = true)]
[assembly: GoImplement<http2clientConnPool, http2clientConnPoolIdleCloser>(Pointer = true)]
[assembly: GoImplement<http2connError, error>]
[assembly: GoImplement<http2dataBuffer, http2pipeBuffer>(Pointer = true)]
[assembly: GoImplement<http2duplicatePseudoHeaderError, error>]
[assembly: GoImplement<http2erringRoundTripper, RoundTripper>]
[assembly: GoImplement<http2erringRoundTripper, erringRoundTripper>]
[assembly: GoImplement<http2flushFrameWriter, http2writeFramer>]
[assembly: GoImplement<http2goAwayFlowError, error>]
[assembly: GoImplement<http2gzipReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<http2handlerPanicRST, http2writeFramer>]
[assembly: GoImplement<http2headerFieldNameError, error>]
[assembly: GoImplement<http2headerFieldValueError, error>]
[assembly: GoImplement<http2httpError, error>(Pointer = true)]
[assembly: GoImplement<http2httpError, net_package.ΔError>(Pointer = true)]
[assembly: GoImplement<http2missingBody, io_package.ReadCloser>]
[assembly: GoImplement<http2noBodyReader, io_package.ReadCloser>]
[assembly: GoImplement<http2noCachedConnError, error>]
[assembly: GoImplement<http2noDialClientConnPool, http2clientConnPoolIdleCloser>]
[assembly: GoImplement<http2noDialH2RoundTripper, CloseIdleConnections_closeIdler>]
[assembly: GoImplement<http2noDialH2RoundTripper, RoundTripper>]
[assembly: GoImplement<http2noDialH2RoundTripper, h2Transport>]
[assembly: GoImplement<http2pipe, http2pipeBuffer>(Pointer = true)]
[assembly: GoImplement<http2priorityWriteScheduler, http2WriteScheduler>(Pointer = true)]
[assembly: GoImplement<http2pseudoHeaderError, error>]
[assembly: GoImplement<http2randomWriteScheduler, http2WriteScheduler>(Pointer = true)]
[assembly: GoImplement<http2requestBody, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, CloseNotifier>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, Flusher>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, Pusher>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, http2stringWriter>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, io_package.StringWriter>(Pointer = true)]
[assembly: GoImplement<http2roundRobinWriteScheduler, http2WriteScheduler>(Pointer = true)]
[assembly: GoImplement<http2serverConn, http2writeContext>(Pointer = true)]
[assembly: GoImplement<http2sortPriorityNodeSiblings, sort_package.Interface>]
[assembly: GoImplement<http2sorter, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<http2stickyErrWriter, io_package.Writer>]
[assembly: GoImplement<http2timeTimer, http2timer>]
[assembly: GoImplement<http2transportResponseBody, io_package.ReadCloser>]
[assembly: GoImplement<http2write100ContinueHeadersFrame, http2writeFramer>]
[assembly: GoImplement<http2writeData, fmt_package.Stringer>(Pointer = true)]
[assembly: GoImplement<http2writeData, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writeGoAway, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writePingAck, http2writeFramer>]
[assembly: GoImplement<http2writePushPromise, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writeResHeaders, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writeSettings, http2writeFramer>]
[assembly: GoImplement<http2writeSettingsAck, http2writeFramer>]
[assembly: GoImplement<http2writeWindowUpdate, http2writeFramer>]
[assembly: GoImplement<initALPNRequest, http2ConfigureServer_baseContexter>]
[assembly: GoImplement<initALPNRequest, ΔHandler>]
[assembly: GoImplement<ioFS, FileSystem>]
[assembly: GoImplement<ioFile, File>]
[assembly: GoImplement<ioFile, go.io.fs_package.ReadDirFile>]
[assembly: GoImplement<ioFile, io_package.ReadCloser>]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.ReadCloser>]
[assembly: GoImplement<loggingConn, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<loggingConn, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<loggingConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<loggingConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<maskedSig, sniffSig>(Pointer = true)]
[assembly: GoImplement<maxBytesReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<mp4Sig, sniffSig>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriter>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
[assembly: GoImplement<net_package.TCPConn, closeWriter>(Pointer = true)]
[assembly: GoImplement<noBody, io_package.ReadCloser>]
[assembly: GoImplement<noBody, io_package.WriterTo>]
[assembly: GoImplement<nothingWrittenError, error>(Promoted = true)]
[assembly: GoImplement<nothingWrittenError, error>]
[assembly: GoImplement<onceCloseListener, net_package.Listener>(Pointer = true)]
[assembly: GoImplement<onceCloseListener, net_package.Listener>(Promoted = true)]
[assembly: GoImplement<os_package.File, File>(Pointer = true)]
[assembly: GoImplement<persistConn, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<persistConnWriter, io_package.ReaderFrom>(Pointer = true)]
[assembly: GoImplement<persistConnWriter, io_package.Writer>]
[assembly: GoImplement<populateResponse, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<readTrackingBody, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<readTrackingBody, io_package.ReadCloser>(Promoted = true)]
[assembly: GoImplement<readWriteCloserBody, io_package.ReadWriteCloser>(Pointer = true)]
[assembly: GoImplement<readWriteCloserBody, io_package.ReadWriteCloser>(Promoted = true)]
[assembly: GoImplement<redirectHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<requestBodyReadError, error>(Promoted = true)]
[assembly: GoImplement<requestBodyReadError, error>]
[assembly: GoImplement<response, CloseNotifier>(Pointer = true)]
[assembly: GoImplement<response, Flusher>(Pointer = true)]
[assembly: GoImplement<response, Hijacker>(Pointer = true)]
[assembly: GoImplement<response, Read_requestTooLarger>(Pointer = true)]
[assembly: GoImplement<response, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<response, http2stringWriter>(Pointer = true)]
[assembly: GoImplement<response, io_package.StringWriter>(Pointer = true)]
[assembly: GoImplement<response, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<rᴛ1, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<rᴛ1, io_package.Reader>]
[assembly: GoImplement<rᴛ1, io_package.WriterTo>(Promoted = true)]
[assembly: GoImplement<serverHandler, ΔHandler>]
[assembly: GoImplement<socksAddr, net_package.ΔAddr>(Pointer = true)]
[assembly: GoImplement<socksConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<socksConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<statusError, error>]
[assembly: GoImplement<stringWriter, http2stringWriter>]
[assembly: GoImplement<stringWriter, io_package.StringWriter>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<textSig, sniffSig>]
[assembly: GoImplement<timeoutError, error>(Pointer = true)]
[assembly: GoImplement<timeoutError, net_package.ΔError>(Pointer = true)]
[assembly: GoImplement<timeoutHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<timeoutWriter, Pusher>(Pointer = true)]
[assembly: GoImplement<timeoutWriter, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<tlsHandshakeTimeoutError, net_package.ΔError>]
[assembly: GoImplement<transportReadFromServerError, error>]
[assembly: GoImplement<unsupportedTEError, error>(Pointer = true)]
[assembly: GoImplement<writerOnly, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<writerOnly, io_package.Writer>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Request, ж<Request>>(Indirect = true)]
[assembly: GoImplicitConv<bufio.Writer, ж<bufio.Writer>>(Indirect = true)]
[assembly: GoImplicitConv<http2ClientConn, ж<http2ClientConn>>(Indirect = true)]
[assembly: GoImplicitConv<http2MetaHeadersFrame, ж<http2MetaHeadersFrame>>(Indirect = true)]
[assembly: GoImplicitConv<http2Server, ж<http2Server>>(Indirect = true)]
[assembly: GoImplicitConv<http2writeData, ж<http2writeData>>]
[assembly: GoImplicitConv<http2writeResHeaders, ж<http2writeResHeaders>>(Indirect = true)]
[assembly: GoImplicitConv<httptrace.ClientTrace, ж<httptrace.ClientTrace>>]
[assembly: GoImplicitConv<tls.Conn, ж<tls.Conn>>(Indirect = true)]
// </ImplicitConversions>

namespace go.net;

[GoPackage("http")]
public static partial class http_package
{
}
