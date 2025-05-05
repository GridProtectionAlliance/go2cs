// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using list = container.list_package;
using context = context_package;
using crypto = crypto_package;
using ecdsa = crypto.ecdsa_package;
using ed25519 = crypto.ed25519_package;
using elliptic = crypto.elliptic_package;
using rand = crypto.rand_package;
using rsa = crypto.rsa_package;
using sha512 = crypto.sha512_package;
using x509 = crypto.x509_package;
using errors = errors_package;
using fmt = fmt_package;
using godebug = @internal.godebug_package;
using io = io_package;
using net = net_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using _ = unsafe_package; // for linkname
using @internal;
using container;

partial class tls_package {

public static readonly UntypedInt VersionTLS10 = /* 0x0301 */ 769;
public static readonly UntypedInt VersionTLS11 = /* 0x0302 */ 770;
public static readonly UntypedInt VersionTLS12 = /* 0x0303 */ 771;
public static readonly UntypedInt VersionTLS13 = /* 0x0304 */ 772;
public static readonly UntypedInt VersionSSL30 = /* 0x0300 */ 768;

// VersionName returns the name for the provided TLS version number
// (e.g. "TLS 1.3"), or a fallback representation of the value if the
// version is not implemented by this package.
public static @string VersionName(uint16 version) {
    switch (version) {
    case VersionSSL30: {
        return "SSLv3"u8;
    }
    case VersionTLS10: {
        return "TLS 1.0"u8;
    }
    case VersionTLS11: {
        return "TLS 1.1"u8;
    }
    case VersionTLS12: {
        return "TLS 1.2"u8;
    }
    case VersionTLS13: {
        return "TLS 1.3"u8;
    }
    default: {
        return fmt.Sprintf("0x%04X"u8, version);
    }}

}

internal static readonly UntypedInt maxPlaintext = 16384; // maximum plaintext payload length
internal static readonly UntypedInt maxCiphertext = /* 16384 + 2048 */ 18432; // maximum ciphertext payload length
internal static readonly UntypedInt maxCiphertextTLS13 = /* 16384 + 256 */ 16640; // maximum ciphertext length in TLS 1.3
internal static readonly UntypedInt recordHeaderLen = 5; // record header length
internal static readonly UntypedInt maxHandshake = 65536; // maximum handshake we support (protocol max is 16 MB)
internal static readonly UntypedInt maxHandshakeCertificateMsg = 262144; // maximum certificate message size (256 KiB)
internal static readonly UntypedInt maxUselessRecords = 16; // maximum number of consecutive non-advancing records

[GoType("num:uint8")] partial struct recordType;

internal static readonly recordType recordTypeChangeCipherSpec = 20;
internal static readonly recordType recordTypeAlert = 21;
internal static readonly recordType recordTypeHandshake = 22;
internal static readonly recordType recordTypeApplicationData = 23;

// TLS handshake message types.
internal const uint8 typeHelloRequest = 0;

internal const uint8 typeClientHello = 1;

internal const uint8 typeServerHello = 2;

internal const uint8 typeNewSessionTicket = 4;

internal const uint8 typeEndOfEarlyData = 5;

internal const uint8 typeEncryptedExtensions = 8;

internal const uint8 typeCertificate = 11;

internal const uint8 typeServerKeyExchange = 12;

internal const uint8 typeCertificateRequest = 13;

internal const uint8 typeServerHelloDone = 14;

internal const uint8 typeCertificateVerify = 15;

internal const uint8 typeClientKeyExchange = 16;

internal const uint8 typeFinished = 20;

internal const uint8 typeCertificateStatus = 22;

internal const uint8 typeKeyUpdate = 24;

internal const uint8 typeMessageHash = 254;       // synthetic message

// TLS compression types.
internal const uint8 compressionNone = 0;

// TLS extension numbers
internal const uint16 extensionServerName = 0;

internal const uint16 extensionStatusRequest = 5;

internal const uint16 extensionSupportedCurves = 10;      // supported_groups in TLS 1.3, see RFC 8446, Section 4.2.7

internal const uint16 extensionSupportedPoints = 11;

internal const uint16 extensionSignatureAlgorithms = 13;

internal const uint16 extensionALPN = 16;

internal const uint16 extensionSCT = 18;

internal const uint16 extensionExtendedMasterSecret = 23;

internal const uint16 extensionSessionTicket = 35;

internal const uint16 extensionPreSharedKey = 41;

internal const uint16 extensionEarlyData = 42;

internal const uint16 extensionSupportedVersions = 43;

internal const uint16 extensionCookie = 44;

internal const uint16 extensionPSKModes = 45;

internal const uint16 extensionCertificateAuthorities = 47;

internal const uint16 extensionSignatureAlgorithmsCert = 50;

internal const uint16 extensionKeyShare = 51;

internal const uint16 extensionQUICTransportParameters = 57;

internal const uint16 extensionRenegotiationInfo = /* 0xff01 */ 65281;

internal const uint16 extensionECHOuterExtensions = /* 0xfd00 */ 64768;

internal const uint16 extensionEncryptedClientHello = /* 0xfe0d */ 65037;

// TLS signaling cipher suite values
internal const uint16 scsvRenegotiation = /* 0x00ff */ 255;

[GoType("num:uint16")] partial struct CurveID;

public static readonly CurveID CurveP256 = 23;
public static readonly CurveID CurveP384 = 24;
public static readonly CurveID CurveP521 = 25;
public static readonly CurveID X25519 = 29;
internal static readonly CurveID x25519Kyber768Draft00 = /* 0x6399 */ 25497;  // X25519Kyber768Draft00

// TLS 1.3 Key Share. See RFC 8446, Section 4.2.8.
[GoType] partial struct keyShare {
    internal CurveID group;
    internal slice<byte> data;
}

// TLS 1.3 PSK Key Exchange Modes. See RFC 8446, Section 4.2.9.
internal const uint8 pskModePlain = 0;

internal const uint8 pskModeDHE = 1;

// TLS 1.3 PSK Identity. Can be a Session Ticket, or a reference to a saved
// session. See RFC 8446, Section 4.2.11.
[GoType] partial struct pskIdentity {
    internal slice<byte> label;
    internal uint32 obfuscatedTicketAge;
}

// TLS Elliptic Curve Point Formats
// https://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-9
internal const uint8 pointFormatUncompressed = 0;

// TLS CertificateStatusType (RFC 3546)
internal const uint8 statusTypeOCSP = 1;

// Certificate types (for certificateRequestMsg)
internal static readonly UntypedInt certTypeRSASign = 1;

internal static readonly UntypedInt certTypeECDSASign = 64; // ECDSA or EdDSA keys, see RFC 8422, Section 3.

// Signature algorithms (for internal signaling use). Starting at 225 to avoid overlap with
// TLS 1.2 codepoints (RFC 5246, Appendix A.4.1), with which these have nothing to do.
internal const uint8 signaturePKCS1v15 = /* iota + 225 */ 225;

internal const uint8 signatureRSAPSS = 226;

internal const uint8 signatureECDSA = 227;

internal const uint8 signatureEd25519 = 228;

// directSigning is a standard Hash value that signals that no pre-hashing
// should be performed, and that the input should be signed directly. It is the
// hash function associated with the Ed25519 signature scheme.
internal static crypto.Hash directSigning = 0;

// See RFC 8446, Section 4.1.3.
// helloRetryRequestRandom is set as the Random value of a ServerHello
// to signal that the message is actually a HelloRetryRequest.
internal static slice<byte> helloRetryRequestRandom = new byte[]{
    207, 33, 173, 116, 229, 154, 97, 17,
    190, 29, 140, 2, 30, 101, 184, 145,
    194, 162, 17, 22, 122, 187, 140, 94,
    7, 158, 9, 226, 200, 168, 51, 156
}.slice();

internal static readonly @string downgradeCanaryTLS12 = "DOWNGRD\x01"u8;
internal static readonly @string downgradeCanaryTLS11 = "DOWNGRD\x00"u8;

// testingOnlyForceDowngradeCanary is set in tests to force the server side to
// include downgrade canaries even if it's using its highers supported version.
internal static bool testingOnlyForceDowngradeCanary;

// ConnectionState records basic TLS details about the connection.
[GoType] partial struct ΔConnectionState {
    // Version is the TLS version used by the connection (e.g. VersionTLS12).
    public uint16 Version;
    // HandshakeComplete is true if the handshake has concluded.
    public bool HandshakeComplete;
    // DidResume is true if this connection was successfully resumed from a
    // previous session with a session ticket or similar mechanism.
    public bool DidResume;
    // CipherSuite is the cipher suite negotiated for the connection (e.g.
    // TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, TLS_AES_128_GCM_SHA256).
    public uint16 CipherSuite;
    // NegotiatedProtocol is the application protocol negotiated with ALPN.
    public @string NegotiatedProtocol;
    // NegotiatedProtocolIsMutual used to indicate a mutual NPN negotiation.
    //
    // Deprecated: this value is always true.
    public bool NegotiatedProtocolIsMutual;
    // ServerName is the value of the Server Name Indication extension sent by
    // the client. It's available both on the server and on the client side.
    public @string ServerName;
    // PeerCertificates are the parsed certificates sent by the peer, in the
    // order in which they were sent. The first element is the leaf certificate
    // that the connection is verified against.
    //
    // On the client side, it can't be empty. On the server side, it can be
    // empty if Config.ClientAuth is not RequireAnyClientCert or
    // RequireAndVerifyClientCert.
    //
    // PeerCertificates and its contents should not be modified.
    public x509.Certificate PeerCertificates;
    // VerifiedChains is a list of one or more chains where the first element is
    // PeerCertificates[0] and the last element is from Config.RootCAs (on the
    // client side) or Config.ClientCAs (on the server side).
    //
    // On the client side, it's set if Config.InsecureSkipVerify is false. On
    // the server side, it's set if Config.ClientAuth is VerifyClientCertIfGiven
    // (and the peer provided a certificate) or RequireAndVerifyClientCert.
    //
    // VerifiedChains and its contents should not be modified.
    public x509.Certificate VerifiedChains;
    // SignedCertificateTimestamps is a list of SCTs provided by the peer
    // through the TLS handshake for the leaf certificate, if any.
    public slice<slice<byte>> SignedCertificateTimestamps;
    // OCSPResponse is a stapled Online Certificate Status Protocol (OCSP)
    // response provided by the peer for the leaf certificate, if any.
    public slice<byte> OCSPResponse;
    // TLSUnique contains the "tls-unique" channel binding value (see RFC 5929,
    // Section 3). This value will be nil for TLS 1.3 connections and for
    // resumed connections that don't support Extended Master Secret (RFC 7627).
    public slice<byte> TLSUnique;
    // ECHAccepted indicates if Encrypted Client Hello was offered by the client
    // and accepted by the server. Currently, ECH is supported only on the
    // client side.
    public bool ECHAccepted;
    // ekm is a closure exposed via ExportKeyingMaterial.
    internal Func<@string, slice<byte>, nint, (<>byte, error)> ekm;
    // testingOnlyDidHRR is true if a HelloRetryRequest was sent/received.
    internal bool testingOnlyDidHRR;
    // testingOnlyCurveID is the selected CurveID, or zero if an RSA exchanges
    // is performed.
    internal CurveID testingOnlyCurveID;
}

// ExportKeyingMaterial returns length bytes of exported key material in a new
// slice as defined in RFC 5705. If context is nil, it is not used as part of
// the seed. If the connection was set to allow renegotiation via
// Config.Renegotiation, or if the connections supports neither TLS 1.3 nor
// Extended Master Secret, this function will return an error.
//
// Exporting key material without Extended Master Secret or TLS 1.3 was disabled
// in Go 1.22 due to security issues (see the Security Considerations sections
// of RFC 5705 and RFC 7627), but can be re-enabled with the GODEBUG setting
// tlsunsafeekm=1.
[GoRecv] public static (slice<byte>, error) ExportKeyingMaterial(this ref ΔConnectionState cs, @string label, slice<byte> context, nint length) {
    return cs.ekm(label, context, length);
}

[GoType("num:nint")] partial struct ClientAuthType;

public static readonly ClientAuthType NoClientCert = /* iota */ 0;
public static readonly ClientAuthType RequestClientCert = 1;
public static readonly ClientAuthType RequireAnyClientCert = 2;
public static readonly ClientAuthType VerifyClientCertIfGiven = 3;
public static readonly ClientAuthType RequireAndVerifyClientCert = 4;

// requiresClientCert reports whether the ClientAuthType requires a client
// certificate to be provided.
internal static bool requiresClientCert(ClientAuthType c) {
    var exprᴛ1 = c;
    if (exprᴛ1 == RequireAnyClientCert || exprᴛ1 == RequireAndVerifyClientCert) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// ClientSessionCache is a cache of ClientSessionState objects that can be used
// by a client to resume a TLS session with a given server. ClientSessionCache
// implementations should expect to be called concurrently from different
// goroutines. Up to TLS 1.2, only ticket-based resumption is supported, not
// SessionID-based resumption. In TLS 1.3 they were merged into PSK modes, which
// are supported via this interface.
[GoType] partial interface ClientSessionCache {
    // Get searches for a ClientSessionState associated with the given key.
    // On return, ok is true if one was found.
    (ж<ClientSessionState> session, bool ok) Get(@string sessionKey);
    // Put adds the ClientSessionState to the cache with the given key. It might
    // get called multiple times in a connection if a TLS 1.3 server provides
    // more than one session ticket. If called with a nil *ClientSessionState,
    // it should remove the cache entry.
    void Put(@string sessionKey, ж<ClientSessionState> cs);
}

[GoType("num:uint16")] partial struct SignatureScheme;

//go:generate stringer -linecomment -type=SignatureScheme,CurveID,ClientAuthType -output=common_string.go
public static readonly SignatureScheme PKCS1WithSHA256 = /* 0x0401 */ 1025;
public static readonly SignatureScheme PKCS1WithSHA384 = /* 0x0501 */ 1281;
public static readonly SignatureScheme PKCS1WithSHA512 = /* 0x0601 */ 1537;
public static readonly SignatureScheme PSSWithSHA256 = /* 0x0804 */ 2052;
public static readonly SignatureScheme PSSWithSHA384 = /* 0x0805 */ 2053;
public static readonly SignatureScheme PSSWithSHA512 = /* 0x0806 */ 2054;
public static readonly SignatureScheme ECDSAWithP256AndSHA256 = /* 0x0403 */ 1027;
public static readonly SignatureScheme ECDSAWithP384AndSHA384 = /* 0x0503 */ 1283;
public static readonly SignatureScheme ECDSAWithP521AndSHA512 = /* 0x0603 */ 1539;
public static readonly SignatureScheme Ed25519 = /* 0x0807 */ 2055;
public static readonly SignatureScheme PKCS1WithSHA1 = /* 0x0201 */ 513;
public static readonly SignatureScheme ECDSAWithSHA1 = /* 0x0203 */ 515;

// ClientHelloInfo contains information from a ClientHello message in order to
// guide application logic in the GetCertificate and GetConfigForClient callbacks.
[GoType] partial struct ClientHelloInfo {
    // CipherSuites lists the CipherSuites supported by the client (e.g.
    // TLS_AES_128_GCM_SHA256, TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256).
    public slice<uint16> CipherSuites;
    // ServerName indicates the name of the server requested by the client
    // in order to support virtual hosting. ServerName is only set if the
    // client is using SNI (see RFC 4366, Section 3.1).
    public @string ServerName;
    // SupportedCurves lists the elliptic curves supported by the client.
    // SupportedCurves is set only if the Supported Elliptic Curves
    // Extension is being used (see RFC 4492, Section 5.1.1).
    public slice<CurveID> SupportedCurves;
    // SupportedPoints lists the point formats supported by the client.
    // SupportedPoints is set only if the Supported Point Formats Extension
    // is being used (see RFC 4492, Section 5.1.2).
    public slice<uint8> SupportedPoints;
    // SignatureSchemes lists the signature and hash schemes that the client
    // is willing to verify. SignatureSchemes is set only if the Signature
    // Algorithms Extension is being used (see RFC 5246, Section 7.4.1.4.1).
    public slice<SignatureScheme> SignatureSchemes;
    // SupportedProtos lists the application protocols supported by the client.
    // SupportedProtos is set only if the Application-Layer Protocol
    // Negotiation Extension is being used (see RFC 7301, Section 3.1).
    //
    // Servers can select a protocol by setting Config.NextProtos in a
    // GetConfigForClient return value.
    public slice<@string> SupportedProtos;
    // SupportedVersions lists the TLS versions supported by the client.
    // For TLS versions less than 1.3, this is extrapolated from the max
    // version advertised by the client, so values other than the greatest
    // might be rejected if used.
    public slice<uint16> SupportedVersions;
    // Conn is the underlying net.Conn for the connection. Do not read
    // from, or write to, this connection; that will cause the TLS
    // connection to fail.
    public net_package.Conn Conn;
    // config is embedded by the GetCertificate or GetConfigForClient caller,
    // for use with SupportsCertificate.
    internal ж<Config> config;
    // ctx is the context of the handshake that is in progress.
    internal context_package.Context ctx;
}

// Context returns the context of the handshake that is in progress.
// This context is a child of the context passed to HandshakeContext,
// if any, and is canceled when the handshake concludes.
[GoRecv] public static context.Context Context(this ref ClientHelloInfo c) {
    return c.ctx;
}

// CertificateRequestInfo contains information from a server's
// CertificateRequest message, which is used to demand a certificate and proof
// of control from a client.
[GoType] partial struct CertificateRequestInfo {
    // AcceptableCAs contains zero or more, DER-encoded, X.501
    // Distinguished Names. These are the names of root or intermediate CAs
    // that the server wishes the returned certificate to be signed by. An
    // empty slice indicates that the server has no preference.
    public slice<slice<byte>> AcceptableCAs;
    // SignatureSchemes lists the signature schemes that the server is
    // willing to verify.
    public slice<SignatureScheme> SignatureSchemes;
    // Version is the TLS version that was negotiated for this connection.
    public uint16 Version;
    // ctx is the context of the handshake that is in progress.
    internal context_package.Context ctx;
}

// Context returns the context of the handshake that is in progress.
// This context is a child of the context passed to HandshakeContext,
// if any, and is canceled when the handshake concludes.
[GoRecv] public static context.Context Context(this ref CertificateRequestInfo c) {
    return c.ctx;
}

[GoType("num:nint")] partial struct RenegotiationSupport;

public static readonly RenegotiationSupport RenegotiateNever = /* iota */ 0;
public static readonly RenegotiationSupport RenegotiateOnceAsClient = 1;
public static readonly RenegotiationSupport RenegotiateFreelyAsClient = 2;

// A Config structure is used to configure a TLS client or server.
// After one has been passed to a TLS function it must not be
// modified. A Config may be reused; the tls package will also not
// modify it.
[GoType] partial struct Config {
    // Rand provides the source of entropy for nonces and RSA blinding.
    // If Rand is nil, TLS uses the cryptographic random reader in package
    // crypto/rand.
    // The Reader must be safe for use by multiple goroutines.
    public io_package.Reader Rand;
    // Time returns the current time as the number of seconds since the epoch.
    // If Time is nil, TLS uses time.Now.
    public Func<time.Time> Time;
    // Certificates contains one or more certificate chains to present to the
    // other side of the connection. The first certificate compatible with the
    // peer's requirements is selected automatically.
    //
    // Server configurations must set one of Certificates, GetCertificate or
    // GetConfigForClient. Clients doing client-authentication may set either
    // Certificates or GetClientCertificate.
    //
    // Note: if there are multiple Certificates, and they don't have the
    // optional field Leaf set, certificate selection will incur a significant
    // per-handshake performance cost.
    public slice<Certificate> Certificates;
    // NameToCertificate maps from a certificate name to an element of
    // Certificates. Note that a certificate name can be of the form
    // '*.example.com' and so doesn't have to be a domain name as such.
    //
    // Deprecated: NameToCertificate only allows associating a single
    // certificate with a given name. Leave this field nil to let the library
    // select the first compatible chain from Certificates.
    public map<@string, ж<Certificate>> NameToCertificate;
    // GetCertificate returns a Certificate based on the given
    // ClientHelloInfo. It will only be called if the client supplies SNI
    // information or if Certificates is empty.
    //
    // If GetCertificate is nil or returns nil, then the certificate is
    // retrieved from NameToCertificate. If NameToCertificate is nil, the
    // best element of Certificates will be used.
    //
    // Once a Certificate is returned it should not be modified.
    public tls.Certificate, error) GetCertificate;
    // GetClientCertificate, if not nil, is called when a server requests a
    // certificate from a client. If set, the contents of Certificates will
    // be ignored.
    //
    // If GetClientCertificate returns an error, the handshake will be
    // aborted and that error will be returned. Otherwise
    // GetClientCertificate must return a non-nil Certificate. If
    // Certificate.Certificate is empty then no certificate will be sent to
    // the server. If this is unacceptable to the server then it may abort
    // the handshake.
    //
    // GetClientCertificate may be called multiple times for the same
    // connection if renegotiation occurs or if TLS 1.3 is in use.
    //
    // Once a Certificate is returned it should not be modified.
    public tls.Certificate, error) GetClientCertificate;
    // GetConfigForClient, if not nil, is called after a ClientHello is
    // received from a client. It may return a non-nil Config in order to
    // change the Config that will be used to handle this connection. If
    // the returned Config is nil, the original Config will be used. The
    // Config returned by this callback may not be subsequently modified.
    //
    // If GetConfigForClient is nil, the Config passed to Server() will be
    // used for all connections.
    //
    // If SessionTicketKey was explicitly set on the returned Config, or if
    // SetSessionTicketKeys was called on the returned Config, those keys will
    // be used. Otherwise, the original Config keys will be used (and possibly
    // rotated if they are automatically managed).
    public tls.Config, error) GetConfigForClient;
    // VerifyPeerCertificate, if not nil, is called after normal
    // certificate verification by either a TLS client or server. It
    // receives the raw ASN.1 certificates provided by the peer and also
    // any verified chains that normal processing found. If it returns a
    // non-nil error, the handshake is aborted and that error results.
    //
    // If normal verification fails then the handshake will abort before
    // considering this callback. If normal verification is disabled (on the
    // client when InsecureSkipVerify is set, or on a server when ClientAuth is
    // RequestClientCert or RequireAnyClientCert), then this callback will be
    // considered but the verifiedChains argument will always be nil. When
    // ClientAuth is NoClientCert, this callback is not called on the server.
    // rawCerts may be empty on the server if ClientAuth is RequestClientCert or
    // VerifyClientCertIfGiven.
    //
    // This callback is not invoked on resumed connections, as certificates are
    // not re-verified on resumption.
    //
    // verifiedChains and its contents should not be modified.
    public x509.Certificate) error VerifyPeerCertificate;
    // VerifyConnection, if not nil, is called after normal certificate
    // verification and after VerifyPeerCertificate by either a TLS client
    // or server. If it returns a non-nil error, the handshake is aborted
    // and that error results.
    //
    // If normal verification fails then the handshake will abort before
    // considering this callback. This callback will run for all connections,
    // including resumptions, regardless of InsecureSkipVerify or ClientAuth
    // settings.
    public Func<ΔConnectionState, error> VerifyConnection;
    // RootCAs defines the set of root certificate authorities
    // that clients use when verifying server certificates.
    // If RootCAs is nil, TLS uses the host's root CA set.
    public ж<crypto.x509_package.CertPool> RootCAs;
    // NextProtos is a list of supported application level protocols, in
    // order of preference. If both peers support ALPN, the selected
    // protocol will be one from this list, and the connection will fail
    // if there is no mutually supported protocol. If NextProtos is empty
    // or the peer doesn't support ALPN, the connection will succeed and
    // ConnectionState.NegotiatedProtocol will be empty.
    public slice<@string> NextProtos;
    // ServerName is used to verify the hostname on the returned
    // certificates unless InsecureSkipVerify is given. It is also included
    // in the client's handshake to support virtual hosting unless it is
    // an IP address.
    public @string ServerName;
    // ClientAuth determines the server's policy for
    // TLS Client Authentication. The default is NoClientCert.
    public ClientAuthType ClientAuth;
    // ClientCAs defines the set of root certificate authorities
    // that servers use if required to verify a client certificate
    // by the policy in ClientAuth.
    public ж<crypto.x509_package.CertPool> ClientCAs;
    // InsecureSkipVerify controls whether a client verifies the server's
    // certificate chain and host name. If InsecureSkipVerify is true, crypto/tls
    // accepts any certificate presented by the server and any host name in that
    // certificate. In this mode, TLS is susceptible to machine-in-the-middle
    // attacks unless custom verification is used. This should be used only for
    // testing or in combination with VerifyConnection or VerifyPeerCertificate.
    public bool InsecureSkipVerify;
    // CipherSuites is a list of enabled TLS 1.0–1.2 cipher suites. The order of
    // the list is ignored. Note that TLS 1.3 ciphersuites are not configurable.
    //
    // If CipherSuites is nil, a safe default list is used. The default cipher
    // suites might change over time. In Go 1.22 RSA key exchange based cipher
    // suites were removed from the default list, but can be re-added with the
    // GODEBUG setting tlsrsakex=1. In Go 1.23 3DES cipher suites were removed
    // from the default list, but can be re-added with the GODEBUG setting
    // tls3des=1.
    public slice<uint16> CipherSuites;
    // PreferServerCipherSuites is a legacy field and has no effect.
    //
    // It used to control whether the server would follow the client's or the
    // server's preference. Servers now select the best mutually supported
    // cipher suite based on logic that takes into account inferred client
    // hardware, server hardware, and security.
    //
    // Deprecated: PreferServerCipherSuites is ignored.
    public bool PreferServerCipherSuites;
    // SessionTicketsDisabled may be set to true to disable session ticket and
    // PSK (resumption) support. Note that on clients, session ticket support is
    // also disabled if ClientSessionCache is nil.
    public bool SessionTicketsDisabled;
    // SessionTicketKey is used by TLS servers to provide session resumption.
    // See RFC 5077 and the PSK mode of RFC 8446. If zero, it will be filled
    // with random data before the first server handshake.
    //
    // Deprecated: if this field is left at zero, session ticket keys will be
    // automatically rotated every day and dropped after seven days. For
    // customizing the rotation schedule or synchronizing servers that are
    // terminating connections for the same host, use SetSessionTicketKeys.
    public array<byte> SessionTicketKey = new(32);
    // ClientSessionCache is a cache of ClientSessionState entries for TLS
    // session resumption. It is only used by clients.
    public ClientSessionCache ClientSessionCache;
    // UnwrapSession is called on the server to turn a ticket/identity
    // previously produced by [WrapSession] into a usable session.
    //
    // UnwrapSession will usually either decrypt a session state in the ticket
    // (for example with [Config.EncryptTicket]), or use the ticket as a handle
    // to recover a previously stored state. It must use [ParseSessionState] to
    // deserialize the session state.
    //
    // If UnwrapSession returns an error, the connection is terminated. If it
    // returns (nil, nil), the session is ignored. crypto/tls may still choose
    // not to resume the returned session.
    public tls.SessionState, error) UnwrapSession;
    // WrapSession is called on the server to produce a session ticket/identity.
    //
    // WrapSession must serialize the session state with [SessionState.Bytes].
    // It may then encrypt the serialized state (for example with
    // [Config.DecryptTicket]) and use it as the ticket, or store the state and
    // return a handle for it.
    //
    // If WrapSession returns an error, the connection is terminated.
    //
    // Warning: the return value will be exposed on the wire and to clients in
    // plaintext. The application is in charge of encrypting and authenticating
    // it (and rotating keys) or returning high-entropy identifiers. Failing to
    // do so correctly can compromise current, previous, and future connections
    // depending on the protocol version.
    public tls.SessionState) (<>byte, error) WrapSession;
    // MinVersion contains the minimum TLS version that is acceptable.
    //
    // By default, TLS 1.2 is currently used as the minimum. TLS 1.0 is the
    // minimum supported by this package.
    //
    // The server-side default can be reverted to TLS 1.0 by including the value
    // "tls10server=1" in the GODEBUG environment variable.
    public uint16 MinVersion;
    // MaxVersion contains the maximum TLS version that is acceptable.
    //
    // By default, the maximum version supported by this package is used,
    // which is currently TLS 1.3.
    public uint16 MaxVersion;
    // CurvePreferences contains the elliptic curves that will be used in
    // an ECDHE handshake, in preference order. If empty, the default will
    // be used. The client will use the first preference as the type for
    // its key share in TLS 1.3. This may change in the future.
    //
    // From Go 1.23, the default includes the X25519Kyber768Draft00 hybrid
    // post-quantum key exchange. To disable it, set CurvePreferences explicitly
    // or use the GODEBUG=tlskyber=0 environment variable.
    public slice<CurveID> CurvePreferences;
    // DynamicRecordSizingDisabled disables adaptive sizing of TLS records.
    // When true, the largest possible TLS record size is always used. When
    // false, the size of TLS records may be adjusted in an attempt to
    // improve latency.
    public bool DynamicRecordSizingDisabled;
    // Renegotiation controls what types of renegotiation are supported.
    // The default, none, is correct for the vast majority of applications.
    public RenegotiationSupport Renegotiation;
    // KeyLogWriter optionally specifies a destination for TLS master secrets
    // in NSS key log format that can be used to allow external programs
    // such as Wireshark to decrypt TLS connections.
    // See https://developer.mozilla.org/en-US/docs/Mozilla/Projects/NSS/Key_Log_Format.
    // Use of KeyLogWriter compromises security and should only be
    // used for debugging.
    public io_package.Writer KeyLogWriter;
    // EncryptedClientHelloConfigList is a serialized ECHConfigList. If
    // provided, clients will attempt to connect to servers using Encrypted
    // Client Hello (ECH) using one of the provided ECHConfigs. Servers
    // currently ignore this field.
    //
    // If the list contains no valid ECH configs, the handshake will fail
    // and return an error.
    //
    // If EncryptedClientHelloConfigList is set, MinVersion, if set, must
    // be VersionTLS13.
    //
    // When EncryptedClientHelloConfigList is set, the handshake will only
    // succeed if ECH is sucessfully negotiated. If the server rejects ECH,
    // an ECHRejectionError error will be returned, which may contain a new
    // ECHConfigList that the server suggests using.
    //
    // How this field is parsed may change in future Go versions, if the
    // encoding described in the final Encrypted Client Hello RFC changes.
    public slice<byte> EncryptedClientHelloConfigList;
    // EncryptedClientHelloRejectionVerify, if not nil, is called when ECH is
    // rejected, in order to verify the ECH provider certificate in the outer
    // Client Hello. If it returns a non-nil error, the handshake is aborted and
    // that error results.
    //
    // Unlike VerifyPeerCertificate and VerifyConnection, normal certificate
    // verification will not be performed before calling
    // EncryptedClientHelloRejectionVerify.
    //
    // If EncryptedClientHelloRejectionVerify is nil and ECH is rejected, the
    // roots in RootCAs will be used to verify the ECH providers public
    // certificate. VerifyPeerCertificate and VerifyConnection are not called
    // when ECH is rejected, even if set, and InsecureSkipVerify is ignored.
    public Func<ΔConnectionState, error> EncryptedClientHelloRejectionVerify;
    // mutex protects sessionTicketKeys and autoSessionTicketKeys.
    internal sync_package.RWMutex mutex;
    // sessionTicketKeys contains zero or more ticket keys. If set, it means
    // the keys were set with SessionTicketKey or SetSessionTicketKeys. The
    // first key is used for new tickets and any subsequent keys can be used to
    // decrypt old tickets. The slice contents are not protected by the mutex
    // and are immutable.
    internal slice<ticketKey> sessionTicketKeys;
    // autoSessionTicketKeys is like sessionTicketKeys but is owned by the
    // auto-rotation logic. See Config.ticketKeys.
    internal slice<ticketKey> autoSessionTicketKeys;
}

internal static readonly time.Duration ticketKeyLifetime = /* 7 * 24 * time.Hour */ 604800000000000; // 7 days
internal static readonly time.Duration ticketKeyRotation = /* 24 * time.Hour */ 86400000000000;

// ticketKey is the internal representation of a session ticket key.
[GoType] partial struct ticketKey {
    internal array<byte> aesKey = new(16);
    internal array<byte> hmacKey = new(16);
    // created is the time at which this ticket key was created. See Config.ticketKeys.
    internal time_package.Time created;
}

// ticketKeyFromBytes converts from the external representation of a session
// ticket key to a ticketKey. Externally, session ticket keys are 32 random
// bytes and this function expands that into sufficient name and key material.
[GoRecv] internal static ticketKey /*key*/ ticketKeyFromBytes(this ref Config c, array<byte> b) {
    ticketKey key = default!;

    b = b.Clone();
    var hashed = sha512.Sum512(b[..]);
    // The first 16 bytes of the hash used to be exposed on the wire as a ticket
    // prefix. They MUST NOT be used as a secret. In the future, it would make
    // sense to use a proper KDF here, like HKDF with a fixed salt.
    static readonly UntypedInt legacyTicketKeyNameLen = 16;
    copy(key.aesKey[..], hashed[(int)(legacyTicketKeyNameLen)..]);
    copy(key.hmacKey[..], hashed[(int)(legacyTicketKeyNameLen + len(key.aesKey))..]);
    key.created = c.time();
    return key;
}

// maxSessionTicketLifetime is the maximum allowed lifetime of a TLS 1.3 session
// ticket, and the lifetime we set for all tickets we send.
internal static readonly time.Duration maxSessionTicketLifetime = /* 7 * 24 * time.Hour */ 604800000000000;

// Clone returns a shallow clone of c or nil if c is nil. It is safe to clone a [Config] that is
// being used concurrently by a TLS client or server.
[GoRecv] public static ж<Config> Clone(this ref Config c) => func((defer, _) => {
    if (c == nil) {
        return default!;
    }
    c.mutex.RLock();
    defer(c.mutex.RUnlock);
    return Ꮡ(new Config(
        Rand: c.Rand,
        Time: c.Time,
        Certificates: c.Certificates,
        NameToCertificate: c.NameToCertificate,
        GetCertificate: c.GetCertificate,
        GetClientCertificate: c.GetClientCertificate,
        GetConfigForClient: c.GetConfigForClient,
        VerifyPeerCertificate: c.VerifyPeerCertificate,
        VerifyConnection: c.VerifyConnection,
        RootCAs: c.RootCAs,
        NextProtos: c.NextProtos,
        ServerName: c.ServerName,
        ClientAuth: c.ClientAuth,
        ClientCAs: c.ClientCAs,
        InsecureSkipVerify: c.InsecureSkipVerify,
        CipherSuites: c.CipherSuites,
        PreferServerCipherSuites: c.PreferServerCipherSuites,
        SessionTicketsDisabled: c.SessionTicketsDisabled,
        SessionTicketKey: c.SessionTicketKey,
        ClientSessionCache: c.ClientSessionCache,
        UnwrapSession: c.UnwrapSession,
        WrapSession: c.WrapSession,
        MinVersion: c.MinVersion,
        MaxVersion: c.MaxVersion,
        CurvePreferences: c.CurvePreferences,
        DynamicRecordSizingDisabled: c.DynamicRecordSizingDisabled,
        Renegotiation: c.Renegotiation,
        KeyLogWriter: c.KeyLogWriter,
        EncryptedClientHelloConfigList: c.EncryptedClientHelloConfigList,
        EncryptedClientHelloRejectionVerify: c.EncryptedClientHelloRejectionVerify,
        sessionTicketKeys: c.sessionTicketKeys,
        autoSessionTicketKeys: c.autoSessionTicketKeys
    ));
});

// deprecatedSessionTicketKey is set as the prefix of SessionTicketKey if it was
// randomized for backwards compatibility but is not in use.
internal static slice<byte> deprecatedSessionTicketKey = slice<byte>("DEPRECATED");

// initLegacySessionTicketKeyRLocked ensures the legacy SessionTicketKey field is
// randomized if empty, and that sessionTicketKeys is populated from it otherwise.
[GoRecv] internal static void initLegacySessionTicketKeyRLocked(this ref Config c) => func((defer, _) => {
    // Don't write if SessionTicketKey is already defined as our deprecated string,
    // or if it is defined by the user but sessionTicketKeys is already set.
    if (c.SessionTicketKey != new byte[]{}.array() && (bytes.HasPrefix(c.SessionTicketKey[..], deprecatedSessionTicketKey) || len(c.sessionTicketKeys) > 0)) {
        return;
    }
    // We need to write some data, so get an exclusive lock and re-check any conditions.
    c.mutex.RUnlock();
    defer(c.mutex.RLock);
    c.mutex.Lock();
    defer(c.mutex.Unlock);
    if (c.SessionTicketKey == new byte[]{}.array()){
        {
            var (_, err) = io.ReadFull(c.rand(), c.SessionTicketKey[..]); if (err != default!) {
                throw panic(fmt.Sprintf("tls: unable to generate random session ticket key: %v"u8, err));
            }
        }
        // Write the deprecated prefix at the beginning so we know we created
        // it. This key with the DEPRECATED prefix isn't used as an actual
        // session ticket key, and is only randomized in case the application
        // reuses it for some reason.
        copy(c.SessionTicketKey[..], deprecatedSessionTicketKey);
    } else 
    if (!bytes.HasPrefix(c.SessionTicketKey[..], deprecatedSessionTicketKey) && len(c.sessionTicketKeys) == 0) {
        c.sessionTicketKeys = new ticketKey[]{c.ticketKeyFromBytes(c.SessionTicketKey)}.slice();
    }
});

// ticketKeys returns the ticketKeys for this connection.
// If configForClient has explicitly set keys, those will
// be returned. Otherwise, the keys on c will be used and
// may be rotated if auto-managed.
// During rotation, any expired session ticket keys are deleted from
// c.sessionTicketKeys. If the session ticket key that is currently
// encrypting tickets (ie. the first ticketKey in c.sessionTicketKeys)
// is not fresh, then a new session ticket key will be
// created and prepended to c.sessionTicketKeys.
[GoRecv] public static slice<ticketKey> ticketKeys(this ref Config c, ж<Config> ᏑconfigForClient) => func((defer, _) => {
    ref var configForClient = ref ᏑconfigForClient.val;

    // If the ConfigForClient callback returned a Config with explicitly set
    // keys, use those, otherwise just use the original Config.
    if (configForClient != nil) {
        configForClient.mutex.RLock();
        if (configForClient.SessionTicketsDisabled) {
            return default!;
        }
        configForClient.initLegacySessionTicketKeyRLocked();
        if (len(configForClient.sessionTicketKeys) != 0) {
            var ret = configForClient.sessionTicketKeys;
            configForClient.mutex.RUnlock();
            return ret;
        }
        configForClient.mutex.RUnlock();
    }
    c.mutex.RLock();
    defer(c.mutex.RUnlock);
    if (c.SessionTicketsDisabled) {
        return default!;
    }
    c.initLegacySessionTicketKeyRLocked();
    if (len(c.sessionTicketKeys) != 0) {
        return c.sessionTicketKeys;
    }
    // Fast path for the common case where the key is fresh enough.
    if (len(c.autoSessionTicketKeys) > 0 && c.time().Sub(c.autoSessionTicketKeys[0].created) < ticketKeyRotation) {
        return c.autoSessionTicketKeys;
    }
    // autoSessionTicketKeys are managed by auto-rotation.
    c.mutex.RUnlock();
    defer(c.mutex.RLock);
    c.mutex.Lock();
    defer(c.mutex.Unlock);
    // Re-check the condition in case it changed since obtaining the new lock.
    if (len(c.autoSessionTicketKeys) == 0 || c.time().Sub(c.autoSessionTicketKeys[0].created) >= ticketKeyRotation) {
        array<byte> newKey = new(32);
        {
            var (_, err) = io.ReadFull(c.rand(), newKey[..]); if (err != default!) {
                throw panic(fmt.Sprintf("unable to generate random session ticket key: %v"u8, err));
            }
        }
        var valid = new slice<ticketKey>(0, len(c.autoSessionTicketKeys) + 1);
        valid = append(valid, c.ticketKeyFromBytes(newKey));
        foreach (var (_, k) in c.autoSessionTicketKeys) {
            // While rotating the current key, also remove any expired ones.
            if (c.time().Sub(k.created) < ticketKeyLifetime) {
                valid = append(valid, k);
            }
        }
        c.autoSessionTicketKeys = valid;
    }
    return c.autoSessionTicketKeys;
});

// SetSessionTicketKeys updates the session ticket keys for a server.
//
// The first key will be used when creating new tickets, while all keys can be
// used for decrypting tickets. It is safe to call this function while the
// server is running in order to rotate the session ticket keys. The function
// will panic if keys is empty.
//
// Calling this function will turn off automatic session ticket key rotation.
//
// If multiple servers are terminating connections for the same host they should
// all have the same session ticket keys. If the session ticket keys leaks,
// previously recorded and future TLS connections using those keys might be
// compromised.
[GoRecv] public static void SetSessionTicketKeys(this ref Config c, slice<array<byte>> keys) {
    if (len(keys) == 0) {
        throw panic("tls: keys must have at least one key");
    }
    var newKeys = new slice<ticketKey>(len(keys));
    foreach (var (i, bytes) in keys) {
        newKeys[i] = c.ticketKeyFromBytes(bytes);
    }
    c.mutex.Lock();
    c.sessionTicketKeys = newKeys;
    c.mutex.Unlock();
}

[GoRecv] internal static io.Reader rand(this ref Config c) {
    var r = c.Rand;
    if (r == default!) {
        return rand.Reader;
    }
    return r;
}

[GoRecv] internal static time.Time time(this ref Config c) {
    var t = c.Time;
    if (t == default!) {
        t = time.Now;
    }
    return t();
}

[GoRecv] internal static slice<uint16> cipherSuites(this ref Config c) {
    if (c.CipherSuites == default!) {
        if (needFIPS()) {
            return defaultCipherSuitesFIPS;
        }
        return defaultCipherSuites();
    }
    if (needFIPS()) {
        var ΔcipherSuites = slices.Clone(c.CipherSuites);
        return slices.DeleteFunc(ΔcipherSuites, 
        var defaultCipherSuitesFIPSʗ1 = defaultCipherSuitesFIPS;
        (uint16 id) => !slices.Contains(defaultCipherSuitesFIPSʗ1, id));
    }
    return c.CipherSuites;
}

public static slice<uint16> ΔsupportedVersions = new uint16[]{
    VersionTLS13,
    VersionTLS12,
    VersionTLS11,
    VersionTLS10
}.slice();

// roleClient and roleServer are meant to call supportedVersions and parents
// with more readability at the callsite.
internal const bool roleClient = true;

internal const bool roleServer = false;

internal static ж<godebug.Setting> tls10server = godebug.New("tls10server"u8);

[GoRecv] internal static slice<uint16> supportedVersions(this ref Config c, bool isClient) {
    var versions = new slice<uint16>(0, len(ΔsupportedVersions));
    foreach (var (_, v) in ΔsupportedVersions) {
        if (needFIPS() && !slices.Contains(defaultSupportedVersionsFIPS, v)) {
            continue;
        }
        if ((c == nil || c.MinVersion == 0) && v < VersionTLS12) {
            if (isClient || tls10server.Value() != "1"u8) {
                continue;
            }
        }
        if (isClient && c.EncryptedClientHelloConfigList != default! && v < VersionTLS13) {
            continue;
        }
        if (c != nil && c.MinVersion != 0 && v < c.MinVersion) {
            continue;
        }
        if (c != nil && c.MaxVersion != 0 && v > c.MaxVersion) {
            continue;
        }
        versions = append(versions, v);
    }
    return versions;
}

[GoRecv] internal static uint16 maxSupportedVersion(this ref Config c, bool isClient) {
    var ΔsupportedVersions = c.supportedVersions(isClient);
    if (len(ΔsupportedVersions) == 0) {
        return 0;
    }
    return ΔsupportedVersions[0];
}

// supportedVersionsFromMax returns a list of supported versions derived from a
// legacy maximum version value. Note that only versions supported by this
// library are returned. Any newer peer will use supportedVersions anyway.
internal static slice<uint16> supportedVersionsFromMax(uint16 maxVersion) {
    var versions = new slice<uint16>(0, len(ΔsupportedVersions));
    foreach (var (_, v) in ΔsupportedVersions) {
        if (v > maxVersion) {
            continue;
        }
        versions = append(versions, v);
    }
    return versions;
}

[GoRecv] internal static slice<CurveID> curvePreferences(this ref Config c, uint16 version) {
    slice<CurveID> curvePreferences = default!;
    if (c != nil && len(c.CurvePreferences) != 0){
        curvePreferences = slices.Clone(c.CurvePreferences);
        if (needFIPS()) {
            return slices.DeleteFunc(curvePreferences, 
            var defaultCurvePreferencesFIPSʗ1 = defaultCurvePreferencesFIPS;
            (CurveID c) => !slices.Contains(defaultCurvePreferencesFIPSʗ1, cΔ1));
        }
    } else 
    if (needFIPS()){
        curvePreferences = slices.Clone(defaultCurvePreferencesFIPS);
    } else {
        curvePreferences = defaultCurvePreferences();
    }
    if (version < VersionTLS13) {
        return slices.DeleteFunc(curvePreferences, 
        (CurveID c) => cΔ2 == x25519Kyber768Draft00);
    }
    return curvePreferences;
}

[GoRecv] internal static bool supportsCurve(this ref Config c, uint16 version, CurveID curve) {
    foreach (var (_, cc) in c.curvePreferences(version)) {
        if (cc == curve) {
            return true;
        }
    }
    return false;
}

// mutualVersion returns the protocol version to use given the advertised
// versions of the peer. Priority is given to the peer preference order.
[GoRecv] internal static (uint16, bool) mutualVersion(this ref Config c, bool isClient, slice<uint16> peerVersions) {
    var ΔsupportedVersions = c.supportedVersions(isClient);
    foreach (var (_, peerVersion) in peerVersions) {
        foreach (var (_, v) in ΔsupportedVersions) {
            if (v == peerVersion) {
                return (v, true);
            }
        }
    }
    return (0, false);
}

// errNoCertificates should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/xtls/xray-core
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname errNoCertificates
internal static error errNoCertificates = errors.New("tls: no certificates configured"u8);

// getCertificate returns the best certificate for the given ClientHelloInfo,
// defaulting to the first element of c.Certificates.
[GoRecv] public static (ж<Certificate>, error) getCertificate(this ref Config c, ж<ClientHelloInfo> ᏑclientHello) {
    ref var clientHello = ref ᏑclientHello.val;

    if (c.GetCertificate != default! && (len(c.Certificates) == 0 || len(clientHello.ServerName) > 0)) {
        (cert, err) = c.GetCertificate(clientHello);
        if (cert != nil || err != default!) {
            return (cert, err);
        }
    }
    if (len(c.Certificates) == 0) {
        return (default!, errNoCertificates);
    }
    if (len(c.Certificates) == 1) {
        // There's only one choice, so no point doing any work.
        return (Ꮡ(c.Certificates[0]), default!);
    }
    if (c.NameToCertificate != default!) {
        @string name = strings.ToLower(clientHello.ServerName);
        {
            var cert = c.NameToCertificate[name];
            var ok = c.NameToCertificate[name]; if (ok) {
                return (cert, default!);
            }
        }
        if (len(name) > 0) {
            var labels = strings.Split(name, "."u8);
            labels[0] = "*"u8;
            @string wildcardName = strings.Join(labels, "."u8);
            {
                var cert = c.NameToCertificate[wildcardName];
                var ok = c.NameToCertificate[wildcardName]; if (ok) {
                    return (cert, default!);
                }
            }
        }
    }
    ref var cert = ref heap(new Certificate(), out var Ꮡcert);

    foreach (var (_, cert) in c.Certificates) {
        {
            var err = clientHello.SupportsCertificate(Ꮡcert); if (err == default!) {
                return (Ꮡcert, default!);
            }
        }
    }
    // If nothing matches, return the first certificate.
    return (Ꮡ(c.Certificates[0]), default!);
}

// SupportsCertificate returns nil if the provided certificate is supported by
// the client that sent the ClientHello. Otherwise, it returns an error
// describing the reason for the incompatibility.
//
// If this [ClientHelloInfo] was passed to a GetConfigForClient or GetCertificate
// callback, this method will take into account the associated [Config]. Note that
// if GetConfigForClient returns a different [Config], the change can't be
// accounted for by this method.
//
// This function will call x509.ParseCertificate unless c.Leaf is set, which can
// incur a significant performance cost.
[GoRecv] public static error SupportsCertificate(this ref ClientHelloInfo chi, ж<Certificate> Ꮡc) {
    ref var c = ref Ꮡc.val;

    // Note we don't currently support certificate_authorities nor
    // signature_algorithms_cert, and don't check the algorithms of the
    // signatures on the chain (which anyway are a SHOULD, see RFC 8446,
    // Section 4.4.2.2).
    var config = chi.config;
    if (config == nil) {
        config = Ꮡ(new Config(nil));
    }
    var (vers, ok) = config.mutualVersion(roleServer, chi.SupportedVersions);
    if (!ok) {
        return errors.New("no mutually supported protocol versions"u8);
    }
    // If the client specified the name they are trying to connect to, the
    // certificate needs to be valid for it.
    if (chi.ServerName != ""u8) {
        (x509Cert, err) = c.leaf();
        if (err != default!) {
            return fmt.Errorf("failed to parse certificate: %w"u8, err);
        }
        {
            var errΔ1 = x509Cert.VerifyHostname(chi.ServerName); if (errΔ1 != default!) {
                return fmt.Errorf("certificate is not valid for requested server name: %w"u8, errΔ1);
            }
        }
    }
    // supportsRSAFallback returns nil if the certificate and connection support
    // the static RSA key exchange, and unsupported otherwise. The logic for
    // supporting static RSA is completely disjoint from the logic for
    // supporting signed key exchanges, so we just check it as a fallback.
    var supportsRSAFallback = 
    var configʗ1 = config;
    (error unsupported) => {
        // TLS 1.3 dropped support for the static RSA key exchange.
        if (vers == VersionTLS13) {
            return unsupported;
        }
        // The static RSA key exchange works by decrypting a challenge with the
        // RSA private key, not by signing, so check the PrivateKey implements
        // crypto.Decrypter, like *rsa.PrivateKey does.
        {
            var (priv, okΔ1) = c.PrivateKey._<crypto.Decrypter>(ᐧ); if (okΔ1){
                {
                    var (_, okΔ2) = priv.Public()._<ж<rsa.PublicKey>>(ᐧ); if (!okΔ2) {
                        return unsupported;
                    }
                }
            } else {
                return unsupported;
            }
        }
        // Finally, there needs to be a mutual cipher suite that uses the static
        // RSA key exchange instead of ECDHE.
        var rsaCipherSuite = selectCipherSuite(chi.CipherSuites, configʗ1.cipherSuites(), 
        (ж<cipherSuite> c) => {
            if ((nint)((~cΔ1).flags & suiteECDHE) != 0) {
                return false;
            }
            if (vers < VersionTLS12 && (nint)((~cΔ1).flags & suiteTLS12) != 0) {
                return false;
            }
            return true;
        });
        if (rsaCipherSuite == nil) {
            return unsupported;
        }
        return default!;
    };
    // If the client sent the signature_algorithms extension, ensure it supports
    // schemes we can use with this certificate and TLS version.
    if (len(chi.SignatureSchemes) > 0) {
        {
            var (_, err) = selectSignatureScheme(vers, Ꮡc, chi.SignatureSchemes); if (err != default!) {
                return supportsRSAFallback(err);
            }
        }
    }
    // In TLS 1.3 we are done because supported_groups is only relevant to the
    // ECDHE computation, point format negotiation is removed, cipher suites are
    // only relevant to the AEAD choice, and static RSA does not exist.
    if (vers == VersionTLS13) {
        return default!;
    }
    // The only signed key exchange we support is ECDHE.
    if (!supportsECDHE(config, vers, chi.SupportedCurves, chi.SupportedPoints)) {
        return supportsRSAFallback(errors.New("client doesn't support ECDHE, can only use legacy RSA key exchange"u8));
    }
    bool ecdsaCipherSuite = default!;
    {
        var (priv, okΔ3) = c.PrivateKey._<crypto.Signer>(ᐧ); if (okΔ3){
            switch (priv.Public().type()) {
            case ж<ecdsa.PublicKey> pub: {
                CurveID curve = default!;
                var exprᴛ1 = (~pub).Curve;
                if (exprᴛ1 == elliptic.P256()) {
                    curve = CurveP256;
                }
                else if (exprᴛ1 == elliptic.P384()) {
                    curve = CurveP384;
                }
                else if (exprᴛ1 == elliptic.P521()) {
                    curve = CurveP521;
                }
                else { /* default: */
                    return supportsRSAFallback(unsupportedCertificateError(Ꮡc));
                }

                bool curveOk = default!;
                foreach (var (_, cΔ2) in chi.SupportedCurves) {
                    if (cΔ2 == curve && config.supportsCurve(vers, cΔ2)) {
                        curveOk = true;
                        break;
                    }
                }
                if (!curveOk) {
                    return errors.New("client doesn't support certificate curve"u8);
                }
                ecdsaCipherSuite = true;
                break;
            }
            case ed25519.PublicKey pub: {
                if (vers < VersionTLS12 || len(chi.SignatureSchemes) == 0) {
                    return errors.New("connection doesn't support Ed25519"u8);
                }
                ecdsaCipherSuite = true;
                break;
            }
            case ж<rsa.PublicKey> pub: {
                break;
            }
            default: {
                var pub = priv.Public().type();
                return supportsRSAFallback(unsupportedCertificateError(Ꮡc));
            }}
        } else {
            return supportsRSAFallback(unsupportedCertificateError(Ꮡc));
        }
    }
    // Make sure that there is a mutually supported cipher suite that works with
    // this certificate. Cipher suite selection will then apply the logic in
    // reverse to pick it. See also serverHandshakeState.cipherSuiteOk.
    var cipherSuite = selectCipherSuite(chi.CipherSuites, config.cipherSuites(), 
    (ж<cipherSuite> c) => {
        if ((nint)((~cΔ3).flags & suiteECDHE) == 0) {
            return false;
        }
        if ((nint)((~cΔ3).flags & suiteECSign) != 0){
            if (!ecdsaCipherSuite) {
                return false;
            }
        } else {
            if (ecdsaCipherSuite) {
                return false;
            }
        }
        if (vers < VersionTLS12 && (nint)((~cΔ3).flags & suiteTLS12) != 0) {
            return false;
        }
        return true;
    });
    if (cipherSuite == nil) {
        return supportsRSAFallback(errors.New("client doesn't support any cipher suites compatible with the certificate"u8));
    }
    return default!;
}

// SupportsCertificate returns nil if the provided certificate is supported by
// the server that sent the CertificateRequest. Otherwise, it returns an error
// describing the reason for the incompatibility.
[GoRecv] public static error SupportsCertificate(this ref CertificateRequestInfo cri, ж<Certificate> Ꮡc) {
    ref var c = ref Ꮡc.val;

    {
        var (_, errΔ1) = selectSignatureScheme(cri.Version, Ꮡc, cri.SignatureSchemes); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    if (len(cri.AcceptableCAs) == 0) {
        return default!;
    }
    foreach (var (j, cert) in c.Certificate) {
        var x509Cert = c.Leaf;
        // Parse the certificate if this isn't the leaf node, or if
        // chain.Leaf was nil.
        if (j != 0 || x509Cert == nil) {
            error err = default!;
            {
                (x509Cert, err) = x509.ParseCertificate(cert); if (err != default!) {
                    return fmt.Errorf("failed to parse certificate #%d in the chain: %w"u8, j, err);
                }
            }
        }
        foreach (var (_, ca) in cri.AcceptableCAs) {
            if (bytes.Equal((~x509Cert).RawIssuer, ca)) {
                return default!;
            }
        }
    }
    return errors.New("chain is not signed by an acceptable CA"u8);
}

// BuildNameToCertificate parses c.Certificates and builds c.NameToCertificate
// from the CommonName and SubjectAlternateName fields of each of the leaf
// certificates.
//
// Deprecated: NameToCertificate only allows associating a single certificate
// with a given name. Leave that field nil to let the library select the first
// compatible chain from Certificates.
[GoRecv] public static void BuildNameToCertificate(this ref Config c) {
    c.NameToCertificate = new map<@string, ж<Certificate>>();
    foreach (var (i, _) in c.Certificates) {
        var cert = Ꮡ(c.Certificates[i]);
        (x509Cert, err) = cert.leaf();
        if (err != default!) {
            continue;
        }
        // If SANs are *not* present, some clients will consider the certificate
        // valid for the name in the Common Name.
        if ((~x509Cert).Subject.CommonName != ""u8 && len((~x509Cert).DNSNames) == 0) {
            c.NameToCertificate[(~x509Cert).Subject.CommonName] = cert;
        }
        foreach (var (_, san) in (~x509Cert).DNSNames) {
            c.NameToCertificate[san] = cert;
        }
    }
}

internal static readonly @string keyLogLabelTLS12 = "CLIENT_RANDOM"u8;
internal static readonly @string keyLogLabelClientHandshake = "CLIENT_HANDSHAKE_TRAFFIC_SECRET"u8;
internal static readonly @string keyLogLabelServerHandshake = "SERVER_HANDSHAKE_TRAFFIC_SECRET"u8;
internal static readonly @string keyLogLabelClientTraffic = "CLIENT_TRAFFIC_SECRET_0"u8;
internal static readonly @string keyLogLabelServerTraffic = "SERVER_TRAFFIC_SECRET_0"u8;

[GoRecv] internal static error writeKeyLog(this ref Config c, @string label, slice<byte> clientRandom, slice<byte> secret) {
    if (c.KeyLogWriter == default!) {
        return default!;
    }
    var logLine = fmt.Appendf(default!, "%s %x %x\n"u8, label, clientRandom, secret);
    writerMutex.Lock();
    var (_, err) = c.KeyLogWriter.Write(logLine);
    writerMutex.Unlock();
    return err;
}

// writerMutex protects all KeyLogWriters globally. It is rarely enabled,
// and is only for debugging, so a global mutex saves space.
internal static sync.Mutex writerMutex;

// A Certificate is a chain of one or more certificates, leaf first.
[GoType] partial struct Certificate {
    public slice<slice<byte>> Certificate;
    // PrivateKey contains the private key corresponding to the public key in
    // Leaf. This must implement crypto.Signer with an RSA, ECDSA or Ed25519 PublicKey.
    // For a server up to TLS 1.2, it can also implement crypto.Decrypter with
    // an RSA PublicKey.
    public crypto_package.PrivateKey PrivateKey;
    // SupportedSignatureAlgorithms is an optional list restricting what
    // signature algorithms the PrivateKey can be used for.
    public slice<SignatureScheme> SupportedSignatureAlgorithms;
    // OCSPStaple contains an optional OCSP response which will be served
    // to clients that request it.
    public slice<byte> OCSPStaple;
    // SignedCertificateTimestamps contains an optional list of Signed
    // Certificate Timestamps which will be served to clients that request it.
    public slice<slice<byte>> SignedCertificateTimestamps;
    // Leaf is the parsed form of the leaf certificate, which may be initialized
    // using x509.ParseCertificate to reduce per-handshake processing. If nil,
    // the leaf certificate will be parsed as needed.
    public ж<crypto.x509_package.Certificate> Leaf;
}

// leaf returns the parsed leaf certificate, either from c.Leaf or by parsing
// the corresponding c.Certificate[0].
[GoRecv] internal static (ж<x509.Certificate>, error) leaf(this ref Certificate c) {
    if (c.Leaf != nil) {
        return (c.Leaf, default!);
    }
    return x509.ParseCertificate(c.Certificate[0]);
}

[GoType] partial interface handshakeMessage {
    (slice<byte>, error) marshal();
    bool unmarshal(slice<byte> _);
}

[GoType] partial interface handshakeMessageWithOriginalBytes :
    handshakeMessage
{
    // originalBytes should return the original bytes that were passed to
    // unmarshal to create the message. If the message was not produced by
    // unmarshal, it should return nil.
    slice<byte> originalBytes();
}

// lruSessionCache is a ClientSessionCache implementation that uses an LRU
// caching strategy.
[GoType] partial struct lruSessionCache {
    public partial ref sync_package.Mutex Mutex { get; }
    internal list.Element m;
    internal ж<container.list_package.List> q;
    internal nint capacity;
}

[GoType] partial struct lruSessionCacheEntry {
    internal @string sessionKey;
    internal ж<ClientSessionState> state;
}

// NewLRUClientSessionCache returns a [ClientSessionCache] with the given
// capacity that uses an LRU strategy. If capacity is < 1, a default capacity
// is used instead.
public static ClientSessionCache NewLRUClientSessionCache(nint capacity) {
    static readonly UntypedInt defaultSessionCacheCapacity = 64;
    if (capacity < 1) {
        capacity = defaultSessionCacheCapacity;
    }
    return new lruSessionCache(
        m: new list.Element(),
        q: list.New(),
        capacity: capacity
    );
}

// Put adds the provided (sessionKey, cs) pair to the cache. If cs is nil, the entry
// corresponding to sessionKey is removed from the cache instead.
[GoRecv] internal static void Put(this ref lruSessionCache c, @string sessionKey, ж<ClientSessionState> Ꮡcs) => func((defer, _) => {
    ref var cs = ref Ꮡcs.val;

    c.Lock();
    defer(c.Unlock);
    {
        var elemΔ1 = c.m[sessionKey];
        var ok = c.m[sessionKey]; if (ok) {
            if (cs == nil){
                c.q.Remove(elemΔ1);
                delete(c.m, sessionKey);
            } else {
                var entryΔ1 = (~elemΔ1).Value._<lruSessionCacheEntry.val>();
                .val.state = cs;
                c.q.MoveToFront(elemΔ1);
            }
            return;
        }
    }
    if (c.q.Len() < c.capacity) {
        var entryΔ2 = Ꮡ(new lruSessionCacheEntry(sessionKey, Ꮡcs));
        c.m[sessionKey] = c.q.PushFront(entryΔ2);
        return;
    }
    var elem = c.q.Back();
    var entry = (~elem).Value._<lruSessionCacheEntry.val>();
    delete(c.m, (~entry).sessionKey);
    entry.val.sessionKey = sessionKey;
    entry.val.state = cs;
    c.q.MoveToFront(elem);
    c.m[sessionKey] = elem;
});

// Get returns the [ClientSessionState] value associated with a given key. It
// returns (nil, false) if no value is found.
[GoRecv] internal static (ж<ClientSessionState>, bool) Get(this ref lruSessionCache c, @string sessionKey) => func((defer, _) => {
    c.Lock();
    defer(c.Unlock);
    {
        var elem = c.m[sessionKey];
        var ok = c.m[sessionKey]; if (ok) {
            c.q.MoveToFront(elem);
            return ((~elem).Value._<lruSessionCacheEntry.val>().state, true);
        }
    }
    return (default!, false);
});

internal static Config emptyConfig;

internal static ж<Config> defaultConfig() {
    return Ꮡ(emptyConfig);
}

internal static error unexpectedMessageError(any wanted, any got) {
    return fmt.Errorf("tls: received unexpected handshake message of type %T when waiting for %T"u8, got, wanted);
}

// supportedSignatureAlgorithms returns the supported signature algorithms.
internal static slice<SignatureScheme> supportedSignatureAlgorithms() {
    if (!needFIPS()) {
        return defaultSupportedSignatureAlgorithms;
    }
    return defaultSupportedSignatureAlgorithmsFIPS;
}

internal static bool isSupportedSignatureAlgorithm(SignatureScheme sigAlg, slice<SignatureScheme> supportedSignatureAlgorithms) {
    foreach (var (_, s) in supportedSignatureAlgorithms) {
        if (s == sigAlg) {
            return true;
        }
    }
    return false;
}

// CertificateVerificationError is returned when certificate verification fails during the handshake.
[GoType] partial struct CertificateVerificationError {
    // UnverifiedCertificates and its contents should not be modified.
    public x509.Certificate UnverifiedCertificates;
    public error Err;
}

[GoRecv] public static @string Error(this ref CertificateVerificationError e) {
    return fmt.Sprintf("tls: failed to verify certificate: %s"u8, e.Err);
}

[GoRecv] public static error Unwrap(this ref CertificateVerificationError e) {
    return e.Err;
}

} // end tls_package
