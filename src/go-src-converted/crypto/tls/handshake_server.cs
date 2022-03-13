// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2022 March 13 05:35:58 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Program Files\Go\src\crypto\tls\handshake_server.go
namespace go.crypto;

using context = context_package;
using crypto = crypto_package;
using ecdsa = crypto.ecdsa_package;
using ed25519 = crypto.ed25519_package;
using rsa = crypto.rsa_package;
using subtle = crypto.subtle_package;
using x509 = crypto.x509_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using io = io_package;
using atomic = sync.atomic_package;
using time = time_package;


// serverHandshakeState contains details of a server handshake in progress.
// It's discarded once the handshake has completed.

public static partial class tls_package {

private partial struct serverHandshakeState {
    public ptr<Conn> c;
    public context.Context ctx;
    public ptr<clientHelloMsg> clientHello;
    public ptr<serverHelloMsg> hello;
    public ptr<cipherSuite> suite;
    public bool ecdheOk;
    public bool ecSignOk;
    public bool rsaDecryptOk;
    public bool rsaSignOk;
    public ptr<sessionState> sessionState;
    public finishedHash finishedHash;
    public slice<byte> masterSecret;
    public ptr<Certificate> cert;
}

// serverHandshake performs a TLS handshake as a server.
private static error serverHandshake(this ptr<Conn> _addr_c, context.Context ctx) {
    ref Conn c = ref _addr_c.val;

    var (clientHello, err) = c.readClientHello(ctx);
    if (err != null) {
        return error.As(err)!;
    }
    if (c.vers == VersionTLS13) {
        serverHandshakeStateTLS13 hs = new serverHandshakeStateTLS13(c:c,ctx:ctx,clientHello:clientHello,);
        return error.As(hs.handshake())!;
    }
    hs = new serverHandshakeState(c:c,ctx:ctx,clientHello:clientHello,);
    return error.As(hs.handshake())!;
}

private static error handshake(this ptr<serverHandshakeState> _addr_hs) {
    ref serverHandshakeState hs = ref _addr_hs.val;

    var c = hs.c;

    {
        var err__prev1 = err;

        var err = hs.processClientHello();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // For an overview of TLS handshaking, see RFC 5246, Section 7.3.
    c.buffering = true;
    if (hs.checkForResumption()) { 
        // The client has included a session ticket and so we do an abbreviated handshake.
        c.didResume = true;
        {
            var err__prev2 = err;

            err = hs.doResumeHandshake();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        {
            var err__prev2 = err;

            err = hs.establishKeys();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        {
            var err__prev2 = err;

            err = hs.sendSessionTicket();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        {
            var err__prev2 = err;

            err = hs.sendFinished(c.serverFinished[..]);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        {
            var err__prev2 = err;

            var (_, err) = c.flush();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        c.clientFinishedIsFirst = false;
        {
            var err__prev2 = err;

            err = hs.readFinished(null);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    else
 { 
        // The client didn't include a session ticket, or it wasn't
        // valid so we do a full handshake.
        {
            var err__prev2 = err;

            err = hs.pickCipherSuite();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        {
            var err__prev2 = err;

            err = hs.doFullHandshake();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        {
            var err__prev2 = err;

            err = hs.establishKeys();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        {
            var err__prev2 = err;

            err = hs.readFinished(c.clientFinished[..]);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        c.clientFinishedIsFirst = true;
        c.buffering = true;
        {
            var err__prev2 = err;

            err = hs.sendSessionTicket();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        {
            var err__prev2 = err;

            err = hs.sendFinished(null);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        {
            var err__prev2 = err;

            (_, err) = c.flush();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    c.ekm = ekmFromMasterSecret(c.vers, hs.suite, hs.masterSecret, hs.clientHello.random, hs.hello.random);
    atomic.StoreUint32(_addr_c.handshakeStatus, 1);

    return error.As(null!)!;
}

// readClientHello reads a ClientHello message and selects the protocol version.
private static (ptr<clientHelloMsg>, error) readClientHello(this ptr<Conn> _addr_c, context.Context ctx) {
    ptr<clientHelloMsg> _p0 = default!;
    error _p0 = default!;
    ref Conn c = ref _addr_c.val;

    var (msg, err) = c.readHandshake();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ptr<clientHelloMsg> (clientHello, ok) = msg._<ptr<clientHelloMsg>>();
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return (_addr_null!, error.As(unexpectedMessageError(clientHello, msg))!);
    }
    ptr<Config> configForClient;
    var originalConfig = c.config;
    if (c.config.GetConfigForClient != null) {
        var chi = clientHelloInfo(ctx, _addr_c, clientHello);
        configForClient, err = c.config.GetConfigForClient(chi);

        if (err != null) {
            c.sendAlert(alertInternalError);
            return (_addr_null!, error.As(err)!);
        }
        else if (configForClient != null) {
            c.config = configForClient;
        }
    }
    c.ticketKeys = originalConfig.ticketKeys(configForClient);

    var clientVersions = clientHello.supportedVersions;
    if (len(clientHello.supportedVersions) == 0) {
        clientVersions = supportedVersionsFromMax(clientHello.vers);
    }
    c.vers, ok = c.config.mutualVersion(clientVersions);
    if (!ok) {
        c.sendAlert(alertProtocolVersion);
        return (_addr_null!, error.As(fmt.Errorf("tls: client offered only unsupported versions: %x", clientVersions))!);
    }
    c.haveVers = true;
    c.@in.version = c.vers;
    c.@out.version = c.vers;

    return (_addr_clientHello!, error.As(null!)!);
}

private static error processClientHello(this ptr<serverHandshakeState> _addr_hs) {
    ref serverHandshakeState hs = ref _addr_hs.val;

    var c = hs.c;

    hs.hello = @new<serverHelloMsg>();
    hs.hello.vers = c.vers;

    var foundCompression = false; 
    // We only support null compression, so check that the client offered it.
    foreach (var (_, compression) in hs.clientHello.compressionMethods) {
        if (compression == compressionNone) {
            foundCompression = true;
            break;
        }
    }    if (!foundCompression) {
        c.sendAlert(alertHandshakeFailure);
        return error.As(errors.New("tls: client does not support uncompressed connections"))!;
    }
    hs.hello.random = make_slice<byte>(32);
    var serverRandom = hs.hello.random; 
    // Downgrade protection canaries. See RFC 8446, Section 4.1.3.
    var maxVers = c.config.maxSupportedVersion();
    if (maxVers >= VersionTLS12 && c.vers < maxVers || testingOnlyForceDowngradeCanary) {
        if (c.vers == VersionTLS12) {
            copy(serverRandom[(int)24..], downgradeCanaryTLS12);
        }
        else
 {
            copy(serverRandom[(int)24..], downgradeCanaryTLS11);
        }
        serverRandom = serverRandom[..(int)24];
    }
    var (_, err) = io.ReadFull(c.config.rand(), serverRandom);
    if (err != null) {
        c.sendAlert(alertInternalError);
        return error.As(err)!;
    }
    if (len(hs.clientHello.secureRenegotiation) != 0) {
        c.sendAlert(alertHandshakeFailure);
        return error.As(errors.New("tls: initial handshake had non-empty renegotiation extension"))!;
    }
    hs.hello.secureRenegotiationSupported = hs.clientHello.secureRenegotiationSupported;
    hs.hello.compressionMethod = compressionNone;
    if (len(hs.clientHello.serverName) > 0) {
        c.serverName = hs.clientHello.serverName;
    }
    var (selectedProto, err) = negotiateALPN(c.config.NextProtos, hs.clientHello.alpnProtocols);
    if (err != null) {
        c.sendAlert(alertNoApplicationProtocol);
        return error.As(err)!;
    }
    hs.hello.alpnProtocol = selectedProto;
    c.clientProtocol = selectedProto;

    hs.cert, err = c.config.getCertificate(clientHelloInfo(hs.ctx, _addr_c, _addr_hs.clientHello));
    if (err != null) {
        if (err == errNoCertificates) {
            c.sendAlert(alertUnrecognizedName);
        }
        else
 {
            c.sendAlert(alertInternalError);
        }
        return error.As(err)!;
    }
    if (hs.clientHello.scts) {
        hs.hello.scts = hs.cert.SignedCertificateTimestamps;
    }
    hs.ecdheOk = supportsECDHE(_addr_c.config, hs.clientHello.supportedCurves, hs.clientHello.supportedPoints);

    if (hs.ecdheOk) { 
        // Although omitting the ec_point_formats extension is permitted, some
        // old OpenSSL version will refuse to handshake if not present.
        //
        // Per RFC 4492, section 5.1.2, implementations MUST support the
        // uncompressed point format. See golang.org/issue/31943.
        hs.hello.supportedPoints = new slice<byte>(new byte[] { pointFormatUncompressed });
    }
    {
        crypto.Signer priv__prev1 = priv;

        crypto.Signer (priv, ok) = hs.cert.PrivateKey._<crypto.Signer>();

        if (ok) {
            switch (priv.Public().type()) {
                case ptr<ecdsa.PublicKey> _:
                    hs.ecSignOk = true;
                    break;
                case ed25519.PublicKey _:
                    hs.ecSignOk = true;
                    break;
                case ptr<rsa.PublicKey> _:
                    hs.rsaSignOk = true;
                    break;
                default:
                {
                    c.sendAlert(alertInternalError);
                    return error.As(fmt.Errorf("tls: unsupported signing key type (%T)", priv.Public()))!;
                    break;
                }
            }
        }
        priv = priv__prev1;

    }
    {
        crypto.Signer priv__prev1 = priv;

        (priv, ok) = hs.cert.PrivateKey._<crypto.Decrypter>();

        if (ok) {
            switch (priv.Public().type()) {
                case ptr<rsa.PublicKey> _:
                    hs.rsaDecryptOk = true;
                    break;
                default:
                {
                    c.sendAlert(alertInternalError);
                    return error.As(fmt.Errorf("tls: unsupported decryption key type (%T)", priv.Public()))!;
                    break;
                }
            }
        }
        priv = priv__prev1;

    }

    return error.As(null!)!;
}

// negotiateALPN picks a shared ALPN protocol that both sides support in server
// preference order. If ALPN is not configured or the peer doesn't support it,
// it returns "" and no error.
private static (@string, error) negotiateALPN(slice<@string> serverProtos, slice<@string> clientProtos) {
    @string _p0 = default;
    error _p0 = default!;

    if (len(serverProtos) == 0 || len(clientProtos) == 0) {
        return ("", error.As(null!)!);
    }
    bool http11fallback = default;
    foreach (var (_, s) in serverProtos) {
        foreach (var (_, c) in clientProtos) {
            if (s == c) {
                return (s, error.As(null!)!);
            }
            if (s == "h2" && c == "http/1.1") {
                http11fallback = true;
            }
        }
    }    if (http11fallback) {
        return ("", error.As(null!)!);
    }
    return ("", error.As(fmt.Errorf("tls: client requested unsupported application protocols (%s)", clientProtos))!);
}

// supportsECDHE returns whether ECDHE key exchanges can be used with this
// pre-TLS 1.3 client.
private static bool supportsECDHE(ptr<Config> _addr_c, slice<CurveID> supportedCurves, slice<byte> supportedPoints) {
    ref Config c = ref _addr_c.val;

    var supportsCurve = false;
    foreach (var (_, curve) in supportedCurves) {
        if (c.supportsCurve(curve)) {
            supportsCurve = true;
            break;
        }
    }    var supportsPointFormat = false;
    foreach (var (_, pointFormat) in supportedPoints) {
        if (pointFormat == pointFormatUncompressed) {
            supportsPointFormat = true;
            break;
        }
    }    return supportsCurve && supportsPointFormat;
}

private static error pickCipherSuite(this ptr<serverHandshakeState> _addr_hs) {
    ref serverHandshakeState hs = ref _addr_hs.val;

    var c = hs.c;

    var preferenceOrder = cipherSuitesPreferenceOrder;
    if (!hasAESGCMHardwareSupport || !aesgcmPreferred(hs.clientHello.cipherSuites)) {
        preferenceOrder = cipherSuitesPreferenceOrderNoAES;
    }
    var configCipherSuites = c.config.cipherSuites();
    var preferenceList = make_slice<ushort>(0, len(configCipherSuites));
    foreach (var (_, suiteID) in preferenceOrder) {
        {
            var id__prev2 = id;

            foreach (var (_, __id) in configCipherSuites) {
                id = __id;
                if (id == suiteID) {
                    preferenceList = append(preferenceList, id);
                    break;
                }
            }

            id = id__prev2;
        }
    }    hs.suite = selectCipherSuite(preferenceList, hs.clientHello.cipherSuites, hs.cipherSuiteOk);
    if (hs.suite == null) {
        c.sendAlert(alertHandshakeFailure);
        return error.As(errors.New("tls: no cipher suite supported by both client and server"))!;
    }
    c.cipherSuite = hs.suite.id;

    {
        var id__prev1 = id;

        foreach (var (_, __id) in hs.clientHello.cipherSuites) {
            id = __id;
            if (id == TLS_FALLBACK_SCSV) { 
                // The client is doing a fallback connection. See RFC 7507.
                if (hs.clientHello.vers < c.config.maxSupportedVersion()) {
                    c.sendAlert(alertInappropriateFallback);
                    return error.As(errors.New("tls: client using inappropriate protocol fallback"))!;
                }
                break;
            }
        }
        id = id__prev1;
    }

    return error.As(null!)!;
}

private static bool cipherSuiteOk(this ptr<serverHandshakeState> _addr_hs, ptr<cipherSuite> _addr_c) {
    ref serverHandshakeState hs = ref _addr_hs.val;
    ref cipherSuite c = ref _addr_c.val;

    if (c.flags & suiteECDHE != 0) {
        if (!hs.ecdheOk) {
            return false;
        }
        if (c.flags & suiteECSign != 0) {
            if (!hs.ecSignOk) {
                return false;
            }
        }
        else if (!hs.rsaSignOk) {
            return false;
        }
    }
    else if (!hs.rsaDecryptOk) {
        return false;
    }
    if (hs.c.vers < VersionTLS12 && c.flags & suiteTLS12 != 0) {
        return false;
    }
    return true;
}

// checkForResumption reports whether we should perform resumption on this connection.
private static bool checkForResumption(this ptr<serverHandshakeState> _addr_hs) {
    ref serverHandshakeState hs = ref _addr_hs.val;

    var c = hs.c;

    if (c.config.SessionTicketsDisabled) {
        return false;
    }
    var (plaintext, usedOldKey) = c.decryptTicket(hs.clientHello.sessionTicket);
    if (plaintext == null) {
        return false;
    }
    hs.sessionState = addr(new sessionState(usedOldKey:usedOldKey));
    var ok = hs.sessionState.unmarshal(plaintext);
    if (!ok) {
        return false;
    }
    var createdAt = time.Unix(int64(hs.sessionState.createdAt), 0);
    if (c.config.time().Sub(createdAt) > maxSessionTicketLifetime) {
        return false;
    }
    if (c.vers != hs.sessionState.vers) {
        return false;
    }
    var cipherSuiteOk = false; 
    // Check that the client is still offering the ciphersuite in the session.
    foreach (var (_, id) in hs.clientHello.cipherSuites) {
        if (id == hs.sessionState.cipherSuite) {
            cipherSuiteOk = true;
            break;
        }
    }    if (!cipherSuiteOk) {
        return false;
    }
    hs.suite = selectCipherSuite(new slice<ushort>(new ushort[] { hs.sessionState.cipherSuite }), c.config.cipherSuites(), hs.cipherSuiteOk);
    if (hs.suite == null) {
        return false;
    }
    var sessionHasClientCerts = len(hs.sessionState.certificates) != 0;
    var needClientCerts = requiresClientCert(c.config.ClientAuth);
    if (needClientCerts && !sessionHasClientCerts) {
        return false;
    }
    if (sessionHasClientCerts && c.config.ClientAuth == NoClientCert) {
        return false;
    }
    return true;
}

private static error doResumeHandshake(this ptr<serverHandshakeState> _addr_hs) {
    ref serverHandshakeState hs = ref _addr_hs.val;

    var c = hs.c;

    hs.hello.cipherSuite = hs.suite.id;
    c.cipherSuite = hs.suite.id; 
    // We echo the client's session ID in the ServerHello to let it know
    // that we're doing a resumption.
    hs.hello.sessionId = hs.clientHello.sessionId;
    hs.hello.ticketSupported = hs.sessionState.usedOldKey;
    hs.finishedHash = newFinishedHash(c.vers, hs.suite);
    hs.finishedHash.discardHandshakeBuffer();
    hs.finishedHash.Write(hs.clientHello.marshal());
    hs.finishedHash.Write(hs.hello.marshal());
    {
        var err__prev1 = err;

        var (_, err) = c.writeRecord(recordTypeHandshake, hs.hello.marshal());

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        var err = c.processCertsFromClient(new Certificate(Certificate:hs.sessionState.certificates,));

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    if (c.config.VerifyConnection != null) {
        {
            var err__prev2 = err;

            err = c.config.VerifyConnection(c.connectionStateLocked());

            if (err != null) {
                c.sendAlert(alertBadCertificate);
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    hs.masterSecret = hs.sessionState.masterSecret;

    return error.As(null!)!;
}

private static error doFullHandshake(this ptr<serverHandshakeState> _addr_hs) {
    ref serverHandshakeState hs = ref _addr_hs.val;

    var c = hs.c;

    if (hs.clientHello.ocspStapling && len(hs.cert.OCSPStaple) > 0) {
        hs.hello.ocspStapling = true;
    }
    hs.hello.ticketSupported = hs.clientHello.ticketSupported && !c.config.SessionTicketsDisabled;
    hs.hello.cipherSuite = hs.suite.id;

    hs.finishedHash = newFinishedHash(hs.c.vers, hs.suite);
    if (c.config.ClientAuth == NoClientCert) { 
        // No need to keep a full record of the handshake if client
        // certificates won't be used.
        hs.finishedHash.discardHandshakeBuffer();
    }
    hs.finishedHash.Write(hs.clientHello.marshal());
    hs.finishedHash.Write(hs.hello.marshal());
    {
        var err__prev1 = err;

        var (_, err) = c.writeRecord(recordTypeHandshake, hs.hello.marshal());

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    ptr<object> certMsg = @new<certificateMsg>();
    certMsg.certificates = hs.cert.Certificate;
    hs.finishedHash.Write(certMsg.marshal());
    {
        var err__prev1 = err;

        (_, err) = c.writeRecord(recordTypeHandshake, certMsg.marshal());

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    if (hs.hello.ocspStapling) {
        ptr<object> certStatus = @new<certificateStatusMsg>();
        certStatus.response = hs.cert.OCSPStaple;
        hs.finishedHash.Write(certStatus.marshal());
        {
            var err__prev2 = err;

            (_, err) = c.writeRecord(recordTypeHandshake, certStatus.marshal());

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    var keyAgreement = hs.suite.ka(c.vers);
    var (skx, err) = keyAgreement.generateServerKeyExchange(c.config, hs.cert, hs.clientHello, hs.hello);
    if (err != null) {
        c.sendAlert(alertHandshakeFailure);
        return error.As(err)!;
    }
    if (skx != null) {
        hs.finishedHash.Write(skx.marshal());
        {
            var err__prev2 = err;

            (_, err) = c.writeRecord(recordTypeHandshake, skx.marshal());

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    ptr<certificateRequestMsg> certReq;
    if (c.config.ClientAuth >= RequestClientCert) { 
        // Request a client certificate
        certReq = @new<certificateRequestMsg>();
        certReq.certificateTypes = new slice<byte>(new byte[] { byte(certTypeRSASign), byte(certTypeECDSASign) });
        if (c.vers >= VersionTLS12) {
            certReq.hasSignatureAlgorithm = true;
            certReq.supportedSignatureAlgorithms = supportedSignatureAlgorithms;
        }
        if (c.config.ClientCAs != null) {
            certReq.certificateAuthorities = c.config.ClientCAs.Subjects();
        }
        hs.finishedHash.Write(certReq.marshal());
        {
            var err__prev2 = err;

            (_, err) = c.writeRecord(recordTypeHandshake, certReq.marshal());

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    ptr<object> helloDone = @new<serverHelloDoneMsg>();
    hs.finishedHash.Write(helloDone.marshal());
    {
        var err__prev1 = err;

        (_, err) = c.writeRecord(recordTypeHandshake, helloDone.marshal());

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        (_, err) = c.flush();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    crypto.PublicKey pub = default; // public key for client auth, if any

    var (msg, err) = c.readHandshake();
    if (err != null) {
        return error.As(err)!;
    }
    if (c.config.ClientAuth >= RequestClientCert) {
        ptr<certificateMsg> (certMsg, ok) = msg._<ptr<certificateMsg>>();
        if (!ok) {
            c.sendAlert(alertUnexpectedMessage);
            return error.As(unexpectedMessageError(certMsg, msg))!;
        }
        hs.finishedHash.Write(certMsg.marshal());

        {
            var err__prev2 = err;

            var err = c.processCertsFromClient(new Certificate(Certificate:certMsg.certificates,));

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        if (len(certMsg.certificates) != 0) {
            pub = c.peerCertificates[0].PublicKey;
        }
        msg, err = c.readHandshake();
        if (err != null) {
            return error.As(err)!;
        }
    }
    if (c.config.VerifyConnection != null) {
        {
            var err__prev2 = err;

            err = c.config.VerifyConnection(c.connectionStateLocked());

            if (err != null) {
                c.sendAlert(alertBadCertificate);
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    ptr<clientKeyExchangeMsg> (ckx, ok) = msg._<ptr<clientKeyExchangeMsg>>();
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return error.As(unexpectedMessageError(ckx, msg))!;
    }
    hs.finishedHash.Write(ckx.marshal());

    var (preMasterSecret, err) = keyAgreement.processClientKeyExchange(c.config, hs.cert, ckx, c.vers);
    if (err != null) {
        c.sendAlert(alertHandshakeFailure);
        return error.As(err)!;
    }
    hs.masterSecret = masterFromPreMasterSecret(c.vers, hs.suite, preMasterSecret, hs.clientHello.random, hs.hello.random);
    {
        var err__prev1 = err;

        err = c.config.writeKeyLog(keyLogLabelTLS12, hs.clientHello.random, hs.masterSecret);

        if (err != null) {
            c.sendAlert(alertInternalError);
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // If we received a client cert in response to our certificate request message,
    // the client will send us a certificateVerifyMsg immediately after the
    // clientKeyExchangeMsg. This message is a digest of all preceding
    // handshake-layer messages that is signed using the private key corresponding
    // to the client's certificate. This allows us to verify that the client is in
    // possession of the private key of the certificate.
    if (len(c.peerCertificates) > 0) {
        msg, err = c.readHandshake();
        if (err != null) {
            return error.As(err)!;
        }
        ptr<certificateVerifyMsg> (certVerify, ok) = msg._<ptr<certificateVerifyMsg>>();
        if (!ok) {
            c.sendAlert(alertUnexpectedMessage);
            return error.As(unexpectedMessageError(certVerify, msg))!;
        }
        byte sigType = default;
        crypto.Hash sigHash = default;
        if (c.vers >= VersionTLS12) {
            if (!isSupportedSignatureAlgorithm(certVerify.signatureAlgorithm, certReq.supportedSignatureAlgorithms)) {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: client certificate used with invalid signature algorithm"))!;
            }
            sigType, sigHash, err = typeAndHashFromSignatureScheme(certVerify.signatureAlgorithm);
            if (err != null) {
                return error.As(c.sendAlert(alertInternalError))!;
            }
        }
        else
 {
            sigType, sigHash, err = legacyTypeAndHashFromPublicKey(pub);
            if (err != null) {
                c.sendAlert(alertIllegalParameter);
                return error.As(err)!;
            }
        }
        var signed = hs.finishedHash.hashForClientCertificate(sigType, sigHash, hs.masterSecret);
        {
            var err__prev2 = err;

            err = verifyHandshakeSignature(sigType, pub, sigHash, signed, certVerify.signature);

            if (err != null) {
                c.sendAlert(alertDecryptError);
                return error.As(errors.New("tls: invalid signature by the client certificate: " + err.Error()))!;
            }

            err = err__prev2;

        }

        hs.finishedHash.Write(certVerify.marshal());
    }
    hs.finishedHash.discardHandshakeBuffer();

    return error.As(null!)!;
}

private static error establishKeys(this ptr<serverHandshakeState> _addr_hs) {
    ref serverHandshakeState hs = ref _addr_hs.val;

    var c = hs.c;

    var (clientMAC, serverMAC, clientKey, serverKey, clientIV, serverIV) = keysFromMasterSecret(c.vers, hs.suite, hs.masterSecret, hs.clientHello.random, hs.hello.random, hs.suite.macLen, hs.suite.keyLen, hs.suite.ivLen);

    var clientCipher = default;    var serverCipher = default;

    hash.Hash clientHash = default;    hash.Hash serverHash = default;



    if (hs.suite.aead == null) {
        clientCipher = hs.suite.cipher(clientKey, clientIV, true);
        clientHash = hs.suite.mac(clientMAC);
        serverCipher = hs.suite.cipher(serverKey, serverIV, false);
        serverHash = hs.suite.mac(serverMAC);
    }
    else
 {
        clientCipher = hs.suite.aead(clientKey, clientIV);
        serverCipher = hs.suite.aead(serverKey, serverIV);
    }
    c.@in.prepareCipherSpec(c.vers, clientCipher, clientHash);
    c.@out.prepareCipherSpec(c.vers, serverCipher, serverHash);

    return error.As(null!)!;
}

private static error readFinished(this ptr<serverHandshakeState> _addr_hs, slice<byte> @out) {
    ref serverHandshakeState hs = ref _addr_hs.val;

    var c = hs.c;

    {
        var err = c.readChangeCipherSpec();

        if (err != null) {
            return error.As(err)!;
        }
    }

    var (msg, err) = c.readHandshake();
    if (err != null) {
        return error.As(err)!;
    }
    ptr<finishedMsg> (clientFinished, ok) = msg._<ptr<finishedMsg>>();
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return error.As(unexpectedMessageError(clientFinished, msg))!;
    }
    var verify = hs.finishedHash.clientSum(hs.masterSecret);
    if (len(verify) != len(clientFinished.verifyData) || subtle.ConstantTimeCompare(verify, clientFinished.verifyData) != 1) {
        c.sendAlert(alertHandshakeFailure);
        return error.As(errors.New("tls: client's Finished message is incorrect"))!;
    }
    hs.finishedHash.Write(clientFinished.marshal());
    copy(out, verify);
    return error.As(null!)!;
}

private static error sendSessionTicket(this ptr<serverHandshakeState> _addr_hs) {
    ref serverHandshakeState hs = ref _addr_hs.val;
 
    // ticketSupported is set in a resumption handshake if the
    // ticket from the client was encrypted with an old session
    // ticket key and thus a refreshed ticket should be sent.
    if (!hs.hello.ticketSupported) {
        return error.As(null!)!;
    }
    var c = hs.c;
    ptr<object> m = @new<newSessionTicketMsg>();

    var createdAt = uint64(c.config.time().Unix());
    if (hs.sessionState != null) { 
        // If this is re-wrapping an old key, then keep
        // the original time it was created.
        createdAt = hs.sessionState.createdAt;
    }
    slice<slice<byte>> certsFromClient = default;
    foreach (var (_, cert) in c.peerCertificates) {
        certsFromClient = append(certsFromClient, cert.Raw);
    }    sessionState state = new sessionState(vers:c.vers,cipherSuite:hs.suite.id,createdAt:createdAt,masterSecret:hs.masterSecret,certificates:certsFromClient,);
    error err = default!;
    m.ticket, err = c.encryptTicket(state.marshal());
    if (err != null) {
        return error.As(err)!;
    }
    hs.finishedHash.Write(m.marshal());
    {
        var (_, err) = c.writeRecord(recordTypeHandshake, m.marshal());

        if (err != null) {
            return error.As(err)!;
        }
    }

    return error.As(null!)!;
}

private static error sendFinished(this ptr<serverHandshakeState> _addr_hs, slice<byte> @out) {
    ref serverHandshakeState hs = ref _addr_hs.val;

    var c = hs.c;

    {
        var (_, err) = c.writeRecord(recordTypeChangeCipherSpec, new slice<byte>(new byte[] { 1 }));

        if (err != null) {
            return error.As(err)!;
        }
    }

    ptr<finishedMsg> finished = @new<finishedMsg>();
    finished.verifyData = hs.finishedHash.serverSum(hs.masterSecret);
    hs.finishedHash.Write(finished.marshal());
    {
        (_, err) = c.writeRecord(recordTypeHandshake, finished.marshal());

        if (err != null) {
            return error.As(err)!;
        }
    }

    copy(out, finished.verifyData);

    return error.As(null!)!;
}

// processCertsFromClient takes a chain of client certificates either from a
// Certificates message or from a sessionState and verifies them. It returns
// the public key of the leaf certificate.
private static error processCertsFromClient(this ptr<Conn> _addr_c, Certificate certificate) {
    ref Conn c = ref _addr_c.val;

    var certificates = certificate.Certificate;
    var certs = make_slice<ptr<x509.Certificate>>(len(certificates));
    error err = default!;
    foreach (var (i, asn1Data) in certificates) {
        certs[i], err = x509.ParseCertificate(asn1Data);

        if (err != null) {
            c.sendAlert(alertBadCertificate);
            return error.As(errors.New("tls: failed to parse client certificate: " + err.Error()))!;
        }
    }    if (len(certs) == 0 && requiresClientCert(c.config.ClientAuth)) {
        c.sendAlert(alertBadCertificate);
        return error.As(errors.New("tls: client didn't provide a certificate"))!;
    }
    if (c.config.ClientAuth >= VerifyClientCertIfGiven && len(certs) > 0) {
        x509.VerifyOptions opts = new x509.VerifyOptions(Roots:c.config.ClientCAs,CurrentTime:c.config.time(),Intermediates:x509.NewCertPool(),KeyUsages:[]x509.ExtKeyUsage{x509.ExtKeyUsageClientAuth},);

        foreach (var (_, cert) in certs[(int)1..]) {
            opts.Intermediates.AddCert(cert);
        }        var (chains, err) = certs[0].Verify(opts);
        if (err != null) {
            c.sendAlert(alertBadCertificate);
            return error.As(errors.New("tls: failed to verify client certificate: " + err.Error()))!;
        }
        c.verifiedChains = chains;
    }
    c.peerCertificates = certs;
    c.ocspResponse = certificate.OCSPStaple;
    c.scts = certificate.SignedCertificateTimestamps;

    if (len(certs) > 0) {
        switch (certs[0].PublicKey.type()) {
            case ptr<ecdsa.PublicKey> _:
                break;
            case ptr<rsa.PublicKey> _:
                break;
            case ed25519.PublicKey _:
                break;
            default:
            {
                c.sendAlert(alertUnsupportedCertificate);
                return error.As(fmt.Errorf("tls: client certificate contains an unsupported public key of type %T", certs[0].PublicKey))!;
                break;
            }
        }
    }
    if (c.config.VerifyPeerCertificate != null) {
        {
            error err__prev2 = err;

            err = c.config.VerifyPeerCertificate(certificates, c.verifiedChains);

            if (err != null) {
                c.sendAlert(alertBadCertificate);
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    return error.As(null!)!;
}

private static ptr<ClientHelloInfo> clientHelloInfo(context.Context ctx, ptr<Conn> _addr_c, ptr<clientHelloMsg> _addr_clientHello) {
    ref Conn c = ref _addr_c.val;
    ref clientHelloMsg clientHello = ref _addr_clientHello.val;

    var supportedVersions = clientHello.supportedVersions;
    if (len(clientHello.supportedVersions) == 0) {
        supportedVersions = supportedVersionsFromMax(clientHello.vers);
    }
    return addr(new ClientHelloInfo(CipherSuites:clientHello.cipherSuites,ServerName:clientHello.serverName,SupportedCurves:clientHello.supportedCurves,SupportedPoints:clientHello.supportedPoints,SignatureSchemes:clientHello.supportedSignatureAlgorithms,SupportedProtos:clientHello.alpnProtocols,SupportedVersions:supportedVersions,Conn:c.conn,config:c.config,ctx:ctx,));
}

} // end tls_package
