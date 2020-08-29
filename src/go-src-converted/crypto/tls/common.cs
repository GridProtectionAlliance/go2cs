// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 August 29 08:31:03 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\common.go
using list = go.container.list_package;
using crypto = go.crypto_package;
using cipherhw = go.crypto.@internal.cipherhw_package;
using rand = go.crypto.rand_package;
using sha512 = go.crypto.sha512_package;
using x509 = go.crypto.x509_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using big = go.math.big_package;
using net = go.net_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        public static readonly ulong VersionSSL30 = 0x0300UL;
        public static readonly ulong VersionTLS10 = 0x0301UL;
        public static readonly ulong VersionTLS11 = 0x0302UL;
        public static readonly ulong VersionTLS12 = 0x0303UL;

        private static readonly long maxPlaintext = 16384L; // maximum plaintext payload length
        private static readonly long maxCiphertext = 16384L + 2048L; // maximum ciphertext payload length
        private static readonly long recordHeaderLen = 5L; // record header length
        private static readonly long maxHandshake = 65536L; // maximum handshake we support (protocol max is 16 MB)
        private static readonly long maxWarnAlertCount = 5L; // maximum number of consecutive warning alerts

        private static readonly var minVersion = VersionTLS10;
        private static readonly var maxVersion = VersionTLS12;

        // TLS record types.
        private partial struct recordType // : byte
        {
        }

        private static readonly recordType recordTypeChangeCipherSpec = 20L;
        private static readonly recordType recordTypeAlert = 21L;
        private static readonly recordType recordTypeHandshake = 22L;
        private static readonly recordType recordTypeApplicationData = 23L;

        // TLS handshake message types.
        private static readonly byte typeHelloRequest = 0L;
        private static readonly byte typeClientHello = 1L;
        private static readonly byte typeServerHello = 2L;
        private static readonly byte typeNewSessionTicket = 4L;
        private static readonly byte typeCertificate = 11L;
        private static readonly byte typeServerKeyExchange = 12L;
        private static readonly byte typeCertificateRequest = 13L;
        private static readonly byte typeServerHelloDone = 14L;
        private static readonly byte typeCertificateVerify = 15L;
        private static readonly byte typeClientKeyExchange = 16L;
        private static readonly byte typeFinished = 20L;
        private static readonly byte typeCertificateStatus = 22L;
        private static readonly byte typeNextProtocol = 67L; // Not IANA assigned

        // TLS compression types.
        private static readonly byte compressionNone = 0L;

        // TLS extension numbers
        private static readonly ushort extensionServerName = 0L;
        private static readonly ushort extensionStatusRequest = 5L;
        private static readonly ushort extensionSupportedCurves = 10L;
        private static readonly ushort extensionSupportedPoints = 11L;
        private static readonly ushort extensionSignatureAlgorithms = 13L;
        private static readonly ushort extensionALPN = 16L;
        private static readonly ushort extensionSCT = 18L; // https://tools.ietf.org/html/rfc6962#section-6
        private static readonly ushort extensionSessionTicket = 35L;
        private static readonly ushort extensionNextProtoNeg = 13172L; // not IANA assigned
        private static readonly ushort extensionRenegotiationInfo = 0xff01UL;

        // TLS signaling cipher suite values
        private static readonly ushort scsvRenegotiation = 0x00ffUL;

        // CurveID is the type of a TLS identifier for an elliptic curve. See
        // http://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-8
        public partial struct CurveID // : ushort
        {
        }

        public static readonly CurveID CurveP256 = 23L;
        public static readonly CurveID CurveP384 = 24L;
        public static readonly CurveID CurveP521 = 25L;
        public static readonly CurveID X25519 = 29L;

        // TLS Elliptic Curve Point Formats
        // http://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-9
        private static readonly byte pointFormatUncompressed = 0L;

        // TLS CertificateStatusType (RFC 3546)
        private static readonly byte statusTypeOCSP = 1L;

        // Certificate types (for certificateRequestMsg)
        private static readonly long certTypeRSASign = 1L; // A certificate containing an RSA key
        private static readonly long certTypeDSSSign = 2L; // A certificate containing a DSA key
        private static readonly long certTypeRSAFixedDH = 3L; // A certificate containing a static DH key
        private static readonly long certTypeDSSFixedDH = 4L; // A certificate containing a static DH key

        // See RFC 4492 sections 3 and 5.5.
        private static readonly long certTypeECDSASign = 64L; // A certificate containing an ECDSA-capable public key, signed with ECDSA.
        private static readonly long certTypeRSAFixedECDH = 65L; // A certificate containing an ECDH-capable public key, signed with RSA.
        private static readonly long certTypeECDSAFixedECDH = 66L; // A certificate containing an ECDH-capable public key, signed with ECDSA.

        // Rest of these are reserved by the TLS spec

        // Signature algorithms for TLS 1.2 (See RFC 5246, section A.4.1)
        private static readonly byte signatureRSA = 1L;
        private static readonly byte signatureECDSA = 3L;

        // supportedSignatureAlgorithms contains the signature and hash algorithms that
        // the code advertises as supported in a TLS 1.2 ClientHello and in a TLS 1.2
        // CertificateRequest. The two fields are merged to match with TLS 1.3.
        // Note that in TLS 1.2, the ECDSA algorithms are not constrained to P-256, etc.
        private static SignatureScheme supportedSignatureAlgorithms = new slice<SignatureScheme>(new SignatureScheme[] { PKCS1WithSHA256, ECDSAWithP256AndSHA256, PKCS1WithSHA384, ECDSAWithP384AndSHA384, PKCS1WithSHA512, ECDSAWithP521AndSHA512, PKCS1WithSHA1, ECDSAWithSHA1 });

        // ConnectionState records basic TLS details about the connection.
        public partial struct ConnectionState
        {
            public ushort Version; // TLS version used by the connection (e.g. VersionTLS12)
            public bool HandshakeComplete; // TLS handshake is complete
            public bool DidResume; // connection resumes a previous TLS connection
            public ushort CipherSuite; // cipher suite in use (TLS_RSA_WITH_RC4_128_SHA, ...)
            public @string NegotiatedProtocol; // negotiated next protocol (not guaranteed to be from Config.NextProtos)
            public bool NegotiatedProtocolIsMutual; // negotiated protocol was advertised by server (client side only)
            public @string ServerName; // server name requested by client, if any (server side only)
            public slice<ref x509.Certificate> PeerCertificates; // certificate chain presented by remote peer
            public slice<slice<ref x509.Certificate>> VerifiedChains; // verified chains built from PeerCertificates
            public slice<slice<byte>> SignedCertificateTimestamps; // SCTs from the server, if any
            public slice<byte> OCSPResponse; // stapled OCSP response from server, if any

// TLSUnique contains the "tls-unique" channel binding value (see RFC
// 5929, section 3). For resumed sessions this value will be nil
// because resumption does not include enough context (see
// https://mitls.org/pages/attacks/3SHAKE#channelbindings). This will
// change in future versions of Go once the TLS master-secret fix has
// been standardized and implemented.
            public slice<byte> TLSUnique;
        }

        // ClientAuthType declares the policy the server will follow for
        // TLS Client Authentication.
        public partial struct ClientAuthType // : long
        {
        }

        public static readonly ClientAuthType NoClientCert = iota;
        public static readonly var RequestClientCert = 0;
        public static readonly var RequireAnyClientCert = 1;
        public static readonly var VerifyClientCertIfGiven = 2;
        public static readonly var RequireAndVerifyClientCert = 3;

        // ClientSessionState contains the state needed by clients to resume TLS
        // sessions.
        public partial struct ClientSessionState
        {
            public slice<byte> sessionTicket; // Encrypted ticket used for session resumption with server
            public ushort vers; // SSL/TLS version negotiated for the session
            public ushort cipherSuite; // Ciphersuite negotiated for the session
            public slice<byte> masterSecret; // MasterSecret generated by client on a full handshake
            public slice<ref x509.Certificate> serverCertificates; // Certificate chain presented by the server
            public slice<slice<ref x509.Certificate>> verifiedChains; // Certificate chains we built for verification
        }

        // ClientSessionCache is a cache of ClientSessionState objects that can be used
        // by a client to resume a TLS session with a given server. ClientSessionCache
        // implementations should expect to be called concurrently from different
        // goroutines. Only ticket-based resumption is supported, not SessionID-based
        // resumption.
        public partial interface ClientSessionCache
        {
            (ref ClientSessionState, bool) Get(@string sessionKey); // Put adds the ClientSessionState to the cache with the given key.
            (ref ClientSessionState, bool) Put(@string sessionKey, ref ClientSessionState cs);
        }

        // SignatureScheme identifies a signature algorithm supported by TLS. See
        // https://tools.ietf.org/html/draft-ietf-tls-tls13-18#section-4.2.3.
        public partial struct SignatureScheme // : ushort
        {
        }

        public static readonly SignatureScheme PKCS1WithSHA1 = 0x0201UL;
        public static readonly SignatureScheme PKCS1WithSHA256 = 0x0401UL;
        public static readonly SignatureScheme PKCS1WithSHA384 = 0x0501UL;
        public static readonly SignatureScheme PKCS1WithSHA512 = 0x0601UL;

        public static readonly SignatureScheme PSSWithSHA256 = 0x0804UL;
        public static readonly SignatureScheme PSSWithSHA384 = 0x0805UL;
        public static readonly SignatureScheme PSSWithSHA512 = 0x0806UL;

        public static readonly SignatureScheme ECDSAWithP256AndSHA256 = 0x0403UL;
        public static readonly SignatureScheme ECDSAWithP384AndSHA384 = 0x0503UL;
        public static readonly SignatureScheme ECDSAWithP521AndSHA512 = 0x0603UL; 

        // Legacy signature and hash algorithms for TLS 1.2.
        public static readonly SignatureScheme ECDSAWithSHA1 = 0x0203UL;

        // ClientHelloInfo contains information from a ClientHello message in order to
        // guide certificate selection in the GetCertificate callback.
        public partial struct ClientHelloInfo
        {
            public slice<ushort> CipherSuites; // ServerName indicates the name of the server requested by the client
// in order to support virtual hosting. ServerName is only set if the
// client is using SNI (see
// http://tools.ietf.org/html/rfc4366#section-3.1).
            public @string ServerName; // SupportedCurves lists the elliptic curves supported by the client.
// SupportedCurves is set only if the Supported Elliptic Curves
// Extension is being used (see
// http://tools.ietf.org/html/rfc4492#section-5.1.1).
            public slice<CurveID> SupportedCurves; // SupportedPoints lists the point formats supported by the client.
// SupportedPoints is set only if the Supported Point Formats Extension
// is being used (see
// http://tools.ietf.org/html/rfc4492#section-5.1.2).
            public slice<byte> SupportedPoints; // SignatureSchemes lists the signature and hash schemes that the client
// is willing to verify. SignatureSchemes is set only if the Signature
// Algorithms Extension is being used (see
// https://tools.ietf.org/html/rfc5246#section-7.4.1.4.1).
            public slice<SignatureScheme> SignatureSchemes; // SupportedProtos lists the application protocols supported by the client.
// SupportedProtos is set only if the Application-Layer Protocol
// Negotiation Extension is being used (see
// https://tools.ietf.org/html/rfc7301#section-3.1).
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
            public net.Conn Conn;
        }

        // CertificateRequestInfo contains information from a server's
        // CertificateRequest message, which is used to demand a certificate and proof
        // of control from a client.
        public partial struct CertificateRequestInfo
        {
            public slice<slice<byte>> AcceptableCAs; // SignatureSchemes lists the signature schemes that the server is
// willing to verify.
            public slice<SignatureScheme> SignatureSchemes;
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
        public partial struct RenegotiationSupport // : long
        {
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
        public partial struct Config
        {
            public io.Reader Rand; // Time returns the current time as the number of seconds since the epoch.
// If Time is nil, TLS uses time.Now.
            public Func<time.Time> Time; // Certificates contains one or more certificate chains to present to
// the other side of the connection. Server configurations must include
// at least one certificate or else set GetCertificate. Clients doing
// client-authentication may set either Certificates or
// GetClientCertificate.
            public slice<Certificate> Certificates; // NameToCertificate maps from a certificate name to an element of
// Certificates. Note that a certificate name can be of the form
// '*.example.com' and so doesn't have to be a domain name as such.
// See Config.BuildNameToCertificate
// The nil value causes the first element of Certificates to be used
// for all connections.
            public map<@string, ref Certificate> NameToCertificate; // GetCertificate returns a Certificate based on the given
// ClientHelloInfo. It will only be called if the client supplies SNI
// information or if Certificates is empty.
//
// If GetCertificate is nil or returns nil, then the certificate is
// retrieved from NameToCertificate. If NameToCertificate is nil, the
// first element of Certificates will be used.
            public Func<ref ClientHelloInfo, (ref Certificate, error)> GetCertificate; // GetClientCertificate, if not nil, is called when a server requests a
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
            public Func<ref CertificateRequestInfo, (ref Certificate, error)> GetClientCertificate; // GetConfigForClient, if not nil, is called after a ClientHello is
// received from a client. It may return a non-nil Config in order to
// change the Config that will be used to handle this connection. If
// the returned Config is nil, the original Config will be used. The
// Config returned by this callback may not be subsequently modified.
//
// If GetConfigForClient is nil, the Config passed to Server() will be
// used for all connections.
//
// Uniquely for the fields in the returned Config, session ticket keys
// will be duplicated from the original Config if not set.
// Specifically, if SetSessionTicketKeys was called on the original
// config but not on the returned config then the ticket keys from the
// original config will be copied into the new config before use.
// Otherwise, if SessionTicketKey was set in the original config but
// not in the returned config then it will be copied into the returned
// config before use. If neither of those cases applies then the key
// material from the returned config will be used for session tickets.
            public Func<ref ClientHelloInfo, (ref Config, error)> GetConfigForClient; // VerifyPeerCertificate, if not nil, is called after normal
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
            public Func<slice<slice<byte>>, slice<slice<ref x509.Certificate>>, error> VerifyPeerCertificate; // RootCAs defines the set of root certificate authorities
// that clients use when verifying server certificates.
// If RootCAs is nil, TLS uses the host's root CA set.
            public ptr<x509.CertPool> RootCAs; // NextProtos is a list of supported, application level protocols.
            public slice<@string> NextProtos; // ServerName is used to verify the hostname on the returned
// certificates unless InsecureSkipVerify is given. It is also included
// in the client's handshake to support virtual hosting unless it is
// an IP address.
            public @string ServerName; // ClientAuth determines the server's policy for
// TLS Client Authentication. The default is NoClientCert.
            public ClientAuthType ClientAuth; // ClientCAs defines the set of root certificate authorities
// that servers use if required to verify a client certificate
// by the policy in ClientAuth.
            public ptr<x509.CertPool> ClientCAs; // InsecureSkipVerify controls whether a client verifies the
// server's certificate chain and host name.
// If InsecureSkipVerify is true, TLS accepts any certificate
// presented by the server and any host name in that certificate.
// In this mode, TLS is susceptible to man-in-the-middle attacks.
// This should be used only for testing.
            public bool InsecureSkipVerify; // CipherSuites is a list of supported cipher suites. If CipherSuites
// is nil, TLS uses a list of suites supported by the implementation.
            public slice<ushort> CipherSuites; // PreferServerCipherSuites controls whether the server selects the
// client's most preferred ciphersuite, or the server's most preferred
// ciphersuite. If true then the server's preference, as expressed in
// the order of elements in CipherSuites, is used.
            public bool PreferServerCipherSuites; // SessionTicketsDisabled may be set to true to disable session ticket
// (resumption) support.
            public bool SessionTicketsDisabled; // SessionTicketKey is used by TLS servers to provide session
// resumption. See RFC 5077. If zero, it will be filled with
// random data before the first server handshake.
//
// If multiple servers are terminating connections for the same host
// they should all have the same SessionTicketKey. If the
// SessionTicketKey leaks, previously recorded and future TLS
// connections using that key are compromised.
            public array<byte> SessionTicketKey; // ClientSessionCache is a cache of ClientSessionState entries for TLS
// session resumption.
            public ClientSessionCache ClientSessionCache; // MinVersion contains the minimum SSL/TLS version that is acceptable.
// If zero, then TLS 1.0 is taken as the minimum.
            public ushort MinVersion; // MaxVersion contains the maximum SSL/TLS version that is acceptable.
// If zero, then the maximum version supported by this package is used,
// which is currently TLS 1.2.
            public ushort MaxVersion; // CurvePreferences contains the elliptic curves that will be used in
// an ECDHE handshake, in preference order. If empty, the default will
// be used.
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
            public io.Writer KeyLogWriter;
            public sync.Once serverInitOnce; // guards calling (*Config).serverInit

// mutex protects sessionTicketKeys.
            public sync.RWMutex mutex; // sessionTicketKeys contains zero or more ticket keys. If the length
// is zero, SessionTicketsDisabled must be true. The first key is used
// for new tickets and any subsequent keys can be used to decrypt old
// tickets.
            public slice<ticketKey> sessionTicketKeys;
        }

        // ticketKeyNameLen is the number of bytes of identifier that is prepended to
        // an encrypted session ticket in order to identify the key used to encrypt it.
        private static readonly long ticketKeyNameLen = 16L;

        // ticketKey is the internal representation of a session ticket key.


        // ticketKey is the internal representation of a session ticket key.
        private partial struct ticketKey
        {
            public array<byte> keyName;
            public array<byte> aesKey;
            public array<byte> hmacKey;
        }

        // ticketKeyFromBytes converts from the external representation of a session
        // ticket key to a ticketKey. Externally, session ticket keys are 32 random
        // bytes and this function expands that into sufficient name and key material.
        private static ticketKey ticketKeyFromBytes(array<byte> b)
        {
            b = b.Clone();

            var hashed = sha512.Sum512(b[..]);
            copy(key.keyName[..], hashed[..ticketKeyNameLen]);
            copy(key.aesKey[..], hashed[ticketKeyNameLen..ticketKeyNameLen + 16L]);
            copy(key.hmacKey[..], hashed[ticketKeyNameLen + 16L..ticketKeyNameLen + 32L]);
            return key;
        }

        // Clone returns a shallow clone of c. It is safe to clone a Config that is
        // being used concurrently by a TLS client or server.
        private static ref Config Clone(this ref Config c)
        { 
            // Running serverInit ensures that it's safe to read
            // SessionTicketsDisabled.
            c.serverInitOnce.Do(() =>
            {
                c.serverInit(null);

            });

            slice<ticketKey> sessionTicketKeys = default;
            c.mutex.RLock();
            sessionTicketKeys = c.sessionTicketKeys;
            c.mutex.RUnlock();

            return ref new Config(Rand:c.Rand,Time:c.Time,Certificates:c.Certificates,NameToCertificate:c.NameToCertificate,GetCertificate:c.GetCertificate,GetClientCertificate:c.GetClientCertificate,GetConfigForClient:c.GetConfigForClient,VerifyPeerCertificate:c.VerifyPeerCertificate,RootCAs:c.RootCAs,NextProtos:c.NextProtos,ServerName:c.ServerName,ClientAuth:c.ClientAuth,ClientCAs:c.ClientCAs,InsecureSkipVerify:c.InsecureSkipVerify,CipherSuites:c.CipherSuites,PreferServerCipherSuites:c.PreferServerCipherSuites,SessionTicketsDisabled:c.SessionTicketsDisabled,SessionTicketKey:c.SessionTicketKey,ClientSessionCache:c.ClientSessionCache,MinVersion:c.MinVersion,MaxVersion:c.MaxVersion,CurvePreferences:c.CurvePreferences,DynamicRecordSizingDisabled:c.DynamicRecordSizingDisabled,Renegotiation:c.Renegotiation,KeyLogWriter:c.KeyLogWriter,sessionTicketKeys:sessionTicketKeys,);
        }

        // serverInit is run under c.serverInitOnce to do initialization of c. If c was
        // returned by a GetConfigForClient callback then the argument should be the
        // Config that was passed to Server, otherwise it should be nil.
        private static void serverInit(this ref Config c, ref Config originalConfig)
        {
            if (c.SessionTicketsDisabled || len(c.ticketKeys()) != 0L)
            {
                return;
            }
            var alreadySet = false;
            foreach (var (_, b) in c.SessionTicketKey)
            {
                if (b != 0L)
                {
                    alreadySet = true;
                    break;
                }
            }
            if (!alreadySet)
            {
                if (originalConfig != null)
                {
                    copy(c.SessionTicketKey[..], originalConfig.SessionTicketKey[..]);
                }                {
                    var (_, err) = io.ReadFull(c.rand(), c.SessionTicketKey[..]);


                    else if (err != null)
                    {
                        c.SessionTicketsDisabled = true;
                        return;
                    }

                }
            }
            if (originalConfig != null)
            {
                originalConfig.mutex.RLock();
                c.sessionTicketKeys = originalConfig.sessionTicketKeys;
                originalConfig.mutex.RUnlock();
            }
            else
            {
                c.sessionTicketKeys = new slice<ticketKey>(new ticketKey[] { ticketKeyFromBytes(c.SessionTicketKey) });
            }
        }

        private static slice<ticketKey> ticketKeys(this ref Config c)
        {
            c.mutex.RLock(); 
            // c.sessionTicketKeys is constant once created. SetSessionTicketKeys
            // will only update it by replacing it with a new value.
            var ret = c.sessionTicketKeys;
            c.mutex.RUnlock();
            return ret;
        }

        // SetSessionTicketKeys updates the session ticket keys for a server. The first
        // key will be used when creating new tickets, while all keys can be used for
        // decrypting tickets. It is safe to call this function while the server is
        // running in order to rotate the session ticket keys. The function will panic
        // if keys is empty.
        private static void SetSessionTicketKeys(this ref Config _c, slice<array<byte>> keys) => func(_c, (ref Config c, Defer _, Panic panic, Recover __) =>
        {
            if (len(keys) == 0L)
            {
                panic("tls: keys must have at least one key");
            }
            var newKeys = make_slice<ticketKey>(len(keys));
            foreach (var (i, bytes) in keys)
            {
                newKeys[i] = ticketKeyFromBytes(bytes);
            }
            c.mutex.Lock();
            c.sessionTicketKeys = newKeys;
            c.mutex.Unlock();
        });

        private static io.Reader rand(this ref Config c)
        {
            var r = c.Rand;
            if (r == null)
            {
                return rand.Reader;
            }
            return r;
        }

        private static time.Time time(this ref Config c)
        {
            var t = c.Time;
            if (t == null)
            {
                t = time.Now;
            }
            return t();
        }

        private static slice<ushort> cipherSuites(this ref Config c)
        {
            var s = c.CipherSuites;
            if (s == null)
            {
                s = defaultCipherSuites();
            }
            return s;
        }

        private static ushort minVersion(this ref Config c)
        {
            if (c == null || c.MinVersion == 0L)
            {
                return minVersion;
            }
            return c.MinVersion;
        }

        private static ushort maxVersion(this ref Config c)
        {
            if (c == null || c.MaxVersion == 0L)
            {
                return maxVersion;
            }
            return c.MaxVersion;
        }

        private static CurveID defaultCurvePreferences = new slice<CurveID>(new CurveID[] { X25519, CurveP256, CurveP384, CurveP521 });

        private static slice<CurveID> curvePreferences(this ref Config c)
        {
            if (c == null || len(c.CurvePreferences) == 0L)
            {
                return defaultCurvePreferences;
            }
            return c.CurvePreferences;
        }

        // mutualVersion returns the protocol version to use given the advertised
        // version of the peer.
        private static (ushort, bool) mutualVersion(this ref Config c, ushort vers)
        {
            var minVersion = c.minVersion();
            var maxVersion = c.maxVersion();

            if (vers < minVersion)
            {
                return (0L, false);
            }
            if (vers > maxVersion)
            {
                vers = maxVersion;
            }
            return (vers, true);
        }

        // getCertificate returns the best certificate for the given ClientHelloInfo,
        // defaulting to the first element of c.Certificates.
        private static (ref Certificate, error) getCertificate(this ref Config c, ref ClientHelloInfo clientHello)
        {
            if (c.GetCertificate != null && (len(c.Certificates) == 0L || len(clientHello.ServerName) > 0L))
            {
                var (cert, err) = c.GetCertificate(clientHello);
                if (cert != null || err != null)
                {
                    return (cert, err);
                }
            }
            if (len(c.Certificates) == 0L)
            {
                return (null, errors.New("tls: no certificates configured"));
            }
            if (len(c.Certificates) == 1L || c.NameToCertificate == null)
            { 
                // There's only one choice, so no point doing any work.
                return (ref c.Certificates[0L], null);
            }
            var name = strings.ToLower(clientHello.ServerName);
            while (len(name) > 0L && name[len(name) - 1L] == '.')
            {
                name = name[..len(name) - 1L];
            }


            {
                var cert__prev1 = cert;

                var (cert, ok) = c.NameToCertificate[name];

                if (ok)
                {
                    return (cert, null);
                } 

                // try replacing labels in the name with wildcards until we get a
                // match.

                cert = cert__prev1;

            } 

            // try replacing labels in the name with wildcards until we get a
            // match.
            var labels = strings.Split(name, ".");
            foreach (var (i) in labels)
            {
                labels[i] = "*";
                var candidate = strings.Join(labels, ".");
                {
                    var cert__prev1 = cert;

                    (cert, ok) = c.NameToCertificate[candidate];

                    if (ok)
                    {
                        return (cert, null);
                    }

                    cert = cert__prev1;

                }
            } 

            // If nothing matches, return the first certificate.
            return (ref c.Certificates[0L], null);
        }

        // BuildNameToCertificate parses c.Certificates and builds c.NameToCertificate
        // from the CommonName and SubjectAlternateName fields of each of the leaf
        // certificates.
        private static void BuildNameToCertificate(this ref Config c)
        {
            c.NameToCertificate = make_map<@string, ref Certificate>();
            foreach (var (i) in c.Certificates)
            {
                var cert = ref c.Certificates[i];
                var (x509Cert, err) = x509.ParseCertificate(cert.Certificate[0L]);
                if (err != null)
                {
                    continue;
                }
                if (len(x509Cert.Subject.CommonName) > 0L)
                {
                    c.NameToCertificate[x509Cert.Subject.CommonName] = cert;
                }
                foreach (var (_, san) in x509Cert.DNSNames)
                {
                    c.NameToCertificate[san] = cert;
                }
            }
        }

        // writeKeyLog logs client random and master secret if logging was enabled by
        // setting c.KeyLogWriter.
        private static error writeKeyLog(this ref Config c, slice<byte> clientRandom, slice<byte> masterSecret)
        {
            if (c.KeyLogWriter == null)
            {
                return error.As(null);
            }
            slice<byte> logLine = (slice<byte>)fmt.Sprintf("CLIENT_RANDOM %x %x\n", clientRandom, masterSecret);

            writerMutex.Lock();
            var (_, err) = c.KeyLogWriter.Write(logLine);
            writerMutex.Unlock();

            return error.As(err);
        }

        // writerMutex protects all KeyLogWriters globally. It is rarely enabled,
        // and is only for debugging, so a global mutex saves space.
        private static sync.Mutex writerMutex = default;

        // A Certificate is a chain of one or more certificates, leaf first.
        public partial struct Certificate
        {
            public slice<slice<byte>> Certificate; // PrivateKey contains the private key corresponding to the public key
// in Leaf. For a server, this must implement crypto.Signer and/or
// crypto.Decrypter, with an RSA or ECDSA PublicKey. For a client
// (performing client authentication), this must be a crypto.Signer
// with an RSA or ECDSA PublicKey.
            public crypto.PrivateKey PrivateKey; // OCSPStaple contains an optional OCSP response which will be served
// to clients that request it.
            public slice<byte> OCSPStaple; // SignedCertificateTimestamps contains an optional list of Signed
// Certificate Timestamps which will be served to clients that request it.
            public slice<slice<byte>> SignedCertificateTimestamps; // Leaf is the parsed form of the leaf certificate, which may be
// initialized using x509.ParseCertificate to reduce per-handshake
// processing for TLS clients doing client authentication. If nil, the
// leaf certificate will be parsed as needed.
            public ptr<x509.Certificate> Leaf;
        }

        private partial interface handshakeMessage
        {
            bool marshal();
            bool unmarshal(slice<byte> _p0);
        }

        // lruSessionCache is a ClientSessionCache implementation that uses an LRU
        // caching strategy.
        private partial struct lruSessionCache
        {
            public ref sync.Mutex Mutex => ref Mutex_val;
            public map<@string, ref list.Element> m;
            public ptr<list.List> q;
            public long capacity;
        }

        private partial struct lruSessionCacheEntry
        {
            public @string sessionKey;
            public ptr<ClientSessionState> state;
        }

        // NewLRUClientSessionCache returns a ClientSessionCache with the given
        // capacity that uses an LRU strategy. If capacity is < 1, a default capacity
        // is used instead.
        public static ClientSessionCache NewLRUClientSessionCache(long capacity)
        {
            const long defaultSessionCacheCapacity = 64L;



            if (capacity < 1L)
            {
                capacity = defaultSessionCacheCapacity;
            }
            return ref new lruSessionCache(m:make(map[string]*list.Element),q:list.New(),capacity:capacity,);
        }

        // Put adds the provided (sessionKey, cs) pair to the cache.
        private static void Put(this ref lruSessionCache _c, @string sessionKey, ref ClientSessionState _cs) => func(_c, _cs, (ref lruSessionCache c, ref ClientSessionState cs, Defer defer, Panic _, Recover __) =>
        {
            c.Lock();
            defer(c.Unlock());

            {
                var elem__prev1 = elem;

                var (elem, ok) = c.m[sessionKey];

                if (ok)
                {
                    ref lruSessionCacheEntry entry = elem.Value._<ref lruSessionCacheEntry>();
                    entry.state = cs;
                    c.q.MoveToFront(elem);
                    return;
                }

                elem = elem__prev1;

            }

            if (c.q.Len() < c.capacity)
            {
                entry = ref new lruSessionCacheEntry(sessionKey,cs);
                c.m[sessionKey] = c.q.PushFront(entry);
                return;
            }
            var elem = c.q.Back();
            entry = elem.Value._<ref lruSessionCacheEntry>();
            delete(c.m, entry.sessionKey);
            entry.sessionKey = sessionKey;
            entry.state = cs;
            c.q.MoveToFront(elem);
            c.m[sessionKey] = elem;
        });

        // Get returns the ClientSessionState value associated with a given key. It
        // returns (nil, false) if no value is found.
        private static (ref ClientSessionState, bool) Get(this ref lruSessionCache _c, @string sessionKey) => func(_c, (ref lruSessionCache c, Defer defer, Panic _, Recover __) =>
        {
            c.Lock();
            defer(c.Unlock());

            {
                var (elem, ok) = c.m[sessionKey];

                if (ok)
                {
                    c.q.MoveToFront(elem);
                    return (elem.Value._<ref lruSessionCacheEntry>().state, true);
                }

            }
            return (null, false);
        });

        // TODO(jsing): Make these available to both crypto/x509 and crypto/tls.
        private partial struct dsaSignature
        {
            public ptr<big.Int> R;
            public ptr<big.Int> S;
        }

        private partial struct ecdsaSignature // : dsaSignature
        {
        }

        private static Config emptyConfig = default;

        private static ref Config defaultConfig()
        {
            return ref emptyConfig;
        }

        private static sync.Once once = default;        private static slice<ushort> varDefaultCipherSuites = default;

        private static slice<ushort> defaultCipherSuites()
        {
            once.Do(initDefaultCipherSuites);
            return varDefaultCipherSuites;
        }

        private static void initDefaultCipherSuites()
        {
            slice<ushort> topCipherSuites = default;
            if (cipherhw.AESGCMSupport())
            { 
                // If AES-GCM hardware is provided then prioritise AES-GCM
                // cipher suites.
                topCipherSuites = new slice<ushort>(new ushort[] { TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384, TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384, TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305, TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305 });
            }
            else
            { 
                // Without AES-GCM hardware, we put the ChaCha20-Poly1305
                // cipher suites first.
                topCipherSuites = new slice<ushort>(new ushort[] { TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305, TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305, TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384, TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384 });
            }
            varDefaultCipherSuites = make_slice<ushort>(0L, len(cipherSuites));
            varDefaultCipherSuites = append(varDefaultCipherSuites, topCipherSuites);

NextCipherSuite:
            foreach (var (_, suite) in cipherSuites)
            {
                if (suite.flags & suiteDefaultOff != 0L)
                {
                    continue;
                }
                foreach (var (_, existing) in varDefaultCipherSuites)
                {
                    if (existing == suite.id)
                    {
                        _continueNextCipherSuite = true;
                        break;
                    }
                }
                varDefaultCipherSuites = append(varDefaultCipherSuites, suite.id);
            }
        }

        private static error unexpectedMessageError(object wanted, object got)
        {
            return error.As(fmt.Errorf("tls: received unexpected handshake message of type %T when waiting for %T", got, wanted));
        }

        private static bool isSupportedSignatureAlgorithm(SignatureScheme sigAlg, slice<SignatureScheme> supportedSignatureAlgorithms)
        {
            foreach (var (_, s) in supportedSignatureAlgorithms)
            {
                if (s == sigAlg)
                {
                    return true;
                }
            }
            return false;
        }

        // signatureFromSignatureScheme maps a signature algorithm to the underlying
        // signature method (without hash function).
        private static byte signatureFromSignatureScheme(SignatureScheme signatureAlgorithm)
        {

            if (signatureAlgorithm == PKCS1WithSHA1 || signatureAlgorithm == PKCS1WithSHA256 || signatureAlgorithm == PKCS1WithSHA384 || signatureAlgorithm == PKCS1WithSHA512) 
                return signatureRSA;
            else if (signatureAlgorithm == ECDSAWithSHA1 || signatureAlgorithm == ECDSAWithP256AndSHA256 || signatureAlgorithm == ECDSAWithP384AndSHA384 || signatureAlgorithm == ECDSAWithP521AndSHA512) 
                return signatureECDSA;
            else 
                return 0L;
                    }
    }
}}
