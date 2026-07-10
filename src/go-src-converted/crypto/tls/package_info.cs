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
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using ecdhꓸCurve = go.crypto.ecdh_package.ΔCurve;
global using ecdhꓸPublicKey = go.crypto.ecdh_package.ΔPublicKey;
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
using net = go.net_package;
using rsa = go.crypto.rsa_package;
using Δx509 = go.crypto.x509_package;
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
[assembly: GoImplement<CertificateVerificationError, error>(Pointer = true)]
[assembly: GoImplement<Conn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<ECHRejectionError, error>(Pointer = true)]
[assembly: GoImplement<RecordHeaderError, error>]
[assembly: GoImplement<alert, error>]
[assembly: GoImplement<atLeastReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<certificateMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<certificateMsgTLS13, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<certificateRequestMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<certificateRequestMsgTLS13, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<certificateStatusMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<certificateVerifyMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<clientHelloMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<clientKeyExchangeMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<crypto_package.Hash, crypto_package.SignerOpts>]
[assembly: GoImplement<cthWrapper, hash_package.Hash>(Pointer = true)]
[assembly: GoImplement<ecdheKeyAgreement, keyAgreement>(Pointer = true)]
[assembly: GoImplement<encryptedExtensionsMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<endOfEarlyDataMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<finishedMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<go.crypto.rsa_package.PSSOptions, crypto_package.SignerOpts>(Pointer = true)]
[assembly: GoImplement<hash_package.Hash, transcriptHash>]
[assembly: GoImplement<helloRequestMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<keyUpdateMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<listener, net_package.Listener>(Pointer = true)]
[assembly: GoImplement<listener, net_package.Listener>(Promoted = true)]
[assembly: GoImplement<lruSessionCache, ClientSessionCache>(Pointer = true)]
[assembly: GoImplement<marshalingFunction, go.vendor.golang.org.x.crypto.cryptobyte_package.MarshalingValue>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.ΔError, error>]
[assembly: GoImplement<newSessionTicketMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<newSessionTicketMsgTLS13, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<permanentError, error>(Pointer = true)]
[assembly: GoImplement<prefixNonceAEAD, aead>(Pointer = true)]
[assembly: GoImplement<rsaKeyAgreement, keyAgreement>]
[assembly: GoImplement<serverHelloDoneMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<serverHelloMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<serverKeyExchangeMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<xorNonceAEAD, aead>(Pointer = true)]
[assembly: GoImplement<ΔfinishedHash, transcriptHash>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<AlertError, alert>(Inverted = true, ValueType = "AlertError")]
[assembly: GoImplicitConv<Certificate, ж<Certificate>>(Indirect = true)]
[assembly: GoImplicitConv<Config, ж<Config>>(Indirect = true)]
[assembly: GoImplicitConv<QUICConfig, ж<QUICConfig>>(Indirect = true)]
[assembly: GoImplicitConv<SessionState, ж<SessionState>>]
[assembly: GoImplicitConv<alert, AlertError>(Inverted = true, ValueType = "alert")]
[assembly: GoImplicitConv<clientHelloMsg, ж<clientHelloMsg>>(Indirect = true)]
[assembly: GoImplicitConv<clientKeyExchangeMsg, ж<clientKeyExchangeMsg>>(Indirect = true)]
[assembly: GoImplicitConv<echContext, ж<echContext>>(Indirect = true)]
[assembly: GoImplicitConv<rsa.PSSOptions, ж<rsa.PSSOptions>>(Indirect = true)]
[assembly: GoImplicitConv<serverHelloMsg, ж<serverHelloMsg>>(Indirect = true)]
[assembly: GoImplicitConv<serverKeyExchangeMsg, ж<serverKeyExchangeMsg>>(Indirect = true)]
[assembly: GoImplicitConv<Δx509.Certificate, ж<Δx509.Certificate>>(Indirect = true)]
// </ImplicitConversions>

namespace go.crypto;

[GoPackage("tls")]
public static partial class tls_package
{
}
