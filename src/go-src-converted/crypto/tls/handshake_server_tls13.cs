// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2022 March 06 22:21:02 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Program Files\Go\src\crypto\tls\handshake_server_tls13.go
using bytes = go.bytes_package;
using context = go.context_package;
using crypto = go.crypto_package;
using hmac = go.crypto.hmac_package;
using rsa = go.crypto.rsa_package;
using errors = go.errors_package;
using hash = go.hash_package;
using io = go.io_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;

namespace go.crypto;

public static partial class tls_package {

    // maxClientPSKIdentities is the number of client PSK identities the server will
    // attempt to validate. It will ignore the rest not to let cheap ClientHello
    // messages cause too much work in session ticket decryption attempts.
private static readonly nint maxClientPSKIdentities = 5;



private partial struct serverHandshakeStateTLS13 {
    public ptr<Conn> c;
    public context.Context ctx;
    public ptr<clientHelloMsg> clientHello;
    public ptr<serverHelloMsg> hello;
    public bool sentDummyCCS;
    public bool usingPSK;
    public ptr<cipherSuiteTLS13> suite;
    public ptr<Certificate> cert;
    public SignatureScheme sigAlg;
    public slice<byte> earlySecret;
    public slice<byte> sharedKey;
    public slice<byte> handshakeSecret;
    public slice<byte> masterSecret;
    public slice<byte> trafficSecret; // client_application_traffic_secret_0
    public hash.Hash transcript;
    public slice<byte> clientFinished;
}

private static error handshake(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    var c = hs.c; 

    // For an overview of the TLS 1.3 handshake, see RFC 8446, Section 2.
    {
        var err__prev1 = err;

        var err = hs.processClientHello();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = hs.checkForResumption();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = hs.pickCertificate();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    c.buffering = true;
    {
        var err__prev1 = err;

        err = hs.sendServerParameters();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = hs.sendServerCertificate();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = hs.sendServerFinished();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 
    // Note that at this point we could start sending application data without
    // waiting for the client's second flight, but the application might not
    // expect the lack of replay protection of the ClientHello parameters.
    {
        var err__prev1 = err;

        var (_, err) = c.flush();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = hs.readClientCertificate();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = hs.readClientFinished();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }


    atomic.StoreUint32(_addr_c.handshakeStatus, 1);

    return error.As(null!)!;

}

private static error processClientHello(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    var c = hs.c;

    hs.hello = @new<serverHelloMsg>(); 

    // TLS 1.3 froze the ServerHello.legacy_version field, and uses
    // supported_versions instead. See RFC 8446, sections 4.1.3 and 4.2.1.
    hs.hello.vers = VersionTLS12;
    hs.hello.supportedVersion = c.vers;

    if (len(hs.clientHello.supportedVersions) == 0) {
        c.sendAlert(alertIllegalParameter);
        return error.As(errors.New("tls: client used the legacy version field to negotiate TLS 1.3"))!;
    }
    foreach (var (_, id) in hs.clientHello.cipherSuites) {
        if (id == TLS_FALLBACK_SCSV) { 
            // Use c.vers instead of max(supported_versions) because an attacker
            // could defeat this by adding an arbitrary high version otherwise.
            if (c.vers < c.config.maxSupportedVersion()) {
                c.sendAlert(alertInappropriateFallback);
                return error.As(errors.New("tls: client using inappropriate protocol fallback"))!;
            }

            break;

        }
    }    if (len(hs.clientHello.compressionMethods) != 1 || hs.clientHello.compressionMethods[0] != compressionNone) {
        c.sendAlert(alertIllegalParameter);
        return error.As(errors.New("tls: TLS 1.3 client supports illegal compression methods"))!;
    }
    hs.hello.random = make_slice<byte>(32);
    {
        var (_, err) = io.ReadFull(c.config.rand(), hs.hello.random);

        if (err != null) {
            c.sendAlert(alertInternalError);
            return error.As(err)!;
        }
    }


    if (len(hs.clientHello.secureRenegotiation) != 0) {
        c.sendAlert(alertHandshakeFailure);
        return error.As(errors.New("tls: initial handshake had non-empty renegotiation extension"))!;
    }
    if (hs.clientHello.earlyData) { 
        // See RFC 8446, Section 4.2.10 for the complicated behavior required
        // here. The scenario is that a different server at our address offered
        // to accept early data in the past, which we can't handle. For now, all
        // 0-RTT enabled session tickets need to expire before a Go server can
        // replace a server or join a pool. That's the same requirement that
        // applies to mixing or replacing with any TLS 1.2 server.
        c.sendAlert(alertUnsupportedExtension);
        return error.As(errors.New("tls: client sent unexpected early data"))!;

    }
    hs.hello.sessionId = hs.clientHello.sessionId;
    hs.hello.compressionMethod = compressionNone;

    var preferenceList = defaultCipherSuitesTLS13;
    if (!hasAESGCMHardwareSupport || !aesgcmPreferred(hs.clientHello.cipherSuites)) {
        preferenceList = defaultCipherSuitesTLS13NoAES;
    }
    foreach (var (_, suiteID) in preferenceList) {
        hs.suite = mutualCipherSuiteTLS13(hs.clientHello.cipherSuites, suiteID);
        if (hs.suite != null) {
            break;
        }
    }    if (hs.suite == null) {
        c.sendAlert(alertHandshakeFailure);
        return error.As(errors.New("tls: no cipher suite supported by both client and server"))!;
    }
    c.cipherSuite = hs.suite.id;
    hs.hello.cipherSuite = hs.suite.id;
    hs.transcript = hs.suite.hash.New(); 

    // Pick the ECDHE group in server preference order, but give priority to
    // groups with a key share, to avoid a HelloRetryRequest round-trip.
    CurveID selectedGroup = default;
    ptr<keyShare> clientKeyShare;
GroupSelection:
    foreach (var (_, preferredGroup) in c.config.curvePreferences()) {
        foreach (var (_, ks) in hs.clientHello.keyShares) {
            if (ks.group == preferredGroup) {
                selectedGroup = ks.group;
                clientKeyShare = _addr_ks;
                _breakGroupSelection = true;
                break;
            }

        }        if (selectedGroup != 0) {
            continue;
        }
        foreach (var (_, group) in hs.clientHello.supportedCurves) {
            if (group == preferredGroup) {
                selectedGroup = group;
                break;
            }
        }
    }    if (selectedGroup == 0) {
        c.sendAlert(alertHandshakeFailure);
        return error.As(errors.New("tls: no ECDHE curve supported by both client and server"))!;
    }
    if (clientKeyShare == null) {
        {
            var err = hs.doHelloRetryRequest(selectedGroup);

            if (err != null) {
                return error.As(err)!;
            }

        }

        clientKeyShare = _addr_hs.clientHello.keyShares[0];

    }
    {
        var (_, ok) = curveForCurveID(selectedGroup);

        if (selectedGroup != X25519 && !ok) {
            c.sendAlert(alertInternalError);
            return error.As(errors.New("tls: CurvePreferences includes unsupported curve"))!;
        }
    }

    var (params, err) = generateECDHEParameters(c.config.rand(), selectedGroup);
    if (err != null) {
        c.sendAlert(alertInternalError);
        return error.As(err)!;
    }
    hs.hello.serverShare = new keyShare(group:selectedGroup,data:params.PublicKey());
    hs.sharedKey = @params.SharedKey(clientKeyShare.data);
    if (hs.sharedKey == null) {
        c.sendAlert(alertIllegalParameter);
        return error.As(errors.New("tls: invalid client key share"))!;
    }
    c.serverName = hs.clientHello.serverName;
    return error.As(null!)!;

}

private static error checkForResumption(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    var c = hs.c;

    if (c.config.SessionTicketsDisabled) {
        return error.As(null!)!;
    }
    var modeOK = false;
    foreach (var (_, mode) in hs.clientHello.pskModes) {
        if (mode == pskModeDHE) {
            modeOK = true;
            break;
        }
    }    if (!modeOK) {
        return error.As(null!)!;
    }
    if (len(hs.clientHello.pskIdentities) != len(hs.clientHello.pskBinders)) {
        c.sendAlert(alertIllegalParameter);
        return error.As(errors.New("tls: invalid or missing PSK binders"))!;
    }
    if (len(hs.clientHello.pskIdentities) == 0) {
        return error.As(null!)!;
    }
    foreach (var (i, identity) in hs.clientHello.pskIdentities) {
        if (i >= maxClientPSKIdentities) {
            break;
        }
        var (plaintext, _) = c.decryptTicket(identity.label);
        if (plaintext == null) {
            continue;
        }
        ptr<object> sessionState = @new<sessionStateTLS13>();
        {
            var ok = sessionState.unmarshal(plaintext);

            if (!ok) {
                continue;
            }

        }


        var createdAt = time.Unix(int64(sessionState.createdAt), 0);
        if (c.config.time().Sub(createdAt) > maxSessionTicketLifetime) {
            continue;
        }
        var pskSuite = cipherSuiteTLS13ByID(sessionState.cipherSuite);
        if (pskSuite == null || pskSuite.hash != hs.suite.hash) {
            continue;
        }
        var sessionHasClientCerts = len(sessionState.certificate.Certificate) != 0;
        var needClientCerts = requiresClientCert(c.config.ClientAuth);
        if (needClientCerts && !sessionHasClientCerts) {
            continue;
        }
        if (sessionHasClientCerts && c.config.ClientAuth == NoClientCert) {
            continue;
        }
        var psk = hs.suite.expandLabel(sessionState.resumptionSecret, "resumption", null, hs.suite.hash.Size());
        hs.earlySecret = hs.suite.extract(psk, null);
        var binderKey = hs.suite.deriveSecret(hs.earlySecret, resumptionBinderLabel, null); 
        // Clone the transcript in case a HelloRetryRequest was recorded.
        var transcript = cloneHash(hs.transcript, hs.suite.hash);
        if (transcript == null) {
            c.sendAlert(alertInternalError);
            return error.As(errors.New("tls: internal error: failed to clone hash"))!;
        }
        transcript.Write(hs.clientHello.marshalWithoutBinders());
        var pskBinder = hs.suite.finishedHash(binderKey, transcript);
        if (!hmac.Equal(hs.clientHello.pskBinders[i], pskBinder)) {
            c.sendAlert(alertDecryptError);
            return error.As(errors.New("tls: invalid PSK binder"))!;
        }
        c.didResume = true;
        {
            var err = c.processCertsFromClient(sessionState.certificate);

            if (err != null) {
                return error.As(err)!;
            }

        }


        hs.hello.selectedIdentityPresent = true;
        hs.hello.selectedIdentity = uint16(i);
        hs.usingPSK = true;
        return error.As(null!)!;

    }    return error.As(null!)!;

}

// cloneHash uses the encoding.BinaryMarshaler and encoding.BinaryUnmarshaler
// interfaces implemented by standard library hashes to clone the state of in
// to a new instance of h. It returns nil if the operation fails.
private static hash.Hash cloneHash(hash.Hash @in, crypto.Hash h) { 
    // Recreate the interface to avoid importing encoding.
    private partial interface binaryMarshaler {
        error MarshalBinary();
        error UnmarshalBinary(slice<byte> data);
    }
    binaryMarshaler (marshaler, ok) = binaryMarshaler.As(in._<binaryMarshaler>())!;
    if (!ok) {
        return null;
    }
    var (state, err) = marshaler.MarshalBinary();
    if (err != null) {
        return null;
    }
    var @out = h.New();
    binaryMarshaler (unmarshaler, ok) = binaryMarshaler.As(out._<binaryMarshaler>())!;
    if (!ok) {
        return null;
    }
    {
        var err = unmarshaler.UnmarshalBinary(state);

        if (err != null) {
            return null;
        }
    }

    return out;

}

private static error pickCertificate(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    var c = hs.c; 

    // Only one of PSK and certificates are used at a time.
    if (hs.usingPSK) {
        return error.As(null!)!;
    }
    if (len(hs.clientHello.supportedSignatureAlgorithms) == 0) {
        return error.As(c.sendAlert(alertMissingExtension))!;
    }
    var (certificate, err) = c.config.getCertificate(clientHelloInfo(hs.ctx, c, hs.clientHello));
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
    hs.sigAlg, err = selectSignatureScheme(c.vers, certificate, hs.clientHello.supportedSignatureAlgorithms);
    if (err != null) { 
        // getCertificate returned a certificate that is unsupported or
        // incompatible with the client's signature algorithms.
        c.sendAlert(alertHandshakeFailure);
        return error.As(err)!;

    }
    hs.cert = certificate;

    return error.As(null!)!;

}

// sendDummyChangeCipherSpec sends a ChangeCipherSpec record for compatibility
// with middleboxes that didn't implement TLS correctly. See RFC 8446, Appendix D.4.
private static error sendDummyChangeCipherSpec(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    if (hs.sentDummyCCS) {
        return error.As(null!)!;
    }
    hs.sentDummyCCS = true;

    var (_, err) = hs.c.writeRecord(recordTypeChangeCipherSpec, new slice<byte>(new byte[] { 1 }));
    return error.As(err)!;

}

private static error doHelloRetryRequest(this ptr<serverHandshakeStateTLS13> _addr_hs, CurveID selectedGroup) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    var c = hs.c; 

    // The first ClientHello gets double-hashed into the transcript upon a
    // HelloRetryRequest. See RFC 8446, Section 4.4.1.
    hs.transcript.Write(hs.clientHello.marshal());
    var chHash = hs.transcript.Sum(null);
    hs.transcript.Reset();
    hs.transcript.Write(new slice<byte>(new byte[] { typeMessageHash, 0, 0, uint8(len(chHash)) }));
    hs.transcript.Write(chHash);

    ptr<serverHelloMsg> helloRetryRequest = addr(new serverHelloMsg(vers:hs.hello.vers,random:helloRetryRequestRandom,sessionId:hs.hello.sessionId,cipherSuite:hs.hello.cipherSuite,compressionMethod:hs.hello.compressionMethod,supportedVersion:hs.hello.supportedVersion,selectedGroup:selectedGroup,));

    hs.transcript.Write(helloRetryRequest.marshal());
    {
        var (_, err) = c.writeRecord(recordTypeHandshake, helloRetryRequest.marshal());

        if (err != null) {
            return error.As(err)!;
        }
    }


    {
        var err = hs.sendDummyChangeCipherSpec();

        if (err != null) {
            return error.As(err)!;
        }
    }


    var (msg, err) = c.readHandshake();
    if (err != null) {
        return error.As(err)!;
    }
    ptr<clientHelloMsg> (clientHello, ok) = msg._<ptr<clientHelloMsg>>();
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return error.As(unexpectedMessageError(clientHello, msg))!;
    }
    if (len(clientHello.keyShares) != 1 || clientHello.keyShares[0].group != selectedGroup) {
        c.sendAlert(alertIllegalParameter);
        return error.As(errors.New("tls: client sent invalid key share in second ClientHello"))!;
    }
    if (clientHello.earlyData) {
        c.sendAlert(alertIllegalParameter);
        return error.As(errors.New("tls: client indicated early data in second ClientHello"))!;
    }
    if (illegalClientHelloChange(clientHello, _addr_hs.clientHello)) {
        c.sendAlert(alertIllegalParameter);
        return error.As(errors.New("tls: client illegally modified second ClientHello"))!;
    }
    hs.clientHello = clientHello;
    return error.As(null!)!;

}

// illegalClientHelloChange reports whether the two ClientHello messages are
// different, with the exception of the changes allowed before and after a
// HelloRetryRequest. See RFC 8446, Section 4.1.2.
private static bool illegalClientHelloChange(ptr<clientHelloMsg> _addr_ch, ptr<clientHelloMsg> _addr_ch1) {
    ref clientHelloMsg ch = ref _addr_ch.val;
    ref clientHelloMsg ch1 = ref _addr_ch1.val;

    if (len(ch.supportedVersions) != len(ch1.supportedVersions) || len(ch.cipherSuites) != len(ch1.cipherSuites) || len(ch.supportedCurves) != len(ch1.supportedCurves) || len(ch.supportedSignatureAlgorithms) != len(ch1.supportedSignatureAlgorithms) || len(ch.supportedSignatureAlgorithmsCert) != len(ch1.supportedSignatureAlgorithmsCert) || len(ch.alpnProtocols) != len(ch1.alpnProtocols)) {
        return true;
    }
    {
        var i__prev1 = i;

        foreach (var (__i) in ch.supportedVersions) {
            i = __i;
            if (ch.supportedVersions[i] != ch1.supportedVersions[i]) {
                return true;
            }
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in ch.cipherSuites) {
            i = __i;
            if (ch.cipherSuites[i] != ch1.cipherSuites[i]) {
                return true;
            }
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in ch.supportedCurves) {
            i = __i;
            if (ch.supportedCurves[i] != ch1.supportedCurves[i]) {
                return true;
            }
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in ch.supportedSignatureAlgorithms) {
            i = __i;
            if (ch.supportedSignatureAlgorithms[i] != ch1.supportedSignatureAlgorithms[i]) {
                return true;
            }
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in ch.supportedSignatureAlgorithmsCert) {
            i = __i;
            if (ch.supportedSignatureAlgorithmsCert[i] != ch1.supportedSignatureAlgorithmsCert[i]) {
                return true;
            }
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in ch.alpnProtocols) {
            i = __i;
            if (ch.alpnProtocols[i] != ch1.alpnProtocols[i]) {
                return true;
            }
        }
        i = i__prev1;
    }

    return ch.vers != ch1.vers || !bytes.Equal(ch.random, ch1.random) || !bytes.Equal(ch.sessionId, ch1.sessionId) || !bytes.Equal(ch.compressionMethods, ch1.compressionMethods) || ch.serverName != ch1.serverName || ch.ocspStapling != ch1.ocspStapling || !bytes.Equal(ch.supportedPoints, ch1.supportedPoints) || ch.ticketSupported != ch1.ticketSupported || !bytes.Equal(ch.sessionTicket, ch1.sessionTicket) || ch.secureRenegotiationSupported != ch1.secureRenegotiationSupported || !bytes.Equal(ch.secureRenegotiation, ch1.secureRenegotiation) || ch.scts != ch1.scts || !bytes.Equal(ch.cookie, ch1.cookie) || !bytes.Equal(ch.pskModes, ch1.pskModes);

}

private static error sendServerParameters(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    var c = hs.c;

    hs.transcript.Write(hs.clientHello.marshal());
    hs.transcript.Write(hs.hello.marshal());
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

        var err = hs.sendDummyChangeCipherSpec();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }


    var earlySecret = hs.earlySecret;
    if (earlySecret == null) {
        earlySecret = hs.suite.extract(null, null);
    }
    hs.handshakeSecret = hs.suite.extract(hs.sharedKey, hs.suite.deriveSecret(earlySecret, "derived", null));

    var clientSecret = hs.suite.deriveSecret(hs.handshakeSecret, clientHandshakeTrafficLabel, hs.transcript);
    c.@in.setTrafficSecret(hs.suite, clientSecret);
    var serverSecret = hs.suite.deriveSecret(hs.handshakeSecret, serverHandshakeTrafficLabel, hs.transcript);
    c.@out.setTrafficSecret(hs.suite, serverSecret);

    err = c.config.writeKeyLog(keyLogLabelClientHandshake, hs.clientHello.random, clientSecret);
    if (err != null) {
        c.sendAlert(alertInternalError);
        return error.As(err)!;
    }
    err = c.config.writeKeyLog(keyLogLabelServerHandshake, hs.clientHello.random, serverSecret);
    if (err != null) {
        c.sendAlert(alertInternalError);
        return error.As(err)!;
    }
    ptr<object> encryptedExtensions = @new<encryptedExtensionsMsg>();

    var (selectedProto, err) = negotiateALPN(c.config.NextProtos, hs.clientHello.alpnProtocols);
    if (err != null) {
        c.sendAlert(alertNoApplicationProtocol);
        return error.As(err)!;
    }
    encryptedExtensions.alpnProtocol = selectedProto;
    c.clientProtocol = selectedProto;

    hs.transcript.Write(encryptedExtensions.marshal());
    {
        var err__prev1 = err;

        (_, err) = c.writeRecord(recordTypeHandshake, encryptedExtensions.marshal());

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }


    return error.As(null!)!;

}

private static bool requestClientCert(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    return hs.c.config.ClientAuth >= RequestClientCert && !hs.usingPSK;
}

private static error sendServerCertificate(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    var c = hs.c; 

    // Only one of PSK and certificates are used at a time.
    if (hs.usingPSK) {
        return error.As(null!)!;
    }
    if (hs.requestClientCert()) { 
        // Request a client certificate
        ptr<object> certReq = @new<certificateRequestMsgTLS13>();
        certReq.ocspStapling = true;
        certReq.scts = true;
        certReq.supportedSignatureAlgorithms = supportedSignatureAlgorithms;
        if (c.config.ClientCAs != null) {
            certReq.certificateAuthorities = c.config.ClientCAs.Subjects();
        }
        hs.transcript.Write(certReq.marshal());
        {
            var (_, err) = c.writeRecord(recordTypeHandshake, certReq.marshal());

            if (err != null) {
                return error.As(err)!;
            }

        }

    }
    ptr<object> certMsg = @new<certificateMsgTLS13>();

    certMsg.certificate = hs.cert.val;
    certMsg.scts = hs.clientHello.scts && len(hs.cert.SignedCertificateTimestamps) > 0;
    certMsg.ocspStapling = hs.clientHello.ocspStapling && len(hs.cert.OCSPStaple) > 0;

    hs.transcript.Write(certMsg.marshal());
    {
        (_, err) = c.writeRecord(recordTypeHandshake, certMsg.marshal());

        if (err != null) {
            return error.As(err)!;
        }
    }


    ptr<object> certVerifyMsg = @new<certificateVerifyMsg>();
    certVerifyMsg.hasSignatureAlgorithm = true;
    certVerifyMsg.signatureAlgorithm = hs.sigAlg;

    var (sigType, sigHash, err) = typeAndHashFromSignatureScheme(hs.sigAlg);
    if (err != null) {
        return error.As(c.sendAlert(alertInternalError))!;
    }
    var signed = signedMessage(sigHash, serverSignatureContext, hs.transcript);
    var signOpts = crypto.SignerOpts(sigHash);
    if (sigType == signatureRSAPSS) {
        signOpts = addr(new rsa.PSSOptions(SaltLength:rsa.PSSSaltLengthEqualsHash,Hash:sigHash));
    }
    crypto.Signer (sig, err) = hs.cert.PrivateKey._<crypto.Signer>().Sign(c.config.rand(), signed, signOpts);
    if (err != null) {
        crypto.Signer @public = hs.cert.PrivateKey._<crypto.Signer>().Public();
        {
            ptr<rsa.PublicKey> (rsaKey, ok) = public._<ptr<rsa.PublicKey>>();

            if (ok && sigType == signatureRSAPSS && rsaKey.N.BitLen() / 8 < sigHash.Size() * 2 + 2) { // key too small for RSA-PSS
                c.sendAlert(alertHandshakeFailure);

            }
            else
 {
                c.sendAlert(alertInternalError);
            }

        }

        return error.As(errors.New("tls: failed to sign handshake: " + err.Error()))!;

    }
    certVerifyMsg.signature = sig;

    hs.transcript.Write(certVerifyMsg.marshal());
    {
        (_, err) = c.writeRecord(recordTypeHandshake, certVerifyMsg.marshal());

        if (err != null) {
            return error.As(err)!;
        }
    }


    return error.As(null!)!;

}

private static error sendServerFinished(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    var c = hs.c;

    ptr<finishedMsg> finished = addr(new finishedMsg(verifyData:hs.suite.finishedHash(c.out.trafficSecret,hs.transcript),));

    hs.transcript.Write(finished.marshal());
    {
        var err__prev1 = err;

        var (_, err) = c.writeRecord(recordTypeHandshake, finished.marshal());

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // Derive secrets that take context through the server Finished.

    hs.masterSecret = hs.suite.extract(null, hs.suite.deriveSecret(hs.handshakeSecret, "derived", null));

    hs.trafficSecret = hs.suite.deriveSecret(hs.masterSecret, clientApplicationTrafficLabel, hs.transcript);
    var serverSecret = hs.suite.deriveSecret(hs.masterSecret, serverApplicationTrafficLabel, hs.transcript);
    c.@out.setTrafficSecret(hs.suite, serverSecret);

    var err = c.config.writeKeyLog(keyLogLabelClientTraffic, hs.clientHello.random, hs.trafficSecret);
    if (err != null) {
        c.sendAlert(alertInternalError);
        return error.As(err)!;
    }
    err = c.config.writeKeyLog(keyLogLabelServerTraffic, hs.clientHello.random, serverSecret);
    if (err != null) {
        c.sendAlert(alertInternalError);
        return error.As(err)!;
    }
    c.ekm = hs.suite.exportKeyingMaterial(hs.masterSecret, hs.transcript); 

    // If we did not request client certificates, at this point we can
    // precompute the client finished and roll the transcript forward to send
    // session tickets in our first flight.
    if (!hs.requestClientCert()) {
        {
            var err__prev2 = err;

            err = hs.sendSessionTickets();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }

    }
    return error.As(null!)!;

}

private static bool shouldSendSessionTickets(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    if (hs.c.config.SessionTicketsDisabled) {
        return false;
    }
    foreach (var (_, pskMode) in hs.clientHello.pskModes) {
        if (pskMode == pskModeDHE) {
            return true;
        }
    }    return false;

}

private static error sendSessionTickets(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    var c = hs.c;

    hs.clientFinished = hs.suite.finishedHash(c.@in.trafficSecret, hs.transcript);
    ptr<finishedMsg> finishedMsg = addr(new finishedMsg(verifyData:hs.clientFinished,));
    hs.transcript.Write(finishedMsg.marshal());

    if (!hs.shouldSendSessionTickets()) {
        return error.As(null!)!;
    }
    var resumptionSecret = hs.suite.deriveSecret(hs.masterSecret, resumptionLabel, hs.transcript);

    ptr<object> m = @new<newSessionTicketMsgTLS13>();

    slice<slice<byte>> certsFromClient = default;
    foreach (var (_, cert) in c.peerCertificates) {
        certsFromClient = append(certsFromClient, cert.Raw);
    }    sessionStateTLS13 state = new sessionStateTLS13(cipherSuite:hs.suite.id,createdAt:uint64(c.config.time().Unix()),resumptionSecret:resumptionSecret,certificate:Certificate{Certificate:certsFromClient,OCSPStaple:c.ocspResponse,SignedCertificateTimestamps:c.scts,},);
    error err = default!;
    m.label, err = c.encryptTicket(state.marshal());
    if (err != null) {
        return error.As(err)!;
    }
    m.lifetime = uint32(maxSessionTicketLifetime / time.Second);

    {
        var (_, err) = c.writeRecord(recordTypeHandshake, m.marshal());

        if (err != null) {
            return error.As(err)!;
        }
    }


    return error.As(null!)!;

}

private static error readClientCertificate(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    var c = hs.c;

    if (!hs.requestClientCert()) { 
        // Make sure the connection is still being verified whether or not
        // the server requested a client certificate.
        if (c.config.VerifyConnection != null) {
            {
                var err__prev3 = err;

                var err = c.config.VerifyConnection(c.connectionStateLocked());

                if (err != null) {
                    c.sendAlert(alertBadCertificate);
                    return error.As(err)!;
                }

                err = err__prev3;

            }

        }
        return error.As(null!)!;

    }
    var (msg, err) = c.readHandshake();
    if (err != null) {
        return error.As(err)!;
    }
    ptr<certificateMsgTLS13> (certMsg, ok) = msg._<ptr<certificateMsgTLS13>>();
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return error.As(unexpectedMessageError(certMsg, msg))!;
    }
    hs.transcript.Write(certMsg.marshal());

    {
        var err__prev1 = err;

        err = c.processCertsFromClient(certMsg.certificate);

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
    if (len(certMsg.certificate.Certificate) != 0) {
        msg, err = c.readHandshake();
        if (err != null) {
            return error.As(err)!;
        }
        ptr<certificateVerifyMsg> (certVerify, ok) = msg._<ptr<certificateVerifyMsg>>();
        if (!ok) {
            c.sendAlert(alertUnexpectedMessage);
            return error.As(unexpectedMessageError(certVerify, msg))!;
        }
        if (!isSupportedSignatureAlgorithm(certVerify.signatureAlgorithm, supportedSignatureAlgorithms)) {
            c.sendAlert(alertIllegalParameter);
            return error.As(errors.New("tls: client certificate used with invalid signature algorithm"))!;
        }
        var (sigType, sigHash, err) = typeAndHashFromSignatureScheme(certVerify.signatureAlgorithm);
        if (err != null) {
            return error.As(c.sendAlert(alertInternalError))!;
        }
        if (sigType == signaturePKCS1v15 || sigHash == crypto.SHA1) {
            c.sendAlert(alertIllegalParameter);
            return error.As(errors.New("tls: client certificate used with invalid signature algorithm"))!;
        }
        var signed = signedMessage(sigHash, clientSignatureContext, hs.transcript);
        {
            var err__prev2 = err;

            err = verifyHandshakeSignature(sigType, c.peerCertificates[0].PublicKey, sigHash, signed, certVerify.signature);

            if (err != null) {
                c.sendAlert(alertDecryptError);
                return error.As(errors.New("tls: invalid signature by the client certificate: " + err.Error()))!;
            }

            err = err__prev2;

        }


        hs.transcript.Write(certVerify.marshal());

    }
    {
        var err__prev1 = err;

        err = hs.sendSessionTickets();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }


    return error.As(null!)!;

}

private static error readClientFinished(this ptr<serverHandshakeStateTLS13> _addr_hs) {
    ref serverHandshakeStateTLS13 hs = ref _addr_hs.val;

    var c = hs.c;

    var (msg, err) = c.readHandshake();
    if (err != null) {
        return error.As(err)!;
    }
    ptr<finishedMsg> (finished, ok) = msg._<ptr<finishedMsg>>();
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return error.As(unexpectedMessageError(finished, msg))!;
    }
    if (!hmac.Equal(hs.clientFinished, finished.verifyData)) {
        c.sendAlert(alertDecryptError);
        return error.As(errors.New("tls: invalid client finished hash"))!;
    }
    c.@in.setTrafficSecret(hs.suite, hs.trafficSecret);

    return error.As(null!)!;

}

} // end tls_package
