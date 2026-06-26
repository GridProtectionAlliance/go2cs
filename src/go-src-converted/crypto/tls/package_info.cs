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
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.crypto.tls_package;

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
[assembly: GoTypeAlias("ConnectionState", "ΔConnectionState")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<(cbcMode, bool), cbcMode>]
[assembly: GoImplement<(context.Context, context.CancelFunc), context_package.Context>]
[assembly: GoImplement<(crypto.Decrypter, bool), crypto_package.Decrypter>]
[assembly: GoImplement<(crypto.Signer, bool), crypto_package.Signer>]
[assembly: GoImplement<(ctx context.Context, cancel context.CancelFunc), context_package.Context>]
[assembly: GoImplement<(net.Conn, error), net_package.Conn>]
[assembly: GoImplement<(net.Error, bool), net_package.ΔError>]
[assembly: GoImplement<(net.Listener, error), net_package.Listener>]
[assembly: GoImplement<Conn, net_package.Conn>]
[assembly: GoImplement<RecordHeaderError, error>]
[assembly: GoImplement<alert, error>]
[assembly: GoImplement<atLeastReader, io_package.Reader>]
[assembly: GoImplement<bool, error>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>]
[assembly: GoImplement<certificateMsg, handshakeMessage>]
[assembly: GoImplement<certificateMsgTLS13, handshakeMessage>]
[assembly: GoImplement<certificateRequestMsg, handshakeMessage>]
[assembly: GoImplement<certificateRequestMsgTLS13, handshakeMessage>]
[assembly: GoImplement<certificateStatusMsg, handshakeMessage>]
[assembly: GoImplement<certificateVerifyMsg, handshakeMessage>]
[assembly: GoImplement<cipher.Block, error), crypto.cipher_package.Block>]
[assembly: GoImplement<cipher.BlockMode, bool), crypto.cipher_package.BlockMode>]
[assembly: GoImplement<clientHelloMsg, handshakeMessage>]
[assembly: GoImplement<clientKeyExchangeMsg, handshakeMessage>]
[assembly: GoImplement<crypto_package.PrivateKey, crypto_package.Decrypter>]
[assembly: GoImplement<crypto_package.PrivateKey, crypto_package.Signer>]
[assembly: GoImplement<encryptedExtensionsMsg, handshakeMessage>]
[assembly: GoImplement<endOfEarlyDataMsg, handshakeMessage>]
[assembly: GoImplement<finishedMsg, handshakeMessage>]
[assembly: GoImplement<hash_package.Hash, io_package.Writer>]
[assembly: GoImplement<helloRequestMsg, handshakeMessage>]
[assembly: GoImplement<keyUpdateMsg, handshakeMessage>]
[assembly: GoImplement<listener, net_package.Listener>]
[assembly: GoImplement<lruSessionCache, ClientSessionCache>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.OpError, error>]
[assembly: GoImplement<net_package.ΔError, error>]
[assembly: GoImplement<newSessionTicketMsg, handshakeMessage>]
[assembly: GoImplement<newSessionTicketMsgTLS13, handshakeMessage>]
[assembly: GoImplement<serverHelloDoneMsg, handshakeMessage>]
[assembly: GoImplement<serverHelloMsg, handshakeMessage>]
[assembly: GoImplement<serverKeyExchangeMsg, handshakeMessage>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<AlertError, alert>(Inverted = true, ValueType = "AlertError")]
[assembly: GoImplicitConv<Config, ж<Config>>(Indirect = true)]
[assembly: GoImplicitConv<QUICConfig, ж<QUICConfig>>(Indirect = true)]
[assembly: GoImplicitConv<alert, AlertError>(Inverted = true, ValueType = "alert")]
[assembly: GoImplicitConv<rsa.PSSOptions, ж<rsa.PSSOptions>>(Indirect = true)]
[assembly: GoImplicitConv<struct{wall uint64; ext int64; loc *time.Location}, struct{wall uint64; ext int64; loc *time.Location}>(Inverted = true)]
// </ImplicitConversions>

namespace go.crypto;

[GoPackage("tls")]
public static partial class tls_package
{
}
