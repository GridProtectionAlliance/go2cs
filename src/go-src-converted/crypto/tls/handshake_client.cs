// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using context = context_package;
using crypto = crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using hpke = go.crypto.@internal.hpke_package;
using mlkem768 = go.crypto.@internal.mlkem768_package;
using rsa = go.crypto.rsa_package;
using subtle = go.crypto.subtle_package;
using Δx509 = go.crypto.x509_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using byteorder = go.@internal.byteorder_package;
using godebug = go.@internal.godebug_package;
using io = io_package;
using net = net_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using ecdh = go.crypto.ecdh_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.sync;
using math;

partial class tls_package {

[GoType] partial struct clientHandshakeState {
    internal ж<Conn> c;
    internal context.Context ctx;
    internal ж<serverHelloMsg> serverHello;
    internal ж<clientHelloMsg> hello;
    internal ж<cipherSuite> suite;
    internal ΔfinishedHash finishedHash;
    internal slice<byte> masterSecret;
    internal ж<SessionState> session; // the session being resumed
    internal slice<byte> ticket;   // a fresh ticket received during this handshake
}

internal static slice<SignatureScheme> testingOnlyForceClientHelloSignatureAlgorithms;

internal static (ж<clientHelloMsg>, ж<keySharePrivateKeys>, ж<echContext>, error) makeClientHello(this ж<Conn> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    var config = c.config;
    if (len((~config).ServerName) == 0 && !(~config).InsecureSkipVerify) {
        return (default!, default!, default!, errors.New("tls: either ServerName or InsecureSkipVerify must be specified in the tls.Config"u8));
    }
    nint nextProtosLength = 0;
    foreach (var (_, proto) in (~config).NextProtos) {
        {
            nint l = len(proto); if (l == 0 || l > 255){
                return (default!, default!, default!, errors.New("tls: invalid NextProtos value"u8));
            } else {
                nextProtosLength += 1 + l;
            }
        }
    }
    if (nextProtosLength > 0xffff) {
        return (default!, default!, default!, errors.New("tls: NextProtos values too large"u8));
    }
    var ΔsupportedVersions = config.supportedVersions(roleClient);
    if (len(ΔsupportedVersions) == 0) {
        return (default!, default!, default!, errors.New("tls: no supported versions satisfy MinVersion and MaxVersion"u8));
    }
    ref var maxVersion = ref heap<uint16>(out var ᏑmaxVersion);
    maxVersion = config.maxSupportedVersion(roleClient);
    var hello = Ꮡ(new clientHelloMsg(
        vers: maxVersion,
        compressionMethods: new uint8[]{compressionNone}.slice(),
        random: new slice<byte>(32),
        extendedMasterSecret: true,
        ocspStapling: true,
        scts: true,
        serverName: hostnameInSNI((~config).ServerName),
        supportedCurves: config.curvePreferences(maxVersion),
        supportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
        secureRenegotiationSupported: true,
        alpnProtocols: (~config).NextProtos,
        supportedVersions: ΔsupportedVersions
    ));
    // The version at the beginning of the ClientHello was capped at TLS 1.2
    // for compatibility reasons. The supported_versions extension is used
    // to negotiate versions now. See RFC 8446, Section 4.2.1.
    if ((~hello).vers > VersionTLS12) {
        hello.Value.vers = VersionTLS12;
    }
    if (c.handshakes > 0) {
        hello.Value.secureRenegotiation = c.clientFinished[..];
    }
    var preferenceOrder = cipherSuitesPreferenceOrder;
    if (!hasAESGCMHardwareSupport) {
        preferenceOrder = cipherSuitesPreferenceOrderNoAES;
    }
    var configCipherSuites = config.cipherSuites();
    hello.Value.cipherSuites = new slice<uint16>(0, len(configCipherSuites));
    foreach (var (_, suiteId) in preferenceOrder) {
        var suite = mutualCipherSuite(configCipherSuites, suiteId);
        if (suite == nil) {
            continue;
        }
        // Don't advertise TLS 1.2-only cipher suites unless
        // we're attempting TLS 1.2.
        if (maxVersion < VersionTLS12 && (nint)((~suite).flags & (nint)suiteTLS12) != 0) {
            continue;
        }
        hello.Value.cipherSuites = append((~hello).cipherSuites, suiteId);
    }
    var (_, err) = io.ReadFull(config.rand(), (~hello).random);
    if (err != default!) {
        return (default!, default!, default!, errors.New("tls: short read from Rand: "u8 + err.Error()));
    }
    // A random session ID is used to detect when the server accepted a ticket
    // and is resuming a session (see RFC 5077). In TLS 1.3, it's always set as
    // a compatibility measure (see RFC 8446, Section 4.1.2).
    //
    // The session ID is not set for QUIC connections (see RFC 9001, Section 8.4).
    if (c.quic == nil) {
        hello.Value.sessionId = new slice<byte>(32);
        {
            var (_, errΔ1) = io.ReadFull(config.rand(), (~hello).sessionId); if (errΔ1 != default!) {
                return (default!, default!, default!, errors.New("tls: short read from Rand: "u8 + errΔ1.Error()));
            }
        }
    }
    if (maxVersion >= VersionTLS12) {
        hello.Value.supportedSignatureAlgorithms = supportedSignatureAlgorithms();
    }
    if (testingOnlyForceClientHelloSignatureAlgorithms != default!) {
        hello.Value.supportedSignatureAlgorithms = testingOnlyForceClientHelloSignatureAlgorithms;
    }
    ж<keySharePrivateKeys> keyShareKeys = default!;
    if ((~hello).supportedVersions[0] == VersionTLS13) {
        // Reset the list of ciphers when the client only supports TLS 1.3.
        if (len((~hello).supportedVersions) == 1) {
            hello.Value.cipherSuites = default!;
        }
        if (hasAESGCMHardwareSupport){
            hello.Value.cipherSuites = append((~hello).cipherSuites, defaultCipherSuitesTLS13.ꓸꓸꓸ);
        } else {
            hello.Value.cipherSuites = append((~hello).cipherSuites, defaultCipherSuitesTLS13NoAES.ꓸꓸꓸ);
        }
        ref var curveID = ref heap<CurveID>(out var ᏑcurveID);
        curveID = config.curvePreferences(maxVersion)[0];
        keyShareKeys = Ꮡ(new keySharePrivateKeys(curveID: curveID));
        if (curveID == x25519Kyber768Draft00){
            (keyShareKeys.Value.ecdhe, err) = generateECDHEKey(config.rand(), X25519);
            if (err != default!) {
                return (default!, default!, default!, err);
            }
            var seed = new slice<byte>(mlkem768.SeedSize);
            {
                var (_, errΔ2) = io.ReadFull(config.rand(), seed); if (errΔ2 != default!) {
                    return (default!, default!, default!, errΔ2);
                }
            }
            (keyShareKeys.Value.kyber, err) = mlkem768.NewKeyFromSeed(seed);
            if (err != default!) {
                return (default!, default!, default!, err);
            }
            // For draft-tls-westerbaan-xyber768d00-03, we send both a hybrid
            // and a standard X25519 key share, since most servers will only
            // support the latter. We reuse the same X25519 ephemeral key for
            // both, as allowed by draft-ietf-tls-hybrid-design-09, Section 3.2.
            hello.Value.keyShares = new keyShare[]{
                new(group: x25519Kyber768Draft00, data: append((~keyShareKeys).ecdhe.PublicKey().Bytes(),
                    (~keyShareKeys).kyber.EncapsulationKey().ꓸꓸꓸ)),
                new(group: X25519, data: (~keyShareKeys).ecdhe.PublicKey().Bytes())
            }.slice();
        } else {
            {
                var (_, ok) = curveForCurveID(curveID); if (!ok) {
                    return (default!, default!, default!, errors.New("tls: CurvePreferences includes unsupported curve"u8));
                }
            }
            (keyShareKeys.Value.ecdhe, err) = generateECDHEKey(config.rand(), curveID);
            if (err != default!) {
                return (default!, default!, default!, err);
            }
            hello.Value.keyShares = new keyShare[]{new(group: curveID, data: (~keyShareKeys).ecdhe.PublicKey().Bytes())}.slice();
        }
    }
    if (c.quic != nil) {
        var (p, errΔ3) = Ꮡc.quicGetTransportParameters();
        if (errΔ3 != default!) {
            return (default!, default!, default!, errΔ3);
        }
        if (p == default!) {
            p = new byte[]{}.slice();
        }
        hello.Value.quicTransportParameters = p;
    }
    ж<echContext> ech = default!;
    if ((~c.config).EncryptedClientHelloConfigList != default!) {
        if ((~c.config).MinVersion != 0 && (~c.config).MinVersion < VersionTLS13) {
            return (default!, default!, default!, errors.New("tls: MinVersion must be >= VersionTLS13 if EncryptedClientHelloConfigList is populated"u8));
        }
        if ((~c.config).MaxVersion != 0 && (~c.config).MaxVersion <= VersionTLS12) {
            return (default!, default!, default!, errors.New("tls: MaxVersion must be >= VersionTLS13 if EncryptedClientHelloConfigList is populated"u8));
        }
        var (echConfigs, errΔ4) = parseECHConfigList((~c.config).EncryptedClientHelloConfigList);
        if (errΔ4 != default!) {
            return (default!, default!, default!, errΔ4);
        }
        var echConfig = pickECHConfig(echConfigs);
        if (echConfig == nil) {
            return (default!, default!, default!, errors.New("tls: EncryptedClientHelloConfigList contains no valid configs"u8));
        }
        ech = Ꮡ(new echContext(config: echConfig));
        hello.Value.encryptedClientHello = new byte[]{1}.slice();
        // indicate inner hello
        // We need to explicitly set these 1.2 fields to nil, as we do not
        // marshal them when encoding the inner hello, otherwise transcripts
        // will later mismatch.
        hello.Value.supportedPoints = default!;
        hello.Value.ticketSupported = false;
        hello.Value.secureRenegotiationSupported = false;
        hello.Value.extendedMasterSecret = false;
        (var echPK, errΔ4) = hpke.ParseHPKEPublicKey((~(~ech).config).KemID, (~(~ech).config).PublicKey);
        if (errΔ4 != default!) {
            return (default!, default!, default!, errΔ4);
        }
        (var suite, errΔ4) = pickECHCipherSuite((~(~ech).config).SymmetricCipherSuite);
        if (errΔ4 != default!) {
            return (default!, default!, default!, errΔ4);
        }
        ech.Value.kdfID = suite.KDFID;
        ech.Value.aeadID = suite.AEADID;
        var info = append(slice<byte>((@string)"tls ech\x00"), (~(~ech).config).raw.ꓸꓸꓸ);
        (ech.Value.encapsulatedKey, ech.Value.hpkeContext, errΔ4) = hpke.SetupSender((~(~ech).config).KemID, suite.KDFID, suite.AEADID, echPK, info);
        if (errΔ4 != default!) {
            return (default!, default!, default!, errΔ4);
        }
    }
    return (hello, keyShareKeys, ech, default!);
}

[GoType] partial struct echContext {
    internal ж<echConfig> config;
    internal ж<hpke.Sender> hpkeContext;
    internal slice<byte> encapsulatedKey;
    internal ж<clientHelloMsg> innerHello;
    internal hash.Hash innerTranscript;
    internal uint16 kdfID;
    internal uint16 aeadID;
    internal bool echRejected;
}

internal static error /*err*/ clientHandshake(this ж<Conn> Ꮡc, context.Context ctx) {
    heap<error>(out var Ꮡerr);
    func((defer, recover) => {
    ref var c = ref Ꮡc.Value;

    ref var err = ref Ꮡerr.ValueSlot;
        if (c.config == nil) {
            c.config = defaultConfig();
        }
        // This may be a renegotiation handshake, in which case some fields
        // need to be reset.
        c.didResume = false;
        (var hello, var keyShareKeys, var ech, err) = Ꮡc.makeClientHello();
        if (err != default!) {
            return;
        }
        (var session, var earlySecret, var binderKey, err) = Ꮡc.loadSession(hello);
        if (err != default!) {
            return;
        }
        if (session != nil) {
            defer(() => {
                // If we got a handshake failure when resuming a session, throw away
                // the session ticket. See RFC 5077, Section 3.2.
                //
                // RFC 8446 makes no mention of dropping tickets on failure, but it
                // does require servers to abort on invalid binders, so we need to
                // delete tickets to recover from a corrupted PSK.
                if (Ꮡerr.ValueSlot != default!) {
                    {
                        @string cacheKey = Ꮡc.Value.clientSessionCacheKey(); if (cacheKey != ""u8) {
                            (~Ꮡc.Value.config).ClientSessionCache.Put(cacheKey, nil);
                        }
                    }
                }
            });
        }
        if (ech != nil) {
            // Split hello into inner and outer
            ech.Value.innerHello = hello.clone();
            // Overwrite the server name in the outer hello with the public facing
            // name.
            hello.Value.serverName = ((@string)(~(~ech).config).PublicName);
            // Generate a new random for the outer hello.
            hello.Value.random = new slice<byte>(32);
            (_, err) = io.ReadFull(c.config.rand(), (~hello).random);
            if (err != default!) {
                err = errors.New("tls: short read from Rand: "u8 + err.Error()); return;
            }
            // NOTE: we don't do PSK GREASE, in line with boringssl, it's meant to
            // work around _possibly_ broken middleboxes, but there is little-to-no
            // evidence that this is actually a problem.
            {
                var errΔ1 = computeAndUpdateOuterECHExtension(hello, (~ech).innerHello, ech, true); if (errΔ1 != default!) {
                    err = errΔ1; return;
                }
            }
        }
        c.serverName = hello.Value.serverName;
        {
            var (_, errΔ2) = Ꮡc.writeHandshakeRecord(new clientHelloMsgжhandshakeMessage(hello), default!); if (errΔ2 != default!) {
                err = errΔ2; return;
            }
        }
        if ((~hello).earlyData) {
            var suite = cipherSuiteTLS13ByID((~session).cipherSuite);
            var transcript = (~suite).hash.New();
            {
                var errΔ3 = transcriptMsg(new clientHelloMsgжhandshakeMessage(hello), new hash_HashᴠtranscriptHash(transcript)); if (errΔ3 != default!) {
                    err = errΔ3; return;
                }
            }
            var earlyTrafficSecret = suite.deriveSecret(earlySecret, clientEarlyTrafficLabel, transcript);
            c.quicSetWriteSecret(QUICEncryptionLevelEarly, (~suite).id, earlyTrafficSecret);
        }
        // serverHelloMsg is not included in the transcript
        (var msg, err) = Ꮡc.readHandshake(default!);
        if (err != default!) {
            return;
        }
        var (serverHello, ok) = msg._<ж<serverHelloMsg>>(ᐧ);
        if (!ok) {
            Ꮡc.sendAlert(alertUnexpectedMessage);
            err = unexpectedMessageError(serverHello, msg); return;
        }
        {
            var errΔ4 = Ꮡc.pickTLSVersion(serverHello); if (errΔ4 != default!) {
                err = errΔ4; return;
            }
        }
        // If we are negotiating a protocol version that's lower than what we
        // support, check for the server downgrade canaries.
        // See RFC 8446, Section 4.1.3.
        var maxVers = c.config.maxSupportedVersion(roleClient);
        var tls12Downgrade = ((@string)((~serverHello).random[24..])) == downgradeCanaryTLS12;
        var tls11Downgrade = ((@string)((~serverHello).random[24..])) == downgradeCanaryTLS11;
        if (maxVers == VersionTLS13 && c.vers <= VersionTLS12 && (tls12Downgrade || tls11Downgrade) || maxVers == VersionTLS12 && c.vers <= VersionTLS11 && tls11Downgrade) {
            Ꮡc.sendAlert(alertIllegalParameter);
            err = errors.New("tls: downgrade attempt detected, possibly due to a MitM attack or a broken middlebox"u8); return;
        }
        if (c.vers == VersionTLS13) {
            var hsΔ1 = Ꮡ(new clientHandshakeStateTLS13(
                c: Ꮡc,
                ctx: ctx,
                serverHello: serverHello,
                hello: hello,
                keyShareKeys: keyShareKeys,
                session: session,
                earlySecret: earlySecret,
                binderKey: binderKey,
                echContext: ech
            ));
            err = hsΔ1.handshake(); return;
        }
        var hs = Ꮡ(new clientHandshakeState(
            c: Ꮡc,
            ctx: ctx,
            serverHello: serverHello,
            hello: hello,
            session: session
        ));
        err = hs.handshake();
    });
    return Ꮡerr.ValueSlot;
}

internal static (ж<SessionState> session, slice<byte> earlySecret, slice<byte> binderKey, error err) loadSession(this ж<Conn> Ꮡc, ж<clientHelloMsg> Ꮡhello) {
    ж<SessionState> session = default!;
    slice<byte> earlySecret = default!;
    slice<byte> binderKey = default!;
    error err = default!;

    ref var c = ref Ꮡc.Value;
    ref var hello = ref Ꮡhello.Value;
    if ((~c.config).SessionTicketsDisabled || (~c.config).ClientSessionCache == default!) {
        return (default!, default!, default!, default!);
    }
    var echInner = bytes.Equal(hello.encryptedClientHello, new byte[]{1}.slice());
    // ticketSupported is a TLS 1.2 extension (as TLS 1.3 replaced tickets with PSK
    // identities) and ECH requires and forces TLS 1.3.
    hello.ticketSupported = true && !echInner;
    if (hello.supportedVersions[0] == VersionTLS13) {
        // Require DHE on resumption as it guarantees forward secrecy against
        // compromise of the session ticket key. See RFC 8446, Section 4.2.9.
        hello.pskModes = new uint8[]{pskModeDHE}.slice();
    }
    // Session resumption is not allowed if renegotiating because
    // renegotiation is primarily used to allow a client to send a client
    // certificate, which would be skipped if session resumption occurred.
    if (c.handshakes != 0) {
        return (default!, default!, default!, default!);
    }
    // Try to resume a previously negotiated TLS session, if available.
    @string cacheKey = c.clientSessionCacheKey();
    if (cacheKey == ""u8) {
        return (default!, default!, default!, default!);
    }
    var (cs, ok) = (~c.config).ClientSessionCache.Get(cacheKey);
    if (!ok || cs == nil) {
        return (default!, default!, default!, default!);
    }
    session = cs.Value.session;
    // Check that version used for the previous session is still valid.
    var versOk = false;
    foreach (var (_, v) in hello.supportedVersions) {
        if (v == (~session).version) {
            versOk = true;
            break;
        }
    }
    if (!versOk) {
        return (default!, default!, default!, default!);
    }
    // Check that the cached server certificate is not expired, and that it's
    // valid for the ServerName. This should be ensured by the cache key, but
    // protect the application from a faulty ClientSessionCache implementation.
    if (c.config.time().After((~(~session).peerCertificates[0]).NotAfter)) {
        // Expired certificate, delete the entry.
        (~c.config).ClientSessionCache.Put(cacheKey, nil);
        return (default!, default!, default!, default!);
    }
    if (!(~c.config).InsecureSkipVerify) {
        if (len((~session).verifiedChains) == 0) {
            // The original connection had InsecureSkipVerify, while this doesn't.
            return (default!, default!, default!, default!);
        }
        {
            var errΔ1 = (~session).peerCertificates[0].VerifyHostname((~c.config).ServerName); if (errΔ1 != default!) {
                return (default!, default!, default!, default!);
            }
        }
    }
    if ((~session).version != VersionTLS13) {
        // In TLS 1.2 the cipher suite must match the resumed session. Ensure we
        // are still offering it.
        if (mutualCipherSuite(hello.cipherSuites, (~session).cipherSuite) == nil) {
            return (default!, default!, default!, default!);
        }
        hello.sessionTicket = session.Value.ticket;
        return (session, earlySecret, binderKey, err);
    }
    // Check that the session ticket is not expired.
    if (c.config.time().After(time_package.Unix((int64)(~session).useBy, 0))) {
        (~c.config).ClientSessionCache.Put(cacheKey, nil);
        return (default!, default!, default!, default!);
    }
    // In TLS 1.3 the KDF hash must match the resumed session. Ensure we
    // offer at least one cipher suite with that hash.
    var cipherSuite = cipherSuiteTLS13ByID((~session).cipherSuite);
    if (cipherSuite == nil) {
        return (default!, default!, default!, default!);
    }
    var cipherSuiteOk = false;
    foreach (var (_, offeredID) in hello.cipherSuites) {
        var offeredSuite = cipherSuiteTLS13ByID(offeredID);
        if (offeredSuite != nil && (~offeredSuite).hash == (~cipherSuite).hash) {
            cipherSuiteOk = true;
            break;
        }
    }
    if (!cipherSuiteOk) {
        return (default!, default!, default!, default!);
    }
    if (c.quic != nil) {
        if ((~c.quic).enableSessionEvents) {
            Ꮡc.quicResumeSession(session);
        }
        // For 0-RTT, the cipher suite has to match exactly, and we need to be
        // offering the same ALPN.
        if ((~session).EarlyData && mutualCipherSuiteTLS13(hello.cipherSuites, (~session).cipherSuite) != nil) {
            foreach (var (_, alpn) in hello.alpnProtocols) {
                if (alpn == (~session).alpnProtocol) {
                    hello.earlyData = true;
                    break;
                }
            }
        }
    }
    // Set the pre_shared_key extension. See RFC 8446, Section 4.2.11.1.
    var ticketAge = c.config.time().Sub(time_package.Unix((int64)(~session).createdAt, 0));
    var identity = new pskIdentity(
        label: (~session).ticket,
        obfuscatedTicketAge: (uint32)(int64)(ticketAge / time_package.Millisecond) + (~session).ageAdd
    );
    hello.pskIdentities = new pskIdentity[]{identity}.slice();
    hello.pskBinders = new slice<byte>[]{new slice<byte>((~cipherSuite).hash.Size())}.slice();
    // Compute the PSK binders. See RFC 8446, Section 4.2.11.2.
    earlySecret = cipherSuite.extract((~session).secret, default!);
    binderKey = cipherSuite.deriveSecret(earlySecret, resumptionBinderLabel, default!);
    var transcript = (~cipherSuite).hash.New();
    {
        var errΔ2 = computeAndUpdatePSK(Ꮡhello, binderKey, transcript, cipherSuite.finishedHash); if (errΔ2 != default!) {
            return (default!, default!, default!, errΔ2);
        }
    }
    return (session, earlySecret, binderKey, err);
}

internal static error pickTLSVersion(this ж<Conn> Ꮡc, ж<serverHelloMsg> ᏑserverHello) {
    ref var c = ref Ꮡc.Value;
    ref var serverHello = ref ᏑserverHello.Value;

    var peerVersion = serverHello.vers;
    if (serverHello.supportedVersion != 0) {
        peerVersion = serverHello.supportedVersion;
    }
    var (vers, ok) = c.config.mutualVersion(roleClient, new uint16[]{peerVersion}.slice());
    if (!ok) {
        Ꮡc.sendAlert(alertProtocolVersion);
        return fmt.Errorf("tls: server selected unsupported protocol version %x"u8, peerVersion);
    }
    c.vers = vers;
    c.haveVers = true;
    c.@in.version = vers;
    c.@out.version = vers;
    return default!;
}

// Does the handshake, either a full one or resumes old session. Requires hs.c,
// hs.hello, hs.serverHello, and, optionally, hs.session to be set.
internal static error handshake(this ж<clientHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.Value;

    var c = hs.c;
    var (isResume, err) = hs.processServerHello();
    if (err != default!) {
        return err;
    }
    hs.finishedHash = newFinishedHash((~c).vers, hs.suite);
    // No signatures of the handshake are needed in a resumption.
    // Otherwise, in a full handshake, if we don't have any certificates
    // configured then we will never send a CertificateVerify message and
    // thus no signatures are needed in that case either.
    if (isResume || (len((~(~c).config).Certificates) == 0 && (~(~c).config).GetClientCertificate == default!)) {
        hs.finishedHash.discardHandshakeBuffer();
    }
    {
        var errΔ1 = transcriptMsg(new clientHelloMsgжhandshakeMessage(hs.hello), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var errΔ2 = transcriptMsg(new serverHelloMsgжhandshakeMessage(hs.serverHello), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    c.Value.buffering = true;
    c.Value.didResume = isResume;
    if (isResume){
        {
            var errΔ3 = hs.establishKeys(); if (errΔ3 != default!) {
                return errΔ3;
            }
        }
        {
            var errΔ4 = Ꮡhs.readSessionTicket(); if (errΔ4 != default!) {
                return errΔ4;
            }
        }
        {
            var errΔ5 = Ꮡhs.readFinished((~c).serverFinished[..]); if (errΔ5 != default!) {
                return errΔ5;
            }
        }
        c.Value.clientFinishedIsFirst = false;
        // Make sure the connection is still being verified whether or not this
        // is a resumption. Resumptions currently don't reverify certificates so
        // they don't call verifyServerCertificate. See Issue 31641.
        if ((~(~c).config).VerifyConnection != default!) {
            {
                var errΔ6 = (~(~c).config).VerifyConnection(c.connectionStateLocked()); if (errΔ6 != default!) {
                    c.sendAlert(alertBadCertificate);
                    return errΔ6;
                }
            }
        }
        {
            var errΔ7 = Ꮡhs.sendFinished((~c).clientFinished[..]); if (errΔ7 != default!) {
                return errΔ7;
            }
        }
        {
            var (_, errΔ8) = c.flush(); if (errΔ8 != default!) {
                return errΔ8;
            }
        }
    } else {
        {
            var errΔ9 = Ꮡhs.doFullHandshake(); if (errΔ9 != default!) {
                return errΔ9;
            }
        }
        {
            var errΔ10 = hs.establishKeys(); if (errΔ10 != default!) {
                return errΔ10;
            }
        }
        {
            var errΔ11 = Ꮡhs.sendFinished((~c).clientFinished[..]); if (errΔ11 != default!) {
                return errΔ11;
            }
        }
        {
            var (_, errΔ12) = c.flush(); if (errΔ12 != default!) {
                return errΔ12;
            }
        }
        c.Value.clientFinishedIsFirst = true;
        {
            var errΔ13 = Ꮡhs.readSessionTicket(); if (errΔ13 != default!) {
                return errΔ13;
            }
        }
        {
            var errΔ14 = Ꮡhs.readFinished((~c).serverFinished[..]); if (errΔ14 != default!) {
                return errΔ14;
            }
        }
    }
    {
        var errΔ15 = hs.saveSessionTicket(); if (errΔ15 != default!) {
            return errΔ15;
        }
    }
    c.Value.ekm = ekmFromMasterSecret((~c).vers, hs.suite, hs.masterSecret, (~hs.hello).random, (~hs.serverHello).random);
    c.of(Conn.ᏑisHandshakeComplete).Store(true);
    return default!;
}

[GoRecv] internal static error pickCipherSuite(this ref clientHandshakeState hs) {
    {
        hs.suite = mutualCipherSuite((~hs.hello).cipherSuites, (~hs.serverHello).cipherSuite); if (hs.suite == nil) {
            hs.c.sendAlert(alertHandshakeFailure);
            return errors.New("tls: server chose an unconfigured cipher suite"u8);
        }
    }
    if ((~(~hs.c).config).CipherSuites == default! && !needFIPS() && rsaKexCiphers[(~hs.suite).id]) {
        tlsrsakex.Value();
        // ensure godebug is initialized
        tlsrsakex.IncNonDefault();
    }
    if ((~(~hs.c).config).CipherSuites == default! && !needFIPS() && tdesCiphers[(~hs.suite).id]) {
        tls3des.Value();
        // ensure godebug is initialized
        tls3des.IncNonDefault();
    }
    hs.c.Value.cipherSuite = hs.suite.Value.id;
    return default!;
}

internal static error doFullHandshake(this ж<clientHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.Value;

    var c = hs.c;
    var (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
    if (err != default!) {
        return err;
    }
    var (certMsg, ok) = msg._<ж<certificateMsg>>(ᐧ);
    if (!ok || len((~certMsg).certificates) == 0) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(certMsg, msg);
    }
    (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
    if (err != default!) {
        return err;
    }
    (var cs, ok) = msg._<ж<certificateStatusMsg>>(ᐧ);
    if (ok) {
        // RFC4366 on Certificate Status Request:
        // The server MAY return a "certificate_status" message.
        if (!(~hs.serverHello).ocspStapling) {
            // If a server returns a "CertificateStatus" message, then the
            // server MUST have included an extension of type "status_request"
            // with empty "extension_data" in the extended server hello.
            c.sendAlert(alertUnexpectedMessage);
            return errors.New("tls: received unexpected CertificateStatus message"u8);
        }
        c.Value.ocspResponse = cs.Value.response;
        (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
        if (err != default!) {
            return err;
        }
    }
    if ((~c).handshakes == 0){
        // If this is the first handshake on a connection, process and
        // (optionally) verify the server's certificates.
        {
            var errΔ1 = c.verifyServerCertificate((~certMsg).certificates); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
    } else {
        // This is a renegotiation handshake. We require that the
        // server's identity (i.e. leaf certificate) is unchanged and
        // thus any previous trust decision is still valid.
        //
        // See https://mitls.org/pages/attacks/3SHAKE for the
        // motivation behind this requirement.
        if (!bytes.Equal((~(~c).peerCertificates[0]).Raw, (~certMsg).certificates[0])) {
            c.sendAlert(alertBadCertificate);
            return errors.New("tls: server's identity changed during renegotiation"u8);
        }
    }
    var keyAgreement = (~hs.suite).ka((~c).vers);
    (var skx, ok) = msg._<ж<serverKeyExchangeMsg>>(ᐧ);
    if (ok) {
        err = keyAgreement.processServerKeyExchange((~c).config, hs.hello, hs.serverHello, (~c).peerCertificates[0], skx);
        if (err != default!) {
            c.sendAlert(alertUnexpectedMessage);
            return err;
        }
        if (len((~skx).key) >= 3 && (~skx).key[0] == 3) {
            /* named curve */
            c.Value.curveID = ((CurveID)byteorder.BeUint16((~skx).key[1..]));
        }
        (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
        if (err != default!) {
            return err;
        }
    }
    ж<Certificate> chainToSend = default!;
    bool certRequested = default!;
    (var certReq, ok) = msg._<ж<certificateRequestMsg>>(ᐧ);
    if (ok) {
        certRequested = true;
        var cri = certificateRequestInfoFromMsg(hs.ctx, (~c).vers, certReq);
        {
            (chainToSend, err) = c.getClientCertificate(cri); if (err != default!) {
                c.sendAlert(alertInternalError);
                return err;
            }
        }
        (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
        if (err != default!) {
            return err;
        }
    }
    (var shd, ok) = msg._<ж<serverHelloDoneMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(shd, msg);
    }
    // If the server requested a certificate then we have to send a
    // Certificate message, even if it's empty because we don't have a
    // certificate to send.
    if (certRequested) {
        certMsg = @new<certificateMsg>();
        certMsg.Value.certificates = chainToSend.Value.ΔCertificate;
        {
            var (_, errΔ2) = hs.c.writeHandshakeRecord(new certificateMsgжhandshakeMessage(certMsg), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    (var preMasterSecret, var ckx, err) = keyAgreement.generateClientKeyExchange((~c).config, hs.hello, (~c).peerCertificates[0]);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return err;
    }
    if (ckx != nil) {
        {
            var (_, errΔ3) = hs.c.writeHandshakeRecord(new clientKeyExchangeMsgжhandshakeMessage(ckx), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ3 != default!) {
                return errΔ3;
            }
        }
    }
    if ((~hs.serverHello).extendedMasterSecret){
        c.Value.extMasterSecret = true;
        hs.masterSecret = extMasterFromPreMasterSecret((~c).vers, hs.suite, preMasterSecret,
            hs.finishedHash.Sum());
    } else {
        hs.masterSecret = masterFromPreMasterSecret((~c).vers, hs.suite, preMasterSecret,
            (~hs.hello).random, (~hs.serverHello).random);
    }
    {
        var errΔ4 = (~c).config.writeKeyLog(keyLogLabelTLS12, (~hs.hello).random, hs.masterSecret); if (errΔ4 != default!) {
            c.sendAlert(alertInternalError);
            return errors.New("tls: failed to write to key log: "u8 + errΔ4.Error());
        }
    }
    if (chainToSend != nil && len((~chainToSend).ΔCertificate) > 0) {
        var certVerify = Ꮡ(new certificateVerifyMsg(nil));
        var (key, okΔ1) = (~chainToSend).PrivateKey._<crypto.Signer>(ᐧ);
        if (!okΔ1) {
            c.sendAlert(alertInternalError);
            return fmt.Errorf("tls: client certificate private key of type %T does not implement crypto.Signer"u8, (~chainToSend).PrivateKey);
        }
        uint8 sigType = default!;
        ref var sigHash = ref heap(new crypto.Hash(), out var ᏑsigHash);
        if ((~c).vers >= VersionTLS12){
            var (signatureAlgorithm, errΔ5) = selectSignatureScheme((~c).vers, chainToSend, (~certReq).supportedSignatureAlgorithms);
            if (errΔ5 != default!) {
                c.sendAlert(alertIllegalParameter);
                return errΔ5;
            }
            (sigType, sigHash, errΔ5) = typeAndHashFromSignatureScheme(signatureAlgorithm);
            if (errΔ5 != default!) {
                return c.sendAlert(alertInternalError);
            }
            certVerify.Value.hasSignatureAlgorithm = true;
            certVerify.Value.signatureAlgorithm = signatureAlgorithm;
        } else {
            (sigType, sigHash, err) = legacyTypeAndHashFromPublicKey(key.Public());
            if (err != default!) {
                c.sendAlert(alertIllegalParameter);
                return err;
            }
        }
        var signed = hs.finishedHash.hashForClientCertificate(sigType, sigHash);
        var signOpts = ((crypto.SignerOpts)new crypto_HashᴠSignerOpts(sigHash));
        if (sigType == signatureRSAPSS) {
            signOpts = new rsa_PSSOptionsжSignerOpts(Ꮡ(new rsa.PSSOptions(SaltLength: rsa.PSSSaltLengthEqualsHash, Hash: sigHash)));
        }
        (certVerify.Value.signature, err) = key.Sign((~c).config.rand(), signed, signOpts);
        if (err != default!) {
            c.sendAlert(alertInternalError);
            return err;
        }
        {
            var (_, errΔ6) = hs.c.writeHandshakeRecord(new certificateVerifyMsgжhandshakeMessage(certVerify), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ6 != default!) {
                return errΔ6;
            }
        }
    }
    hs.finishedHash.discardHandshakeBuffer();
    return default!;
}

[GoRecv] internal static error establishKeys(this ref clientHandshakeState hs) {
    var c = hs.c;
    var (clientMAC, serverMAC, clientKey, serverKey, clientIV, serverIV) = keysFromMasterSecret((~c).vers, hs.suite, hs.masterSecret, (~hs.hello).random, (~hs.serverHello).random, (~hs.suite).macLen, (~hs.suite).keyLen, (~hs.suite).ivLen);
    any clientCipher = default!;
    any serverCipher = default!;
    hash.Hash clientHash = default!;
    hash.Hash serverHash = default!;
    if ((~hs.suite).cipher != default!){
        clientCipher = (~hs.suite).cipher(clientKey, clientIV, false);
        /* not for reading */
        clientHash = (~hs.suite).mac(clientMAC);
        serverCipher = (~hs.suite).cipher(serverKey, serverIV, true);
        /* for reading */
        serverHash = (~hs.suite).mac(serverMAC);
    } else {
        clientCipher = (~hs.suite).aead(clientKey, clientIV);
        serverCipher = (~hs.suite).aead(serverKey, serverIV);
    }
    c.of(Conn.Ꮡin).prepareCipherSpec((~c).vers, serverCipher, serverHash);
    c.of(Conn.Ꮡout).prepareCipherSpec((~c).vers, clientCipher, clientHash);
    return default!;
}

[GoRecv] internal static bool serverResumedSession(this ref clientHandshakeState hs) {
    // If the server responded with the same sessionId then it means the
    // sessionTicket is being used to resume a TLS session.
    return hs.session != nil && (~hs.hello).sessionId != default! && bytes.Equal((~hs.serverHello).sessionId, (~hs.hello).sessionId);
}

[GoRecv] internal static (bool, error) processServerHello(this ref clientHandshakeState hs) {
    var c = hs.c;
    {
        var err = hs.pickCipherSuite(); if (err != default!) {
            return (false, err);
        }
    }
    if ((~hs.serverHello).compressionMethod != compressionNone) {
        c.sendAlert(alertUnexpectedMessage);
        return (false, errors.New("tls: server selected unsupported compression format"u8));
    }
    if ((~c).handshakes == 0 && (~hs.serverHello).secureRenegotiationSupported) {
        c.Value.secureRenegotiation = true;
        if (len((~hs.serverHello).secureRenegotiation) != 0) {
            c.sendAlert(alertHandshakeFailure);
            return (false, errors.New("tls: initial handshake had non-empty renegotiation extension"u8));
        }
    }
    if ((~c).handshakes > 0 && (~c).secureRenegotiation) {
        array<byte> expectedSecureRenegotiation = new(24);
        copy(expectedSecureRenegotiation[..], (~c).clientFinished[..]);
        copy(expectedSecureRenegotiation[12..], (~c).serverFinished[..]);
        if (!bytes.Equal((~hs.serverHello).secureRenegotiation, expectedSecureRenegotiation[..])) {
            c.sendAlert(alertHandshakeFailure);
            return (false, errors.New("tls: incorrect renegotiation extension contents"u8));
        }
    }
    {
        var err = checkALPN((~hs.hello).alpnProtocols, (~hs.serverHello).alpnProtocol, false); if (err != default!) {
            c.sendAlert(alertUnsupportedExtension);
            return (false, err);
        }
    }
    c.Value.clientProtocol = hs.serverHello.Value.alpnProtocol;
    c.Value.scts = hs.serverHello.Value.scts;
    if (!hs.serverResumedSession()) {
        return (false, default!);
    }
    if ((~hs.session).version != (~c).vers) {
        c.sendAlert(alertHandshakeFailure);
        return (false, errors.New("tls: server resumed a session with a different version"u8));
    }
    if ((~hs.session).cipherSuite != (~hs.suite).id) {
        c.sendAlert(alertHandshakeFailure);
        return (false, errors.New("tls: server resumed a session with a different cipher suite"u8));
    }
    // RFC 7627, Section 5.3
    if ((~hs.session).extMasterSecret != (~hs.serverHello).extendedMasterSecret) {
        c.sendAlert(alertHandshakeFailure);
        return (false, errors.New("tls: server resumed a session with a different EMS extension"u8));
    }
    // Restore master secret and certificates from previous state
    hs.masterSecret = hs.session.Value.secret;
    c.Value.extMasterSecret = hs.session.Value.extMasterSecret;
    c.Value.peerCertificates = hs.session.Value.peerCertificates;
    c.Value.activeCertHandles = hs.c.Value.activeCertHandles;
    c.Value.verifiedChains = hs.session.Value.verifiedChains;
    c.Value.ocspResponse = hs.session.Value.ocspResponse;
    // Let the ServerHello SCTs override the session SCTs from the original
    // connection, if any are provided
    if (len((~c).scts) == 0 && len((~hs.session).scts) != 0) {
        c.Value.scts = hs.session.Value.scts;
    }
    return (true, default!);
}

// checkALPN ensure that the server's choice of ALPN protocol is compatible with
// the protocols that we advertised in the Client Hello.
internal static error checkALPN(slice<@string> clientProtos, @string serverProto, bool quic) {
    if (serverProto == ""u8) {
        if (quic && len(clientProtos) > 0) {
            // RFC 9001, Section 8.1
            return errors.New("tls: server did not select an ALPN protocol"u8);
        }
        return default!;
    }
    if (len(clientProtos) == 0) {
        return errors.New("tls: server advertised unrequested ALPN extension"u8);
    }
    foreach (var (_, proto) in clientProtos) {
        if (proto == serverProto) {
            return default!;
        }
    }
    return errors.New("tls: server selected unadvertised ALPN protocol"u8);
}

internal static error readFinished(this ж<clientHandshakeState> Ꮡhs, slice<byte> @out) {
    ref var hs = ref Ꮡhs.Value;

    var c = hs.c;
    {
        var errΔ1 = c.readChangeCipherSpec(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    // finishedMsg is included in the transcript, but not until after we
    // check the client version, since the state before this message was
    // sent is used during verification.
    var (msg, err) = c.readHandshake(default!);
    if (err != default!) {
        return err;
    }
    var (serverFinished, ok) = msg._<ж<finishedMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(serverFinished, msg);
    }
    var verify = hs.finishedHash.serverSum(hs.masterSecret);
    if (len(verify) != len((~serverFinished).verifyData) || subtle.ConstantTimeCompare(verify, (~serverFinished).verifyData) != 1) {
        c.sendAlert(alertHandshakeFailure);
        return errors.New("tls: server's Finished message was incorrect"u8);
    }
    {
        var errΔ2 = transcriptMsg(new finishedMsgжhandshakeMessage(serverFinished), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    copy(@out, verify);
    return default!;
}

internal static error readSessionTicket(this ж<clientHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.Value;

    if (!(~hs.serverHello).ticketSupported) {
        return default!;
    }
    var c = hs.c;
    if (!(~hs.hello).ticketSupported) {
        c.sendAlert(alertIllegalParameter);
        return errors.New("tls: server sent unrequested session ticket"u8);
    }
    var (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
    if (err != default!) {
        return err;
    }
    var (sessionTicketMsg, ok) = msg._<ж<newSessionTicketMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(sessionTicketMsg, msg);
    }
    hs.ticket = sessionTicketMsg.Value.ticket;
    return default!;
}

[GoRecv] internal static error saveSessionTicket(this ref clientHandshakeState hs) {
    if (hs.ticket == default!) {
        return default!;
    }
    var c = hs.c;
    @string cacheKey = c.clientSessionCacheKey();
    if (cacheKey == ""u8) {
        return default!;
    }
    var session = c.sessionState();
    session.Value.secret = hs.masterSecret;
    session.Value.ticket = hs.ticket;
    var cs = Ꮡ(new ClientSessionState(session: session));
    (~(~c).config).ClientSessionCache.Put(cacheKey, cs);
    return default!;
}

internal static error sendFinished(this ж<clientHandshakeState> Ꮡhs, slice<byte> @out) {
    ref var hs = ref Ꮡhs.Value;

    var c = hs.c;
    {
        var err = c.writeChangeCipherRecord(); if (err != default!) {
            return err;
        }
    }
    var finished = @new<finishedMsg>();
    finished.Value.verifyData = hs.finishedHash.clientSum(hs.masterSecret);
    {
        var (_, err) = hs.c.writeHandshakeRecord(new finishedMsgжhandshakeMessage(finished), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (err != default!) {
            return err;
        }
    }
    copy(@out, (~finished).verifyData);
    return default!;
}

// defaultMaxRSAKeySize is the maximum RSA key size in bits that we are willing
// to verify the signatures of during a TLS handshake.
internal static readonly UntypedInt defaultMaxRSAKeySize = 8192;

internal static ж<godebug.Setting> tlsmaxrsasize = godebug.New("tlsmaxrsasize"u8);

internal static (nint max, bool ok) checkKeySize(nint n) {
    nint max = default!;
    bool ok = default!;

    {
        @string v = tlsmaxrsasize.Value(); if (v != ""u8) {
            {
                var (maxΔ1, err) = strconv.Atoi(v); if (err == default!) {
                    if ((n <= maxΔ1) != (n <= defaultMaxRSAKeySize)) {
                        tlsmaxrsasize.IncNonDefault();
                    }
                    return (maxΔ1, n <= maxΔ1);
                }
            }
        }
    }
    return (defaultMaxRSAKeySize, n <= defaultMaxRSAKeySize);
}

// verifyServerCertificate parses and verifies the provided chain, setting
// c.verifiedChains and c.peerCertificates or sending the appropriate alert.
internal static error verifyServerCertificate(this ж<Conn> Ꮡc, slice<slice<byte>> certificates) {
    ref var c = ref Ꮡc.Value;

    var activeHandles = new slice<ж<activeCert>>(len(certificates));
    var certs = new slice<ж<Δx509.Certificate>>(len(certificates));
    foreach (var (i, asn1Data) in certificates) {
        var (cert, err) = globalCertCache.newCert(asn1Data);
        if (err != default!) {
            Ꮡc.sendAlert(alertBadCertificate);
            return errors.New("tls: failed to parse certificate from server: "u8 + err.Error());
        }
        if ((~(~cert).cert).PublicKeyAlgorithm == Δx509.RSA) {
            nint n = (~(~(~cert).cert).PublicKey._<ж<rsa.PublicKey>>()).N.BitLen();
            {
                var (max, ok) = checkKeySize(n); if (!ok) {
                    Ꮡc.sendAlert(alertBadCertificate);
                    return fmt.Errorf("tls: server sent certificate containing RSA key larger than %d bits"u8, max);
                }
            }
        }
        activeHandles[i] = cert;
        certs[i] = cert.Value.cert;
    }
    var echRejected = (~c.config).EncryptedClientHelloConfigList != default! && !c.echAccepted;
    if (echRejected){
        if ((~c.config).EncryptedClientHelloRejectionVerify != default!){
            {
                var err = (~c.config).EncryptedClientHelloRejectionVerify(Ꮡc.connectionStateLocked()); if (err != default!) {
                    Ꮡc.sendAlert(alertBadCertificate);
                    return err;
                }
            }
        } else {
            var opts = new Δx509.VerifyOptions(
                Roots: (~c.config).RootCAs,
                CurrentTime: c.config.time(),
                DNSName: c.serverName,
                Intermediates: Δx509.NewCertPool()
            );
            foreach (var (_, cert) in certs[1..]) {
                opts.Intermediates.AddCert(cert);
            }
            error err = default!;
            (c.verifiedChains, err) = certs[0].Verify(opts);
            if (err != default!) {
                Ꮡc.sendAlert(alertBadCertificate);
                return new CertificateVerificationErrorжerror(Ꮡ(new CertificateVerificationError(UnverifiedCertificates: certs, Err: err)));
            }
        }
    } else 
    if (!(~c.config).InsecureSkipVerify) {
        var opts = new Δx509.VerifyOptions(
            Roots: (~c.config).RootCAs,
            CurrentTime: c.config.time(),
            DNSName: (~c.config).ServerName,
            Intermediates: Δx509.NewCertPool()
        );
        foreach (var (_, cert) in certs[1..]) {
            opts.Intermediates.AddCert(cert);
        }
        error err = default!;
        (c.verifiedChains, err) = certs[0].Verify(opts);
        if (err != default!) {
            Ꮡc.sendAlert(alertBadCertificate);
            return new CertificateVerificationErrorжerror(Ꮡ(new CertificateVerificationError(UnverifiedCertificates: certs, Err: err)));
        }
    }
    switch ((~certs[0]).PublicKey.type()) {
    case ж<rsa.PublicKey> _:
    case ж<ecdsa.PublicKey> _:
    case ed25519.PublicKey _: {
        break;
        break;
    }
    default: {
        Ꮡc.sendAlert(alertUnsupportedCertificate);
        return fmt.Errorf("tls: server's certificate contains an unsupported type of public key: %T"u8, (~certs[0]).PublicKey);
    }}

    c.activeCertHandles = activeHandles;
    c.peerCertificates = certs;
    if ((~c.config).VerifyPeerCertificate != default! && !echRejected) {
        {
            var err = (~c.config).VerifyPeerCertificate(certificates, c.verifiedChains); if (err != default!) {
                Ꮡc.sendAlert(alertBadCertificate);
                return err;
            }
        }
    }
    if ((~c.config).VerifyConnection != default! && !echRejected) {
        {
            var err = (~c.config).VerifyConnection(Ꮡc.connectionStateLocked()); if (err != default!) {
                Ꮡc.sendAlert(alertBadCertificate);
                return err;
            }
        }
    }
    return default!;
}

// certificateRequestInfoFromMsg generates a CertificateRequestInfo from a TLS
// <= 1.2 CertificateRequest, making an effort to fill in missing information.
internal static ж<CertificateRequestInfo> certificateRequestInfoFromMsg(context.Context ctx, uint16 vers, ж<certificateRequestMsg> ᏑcertReq) {
    ref var certReq = ref ᏑcertReq.Value;

    var cri = Ꮡ(new CertificateRequestInfo(
        AcceptableCAs: certReq.certificateAuthorities,
        Version: vers,
        ctx: ctx
    ));
    bool rsaAvail = default!;
    bool ecAvail = default!;
    foreach (var (_, certType) in certReq.certificateTypes) {
        var exprᴛ1 = certType;
        if (exprᴛ1 == certTypeRSASign) {
            rsaAvail = true;
        }
        else if (exprᴛ1 == certTypeECDSASign) {
            ecAvail = true;
        }

    }
    if (!certReq.hasSignatureAlgorithm) {
        // Prior to TLS 1.2, signature schemes did not exist. In this case we
        // make up a list based on the acceptable certificate types, to help
        // GetClientCertificate and SupportsCertificate select the right certificate.
        // The hash part of the SignatureScheme is a lie here, because
        // TLS 1.0 and 1.1 always use MD5+SHA1 for RSA and SHA1 for ECDSA.
        switch (ᐧ) {
        case {} when rsaAvail && ecAvail: {
            cri.Value.SignatureSchemes = new SignatureScheme[]{
                ECDSAWithP256AndSHA256, ECDSAWithP384AndSHA384, ECDSAWithP521AndSHA512,
                PKCS1WithSHA256, PKCS1WithSHA384, PKCS1WithSHA512, PKCS1WithSHA1
            }.slice();
            break;
        }
        case {} when rsaAvail: {
            cri.Value.SignatureSchemes = new SignatureScheme[]{
                PKCS1WithSHA256, PKCS1WithSHA384, PKCS1WithSHA512, PKCS1WithSHA1
            }.slice();
            break;
        }
        case {} when ecAvail: {
            cri.Value.SignatureSchemes = new SignatureScheme[]{
                ECDSAWithP256AndSHA256, ECDSAWithP384AndSHA384, ECDSAWithP521AndSHA512
            }.slice();
            break;
        }}

        return cri;
    }
    // Filter the signature schemes based on the certificate types.
    // See RFC 5246, Section 7.4.4 (where it calls this "somewhat complicated").
    cri.Value.SignatureSchemes = new slice<SignatureScheme>(0, len(certReq.supportedSignatureAlgorithms));
    foreach (var (_, sigScheme) in certReq.supportedSignatureAlgorithms) {
        var (sigType, _, err) = typeAndHashFromSignatureScheme(sigScheme);
        if (err != default!) {
            continue;
        }
        switch (sigType) {
        case signatureECDSA or signatureEd25519: {
            if (ecAvail) {
                cri.Value.SignatureSchemes = append((~cri).SignatureSchemes, sigScheme);
            }
            break;
        }
        case signatureRSAPSS or signaturePKCS1v15: {
            if (rsaAvail) {
                cri.Value.SignatureSchemes = append((~cri).SignatureSchemes, sigScheme);
            }
            break;
        }}

    }
    return cri;
}

[GoRecv] internal static (ж<Certificate>, error) getClientCertificate(this ref Conn c, ж<CertificateRequestInfo> Ꮡcri) {
    ref var cri = ref Ꮡcri.Value;

    if ((~c.config).GetClientCertificate != default!) {
        return (~c.config).GetClientCertificate(Ꮡcri);
    }
    foreach (var (_, vᴛ1) in (~c.config).Certificates) {
        ref var chain = ref heap(new Certificate(), out var Ꮡchain);
        chain = vᴛ1;

        {
            var err = cri.SupportsCertificate(Ꮡchain); if (err != default!) {
                continue;
            }
        }
        return (Ꮡchain, default!);
    }
    // No acceptable certificate found. Don't send a certificate.
    return (@new<Certificate>(), default!);
}

// clientSessionCacheKey returns a key used to cache sessionTickets that could
// be used to resume previously negotiated TLS sessions with a server.
[GoRecv] internal static @string clientSessionCacheKey(this ref Conn c) {
    if (len((~c.config).ServerName) > 0) {
        return (~c.config).ServerName;
    }
    if (c.conn != default!) {
        return c.conn.RemoteAddr().String();
    }
    return ""u8;
}

// hostnameInSNI converts name into an appropriate hostname for SNI.
// Literal IP addresses and absolute FQDNs are not permitted as SNI values.
// See RFC 6066, Section 3.
internal static @string hostnameInSNI(@string name) {
    @string host = name;
    if (len(host) > 0 && host[0] == (rune)'[' && host[len(host) - 1] == (rune)']') {
        host = host[1..(int)(len(host) - 1)];
    }
    {
        nint i = strings.LastIndex(host, "%"u8); if (i > 0) {
            host = host[..(int)(i)];
        }
    }
    if (net.ParseIP(host) != default!) {
        return ""u8;
    }
    while (len(name) > 0 && name[len(name) - 1] == (rune)'.') {
        name = name[..(int)(len(name) - 1)];
    }
    return name;
}

internal static error computeAndUpdatePSK(ж<clientHelloMsg> Ꮡm, slice<byte> binderKey, hash.Hash transcript, Func<slice<byte>, hash.Hash, slice<byte>> ΔfinishedHash) {
    ref var m = ref Ꮡm.Value;

    var (helloBytes, err) = Ꮡm.marshalWithoutBinders();
    if (err != default!) {
        return err;
    }
    transcript.Write(helloBytes);
    var pskBinders = new slice<byte>[]{ΔfinishedHash(binderKey, transcript)}.slice();
    return m.updateBinders(pskBinders);
}

} // end tls_package
