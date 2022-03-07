// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2022 March 06 22:19:31 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Program Files\Go\src\crypto\tls\common.go
using bytes = go.bytes_package;
using list = go.container.list_package;
using context = go.context_package;
using crypto = go.crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using elliptic = go.crypto.elliptic_package;
using rand = go.crypto.rand_package;
using rsa = go.crypto.rsa_package;
using sha512 = go.crypto.sha512_package;
using x509 = go.crypto.x509_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using net = go.net_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using System;


namespace go.crypto;

public static partial class tls_package {

public static readonly nuint VersionTLS10 = 0x0301;
public static readonly nuint VersionTLS11 = 0x0302;
public static readonly nuint VersionTLS12 = 0x0303;
public static readonly nuint VersionTLS13 = 0x0304; 

// Deprecated: SSLv3 is cryptographically broken, and is no longer
// supported by this package. See golang.org/issue/32716.
public static readonly nuint VersionSSL30 = 0x0300;


private static readonly nint maxPlaintext = 16384; // maximum plaintext payload length
private static readonly nint maxCiphertext = 16384 + 2048; // maximum ciphertext payload length
private static readonly nint maxCiphertextTLS13 = 16384 + 256; // maximum ciphertext length in TLS 1.3
private static readonly nint recordHeaderLen = 5; // record header length
private static readonly nint maxHandshake = 65536; // maximum handshake we support (protocol max is 16 MB)
private static readonly nint maxUselessRecords = 16; // maximum number of consecutive non-advancing records

// TLS record types.
private partial struct recordType { // : byte
}

private static readonly recordType recordTypeChangeCipherSpec = 20;
private static readonly recordType recordTypeAlert = 21;
private static readonly recordType recordTypeHandshake = 22;
private static readonly recordType recordTypeApplicationData = 23;


// TLS handshake message types.
private static readonly byte typeHelloRequest = 0;
private static readonly byte typeClientHello = 1;
private static readonly byte typeServerHello = 2;
private static readonly byte typeNewSessionTicket = 4;
private static readonly byte typeEndOfEarlyData = 5;
private static readonly byte typeEncryptedExtensions = 8;
private static readonly byte typeCertificate = 11;
private static readonly byte typeServerKeyExchange = 12;
private static readonly byte typeCertificateRequest = 13;
private static readonly byte typeServerHelloDone = 14;
private static readonly byte typeCertificateVerify = 15;
private static readonly byte typeClientKeyExchange = 16;
private static readonly byte typeFinished = 20;
private static readonly byte typeCertificateStatus = 22;
private static readonly byte typeKeyUpdate = 24;
private static readonly byte typeNextProtocol = 67; // Not IANA assigned
private static readonly byte typeMessageHash = 254; // synthetic message

// TLS compression types.
private static readonly byte compressionNone = 0;


// TLS extension numbers
private static readonly ushort extensionServerName = 0;
private static readonly ushort extensionStatusRequest = 5;
private static readonly ushort extensionSupportedCurves = 10; // supported_groups in TLS 1.3, see RFC 8446, Section 4.2.7
private static readonly ushort extensionSupportedPoints = 11;
private static readonly ushort extensionSignatureAlgorithms = 13;
private static readonly ushort extensionALPN = 16;
private static readonly ushort extensionSCT = 18;
private static readonly ushort extensionSessionTicket = 35;
private static readonly ushort extensionPreSharedKey = 41;
private static readonly ushort extensionEarlyData = 42;
private static readonly ushort extensionSupportedVersions = 43;
private static readonly ushort extensionCookie = 44;
private static readonly ushort extensionPSKModes = 45;
private static readonly ushort extensionCertificateAuthorities = 47;
private static readonly ushort extensionSignatureAlgorithmsCert = 50;
private static readonly ushort extensionKeyShare = 51;
private static readonly ushort extensionRenegotiationInfo = 0xff01;


// TLS signaling cipher suite values
private static readonly ushort scsvRenegotiation = 0x00ff;


// CurveID is the type of a TLS identifier for an elliptic curve. See
// https://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-8.
//
// In TLS 1.3, this type is called NamedGroup, but at this time this library
// only supports Elliptic Curve based groups. See RFC 8446, Section 4.2.7.
public partial struct CurveID { // : ushort
}

public static readonly CurveID CurveP256 = 23;
public static readonly CurveID CurveP384 = 24;
public static readonly CurveID CurveP521 = 25;
public static readonly CurveID X25519 = 29;


// TLS 1.3 Key Share. See RFC 8446, Section 4.2.8.
private partial struct keyShare {
    public CurveID group;
    public slice<byte> data;
}

// TLS 1.3 PSK Key Exchange Modes. See RFC 8446, Section 4.2.9.
private static readonly byte pskModePlain = 0;
private static readonly byte pskModeDHE = 1;


// TLS 1.3 PSK Identity. Can be a Session Ticket, or a reference to a saved
// session. See RFC 8446, Section 4.2.11.
private partial struct pskIdentity {
    public slice<byte> label;
    public uint obfuscatedTicketAge;
}

// TLS Elliptic Curve Point Formats
// https://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-9
private static readonly byte pointFormatUncompressed = 0;


// TLS CertificateStatusType (RFC 3546)
private static readonly byte statusTypeOCSP = 1;


// Certificate types (for certificateRequestMsg)
private static readonly nint certTypeRSASign = 1;
private static readonly nint certTypeECDSASign = 64; // ECDSA or EdDSA keys, see RFC 8422, Section 3.

// Signature algorithms (for internal signaling use). Starting at 225 to avoid overlap with
// TLS 1.2 codepoints (RFC 5246, Appendix A.4.1), with which these have nothing to do.
private static readonly byte signaturePKCS1v15 = iota + 225;
private static readonly var signatureRSAPSS = 0;
private static readonly var signatureECDSA = 1;
private static readonly var signatureEd25519 = 2;


// directSigning is a standard Hash value that signals that no pre-hashing
// should be performed, and that the input should be signed directly. It is the
// hash function associated with the Ed25519 signature scheme.
private static crypto.Hash directSigning = 0;

// supportedSignatureAlgorithms contains the signature and hash algorithms that
// the code advertises as supported in a TLS 1.2+ ClientHello and in a TLS 1.2+
// CertificateRequest. The two fields are merged to match with TLS 1.3.
// Note that in TLS 1.2, the ECDSA algorithms are not constrained to P-256, etc.
private static SignatureScheme supportedSignatureAlgorithms = new slice<SignatureScheme>(new SignatureScheme[] { PSSWithSHA256, ECDSAWithP256AndSHA256, Ed25519, PSSWithSHA384, PSSWithSHA512, PKCS1WithSHA256, PKCS1WithSHA384, PKCS1WithSHA512, ECDSAWithP384AndSHA384, ECDSAWithP521AndSHA512, PKCS1WithSHA1, ECDSAWithSHA1 });

// helloRetryRequestRandom is set as the Random value of a ServerHello
// to signal that the message is actually a HelloRetryRequest.
private static byte helloRetryRequestRandom = new slice<byte>(new byte[] { 0xCF, 0x21, 0xAD, 0x74, 0xE5, 0x9A, 0x61, 0x11, 0xBE, 0x1D, 0x8C, 0x02, 0x1E, 0x65, 0xB8, 0x91, 0xC2, 0xA2, 0x11, 0x16, 0x7A, 0xBB, 0x8C, 0x5E, 0x07, 0x9E, 0x09, 0xE2, 0xC8, 0xA8, 0x33, 0x9C });

 
// downgradeCanaryTLS12 or downgradeCanaryTLS11 is embedded in the server
// random as a downgrade protection if the server would be capable of
// negotiating a higher version. See RFC 8446, Section 4.1.3.
private static readonly @string downgradeCanaryTLS12 = "DOWNGRD\x01";
private static readonly @string downgradeCanaryTLS11 = "DOWNGRD\x00";


// testingOnlyForceDowngradeCanary is set in tests to force the server side to
// include downgrade canaries even if it's using its highers supported version.
private static bool testingOnlyForceDowngradeCanary = default;

// ConnectionState records basic TLS details about the connection.
public partial struct ConnectionState {
    public ushort Version; // HandshakeComplete is true if the handshake has concluded.
    public bool HandshakeComplete; // DidResume is true if this connection was successfully resumed from a
// previous session with a session ticket or similar mechanism.
    public bool DidResume; // CipherSuite is the cipher suite negotiated for the connection (e.g.
// TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, TLS_AES_128_GCM_SHA256).
    public ushort CipherSuite; // NegotiatedProtocol is the application protocol negotiated with ALPN.
    public @string NegotiatedProtocol; // NegotiatedProtocolIsMutual used to indicate a mutual NPN negotiation.
//
// Deprecated: this value is always true.
    public bool NegotiatedProtocolIsMutual; // ServerName is the value of the Server Name Indication extension sent by
// the client. It's available both on the server and on the client side.
    public @string ServerName; // PeerCertificates are the parsed certificates sent by the peer, in the
// order in which they were sent. The first element is the leaf certificate
// that the connection is verified against.
//
// On the client side, it can't be empty. On the server side, it can be
// empty if Config.ClientAuth is not RequireAnyClientCert or
// RequireAndVerifyClientCert.
    public slice<ptr<x509.Certificate>> PeerCertificates; // VerifiedChains is a list of one or more chains where the first element is
// PeerCertificates[0] and the last element is from Config.RootCAs (on the
// client side) or Config.ClientCAs (on the server side).
//
// On the client side, it's set if Config.InsecureSkipVerify is false. On
// the server side, it's set if Config.ClientAuth is VerifyClientCertIfGiven
// (and the peer provided a certificate) or RequireAndVerifyClientCert.
    public slice<slice<ptr<x509.Certificate>>> VerifiedChains; // SignedCertificateTimestamps is a list of SCTs provided by the peer
// through the TLS handshake for the leaf certificate, if any.
    public slice<slice<byte>> SignedCertificateTimestamps; // OCSPResponse is a stapled Online Certificate Status Protocol (OCSP)
// response provided by the peer for the leaf certificate, if any.
    public slice<byte> OCSPResponse; // TLSUnique contains the "tls-unique" channel binding value (see RFC 5929,
// Section 3). This value will be nil for TLS 1.3 connections and for all
// resumed connections.
//
// Deprecated: there are conditions in which this value might not be unique
// to a connection. See the Security Considerations sections of RFC 5705 and
// RFC 7627, and https://mitls.org/pages/attacks/3SHAKE#channelbindings.
    public slice<byte> TLSUnique; // ekm is a closure exposed via ExportKeyingMaterial.
    public Func<@string, slice<byte>, nint, (slice<byte>, error)> ekm;
}

// ExportKeyingMaterial returns length bytes of exported key material in a new
// slice as defined in RFC 5705. If context is nil, it is not used as part of
// the seed. If the connection was set to allow renegotiation via
// Config.Renegotiation, this function will return an error.
private static (slice<byte>, error) ExportKeyingMaterial(this ptr<ConnectionState> _addr_cs, @string label, slice<byte> context, nint length) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref ConnectionState cs = ref _addr_cs.val;

    return cs.ekm(label, context, length);
}

// ClientAuthType declares the policy the server will follow for
// TLS Client Authentication.
public partial struct ClientAuthType { // : nint
}

 
// NoClientCert indicates that no client certificate should be requested
// during the handshake, and if any certificates are sent they will not
// be verified.
public static readonly ClientAuthType NoClientCert = iota; 
// RequestClientCert indicates that a client certificate should be requested
// during the handshake, but does not require that the client send any
// certificates.
public static readonly var RequestClientCert = 0; 
// RequireAnyClientCert indicates that a client certificate should be requested
// during the handshake, and that at least one certificate is required to be
// sent by the client, but that certificate is not required to be valid.
public static readonly var RequireAnyClientCert = 1; 
// VerifyClientCertIfGiven indicates that a client certificate should be requested
// during the handshake, but does not require that the client sends a
// certificate. If the client does send a certificate it is required to be
// valid.
public static readonly var VerifyClientCertIfGiven = 2; 
// RequireAndVerifyClientCert indicates that a client certificate should be requested
// during the handshake, and that at least one valid certificate is required
// to be sent by the client.
public static readonly var RequireAndVerifyClientCert = 3;


// requiresClientCert reports whether the ClientAuthType requires a client
// certificate to be provided.
private static bool requiresClientCert(ClientAuthType c) {

    if (c == RequireAnyClientCert || c == RequireAndVerifyClientCert) 
        return true;
    else 
        return false;
    
}

// ClientSessionState contains the state needed by clients to resume TLS
// sessions.
public partial struct ClientSessionState {
    public slice<byte> sessionTicket; // Encrypted ticket used for session resumption with server
    public ushort vers; // TLS version negotiated for the session
    public ushort cipherSuite; // Ciphersuite negotiated for the session
    public slice<byte> masterSecret; // Full handshake MasterSecret, or TLS 1.3 resumption_master_secret
    public slice<ptr<x509.Certificate>> serverCertificates; // Certificate chain presented by the server
    public slice<slice<ptr<x509.Certificate>>> verifiedChains; // Certificate chains we built for verification
    public time.Time receivedAt; // When the session ticket was received from the server
    public slice<byte> ocspResponse; // Stapled OCSP response presented by the server
    public slice<slice<byte>> scts; // SCTs presented by the server

// TLS 1.3 fields.
    public slice<byte> nonce; // Ticket nonce sent by the server, to derive PSK
    public time.Time useBy; // Expiration of the ticket lifetime as set by the server
    public uint ageAdd; // Random obfuscation factor for sending the ticket age
}

// ClientSessionCache is a cache of ClientSessionState objects that can be used
// by a client to resume a TLS session with a given server. ClientSessionCache
// implementations should expect to be called concurrently from different
// goroutines. Up to TLS 1.2, only ticket-based resumption is supported, not
// SessionID-based resumption. In TLS 1.3 they were merged into PSK modes, which
// are supported via this interface.
public partial interface ClientSessionCache {
    (ptr<ClientSessionState>, bool) Get(@string sessionKey); // Put adds the ClientSessionState to the cache with the given key. It might
// get called multiple times in a connection if a TLS 1.3 server provides
// more than one session ticket. If called with a nil *ClientSessionState,
// it should remove the cache entry.
    (ptr<ClientSessionState>, bool) Put(@string sessionKey, ptr<ClientSessionState> cs);
}

//go:generate stringer -type=SignatureScheme,CurveID,ClientAuthType -output=common_string.go

// SignatureScheme identifies a signature algorithm supported by TLS. See
// RFC 8446, Section 4.2.3.
public partial struct SignatureScheme { // : ushort
}

 
// RSASSA-PKCS1-v1_5 algorithms.
public static readonly SignatureScheme PKCS1WithSHA256 = 0x0401;
public static readonly SignatureScheme PKCS1WithSHA384 = 0x0501;
public static readonly SignatureScheme PKCS1WithSHA512 = 0x0601; 

// RSASSA-PSS algorithms with public key OID rsaEncryption.
public static readonly SignatureScheme PSSWithSHA256 = 0x0804;
public static readonly SignatureScheme PSSWithSHA384 = 0x0805;
public static readonly SignatureScheme PSSWithSHA512 = 0x0806; 

// ECDSA algorithms. Only constrained to a specific curve in TLS 1.3.
public static readonly SignatureScheme ECDSAWithP256AndSHA256 = 0x0403;
public static readonly SignatureScheme ECDSAWithP384AndSHA384 = 0x0503;
public static readonly SignatureScheme ECDSAWithP521AndSHA512 = 0x0603; 

// EdDSA algorithms.
public static readonly SignatureScheme Ed25519 = 0x0807; 

// Legacy signature and hash algorithms for TLS 1.2.
public static readonly SignatureScheme PKCS1WithSHA1 = 0x0201;
public static readonly SignatureScheme ECDSAWithSHA1 = 0x0203;


// ClientHelloInfo contains information from a ClientHello message in order to
// guide application logic in the GetCertificate and GetConfigForClient callbacks.
public partial struct ClientHelloInfo {
    public slice<ushort> CipherSuites; // ServerName indicates the name of the server requested by the client
// in order to support virtual hosting. ServerName is only set if the
// client is using SNI (see RFC 4366, Section 3.1).
    public @string ServerName; // SupportedCurves lists the elliptic curves supported by the client.
// SupportedCurves is set only if the Supported Elliptic Curves
// Extension is being used (see RFC 4492, Section 5.1.1).
    public slice<CurveID> SupportedCurves; // SupportedPoints lists the point formats supported by the client.
// SupportedPoints is set only if the Supported Point Formats Extension
// is being used (see RFC 4492, Section 5.1.2).
    public slice<byte> SupportedPoints; // SignatureSchemes lists the signature and hash schemes that the client
// is willing to verify. SignatureSchemes is set only if the Signature
// Algorithms Extension is being used (see RFC 5246, Section 7.4.1.4.1).
    public slice<SignatureScheme> SignatureSchemes; // SupportedProtos lists the application protocols supported by the client.
// SupportedProtos is set only if the Application-Layer Protocol
// Negotiation Extension is being used (see RFC 7301, Section 3.1).
//
// Servers can select a protocol by setting Config.NextProtos in a
// GetConfigForClient return value.
    public slice<@string> SupportedProtos; // SupportedVersions lists the TLS versions supported by the client.
// For TLS versions less than 1.3, this is extrapolated from the max
// version advertised by the client, so values other than the greatest
// might be rejected if used.
    public slice<ushort> SupportedVersions; // Conn is the underlying net.Conn for the connection. Do not read
// from, or write to, this connection; that will cause the TLS
// connection to fail.
    public net.Conn Conn; // config is embedded by the GetCertificate or GetConfigForClient caller,
// for use with SupportsCertificate.
    public ptr<Config> config; // ctx is the context of the handshake that is in progress.
    public context.Context ctx;
}

// Context returns the context of the handshake that is in progress.
// This context is a child of the context passed to HandshakeContext,
// if any, and is canceled when the handshake concludes.
private static context.Context Context(this ptr<ClientHelloInfo> _addr_c) {
    ref ClientHelloInfo c = ref _addr_c.val;

    return c.ctx;
}

// CertificateRequestInfo contains information from a server's
// CertificateRequest message, which is used to demand a certificate and proof
// of control from a client.
public partial struct CertificateRequestInfo {
    public slice<slice<byte>> AcceptableCAs; // SignatureSchemes lists the signature schemes that the server is
// willing to verify.
    public slice<SignatureScheme> SignatureSchemes; // Version is the TLS version that was negotiated for this connection.
    public ushort Version; // ctx is the context of the handshake that is in progress.
    public context.Context ctx;
}

// Context returns the context of the handshake that is in progress.
// This context is a child of the context passed to HandshakeContext,
// if any, and is canceled when the handshake concludes.
private static context.Context Context(this ptr<CertificateRequestInfo> _addr_c) {
    ref CertificateRequestInfo c = ref _addr_c.val;

    return c.ctx;
}

// RenegotiationSupport enumerates the different levels of support for TLS
// renegotiation. TLS renegotiation is the act of performing subsequent
// handshakes on a connection after the first. This significantly complicates
// the state machine and has been the source of numerous, subtle security
// issues. Initiating a renegotiation is not supported, but support for
// accepting renegotiation requests may be enabled.
//
// Even when enabled, the server may not change its identity between handshakes
// (i.e. the leaf certificate must be the same). Additionally, concurrent
// handshake and application data flow is not permitted so renegotiation can
// only be used with protocols that synchronise with the renegotiation, such as
// HTTPS.
//
// Renegotiation is not defined in TLS 1.3.
public partial struct RenegotiationSupport { // : nint
}

 
// RenegotiateNever disables renegotiation.
public static readonly RenegotiationSupport RenegotiateNever = iota; 

// RenegotiateOnceAsClient allows a remote server to request
// renegotiation once per connection.
public static readonly var RenegotiateOnceAsClient = 0; 

// RenegotiateFreelyAsClient allows a remote server to repeatedly
// request renegotiation.
public static readonly var RenegotiateFreelyAsClient = 1;


// A Config structure is used to configure a TLS client or server.
// After one has been passed to a TLS function it must not be
// modified. A Config may be reused; the tls package will also not
// modify it.
public partial struct Config {
    public io.Reader Rand; // Time returns the current time as the number of seconds since the epoch.
// If Time is nil, TLS uses time.Now.
    public Func<time.Time> Time; // Certificates contains one or more certificate chains to present to the
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
    public slice<Certificate> Certificates; // NameToCertificate maps from a certificate name to an element of
// Certificates. Note that a certificate name can be of the form
// '*.example.com' and so doesn't have to be a domain name as such.
//
// Deprecated: NameToCertificate only allows associating a single
// certificate with a given name. Leave this field nil to let the library
// select the first compatible chain from Certificates.
    public map<@string, ptr<Certificate>> NameToCertificate; // GetCertificate returns a Certificate based on the given
// ClientHelloInfo. It will only be called if the client supplies SNI
// information or if Certificates is empty.
//
// If GetCertificate is nil or returns nil, then the certificate is
// retrieved from NameToCertificate. If NameToCertificate is nil, the
// best element of Certificates will be used.
    public Func<ptr<ClientHelloInfo>, (ptr<Certificate>, error)> GetCertificate; // GetClientCertificate, if not nil, is called when a server requests a
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
    public Func<ptr<CertificateRequestInfo>, (ptr<Certificate>, error)> GetClientCertificate; // GetConfigForClient, if not nil, is called after a ClientHello is
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
    public Func<ptr<ClientHelloInfo>, (ptr<Config>, error)> GetConfigForClient; // VerifyPeerCertificate, if not nil, is called after normal
// certificate verification by either a TLS client or server. It
// receives the raw ASN.1 certificates provided by the peer and also
// any verified chains that normal processing found. If it returns a
// non-nil error, the handshake is aborted and that error results.
//
// If normal verification fails then the handshake will abort before
// considering this callback. If normal verification is disabled by
// setting InsecureSkipVerify, or (for a server) when ClientAuth is
// RequestClientCert or RequireAnyClientCert, then this callback will
// be considered but the verifiedChains argument will always be nil.
    public Func<slice<slice<byte>>, slice<slice<ptr<x509.Certificate>>>, error> VerifyPeerCertificate; // VerifyConnection, if not nil, is called after normal certificate
// verification and after VerifyPeerCertificate by either a TLS client
// or server. If it returns a non-nil error, the handshake is aborted
// and that error results.
//
// If normal verification fails then the handshake will abort before
// considering this callback. This callback will run for all connections
// regardless of InsecureSkipVerify or ClientAuth settings.
    public Func<ConnectionState, error> VerifyConnection; // RootCAs defines the set of root certificate authorities
// that clients use when verifying server certificates.
// If RootCAs is nil, TLS uses the host's root CA set.
    public ptr<x509.CertPool> RootCAs; // NextProtos is a list of supported application level protocols, in
// order of preference. If both peers support ALPN, the selected
// protocol will be one from this list, and the connection will fail
// if there is no mutually supported protocol. If NextProtos is empty
// or the peer doesn't support ALPN, the connection will succeed and
// ConnectionState.NegotiatedProtocol will be empty.
    public slice<@string> NextProtos; // ServerName is used to verify the hostname on the returned
// certificates unless InsecureSkipVerify is given. It is also included
// in the client's handshake to support virtual hosting unless it is
// an IP address.
    public @string ServerName; // ClientAuth determines the server's policy for
// TLS Client Authentication. The default is NoClientCert.
    public ClientAuthType ClientAuth; // ClientCAs defines the set of root certificate authorities
// that servers use if required to verify a client certificate
// by the policy in ClientAuth.
    public ptr<x509.CertPool> ClientCAs; // InsecureSkipVerify controls whether a client verifies the server's
// certificate chain and host name. If InsecureSkipVerify is true, crypto/tls
// accepts any certificate presented by the server and any host name in that
// certificate. In this mode, TLS is susceptible to machine-in-the-middle
// attacks unless custom verification is used. This should be used only for
// testing or in combination with VerifyConnection or VerifyPeerCertificate.
    public bool InsecureSkipVerify; // CipherSuites is a list of enabled TLS 1.0–1.2 cipher suites. The order of
// the list is ignored. Note that TLS 1.3 ciphersuites are not configurable.
//
// If CipherSuites is nil, a safe default list is used. The default cipher
// suites might change over time.
    public slice<ushort> CipherSuites; // PreferServerCipherSuites is a legacy field and has no effect.
//
// It used to control whether the server would follow the client's or the
// server's preference. Servers now select the best mutually supported
// cipher suite based on logic that takes into account inferred client
// hardware, server hardware, and security.
//
// Deprected: PreferServerCipherSuites is ignored.
    public bool PreferServerCipherSuites; // SessionTicketsDisabled may be set to true to disable session ticket and
// PSK (resumption) support. Note that on clients, session ticket support is
// also disabled if ClientSessionCache is nil.
    public bool SessionTicketsDisabled; // SessionTicketKey is used by TLS servers to provide session resumption.
// See RFC 5077 and the PSK mode of RFC 8446. If zero, it will be filled
// with random data before the first server handshake.
//
// Deprecated: if this field is left at zero, session ticket keys will be
// automatically rotated every day and dropped after seven days. For
// customizing the rotation schedule or synchronizing servers that are
// terminating connections for the same host, use SetSessionTicketKeys.
    public array<byte> SessionTicketKey; // ClientSessionCache is a cache of ClientSessionState entries for TLS
// session resumption. It is only used by clients.
    public ClientSessionCache ClientSessionCache; // MinVersion contains the minimum TLS version that is acceptable.
// If zero, TLS 1.0 is currently taken as the minimum.
    public ushort MinVersion; // MaxVersion contains the maximum TLS version that is acceptable.
// If zero, the maximum version supported by this package is used,
// which is currently TLS 1.3.
    public ushort MaxVersion; // CurvePreferences contains the elliptic curves that will be used in
// an ECDHE handshake, in preference order. If empty, the default will
// be used. The client will use the first preference as the type for
// its key share in TLS 1.3. This may change in the future.
    public slice<CurveID> CurvePreferences; // DynamicRecordSizingDisabled disables adaptive sizing of TLS records.
// When true, the largest possible TLS record size is always used. When
// false, the size of TLS records may be adjusted in an attempt to
// improve latency.
    public bool DynamicRecordSizingDisabled; // Renegotiation controls what types of renegotiation are supported.
// The default, none, is correct for the vast majority of applications.
    public RenegotiationSupport Renegotiation; // KeyLogWriter optionally specifies a destination for TLS master secrets
// in NSS key log format that can be used to allow external programs
// such as Wireshark to decrypt TLS connections.
// See https://developer.mozilla.org/en-US/docs/Mozilla/Projects/NSS/Key_Log_Format.
// Use of KeyLogWriter compromises security and should only be
// used for debugging.
    public io.Writer KeyLogWriter; // mutex protects sessionTicketKeys and autoSessionTicketKeys.
    public sync.RWMutex mutex; // sessionTicketKeys contains zero or more ticket keys. If set, it means the
// the keys were set with SessionTicketKey or SetSessionTicketKeys. The
// first key is used for new tickets and any subsequent keys can be used to
// decrypt old tickets. The slice contents are not protected by the mutex
// and are immutable.
    public slice<ticketKey> sessionTicketKeys; // autoSessionTicketKeys is like sessionTicketKeys but is owned by the
// auto-rotation logic. See Config.ticketKeys.
    public slice<ticketKey> autoSessionTicketKeys;
}

 
// ticketKeyNameLen is the number of bytes of identifier that is prepended to
// an encrypted session ticket in order to identify the key used to encrypt it.
private static readonly nint ticketKeyNameLen = 16; 

// ticketKeyLifetime is how long a ticket key remains valid and can be used to
// resume a client connection.
private static readonly nint ticketKeyLifetime = 7 * 24 * time.Hour; // 7 days

// ticketKeyRotation is how often the server should rotate the session ticket key
// that is used for new tickets.
private static readonly nint ticketKeyRotation = 24 * time.Hour;


// ticketKey is the internal representation of a session ticket key.
private partial struct ticketKey {
    public array<byte> keyName;
    public array<byte> aesKey;
    public array<byte> hmacKey; // created is the time at which this ticket key was created. See Config.ticketKeys.
    public time.Time created;
}

// ticketKeyFromBytes converts from the external representation of a session
// ticket key to a ticketKey. Externally, session ticket keys are 32 random
// bytes and this function expands that into sufficient name and key material.
private static ticketKey ticketKeyFromBytes(this ptr<Config> _addr_c, array<byte> b) {
    ticketKey key = default;
    b = b.Clone();
    ref Config c = ref _addr_c.val;

    var hashed = sha512.Sum512(b[..]);
    copy(key.keyName[..], hashed[..(int)ticketKeyNameLen]);
    copy(key.aesKey[..], hashed[(int)ticketKeyNameLen..(int)ticketKeyNameLen + 16]);
    copy(key.hmacKey[..], hashed[(int)ticketKeyNameLen + 16..(int)ticketKeyNameLen + 32]);
    key.created = c.time();
    return key;
}

// maxSessionTicketLifetime is the maximum allowed lifetime of a TLS 1.3 session
// ticket, and the lifetime we set for tickets we send.
private static readonly nint maxSessionTicketLifetime = 7 * 24 * time.Hour;

// Clone returns a shallow clone of c or nil if c is nil. It is safe to clone a Config that is
// being used concurrently by a TLS client or server.


// Clone returns a shallow clone of c or nil if c is nil. It is safe to clone a Config that is
// being used concurrently by a TLS client or server.
private static ptr<Config> Clone(this ptr<Config> _addr_c) => func((defer, _, _) => {
    ref Config c = ref _addr_c.val;

    if (c == null) {
        return _addr_null!;
    }
    c.mutex.RLock();
    defer(c.mutex.RUnlock());
    return addr(new Config(Rand:c.Rand,Time:c.Time,Certificates:c.Certificates,NameToCertificate:c.NameToCertificate,GetCertificate:c.GetCertificate,GetClientCertificate:c.GetClientCertificate,GetConfigForClient:c.GetConfigForClient,VerifyPeerCertificate:c.VerifyPeerCertificate,VerifyConnection:c.VerifyConnection,RootCAs:c.RootCAs,NextProtos:c.NextProtos,ServerName:c.ServerName,ClientAuth:c.ClientAuth,ClientCAs:c.ClientCAs,InsecureSkipVerify:c.InsecureSkipVerify,CipherSuites:c.CipherSuites,PreferServerCipherSuites:c.PreferServerCipherSuites,SessionTicketsDisabled:c.SessionTicketsDisabled,SessionTicketKey:c.SessionTicketKey,ClientSessionCache:c.ClientSessionCache,MinVersion:c.MinVersion,MaxVersion:c.MaxVersion,CurvePreferences:c.CurvePreferences,DynamicRecordSizingDisabled:c.DynamicRecordSizingDisabled,Renegotiation:c.Renegotiation,KeyLogWriter:c.KeyLogWriter,sessionTicketKeys:c.sessionTicketKeys,autoSessionTicketKeys:c.autoSessionTicketKeys,));

});

// deprecatedSessionTicketKey is set as the prefix of SessionTicketKey if it was
// randomized for backwards compatibility but is not in use.
private static slice<byte> deprecatedSessionTicketKey = (slice<byte>)"DEPRECATED";

// initLegacySessionTicketKeyRLocked ensures the legacy SessionTicketKey field is
// randomized if empty, and that sessionTicketKeys is populated from it otherwise.
private static void initLegacySessionTicketKeyRLocked(this ptr<Config> _addr_c) => func((defer, panic, _) => {
    ref Config c = ref _addr_c.val;
 
    // Don't write if SessionTicketKey is already defined as our deprecated string,
    // or if it is defined by the user but sessionTicketKeys is already set.
    if (c.SessionTicketKey != new array<byte>(new byte[] {  }) && (bytes.HasPrefix(c.SessionTicketKey[..], deprecatedSessionTicketKey) || len(c.sessionTicketKeys) > 0)) {
        return ;
    }
    c.mutex.RUnlock();
    defer(c.mutex.RLock());
    c.mutex.Lock();
    defer(c.mutex.Unlock());
    if (c.SessionTicketKey == new array<byte>(new byte[] {  })) {
        {
            var (_, err) = io.ReadFull(c.rand(), c.SessionTicketKey[..]);

            if (err != null) {
                panic(fmt.Sprintf("tls: unable to generate random session ticket key: %v", err));
            } 
            // Write the deprecated prefix at the beginning so we know we created
            // it. This key with the DEPRECATED prefix isn't used as an actual
            // session ticket key, and is only randomized in case the application
            // reuses it for some reason.

        } 
        // Write the deprecated prefix at the beginning so we know we created
        // it. This key with the DEPRECATED prefix isn't used as an actual
        // session ticket key, and is only randomized in case the application
        // reuses it for some reason.
        copy(c.SessionTicketKey[..], deprecatedSessionTicketKey);

    }
    else if (!bytes.HasPrefix(c.SessionTicketKey[..], deprecatedSessionTicketKey) && len(c.sessionTicketKeys) == 0) {
        c.sessionTicketKeys = new slice<ticketKey>(new ticketKey[] { c.ticketKeyFromBytes(c.SessionTicketKey) });
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
private static slice<ticketKey> ticketKeys(this ptr<Config> _addr_c, ptr<Config> _addr_configForClient) => func((defer, panic, _) => {
    ref Config c = ref _addr_c.val;
    ref Config configForClient = ref _addr_configForClient.val;
 
    // If the ConfigForClient callback returned a Config with explicitly set
    // keys, use those, otherwise just use the original Config.
    if (configForClient != null) {
        configForClient.mutex.RLock();
        if (configForClient.SessionTicketsDisabled) {
            return null;
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
    defer(c.mutex.RUnlock());
    if (c.SessionTicketsDisabled) {
        return null;
    }
    c.initLegacySessionTicketKeyRLocked();
    if (len(c.sessionTicketKeys) != 0) {
        return c.sessionTicketKeys;
    }
    if (len(c.autoSessionTicketKeys) > 0 && c.time().Sub(c.autoSessionTicketKeys[0].created) < ticketKeyRotation) {
        return c.autoSessionTicketKeys;
    }
    c.mutex.RUnlock();
    defer(c.mutex.RLock());
    c.mutex.Lock();
    defer(c.mutex.Unlock()); 
    // Re-check the condition in case it changed since obtaining the new lock.
    if (len(c.autoSessionTicketKeys) == 0 || c.time().Sub(c.autoSessionTicketKeys[0].created) >= ticketKeyRotation) {
        array<byte> newKey = new array<byte>(32);
        {
            var (_, err) = io.ReadFull(c.rand(), newKey[..]);

            if (err != null) {
                panic(fmt.Sprintf("unable to generate random session ticket key: %v", err));
            }

        }

        var valid = make_slice<ticketKey>(0, len(c.autoSessionTicketKeys) + 1);
        valid = append(valid, c.ticketKeyFromBytes(newKey));
        foreach (var (_, k) in c.autoSessionTicketKeys) { 
            // While rotating the current key, also remove any expired ones.
            if (c.time().Sub(k.created) < ticketKeyLifetime) {
                valid = append(valid, k);
            }

        }        c.autoSessionTicketKeys = valid;

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
private static void SetSessionTicketKeys(this ptr<Config> _addr_c, slice<array<byte>> keys) => func((_, panic, _) => {
    ref Config c = ref _addr_c.val;

    if (len(keys) == 0) {
        panic("tls: keys must have at least one key");
    }
    var newKeys = make_slice<ticketKey>(len(keys));
    foreach (var (i, bytes) in keys) {
        newKeys[i] = c.ticketKeyFromBytes(bytes);
    }    c.mutex.Lock();
    c.sessionTicketKeys = newKeys;
    c.mutex.Unlock();

});

private static io.Reader rand(this ptr<Config> _addr_c) {
    ref Config c = ref _addr_c.val;

    var r = c.Rand;
    if (r == null) {
        return rand.Reader;
    }
    return r;

}

private static time.Time time(this ptr<Config> _addr_c) {
    ref Config c = ref _addr_c.val;

    var t = c.Time;
    if (t == null) {
        t = time.Now;
    }
    return t();

}

private static slice<ushort> cipherSuites(this ptr<Config> _addr_c) {
    ref Config c = ref _addr_c.val;

    if (c.CipherSuites != null) {
        return c.CipherSuites;
    }
    return defaultCipherSuites;

}

private static ushort supportedVersions = new slice<ushort>(new ushort[] { VersionTLS13, VersionTLS12, VersionTLS11, VersionTLS10 });

private static slice<ushort> supportedVersions(this ptr<Config> _addr_c) {
    ref Config c = ref _addr_c.val;

    var versions = make_slice<ushort>(0, len(supportedVersions));
    foreach (var (_, v) in supportedVersions) {
        if (c != null && c.MinVersion != 0 && v < c.MinVersion) {
            continue;
        }
        if (c != null && c.MaxVersion != 0 && v > c.MaxVersion) {
            continue;
        }
        versions = append(versions, v);

    }    return versions;

}

private static ushort maxSupportedVersion(this ptr<Config> _addr_c) {
    ref Config c = ref _addr_c.val;

    var supportedVersions = c.supportedVersions();
    if (len(supportedVersions) == 0) {
        return 0;
    }
    return supportedVersions[0];

}

// supportedVersionsFromMax returns a list of supported versions derived from a
// legacy maximum version value. Note that only versions supported by this
// library are returned. Any newer peer will use supportedVersions anyway.
private static slice<ushort> supportedVersionsFromMax(ushort maxVersion) {
    var versions = make_slice<ushort>(0, len(supportedVersions));
    foreach (var (_, v) in supportedVersions) {
        if (v > maxVersion) {
            continue;
        }
        versions = append(versions, v);

    }    return versions;

}

private static CurveID defaultCurvePreferences = new slice<CurveID>(new CurveID[] { X25519, CurveP256, CurveP384, CurveP521 });

private static slice<CurveID> curvePreferences(this ptr<Config> _addr_c) {
    ref Config c = ref _addr_c.val;

    if (c == null || len(c.CurvePreferences) == 0) {
        return defaultCurvePreferences;
    }
    return c.CurvePreferences;

}

private static bool supportsCurve(this ptr<Config> _addr_c, CurveID curve) {
    ref Config c = ref _addr_c.val;

    foreach (var (_, cc) in c.curvePreferences()) {
        if (cc == curve) {
            return true;
        }
    }    return false;

}

// mutualVersion returns the protocol version to use given the advertised
// versions of the peer. Priority is given to the peer preference order.
private static (ushort, bool) mutualVersion(this ptr<Config> _addr_c, slice<ushort> peerVersions) {
    ushort _p0 = default;
    bool _p0 = default;
    ref Config c = ref _addr_c.val;

    var supportedVersions = c.supportedVersions();
    foreach (var (_, peerVersion) in peerVersions) {
        foreach (var (_, v) in supportedVersions) {
            if (v == peerVersion) {
                return (v, true);
            }
        }
    }    return (0, false);

}

private static var errNoCertificates = errors.New("tls: no certificates configured");

// getCertificate returns the best certificate for the given ClientHelloInfo,
// defaulting to the first element of c.Certificates.
private static (ptr<Certificate>, error) getCertificate(this ptr<Config> _addr_c, ptr<ClientHelloInfo> _addr_clientHello) {
    ptr<Certificate> _p0 = default!;
    error _p0 = default!;
    ref Config c = ref _addr_c.val;
    ref ClientHelloInfo clientHello = ref _addr_clientHello.val;

    if (c.GetCertificate != null && (len(c.Certificates) == 0 || len(clientHello.ServerName) > 0)) {
        var (cert, err) = c.GetCertificate(clientHello);
        if (cert != null || err != null) {
            return (_addr_cert!, error.As(err)!);
        }
    }
    if (len(c.Certificates) == 0) {
        return (_addr_null!, error.As(errNoCertificates)!);
    }
    if (len(c.Certificates) == 1) { 
        // There's only one choice, so no point doing any work.
        return (_addr__addr_c.Certificates[0]!, error.As(null!)!);

    }
    if (c.NameToCertificate != null) {
        var name = strings.ToLower(clientHello.ServerName);
        {
            var cert__prev2 = cert;

            var (cert, ok) = c.NameToCertificate[name];

            if (ok) {
                return (_addr_cert!, error.As(null!)!);
            }

            cert = cert__prev2;

        }

        if (len(name) > 0) {
            var labels = strings.Split(name, ".");
            labels[0] = "*";
            var wildcardName = strings.Join(labels, ".");
            {
                var cert__prev3 = cert;

                (cert, ok) = c.NameToCertificate[wildcardName];

                if (ok) {
                    return (_addr_cert!, error.As(null!)!);
                }

                cert = cert__prev3;

            }

        }
    }
    {
        var cert__prev1 = cert;

        foreach (var (_, __cert) in c.Certificates) {
            cert = __cert;
            {
                var err = clientHello.SupportsCertificate(_addr_cert);

                if (err == null) {
                    return (_addr__addr_cert!, error.As(null!)!);
                }

            }

        }
        cert = cert__prev1;
    }

    return (_addr__addr_c.Certificates[0]!, error.As(null!)!);

}

// SupportsCertificate returns nil if the provided certificate is supported by
// the client that sent the ClientHello. Otherwise, it returns an error
// describing the reason for the incompatibility.
//
// If this ClientHelloInfo was passed to a GetConfigForClient or GetCertificate
// callback, this method will take into account the associated Config. Note that
// if GetConfigForClient returns a different Config, the change can't be
// accounted for by this method.
//
// This function will call x509.ParseCertificate unless c.Leaf is set, which can
// incur a significant performance cost.
private static error SupportsCertificate(this ptr<ClientHelloInfo> _addr_chi, ptr<Certificate> _addr_c) {
    ref ClientHelloInfo chi = ref _addr_chi.val;
    ref Certificate c = ref _addr_c.val;
 
    // Note we don't currently support certificate_authorities nor
    // signature_algorithms_cert, and don't check the algorithms of the
    // signatures on the chain (which anyway are a SHOULD, see RFC 8446,
    // Section 4.4.2.2).

    var config = chi.config;
    if (config == null) {
        config = addr(new Config());
    }
    var (vers, ok) = config.mutualVersion(chi.SupportedVersions);
    if (!ok) {
        return error.As(errors.New("no mutually supported protocol versions"))!;
    }
    if (chi.ServerName != "") {
        var (x509Cert, err) = c.leaf();
        if (err != null) {
            return error.As(fmt.Errorf("failed to parse certificate: %w", err))!;
        }
        {
            var err = x509Cert.VerifyHostname(chi.ServerName);

            if (err != null) {
                return error.As(fmt.Errorf("certificate is not valid for requested server name: %w", err))!;
            }

        }

    }
    Func<error, error> supportsRSAFallback = unsupported => { 
        // TLS 1.3 dropped support for the static RSA key exchange.
        if (vers == VersionTLS13) {
            return error.As(unsupported)!;
        }
        {
            crypto.Decrypter priv__prev1 = priv;

            crypto.Decrypter (priv, ok) = c.PrivateKey._<crypto.Decrypter>();

            if (ok) {
                {
                    ptr<rsa.PublicKey> (_, ok) = priv.Public()._<ptr<rsa.PublicKey>>();

                    if (!ok) {
                        return error.As(unsupported)!;
                    }

                }

            }
            else
 {
                return error.As(unsupported)!;
            } 
            // Finally, there needs to be a mutual cipher suite that uses the static
            // RSA key exchange instead of ECDHE.

            priv = priv__prev1;

        } 
        // Finally, there needs to be a mutual cipher suite that uses the static
        // RSA key exchange instead of ECDHE.
        var rsaCipherSuite = selectCipherSuite(chi.CipherSuites, config.cipherSuites(), c => {
            if (c.flags & suiteECDHE != 0) {
                return error.As(false)!;
            }
            if (vers < VersionTLS12 && c.flags & suiteTLS12 != 0) {
                return error.As(false)!;
            }
            return error.As(true)!;
        });
        if (rsaCipherSuite == null) {
            return error.As(unsupported)!;
        }
        return error.As(null!)!;

    }; 

    // If the client sent the signature_algorithms extension, ensure it supports
    // schemes we can use with this certificate and TLS version.
    if (len(chi.SignatureSchemes) > 0) {
        {
            var (_, err) = selectSignatureScheme(vers, c, chi.SignatureSchemes);

            if (err != null) {
                return error.As(supportsRSAFallback(err))!;
            }

        }

    }
    if (vers == VersionTLS13) {
        return error.As(null!)!;
    }
    if (!supportsECDHE(config, chi.SupportedCurves, chi.SupportedPoints)) {
        return error.As(supportsRSAFallback(errors.New("client doesn't support ECDHE, can only use legacy RSA key exchange")))!;
    }
    bool ecdsaCipherSuite = default;
    {
        crypto.Decrypter priv__prev1 = priv;

        (priv, ok) = c.PrivateKey._<crypto.Signer>();

        if (ok) {
            switch (priv.Public().type()) {
                case ptr<ecdsa.PublicKey> pub:
                    CurveID curve = default;

                    if (pub.Curve == elliptic.P256()) 
                        curve = CurveP256;
                    else if (pub.Curve == elliptic.P384()) 
                        curve = CurveP384;
                    else if (pub.Curve == elliptic.P521()) 
                        curve = CurveP521;
                    else 
                        return error.As(supportsRSAFallback(unsupportedCertificateError(c)))!;
                                        bool curveOk = default;
                    foreach (var (_, c) in chi.SupportedCurves) {
                        if (c == curve && config.supportsCurve(c)) {
                            curveOk = true;
                            break;
                        }
                    }
        else
                    if (!curveOk) {
                        return error.As(errors.New("client doesn't support certificate curve"))!;
                    }

                    ecdsaCipherSuite = true;
                    break;
                case ed25519.PublicKey pub:
                    if (vers < VersionTLS12 || len(chi.SignatureSchemes) == 0) {
                        return error.As(errors.New("connection doesn't support Ed25519"))!;
                    }
                    ecdsaCipherSuite = true;
                    break;
                case ptr<rsa.PublicKey> pub:
                    break;
                default:
                {
                    var pub = priv.Public().type();
                    return error.As(supportsRSAFallback(unsupportedCertificateError(c)))!;
                    break;
                }
            }

        } {
            return error.As(supportsRSAFallback(unsupportedCertificateError(c)))!;
        }
        priv = priv__prev1;

    } 

    // Make sure that there is a mutually supported cipher suite that works with
    // this certificate. Cipher suite selection will then apply the logic in
    // reverse to pick it. See also serverHandshakeState.cipherSuiteOk.
    var cipherSuite = selectCipherSuite(chi.CipherSuites, config.cipherSuites(), c => {
        if (c.flags & suiteECDHE == 0) {
            return error.As(false)!;
        }
        if (c.flags & suiteECSign != 0) {
            if (!ecdsaCipherSuite) {
                return error.As(false)!;
            }
        }
        else
 {
            if (ecdsaCipherSuite) {
                return error.As(false)!;
            }
        }
        if (vers < VersionTLS12 && c.flags & suiteTLS12 != 0) {
            return error.As(false)!;
        }
        return error.As(true)!;

    });
    if (cipherSuite == null) {
        return error.As(supportsRSAFallback(errors.New("client doesn't support any cipher suites compatible with the certificate")))!;
    }
    return error.As(null!)!;

}

// SupportsCertificate returns nil if the provided certificate is supported by
// the server that sent the CertificateRequest. Otherwise, it returns an error
// describing the reason for the incompatibility.
private static error SupportsCertificate(this ptr<CertificateRequestInfo> _addr_cri, ptr<Certificate> _addr_c) {
    ref CertificateRequestInfo cri = ref _addr_cri.val;
    ref Certificate c = ref _addr_c.val;

    {
        var (_, err) = selectSignatureScheme(cri.Version, c, cri.SignatureSchemes);

        if (err != null) {
            return error.As(err)!;
        }
    }


    if (len(cri.AcceptableCAs) == 0) {
        return error.As(null!)!;
    }
    foreach (var (j, cert) in c.Certificate) {
        var x509Cert = c.Leaf; 
        // Parse the certificate if this isn't the leaf node, or if
        // chain.Leaf was nil.
        if (j != 0 || x509Cert == null) {
            error err = default!;
            x509Cert, err = x509.ParseCertificate(cert);

            if (err != null) {
                return error.As(fmt.Errorf("failed to parse certificate #%d in the chain: %w", j, err))!;
            }

        }
        foreach (var (_, ca) in cri.AcceptableCAs) {
            if (bytes.Equal(x509Cert.RawIssuer, ca)) {
                return error.As(null!)!;
            }
        }
    }    return error.As(errors.New("chain is not signed by an acceptable CA"))!;

}

// BuildNameToCertificate parses c.Certificates and builds c.NameToCertificate
// from the CommonName and SubjectAlternateName fields of each of the leaf
// certificates.
//
// Deprecated: NameToCertificate only allows associating a single certificate
// with a given name. Leave that field nil to let the library select the first
// compatible chain from Certificates.
private static void BuildNameToCertificate(this ptr<Config> _addr_c) {
    ref Config c = ref _addr_c.val;

    c.NameToCertificate = make_map<@string, ptr<Certificate>>();
    foreach (var (i) in c.Certificates) {
        var cert = _addr_c.Certificates[i];
        var (x509Cert, err) = cert.leaf();
        if (err != null) {
            continue;
        }
        if (x509Cert.Subject.CommonName != "" && len(x509Cert.DNSNames) == 0) {
            c.NameToCertificate[x509Cert.Subject.CommonName] = cert;
        }
        foreach (var (_, san) in x509Cert.DNSNames) {
            c.NameToCertificate[san] = cert;
        }
    }
}

private static readonly @string keyLogLabelTLS12 = "CLIENT_RANDOM";
private static readonly @string keyLogLabelClientHandshake = "CLIENT_HANDSHAKE_TRAFFIC_SECRET";
private static readonly @string keyLogLabelServerHandshake = "SERVER_HANDSHAKE_TRAFFIC_SECRET";
private static readonly @string keyLogLabelClientTraffic = "CLIENT_TRAFFIC_SECRET_0";
private static readonly @string keyLogLabelServerTraffic = "SERVER_TRAFFIC_SECRET_0";


private static error writeKeyLog(this ptr<Config> _addr_c, @string label, slice<byte> clientRandom, slice<byte> secret) {
    ref Config c = ref _addr_c.val;

    if (c.KeyLogWriter == null) {
        return error.As(null!)!;
    }
    slice<byte> logLine = (slice<byte>)fmt.Sprintf("%s %x %x\n", label, clientRandom, secret);

    writerMutex.Lock();
    var (_, err) = c.KeyLogWriter.Write(logLine);
    writerMutex.Unlock();

    return error.As(err)!;

}

// writerMutex protects all KeyLogWriters globally. It is rarely enabled,
// and is only for debugging, so a global mutex saves space.
private static sync.Mutex writerMutex = default;

// A Certificate is a chain of one or more certificates, leaf first.
public partial struct Certificate {
    public slice<slice<byte>> Certificate; // PrivateKey contains the private key corresponding to the public key in
// Leaf. This must implement crypto.Signer with an RSA, ECDSA or Ed25519 PublicKey.
// For a server up to TLS 1.2, it can also implement crypto.Decrypter with
// an RSA PublicKey.
    public crypto.PrivateKey PrivateKey; // SupportedSignatureAlgorithms is an optional list restricting what
// signature algorithms the PrivateKey can be used for.
    public slice<SignatureScheme> SupportedSignatureAlgorithms; // OCSPStaple contains an optional OCSP response which will be served
// to clients that request it.
    public slice<byte> OCSPStaple; // SignedCertificateTimestamps contains an optional list of Signed
// Certificate Timestamps which will be served to clients that request it.
    public slice<slice<byte>> SignedCertificateTimestamps; // Leaf is the parsed form of the leaf certificate, which may be initialized
// using x509.ParseCertificate to reduce per-handshake processing. If nil,
// the leaf certificate will be parsed as needed.
    public ptr<x509.Certificate> Leaf;
}

// leaf returns the parsed leaf certificate, either from c.Leaf or by parsing
// the corresponding c.Certificate[0].
private static (ptr<x509.Certificate>, error) leaf(this ptr<Certificate> _addr_c) {
    ptr<x509.Certificate> _p0 = default!;
    error _p0 = default!;
    ref Certificate c = ref _addr_c.val;

    if (c.Leaf != null) {
        return (_addr_c.Leaf!, error.As(null!)!);
    }
    return _addr_x509.ParseCertificate(c.Certificate[0])!;

}

private partial interface handshakeMessage {
    bool marshal();
    bool unmarshal(slice<byte> _p0);
}

// lruSessionCache is a ClientSessionCache implementation that uses an LRU
// caching strategy.
private partial struct lruSessionCache {
    public ref sync.Mutex Mutex => ref Mutex_val;
    public map<@string, ptr<list.Element>> m;
    public ptr<list.List> q;
    public nint capacity;
}

private partial struct lruSessionCacheEntry {
    public @string sessionKey;
    public ptr<ClientSessionState> state;
}

// NewLRUClientSessionCache returns a ClientSessionCache with the given
// capacity that uses an LRU strategy. If capacity is < 1, a default capacity
// is used instead.
public static ClientSessionCache NewLRUClientSessionCache(nint capacity) {
    const nint defaultSessionCacheCapacity = 64;



    if (capacity < 1) {
        capacity = defaultSessionCacheCapacity;
    }
    return addr(new lruSessionCache(m:make(map[string]*list.Element),q:list.New(),capacity:capacity,));

}

// Put adds the provided (sessionKey, cs) pair to the cache. If cs is nil, the entry
// corresponding to sessionKey is removed from the cache instead.
private static void Put(this ptr<lruSessionCache> _addr_c, @string sessionKey, ptr<ClientSessionState> _addr_cs) => func((defer, _, _) => {
    ref lruSessionCache c = ref _addr_c.val;
    ref ClientSessionState cs = ref _addr_cs.val;

    c.Lock();
    defer(c.Unlock());

    {
        var elem__prev1 = elem;

        var (elem, ok) = c.m[sessionKey];

        if (ok) {
            if (cs == null) {
                c.q.Remove(elem);
                delete(c.m, sessionKey);
            }
            else
 {
                ptr<lruSessionCacheEntry> entry = elem.Value._<ptr<lruSessionCacheEntry>>();
                entry.state = cs;
                c.q.MoveToFront(elem);
            }

            return ;

        }
        elem = elem__prev1;

    }


    if (c.q.Len() < c.capacity) {
        entry = addr(new lruSessionCacheEntry(sessionKey,cs));
        c.m[sessionKey] = c.q.PushFront(entry);
        return ;
    }
    var elem = c.q.Back();
    entry = elem.Value._<ptr<lruSessionCacheEntry>>();
    delete(c.m, entry.sessionKey);
    entry.sessionKey = sessionKey;
    entry.state = cs;
    c.q.MoveToFront(elem);
    c.m[sessionKey] = elem;

});

// Get returns the ClientSessionState value associated with a given key. It
// returns (nil, false) if no value is found.
private static (ptr<ClientSessionState>, bool) Get(this ptr<lruSessionCache> _addr_c, @string sessionKey) => func((defer, _, _) => {
    ptr<ClientSessionState> _p0 = default!;
    bool _p0 = default;
    ref lruSessionCache c = ref _addr_c.val;

    c.Lock();
    defer(c.Unlock());

    {
        var (elem, ok) = c.m[sessionKey];

        if (ok) {
            c.q.MoveToFront(elem);
            return (elem.Value._<ptr<lruSessionCacheEntry>>().state, true);
        }
    }

    return (_addr_null!, false);

});

private static Config emptyConfig = default;

private static ptr<Config> defaultConfig() {
    return _addr__addr_emptyConfig!;
}

private static error unexpectedMessageError(object wanted, object got) {
    return error.As(fmt.Errorf("tls: received unexpected handshake message of type %T when waiting for %T", got, wanted))!;
}

private static bool isSupportedSignatureAlgorithm(SignatureScheme sigAlg, slice<SignatureScheme> supportedSignatureAlgorithms) {
    foreach (var (_, s) in supportedSignatureAlgorithms) {
        if (s == sigAlg) {
            return true;
        }
    }    return false;

}

} // end tls_package
