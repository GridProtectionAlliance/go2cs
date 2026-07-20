// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using context = context_package;
using crypto = crypto_package;
using hmac = go.crypto.hmac_package;
using mlkem768 = go.crypto.@internal.mlkem768_package;
using rsa = go.crypto.rsa_package;
using errors = errors_package;
using hash = hash_package;
using byteorder = go.@internal.byteorder_package;
using io = io_package;
using slices = slices_package;
using time = time_package;
using ecdh = go.crypto.ecdh_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.sync;
using math;
using x509 = go.crypto.x509_package;

partial class tls_package {

// maxClientPSKIdentities is the number of client PSK identities the server will
// attempt to validate. It will ignore the rest not to let cheap ClientHello
// messages cause too much work in session ticket decryption attempts.
internal static readonly UntypedInt maxClientPSKIdentities = 5;

[GoType] partial struct serverHandshakeStateTLS13 {
    internal ж<Conn> c;
    internal context.Context ctx;
    internal ж<clientHelloMsg> clientHello;
    internal ж<serverHelloMsg> hello;
    internal bool sentDummyCCS;
    internal bool usingPSK;
    internal bool earlyData;
    internal ж<cipherSuiteTLS13> suite;
    internal ж<Certificate> cert;
    internal SignatureScheme sigAlg;
    internal slice<byte> earlySecret;
    internal slice<byte> sharedKey;
    internal slice<byte> handshakeSecret;
    internal slice<byte> masterSecret;
    internal slice<byte> trafficSecret; // client_application_traffic_secret_0
    internal hash.Hash transcript;
    internal slice<byte> clientFinished;
}

[GoRecv] internal static error handshake(this ref serverHandshakeStateTLS13 hs) {
    var c = hs.c;
    if (needFIPS()) {
        return errors.New("tls: internal error: TLS 1.3 reached in FIPS mode"u8);
    }
    // For an overview of the TLS 1.3 handshake, see RFC 8446, Section 2.
    {
        var err = hs.processClientHello(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.checkForResumption(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.pickCertificate(); if (err != default!) {
            return err;
        }
    }
    c.Value.buffering = true;
    {
        var err = hs.sendServerParameters(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.sendServerCertificate(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.sendServerFinished(); if (err != default!) {
            return err;
        }
    }
    // Note that at this point we could start sending application data without
    // waiting for the client's second flight, but the application might not
    // expect the lack of replay protection of the ClientHello parameters.
    {
        var (_, err) = c.flush(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.readClientCertificate(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.readClientFinished(); if (err != default!) {
            return err;
        }
    }
    c.of(Conn.ᏑisHandshakeComplete).Store(true);
    return default!;
}

[GoRecv] internal static error processClientHello(this ref serverHandshakeStateTLS13 hs) {
    var c = hs.c;
    hs.hello = @new<serverHelloMsg>();
    // TLS 1.3 froze the ServerHello.legacy_version field, and uses
    // supported_versions instead. See RFC 8446, sections 4.1.3 and 4.2.1.
    hs.hello.Value.vers = VersionTLS12;
    hs.hello.Value.supportedVersion = c.Value.vers;
    if (len((~hs.clientHello).supportedVersions) == 0) {
        c.sendAlert(alertIllegalParameter);
        return errors.New("tls: client used the legacy version field to negotiate TLS 1.3"u8);
    }
    // Abort if the client is doing a fallback and landing lower than what we
    // support. See RFC 7507, which however does not specify the interaction
    // with supported_versions. The only difference is that with
    // supported_versions a client has a chance to attempt a [TLS 1.2, TLS 1.4]
    // handshake in case TLS 1.3 is broken but 1.2 is not. Alas, in that case,
    // it will have to drop the TLS_FALLBACK_SCSV protection if it falls back to
    // TLS 1.2, because a TLS 1.3 server would abort here. The situation before
    // supported_versions was not better because there was just no way to do a
    // TLS 1.4 handshake without risking the server selecting TLS 1.3.
    foreach (var (_, id) in (~hs.clientHello).cipherSuites) {
        if (id == TLS_FALLBACK_SCSV) {
            // Use c.vers instead of max(supported_versions) because an attacker
            // could defeat this by adding an arbitrary high version otherwise.
            if ((~c).vers < (~c).config.maxSupportedVersion(roleServer)) {
                c.sendAlert(alertInappropriateFallback);
                return errors.New("tls: client using inappropriate protocol fallback"u8);
            }
            break;
        }
    }
    if (len((~hs.clientHello).compressionMethods) != 1 || (~hs.clientHello).compressionMethods[0] != compressionNone) {
        c.sendAlert(alertIllegalParameter);
        return errors.New("tls: TLS 1.3 client supports illegal compression methods"u8);
    }
    hs.hello.Value.random = new slice<byte>(32);
    {
        var (_, errΔ1) = io.ReadFull((~c).config.rand(), (~hs.hello).random); if (errΔ1 != default!) {
            c.sendAlert(alertInternalError);
            return errΔ1;
        }
    }
    if (len((~hs.clientHello).secureRenegotiation) != 0) {
        c.sendAlert(alertHandshakeFailure);
        return errors.New("tls: initial handshake had non-empty renegotiation extension"u8);
    }
    if ((~hs.clientHello).earlyData && (~c).quic != nil){
        if (len((~hs.clientHello).pskIdentities) == 0) {
            c.sendAlert(alertIllegalParameter);
            return errors.New("tls: early_data without pre_shared_key"u8);
        }
    } else 
    if ((~hs.clientHello).earlyData) {
        // See RFC 8446, Section 4.2.10 for the complicated behavior required
        // here. The scenario is that a different server at our address offered
        // to accept early data in the past, which we can't handle. For now, all
        // 0-RTT enabled session tickets need to expire before a Go server can
        // replace a server or join a pool. That's the same requirement that
        // applies to mixing or replacing with any TLS 1.2 server.
        c.sendAlert(alertUnsupportedExtension);
        return errors.New("tls: client sent unexpected early data"u8);
    }
    hs.hello.Value.sessionId = hs.clientHello.Value.sessionId;
    hs.hello.Value.compressionMethod = compressionNone;
    var preferenceList = defaultCipherSuitesTLS13;
    if (!hasAESGCMHardwareSupport || !aesgcmPreferred((~hs.clientHello).cipherSuites)) {
        preferenceList = defaultCipherSuitesTLS13NoAES;
    }
    foreach (var (_, suiteID) in preferenceList) {
        hs.suite = mutualCipherSuiteTLS13((~hs.clientHello).cipherSuites, suiteID);
        if (hs.suite != nil) {
            break;
        }
    }
    if (hs.suite == nil) {
        c.sendAlert(alertHandshakeFailure);
        return errors.New("tls: no cipher suite supported by both client and server"u8);
    }
    c.Value.cipherSuite = hs.suite.Value.id;
    hs.hello.Value.cipherSuite = hs.suite.Value.id;
    hs.transcript = (~hs.suite).hash.New();
    // Pick the key exchange method in server preference order, but give
    // priority to key shares, to avoid a HelloRetryRequest round-trip.
    CurveID selectedGroup = default!;
    ж<keyShare> clientKeyShare = default!;
    var preferredGroups = (~c).config.curvePreferences((~c).vers);
    foreach (var (_, preferredGroup) in preferredGroups) {
        nint ki = slices.IndexFunc((~hs.clientHello).keyShares, (keyShare ks) => ks.group == preferredGroup);
        if (ki != -1) {
            clientKeyShare = Ꮡ((~hs.clientHello).keyShares[ki]);
            selectedGroup = clientKeyShare.Value.group;
            if (!slices.Contains((~hs.clientHello).supportedCurves, selectedGroup)) {
                c.sendAlert(alertIllegalParameter);
                return errors.New("tls: client sent key share for group it does not support"u8);
            }
            break;
        }
    }
    if (selectedGroup == 0) {
        foreach (var (_, preferredGroup) in preferredGroups) {
            if (slices.Contains((~hs.clientHello).supportedCurves, preferredGroup)) {
                selectedGroup = preferredGroup;
                break;
            }
        }
    }
    if (selectedGroup == 0) {
        c.sendAlert(alertHandshakeFailure);
        return errors.New("tls: no ECDHE curve supported by both client and server"u8);
    }
    if (clientKeyShare == nil) {
        var (ks, errΔ2) = hs.doHelloRetryRequest(selectedGroup);
        if (errΔ2 != default!) {
            return errΔ2;
        }
        clientKeyShare = ks;
    }
    c.Value.curveID = selectedGroup;
    var ecdhGroup = selectedGroup;
    var ecdhData = clientKeyShare.Value.data;
    if (selectedGroup == x25519Kyber768Draft00) {
        ecdhGroup = X25519;
        if (len(ecdhData) != x25519PublicKeySize + mlkem768.EncapsulationKeySize) {
            c.sendAlert(alertIllegalParameter);
            return errors.New("tls: invalid Kyber client key share"u8);
        }
        ecdhData = ecdhData[..(int)(x25519PublicKeySize)];
    }
    {
        var (_, ok) = curveForCurveID(ecdhGroup); if (!ok) {
            c.sendAlert(alertInternalError);
            return errors.New("tls: CurvePreferences includes unsupported curve"u8);
        }
    }
    var (key, err) = generateECDHEKey((~c).config.rand(), ecdhGroup);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return err;
    }
    hs.hello.Value.serverShare = new keyShare(group: selectedGroup, data: key.PublicKey().Bytes());
    (var peerKey, err) = key.Curve().NewPublicKey(ecdhData);
    if (err != default!) {
        c.sendAlert(alertIllegalParameter);
        return errors.New("tls: invalid client key share"u8);
    }
    (hs.sharedKey, err) = key.ECDH(peerKey);
    if (err != default!) {
        c.sendAlert(alertIllegalParameter);
        return errors.New("tls: invalid client key share"u8);
    }
    if (selectedGroup == x25519Kyber768Draft00) {
        var (ciphertext, kyberShared, errΔ3) = kyberEncapsulate((~clientKeyShare).data[(int)(x25519PublicKeySize)..]);
        if (errΔ3 != default!) {
            c.sendAlert(alertIllegalParameter);
            return errors.New("tls: invalid Kyber client key share"u8);
        }
        hs.sharedKey = append(hs.sharedKey, kyberShared.ꓸꓸꓸ);
        hs.hello.Value.serverShare.data = append((~hs.hello).serverShare.data, ciphertext.ꓸꓸꓸ);
    }
    (var selectedProto, err) = negotiateALPN((~(~c).config).NextProtos, (~hs.clientHello).alpnProtocols, (~c).quic != nil);
    if (err != default!) {
        c.sendAlert(alertNoApplicationProtocol);
        return err;
    }
    c.Value.clientProtocol = selectedProto;
    if ((~c).quic != nil){
        // RFC 9001 Section 4.2: Clients MUST NOT offer TLS versions older than 1.3.
        foreach (var (_, v) in (~hs.clientHello).supportedVersions) {
            if (v < VersionTLS13) {
                c.sendAlert(alertProtocolVersion);
                return errors.New("tls: client offered TLS version older than TLS 1.3"u8);
            }
        }
        // RFC 9001 Section 8.2.
        if ((~hs.clientHello).quicTransportParameters == default!) {
            c.sendAlert(alertMissingExtension);
            return errors.New("tls: client did not send a quic_transport_parameters extension"u8);
        }
        c.quicSetTransportParameters((~hs.clientHello).quicTransportParameters);
    } else {
        if ((~hs.clientHello).quicTransportParameters != default!) {
            c.sendAlert(alertUnsupportedExtension);
            return errors.New("tls: client sent an unexpected quic_transport_parameters extension"u8);
        }
    }
    c.Value.serverName = hs.clientHello.Value.serverName;
    return default!;
}

[GoRecv] internal static error checkForResumption(this ref serverHandshakeStateTLS13 hs) {
    var c = hs.c;
    if ((~(~c).config).SessionTicketsDisabled) {
        return default!;
    }
    var modeOK = false;
    foreach (var (_, mode) in (~hs.clientHello).pskModes) {
        if (mode == pskModeDHE) {
            modeOK = true;
            break;
        }
    }
    if (!modeOK) {
        return default!;
    }
    if (len((~hs.clientHello).pskIdentities) != len((~hs.clientHello).pskBinders)) {
        c.sendAlert(alertIllegalParameter);
        return errors.New("tls: invalid or missing PSK binders"u8);
    }
    if (len((~hs.clientHello).pskIdentities) == 0) {
        return default!;
    }
    foreach (var (i, identity) in (~hs.clientHello).pskIdentities) {
        if (i >= maxClientPSKIdentities) {
            break;
        }
        ж<SessionState> sessionState = default!;
        if ((~(~c).config).UnwrapSession != default!){
            error errΔ1 = default!;
            (sessionState, errΔ1) = (~(~c).config).UnwrapSession(identity.label, c.connectionStateLocked());
            if (errΔ1 != default!) {
                return errΔ1;
            }
            if (sessionState == nil) {
                continue;
            }
        } else {
            var plaintext = (~c).config.decryptTicket(identity.label, (~c).ticketKeys);
            if (plaintext == default!) {
                continue;
            }
            error errΔ2 = default!;
            (sessionState, errΔ2) = ParseSessionState(plaintext);
            if (errΔ2 != default!) {
                continue;
            }
        }
        if ((~sessionState).version != VersionTLS13) {
            continue;
        }
        var createdAt = time_package.Unix((int64)(~sessionState).createdAt, 0);
        if ((~c).config.time().Sub(createdAt) > maxSessionTicketLifetime) {
            continue;
        }
        var pskSuite = cipherSuiteTLS13ByID((~sessionState).cipherSuite);
        if (pskSuite == nil || (~pskSuite).hash != (~hs.suite).hash) {
            continue;
        }
        // PSK connections don't re-establish client certificates, but carry
        // them over in the session ticket. Ensure the presence of client certs
        // in the ticket is consistent with the configured requirements.
        var sessionHasClientCerts = len((~sessionState).peerCertificates) != 0;
        var needClientCerts = requiresClientCert((~(~c).config).ClientAuth);
        if (needClientCerts && !sessionHasClientCerts) {
            continue;
        }
        if (sessionHasClientCerts && (~(~c).config).ClientAuth == NoClientCert) {
            continue;
        }
        if (sessionHasClientCerts && (~c).config.time().After((~(~sessionState).peerCertificates[0]).NotAfter)) {
            continue;
        }
        if (sessionHasClientCerts && (~(~c).config).ClientAuth >= VerifyClientCertIfGiven && len((~sessionState).verifiedChains) == 0) {
            continue;
        }
        if ((~c).quic != nil && (~(~c).quic).enableSessionEvents) {
            {
                var errΔ3 = c.quicResumeSession(sessionState); if (errΔ3 != default!) {
                    return errΔ3;
                }
            }
        }
        hs.earlySecret = hs.suite.extract((~sessionState).secret, default!);
        var binderKey = hs.suite.deriveSecret(hs.earlySecret, resumptionBinderLabel, default!);
        // Clone the transcript in case a HelloRetryRequest was recorded.
        var transcript = cloneHash(hs.transcript, (~hs.suite).hash);
        if (transcript == default!) {
            c.sendAlert(alertInternalError);
            return errors.New("tls: internal error: failed to clone hash"u8);
        }
        var (clientHelloBytes, err) = hs.clientHello.marshalWithoutBinders();
        if (err != default!) {
            c.sendAlert(alertInternalError);
            return err;
        }
        transcript.Write(clientHelloBytes);
        var pskBinder = hs.suite.finishedHash(binderKey, transcript);
        if (!hmac.Equal((~hs.clientHello).pskBinders[i], pskBinder)) {
            c.sendAlert(alertDecryptError);
            return errors.New("tls: invalid PSK binder"u8);
        }
        if ((~c).quic != nil && (~hs.clientHello).earlyData && i == 0 && (~sessionState).EarlyData && (~sessionState).cipherSuite == (~hs.suite).id && (~sessionState).alpnProtocol == (~c).clientProtocol) {
            hs.earlyData = true;
            var transcriptΔ1 = (~hs.suite).hash.New();
            {
                var errΔ1 = transcriptMsg(new clientHelloMsgжhandshakeMessage(hs.clientHello), new hash_HashᴠtranscriptHash(transcriptΔ1)); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
            var earlyTrafficSecret = hs.suite.deriveSecret(hs.earlySecret, clientEarlyTrafficLabel, transcriptΔ1);
            c.quicSetReadSecret(QUICEncryptionLevelEarly, (~hs.suite).id, earlyTrafficSecret);
        }
        c.Value.didResume = true;
        c.Value.peerCertificates = sessionState.Value.peerCertificates;
        c.Value.ocspResponse = sessionState.Value.ocspResponse;
        c.Value.scts = sessionState.Value.scts;
        c.Value.verifiedChains = sessionState.Value.verifiedChains;
        hs.hello.Value.selectedIdentityPresent = true;
        hs.hello.Value.selectedIdentity = (uint16)i;
        hs.usingPSK = true;
        return default!;
    }
    return default!;
}

// Recreate the interface to avoid importing encoding.
[GoType("dyn")] partial interface cloneHash_binaryMarshaler {
    (slice<byte> data, error err) MarshalBinary();
    error UnmarshalBinary(slice<byte> data);
}

// cloneHash uses the encoding.BinaryMarshaler and encoding.BinaryUnmarshaler
// interfaces implemented by standard library hashes to clone the state of in
// to a new instance of h. It returns nil if the operation fails.
internal static hash.Hash cloneHash(hash.Hash @in, crypto.Hash h) {
    var (marshaler, ok) = @in._<cloneHash_binaryMarshaler>(ᐧ);
    if (!ok) {
        return default!;
    }
    var (state, err) = marshaler.MarshalBinary();
    if (err != default!) {
        return default!;
    }
    var @out = h.New();
    (var unmarshaler, ok) = @out._<cloneHash_binaryMarshaler>(ᐧ);
    if (!ok) {
        return default!;
    }
    {
        var errΔ1 = unmarshaler.UnmarshalBinary(state); if (errΔ1 != default!) {
            return default!;
        }
    }
    return @out;
}

[GoRecv] internal static error pickCertificate(this ref serverHandshakeStateTLS13 hs) {
    var c = hs.c;
    // Only one of PSK and certificates are used at a time.
    if (hs.usingPSK) {
        return default!;
    }
    // signature_algorithms is required in TLS 1.3. See RFC 8446, Section 4.2.3.
    if (len((~hs.clientHello).supportedSignatureAlgorithms) == 0) {
        return c.sendAlert(alertMissingExtension);
    }
    var (certificate, err) = (~c).config.getCertificate(clientHelloInfo(hs.ctx, c, hs.clientHello));
    if (err != default!) {
        if (AreEqual(err, errNoCertificates)){
            c.sendAlert(alertUnrecognizedName);
        } else {
            c.sendAlert(alertInternalError);
        }
        return err;
    }
    (hs.sigAlg, err) = selectSignatureScheme((~c).vers, certificate, (~hs.clientHello).supportedSignatureAlgorithms);
    if (err != default!) {
        // getCertificate returned a certificate that is unsupported or
        // incompatible with the client's signature algorithms.
        c.sendAlert(alertHandshakeFailure);
        return err;
    }
    hs.cert = certificate;
    return default!;
}

// sendDummyChangeCipherSpec sends a ChangeCipherSpec record for compatibility
// with middleboxes that didn't implement TLS correctly. See RFC 8446, Appendix D.4.
[GoRecv] internal static error sendDummyChangeCipherSpec(this ref serverHandshakeStateTLS13 hs) {
    if ((~hs.c).quic != nil) {
        return default!;
    }
    if (hs.sentDummyCCS) {
        return default!;
    }
    hs.sentDummyCCS = true;
    return hs.c.writeChangeCipherRecord();
}

[GoRecv] internal static (ж<keyShare>, error) doHelloRetryRequest(this ref serverHandshakeStateTLS13 hs, CurveID selectedGroup) {
    var c = hs.c;
    // The first ClientHello gets double-hashed into the transcript upon a
    // HelloRetryRequest. See RFC 8446, Section 4.4.1.
    {
        var errΔ1 = transcriptMsg(new clientHelloMsgжhandshakeMessage(hs.clientHello), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    var chHash = hs.transcript.Sum(default!);
    hs.transcript.Reset();
    hs.transcript.Write(new byte[]{typeMessageHash, 0, 0, (uint8)len(chHash)}.slice());
    hs.transcript.Write(chHash);
    var helloRetryRequest = Ꮡ(new serverHelloMsg(
        vers: (~hs.hello).vers,
        random: helloRetryRequestRandom,
        sessionId: (~hs.hello).sessionId,
        cipherSuite: (~hs.hello).cipherSuite,
        compressionMethod: (~hs.hello).compressionMethod,
        supportedVersion: (~hs.hello).supportedVersion,
        selectedGroup: selectedGroup
    ));
    {
        var (_, errΔ2) = hs.c.writeHandshakeRecord(new serverHelloMsgжhandshakeMessage(helloRetryRequest), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
    }
    {
        var errΔ3 = hs.sendDummyChangeCipherSpec(); if (errΔ3 != default!) {
            return (default!, errΔ3);
        }
    }
    // clientHelloMsg is not included in the transcript.
    var (msg, err) = c.readHandshake(default!);
    if (err != default!) {
        return (default!, err);
    }
    var (clientHello, ok) = msg._<ж<clientHelloMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return (default!, unexpectedMessageError(clientHello, msg));
    }
    if (len((~clientHello).keyShares) != 1) {
        c.sendAlert(alertIllegalParameter);
        return (default!, errors.New("tls: client didn't send one key share in second ClientHello"u8));
    }
    var ks = Ꮡ((~clientHello).keyShares, 0);
    if ((~ks).group != selectedGroup) {
        c.sendAlert(alertIllegalParameter);
        return (default!, errors.New("tls: client sent unexpected key share in second ClientHello"u8));
    }
    if ((~clientHello).earlyData) {
        c.sendAlert(alertIllegalParameter);
        return (default!, errors.New("tls: client indicated early data in second ClientHello"u8));
    }
    if (illegalClientHelloChange(clientHello, hs.clientHello)) {
        c.sendAlert(alertIllegalParameter);
        return (default!, errors.New("tls: client illegally modified second ClientHello"u8));
    }
    c.Value.didHRR = true;
    hs.clientHello = clientHello;
    return (ks, default!);
}

// illegalClientHelloChange reports whether the two ClientHello messages are
// different, with the exception of the changes allowed before and after a
// HelloRetryRequest. See RFC 8446, Section 4.1.2.
internal static bool illegalClientHelloChange(ж<clientHelloMsg> Ꮡch, ж<clientHelloMsg> Ꮡch1) {
    ref var ch = ref Ꮡch.Value;
    ref var ch1 = ref Ꮡch1.Value;

    if (len(ch.supportedVersions) != len(ch1.supportedVersions) || len(ch.cipherSuites) != len(ch1.cipherSuites) || len(ch.supportedCurves) != len(ch1.supportedCurves) || len(ch.supportedSignatureAlgorithms) != len(ch1.supportedSignatureAlgorithms) || len(ch.supportedSignatureAlgorithmsCert) != len(ch1.supportedSignatureAlgorithmsCert) || len(ch.alpnProtocols) != len(ch1.alpnProtocols)) {
        return true;
    }
    foreach (var (i, _) in ch.supportedVersions) {
        if (ch.supportedVersions[i] != ch1.supportedVersions[i]) {
            return true;
        }
    }
    foreach (var (i, _) in ch.cipherSuites) {
        if (ch.cipherSuites[i] != ch1.cipherSuites[i]) {
            return true;
        }
    }
    foreach (var (i, _) in ch.supportedCurves) {
        if (ch.supportedCurves[i] != ch1.supportedCurves[i]) {
            return true;
        }
    }
    foreach (var (i, _) in ch.supportedSignatureAlgorithms) {
        if (ch.supportedSignatureAlgorithms[i] != ch1.supportedSignatureAlgorithms[i]) {
            return true;
        }
    }
    foreach (var (i, _) in ch.supportedSignatureAlgorithmsCert) {
        if (ch.supportedSignatureAlgorithmsCert[i] != ch1.supportedSignatureAlgorithmsCert[i]) {
            return true;
        }
    }
    foreach (var (i, _) in ch.alpnProtocols) {
        if (ch.alpnProtocols[i] != ch1.alpnProtocols[i]) {
            return true;
        }
    }
    return ch.vers != ch1.vers || !bytes.Equal(ch.random, ch1.random) || !bytes.Equal(ch.sessionId, ch1.sessionId) || !bytes.Equal(ch.compressionMethods, ch1.compressionMethods) || ch.serverName != ch1.serverName || ch.ocspStapling != ch1.ocspStapling || !bytes.Equal(ch.supportedPoints, ch1.supportedPoints) || ch.ticketSupported != ch1.ticketSupported || !bytes.Equal(ch.sessionTicket, ch1.sessionTicket) || ch.secureRenegotiationSupported != ch1.secureRenegotiationSupported || !bytes.Equal(ch.secureRenegotiation, ch1.secureRenegotiation) || ch.scts != ch1.scts || !bytes.Equal(ch.cookie, ch1.cookie) || !bytes.Equal(ch.pskModes, ch1.pskModes);
}

[GoRecv] internal static error sendServerParameters(this ref serverHandshakeStateTLS13 hs) {
    var c = hs.c;
    {
        var errΔ1 = transcriptMsg(new clientHelloMsgжhandshakeMessage(hs.clientHello), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var (_, errΔ2) = hs.c.writeHandshakeRecord(new serverHelloMsgжhandshakeMessage(hs.hello), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    {
        var errΔ3 = hs.sendDummyChangeCipherSpec(); if (errΔ3 != default!) {
            return errΔ3;
        }
    }
    var earlySecret = hs.earlySecret;
    if (earlySecret == default!) {
        earlySecret = hs.suite.extract(default!, default!);
    }
    hs.handshakeSecret = hs.suite.extract(hs.sharedKey,
        hs.suite.deriveSecret(earlySecret, "derived"u8, default!));
    var clientSecret = hs.suite.deriveSecret(hs.handshakeSecret,
        clientHandshakeTrafficLabel, hs.transcript);
    c.of(Conn.Ꮡin).setTrafficSecret(hs.suite, QUICEncryptionLevelHandshake, clientSecret);
    var serverSecret = hs.suite.deriveSecret(hs.handshakeSecret,
        serverHandshakeTrafficLabel, hs.transcript);
    c.of(Conn.Ꮡout).setTrafficSecret(hs.suite, QUICEncryptionLevelHandshake, serverSecret);
    if ((~c).quic != nil) {
        if (c.of(Conn.Ꮡhand).Len() != 0) {
            c.sendAlert(alertUnexpectedMessage);
        }
        c.quicSetWriteSecret(QUICEncryptionLevelHandshake, (~hs.suite).id, serverSecret);
        c.quicSetReadSecret(QUICEncryptionLevelHandshake, (~hs.suite).id, clientSecret);
    }
    var err = (~c).config.writeKeyLog(keyLogLabelClientHandshake, (~hs.clientHello).random, clientSecret);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return err;
    }
    err = (~c).config.writeKeyLog(keyLogLabelServerHandshake, (~hs.clientHello).random, serverSecret);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return err;
    }
    var encryptedExtensions = @new<encryptedExtensionsMsg>();
    encryptedExtensions.Value.alpnProtocol = c.Value.clientProtocol;
    if ((~c).quic != nil) {
        var (p, errΔ4) = c.quicGetTransportParameters();
        if (errΔ4 != default!) {
            return errΔ4;
        }
        encryptedExtensions.Value.quicTransportParameters = p;
        encryptedExtensions.Value.earlyData = hs.earlyData;
    }
    {
        var (_, errΔ5) = hs.c.writeHandshakeRecord(new encryptedExtensionsMsgжhandshakeMessage(encryptedExtensions), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ5 != default!) {
            return errΔ5;
        }
    }
    return default!;
}

[GoRecv] internal static bool requestClientCert(this ref serverHandshakeStateTLS13 hs) {
    return (~(~hs.c).config).ClientAuth >= RequestClientCert && !hs.usingPSK;
}

[GoRecv] internal static error sendServerCertificate(this ref serverHandshakeStateTLS13 hs) {
    var c = hs.c;
    // Only one of PSK and certificates are used at a time.
    if (hs.usingPSK) {
        return default!;
    }
    if (hs.requestClientCert()) {
        // Request a client certificate
        var certReq = @new<certificateRequestMsgTLS13>();
        certReq.Value.ocspStapling = true;
        certReq.Value.scts = true;
        certReq.Value.supportedSignatureAlgorithms = supportedSignatureAlgorithms();
        if ((~(~c).config).ClientCAs != nil) {
            certReq.Value.certificateAuthorities = (~(~c).config).ClientCAs.Subjects();
        }
        {
            var (_, errΔ1) = hs.c.writeHandshakeRecord(new certificateRequestMsgTLS13жhandshakeMessage(certReq), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
    }
    var certMsg = @new<certificateMsgTLS13>();
    certMsg.Value.certificate = hs.cert.Value;
    certMsg.Value.scts = (~hs.clientHello).scts && len((~hs.cert).SignedCertificateTimestamps) > 0;
    certMsg.Value.ocspStapling = (~hs.clientHello).ocspStapling && len((~hs.cert).OCSPStaple) > 0;
    {
        var (_, errΔ2) = hs.c.writeHandshakeRecord(new certificateMsgTLS13жhandshakeMessage(certMsg), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    var certVerifyMsg = @new<certificateVerifyMsg>();
    certVerifyMsg.Value.hasSignatureAlgorithm = true;
    certVerifyMsg.Value.signatureAlgorithm = hs.sigAlg;
    ref var sigHash = ref heap<crypto.Hash>(out var ᏑsigHash);
    (var sigType, sigHash, var err) = typeAndHashFromSignatureScheme(hs.sigAlg);
    if (err != default!) {
        return c.sendAlert(alertInternalError);
    }
    var signed = signedMessage(sigHash, serverSignatureContext, hs.transcript);
    var signOpts = ((crypto.SignerOpts)sigHash);
    if (sigType == signatureRSAPSS) {
        signOpts = new rsa_PSSOptionsжSignerOpts(Ꮡ(new rsa.PSSOptions(SaltLength: rsa.PSSSaltLengthEqualsHash, Hash: sigHash)));
    }
    (var sig, err) = (~hs.cert).PrivateKey._<crypto.Signer>().Sign((~c).config.rand(), signed, signOpts);
    if (err != default!) {
        var @public = (~hs.cert).PrivateKey._<crypto.Signer>().Public();
        {
            var (rsaKey, ok) = @public._<ж<rsa.PublicKey>>(ᐧ); if (ok && sigType == signatureRSAPSS && (~rsaKey).N.BitLen() / 8 < sigHash.Size() * 2 + 2){
                // key too small for RSA-PSS
                c.sendAlert(alertHandshakeFailure);
            } else {
                c.sendAlert(alertInternalError);
            }
        }
        return errors.New("tls: failed to sign handshake: "u8 + err.Error());
    }
    certVerifyMsg.Value.signature = sig;
    {
        var (_, errΔ3) = hs.c.writeHandshakeRecord(new certificateVerifyMsgжhandshakeMessage(certVerifyMsg), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ3 != default!) {
            return errΔ3;
        }
    }
    return default!;
}

[GoRecv] internal static error sendServerFinished(this ref serverHandshakeStateTLS13 hs) {
    var c = hs.c;
    var finished = Ꮡ(new finishedMsg(
        verifyData: hs.suite.finishedHash((~c).@out.trafficSecret, hs.transcript)
    ));
    {
        var (_, errΔ1) = hs.c.writeHandshakeRecord(new finishedMsgжhandshakeMessage(finished), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    // Derive secrets that take context through the server Finished.
    hs.masterSecret = hs.suite.extract(default!,
        hs.suite.deriveSecret(hs.handshakeSecret, "derived"u8, default!));
    hs.trafficSecret = hs.suite.deriveSecret(hs.masterSecret,
        clientApplicationTrafficLabel, hs.transcript);
    var serverSecret = hs.suite.deriveSecret(hs.masterSecret,
        serverApplicationTrafficLabel, hs.transcript);
    c.of(Conn.Ꮡout).setTrafficSecret(hs.suite, QUICEncryptionLevelApplication, serverSecret);
    if ((~c).quic != nil) {
        if (c.of(Conn.Ꮡhand).Len() != 0) {
            // TODO: Handle this in setTrafficSecret?
            c.sendAlert(alertUnexpectedMessage);
        }
        c.quicSetWriteSecret(QUICEncryptionLevelApplication, (~hs.suite).id, serverSecret);
    }
    var err = (~c).config.writeKeyLog(keyLogLabelClientTraffic, (~hs.clientHello).random, hs.trafficSecret);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return err;
    }
    err = (~c).config.writeKeyLog(keyLogLabelServerTraffic, (~hs.clientHello).random, serverSecret);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return err;
    }
    c.Value.ekm = hs.suite.exportKeyingMaterial(hs.masterSecret, hs.transcript);
    // If we did not request client certificates, at this point we can
    // precompute the client finished and roll the transcript forward to send
    // session tickets in our first flight.
    if (!hs.requestClientCert()) {
        {
            var errΔ2 = hs.sendSessionTickets(); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    return default!;
}

[GoRecv] internal static bool shouldSendSessionTickets(this ref serverHandshakeStateTLS13 hs) {
    if ((~(~hs.c).config).SessionTicketsDisabled) {
        return false;
    }
    // QUIC tickets are sent by QUICConn.SendSessionTicket, not automatically.
    if ((~hs.c).quic != nil) {
        return false;
    }
    // Don't send tickets the client wouldn't use. See RFC 8446, Section 4.2.9.
    foreach (var (_, pskMode) in (~hs.clientHello).pskModes) {
        if (pskMode == pskModeDHE) {
            return true;
        }
    }
    return false;
}

[GoRecv] internal static error sendSessionTickets(this ref serverHandshakeStateTLS13 hs) {
    var c = hs.c;
    hs.clientFinished = hs.suite.finishedHash((~c).@in.trafficSecret, hs.transcript);
    var finishedMsg = Ꮡ(new finishedMsg(
        verifyData: hs.clientFinished
    ));
    {
        var err = transcriptMsg(new finishedMsgжhandshakeMessage(finishedMsg), new hash_HashᴠtranscriptHash(hs.transcript)); if (err != default!) {
            return err;
        }
    }
    c.Value.resumptionSecret = hs.suite.deriveSecret(hs.masterSecret,
        resumptionLabel, hs.transcript);
    if (!hs.shouldSendSessionTickets()) {
        return default!;
    }
    return c.sendSessionTicket(false, default!);
}

internal static error sendSessionTicket(this ж<Conn> Ꮡc, bool earlyData, slice<slice<byte>> extra) {
    ref var c = ref Ꮡc.Value;

    var suite = cipherSuiteTLS13ByID(c.cipherSuite);
    if (suite == nil) {
        return errors.New("tls: internal error: unknown cipher suite"u8);
    }
    // ticket_nonce, which must be unique per connection, is always left at
    // zero because we only ever send one ticket per connection.
    var psk = suite.expandLabel(c.resumptionSecret, "resumption"u8,
        default!, (~suite).hash.Size());
    var m = @new<newSessionTicketMsgTLS13>();
    var state = c.sessionState();
    state.Value.secret = psk;
    state.Value.EarlyData = earlyData;
    state.Value.Extra = extra;
    if ((~c.config).WrapSession != default!){
        error err = default!;
        (m.Value.label, err) = (~c.config).WrapSession(Ꮡc.connectionStateLocked(), state);
        if (err != default!) {
            return err;
        }
    } else {
        var (stateBytes, err) = state.Bytes();
        if (err != default!) {
            Ꮡc.sendAlert(alertInternalError);
            return err;
        }
        (m.Value.label, err) = c.config.encryptTicket(stateBytes, c.ticketKeys);
        if (err != default!) {
            return err;
        }
    }
    m.Value.lifetime = (uint32)(int64)(maxSessionTicketLifetime / time_package.ΔSecond);
    // ticket_age_add is a random 32-bit value. See RFC 8446, section 4.6.1
    // The value is not stored anywhere; we never need to check the ticket age
    // because 0-RTT is not supported.
    var ageAdd = new slice<byte>(4);
    {
        var (_, err) = c.config.rand().Read(ageAdd); if (err != default!) {
            return err;
        }
    }
    m.Value.ageAdd = byteorder.LeUint32(ageAdd);
    if (earlyData) {
        // RFC 9001, Section 4.6.1
        m.Value.maxEarlyData = 0xffffffffU;
    }
    {
        var (_, err) = Ꮡc.writeHandshakeRecord(new newSessionTicketMsgTLS13жhandshakeMessage(m), default!); if (err != default!) {
            return err;
        }
    }
    return default!;
}

[GoRecv] internal static error readClientCertificate(this ref serverHandshakeStateTLS13 hs) {
    var c = hs.c;
    if (!hs.requestClientCert()) {
        // Make sure the connection is still being verified whether or not
        // the server requested a client certificate.
        if ((~(~c).config).VerifyConnection != default!) {
            {
                var errΔ1 = (~(~c).config).VerifyConnection(c.connectionStateLocked()); if (errΔ1 != default!) {
                    c.sendAlert(alertBadCertificate);
                    return errΔ1;
                }
            }
        }
        return default!;
    }
    // If we requested a client certificate, then the client must send a
    // certificate message. If it's empty, no CertificateVerify is sent.
    var (msg, err) = c.readHandshake(new hash_HashᴠtranscriptHash(hs.transcript));
    if (err != default!) {
        return err;
    }
    var (certMsg, ok) = msg._<ж<certificateMsgTLS13>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(certMsg, msg);
    }
    {
        var errΔ2 = c.processCertsFromClient((~certMsg).certificate); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    if ((~(~c).config).VerifyConnection != default!) {
        {
            var errΔ3 = (~(~c).config).VerifyConnection(c.connectionStateLocked()); if (errΔ3 != default!) {
                c.sendAlert(alertBadCertificate);
                return errΔ3;
            }
        }
    }
    if (len((~certMsg).certificate.ΔCertificate) != 0) {
        // certificateVerifyMsg is included in the transcript, but not until
        // after we verify the handshake signature, since the state before
        // this message was sent is used.
        (msg, err) = c.readHandshake(default!);
        if (err != default!) {
            return err;
        }
        var (certVerify, okΔ1) = msg._<ж<certificateVerifyMsg>>(ᐧ);
        if (!okΔ1) {
            c.sendAlert(alertUnexpectedMessage);
            return unexpectedMessageError(certVerify, msg);
        }
        // See RFC 8446, Section 4.4.3.
        if (!isSupportedSignatureAlgorithm((~certVerify).signatureAlgorithm, supportedSignatureAlgorithms())) {
            c.sendAlert(alertIllegalParameter);
            return errors.New("tls: client certificate used with invalid signature algorithm"u8);
        }
        var (sigType, sigHash, errΔ4) = typeAndHashFromSignatureScheme((~certVerify).signatureAlgorithm);
        if (errΔ4 != default!) {
            return c.sendAlert(alertInternalError);
        }
        if (sigType == signaturePKCS1v15 || sigHash == crypto.SHA1) {
            c.sendAlert(alertIllegalParameter);
            return errors.New("tls: client certificate used with invalid signature algorithm"u8);
        }
        var signed = signedMessage(sigHash, clientSignatureContext, hs.transcript);
        {
            var errΔ5 = verifyHandshakeSignature(sigType, (~(~c).peerCertificates[0]).PublicKey,
                sigHash, signed, (~certVerify).signature); if (errΔ5 != default!) {
                c.sendAlert(alertDecryptError);
                return errors.New("tls: invalid signature by the client certificate: "u8 + errΔ5.Error());
            }
        }
        {
            var errΔ6 = transcriptMsg(new certificateVerifyMsgжhandshakeMessage(certVerify), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ6 != default!) {
                return errΔ6;
            }
        }
    }
    // If we waited until the client certificates to send session tickets, we
    // are ready to do it now.
    {
        var errΔ7 = hs.sendSessionTickets(); if (errΔ7 != default!) {
            return errΔ7;
        }
    }
    return default!;
}

[GoRecv] internal static error readClientFinished(this ref serverHandshakeStateTLS13 hs) {
    var c = hs.c;
    // finishedMsg is not included in the transcript.
    var (msg, err) = c.readHandshake(default!);
    if (err != default!) {
        return err;
    }
    var (finished, ok) = msg._<ж<finishedMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(finished, msg);
    }
    if (!hmac.Equal(hs.clientFinished, (~finished).verifyData)) {
        c.sendAlert(alertDecryptError);
        return errors.New("tls: invalid client finished hash"u8);
    }
    c.of(Conn.Ꮡin).setTrafficSecret(hs.suite, QUICEncryptionLevelApplication, hs.trafficSecret);
    return default!;
}

} // end tls_package
