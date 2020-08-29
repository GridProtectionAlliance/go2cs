// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 August 29 08:31:15 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\handshake_client.go
using bytes = go.bytes_package;
using crypto = go.crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using rsa = go.crypto.rsa_package;
using subtle = go.crypto.subtle_package;
using x509 = go.crypto.x509_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using net = go.net_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        private partial struct clientHandshakeState
        {
            public ptr<Conn> c;
            public ptr<serverHelloMsg> serverHello;
            public ptr<clientHelloMsg> hello;
            public ptr<cipherSuite> suite;
            public finishedHash finishedHash;
            public slice<byte> masterSecret;
            public ptr<ClientSessionState> session;
        }

        private static (ref clientHelloMsg, error) makeClientHello(ref Config config)
        {
            if (len(config.ServerName) == 0L && !config.InsecureSkipVerify)
            {
                return (null, errors.New("tls: either ServerName or InsecureSkipVerify must be specified in the tls.Config"));
            }
            long nextProtosLength = 0L;
            foreach (var (_, proto) in config.NextProtos)
            {
                {
                    var l = len(proto);

                    if (l == 0L || l > 255L)
                    {
                        return (null, errors.New("tls: invalid NextProtos value"));
                    }
                    else
                    {
                        nextProtosLength += 1L + l;
                    }

                }
            }
            if (nextProtosLength > 0xffffUL)
            {
                return (null, errors.New("tls: NextProtos values too large"));
            }
            clientHelloMsg hello = ref new clientHelloMsg(vers:config.maxVersion(),compressionMethods:[]uint8{compressionNone},random:make([]byte,32),ocspStapling:true,scts:true,serverName:hostnameInSNI(config.ServerName),supportedCurves:config.curvePreferences(),supportedPoints:[]uint8{pointFormatUncompressed},nextProtoNeg:len(config.NextProtos)>0,secureRenegotiationSupported:true,alpnProtocols:config.NextProtos,);
            var possibleCipherSuites = config.cipherSuites();
            hello.cipherSuites = make_slice<ushort>(0L, len(possibleCipherSuites));

NextCipherSuite:

            foreach (var (_, suiteId) in possibleCipherSuites)
            {
                foreach (var (_, suite) in cipherSuites)
                {
                    if (suite.id != suiteId)
                    {
                        continue;
                    } 
                    // Don't advertise TLS 1.2-only cipher suites unless
                    // we're attempting TLS 1.2.
                    if (hello.vers < VersionTLS12 && suite.flags & suiteTLS12 != 0L)
                    {
                        continue;
                    }
                    hello.cipherSuites = append(hello.cipherSuites, suiteId);
                    _continueNextCipherSuite = true;
                    break;
                }
            }
            var (_, err) = io.ReadFull(config.rand(), hello.random);
            if (err != null)
            {
                return (null, errors.New("tls: short read from Rand: " + err.Error()));
            }
            if (hello.vers >= VersionTLS12)
            {
                hello.supportedSignatureAlgorithms = supportedSignatureAlgorithms;
            }
            return (hello, null);
        }

        // c.out.Mutex <= L; c.handshakeMutex <= L.
        private static error clientHandshake(this ref Conn c)
        {
            if (c.config == null)
            {
                c.config = defaultConfig();
            } 

            // This may be a renegotiation handshake, in which case some fields
            // need to be reset.
            c.didResume = false;

            var (hello, err) = makeClientHello(c.config);
            if (err != null)
            {
                return error.As(err);
            }
            if (c.handshakes > 0L)
            {
                hello.secureRenegotiation = c.clientFinished[..];
            }
            ref ClientSessionState session = default;
            @string cacheKey = default;
            var sessionCache = c.config.ClientSessionCache;
            if (c.config.SessionTicketsDisabled)
            {
                sessionCache = null;
            }
            if (sessionCache != null)
            {
                hello.ticketSupported = true;
            } 

            // Session resumption is not allowed if renegotiating because
            // renegotiation is primarily used to allow a client to send a client
            // certificate, which would be skipped if session resumption occurred.
            if (sessionCache != null && c.handshakes == 0L)
            { 
                // Try to resume a previously negotiated TLS session, if
                // available.
                cacheKey = clientSessionCacheKey(c.conn.RemoteAddr(), c.config);
                var (candidateSession, ok) = sessionCache.Get(cacheKey);
                if (ok)
                { 
                    // Check that the ciphersuite/version used for the
                    // previous session are still valid.
                    var cipherSuiteOk = false;
                    foreach (var (_, id) in hello.cipherSuites)
                    {
                        if (id == candidateSession.cipherSuite)
                        {
                            cipherSuiteOk = true;
                            break;
                        }
                    }
                    var versOk = candidateSession.vers >= c.config.minVersion() && candidateSession.vers <= c.config.maxVersion();
                    if (versOk && cipherSuiteOk)
                    {
                        session = candidateSession;
                    }
                }
            }
            if (session != null)
            {
                hello.sessionTicket = session.sessionTicket; 
                // A random session ID is used to detect when the
                // server accepted the ticket and is resuming a session
                // (see RFC 5077).
                hello.sessionId = make_slice<byte>(16L);
                {
                    var (_, err) = io.ReadFull(c.config.rand(), hello.sessionId);

                    if (err != null)
                    {
                        return error.As(errors.New("tls: short read from Rand: " + err.Error()));
                    }

                }
            }
            clientHandshakeState hs = ref new clientHandshakeState(c:c,hello:hello,session:session,);

            err = hs.handshake();

            if (err != null)
            {
                return error.As(err);
            } 

            // If we had a successful handshake and hs.session is different from
            // the one already cached - cache a new one
            if (sessionCache != null && hs.session != null && session != hs.session)
            {
                sessionCache.Put(cacheKey, hs.session);
            }
            return error.As(null);
        }

        // Does the handshake, either a full one or resumes old session.
        // Requires hs.c, hs.hello, and, optionally, hs.session to be set.
        private static error handshake(this ref clientHandshakeState hs)
        {
            var c = hs.c; 

            // send ClientHello
            {
                var err__prev1 = err;

                var (_, err) = c.writeRecord(recordTypeHandshake, hs.hello.marshal());

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }

            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err);
            }
            bool ok = default;
            hs.serverHello, ok = msg._<ref serverHelloMsg>();

            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(hs.serverHello, msg));
            }
            err = hs.pickTLSVersion();

            if (err != null)
            {
                return error.As(err);
            }
            err = hs.pickCipherSuite();

            if (err != null)
            {
                return error.As(err);
            }
            var (isResume, err) = hs.processServerHello();
            if (err != null)
            {
                return error.As(err);
            }
            hs.finishedHash = newFinishedHash(c.vers, hs.suite); 

            // No signatures of the handshake are needed in a resumption.
            // Otherwise, in a full handshake, if we don't have any certificates
            // configured then we will never send a CertificateVerify message and
            // thus no signatures are needed in that case either.
            if (isResume || (len(c.config.Certificates) == 0L && c.config.GetClientCertificate == null))
            {
                hs.finishedHash.discardHandshakeBuffer();
            }
            hs.finishedHash.Write(hs.hello.marshal());
            hs.finishedHash.Write(hs.serverHello.marshal());

            c.buffering = true;
            if (isResume)
            {
                {
                    var err__prev2 = err;

                    var err = hs.establishKeys();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                {
                    var err__prev2 = err;

                    err = hs.readSessionTicket();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                {
                    var err__prev2 = err;

                    err = hs.readFinished(c.serverFinished[..]);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                c.clientFinishedIsFirst = false;
                {
                    var err__prev2 = err;

                    err = hs.sendFinished(c.clientFinished[..]);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                {
                    var err__prev2 = err;

                    (_, err) = c.flush();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            else
            {
                {
                    var err__prev2 = err;

                    err = hs.doFullHandshake();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                {
                    var err__prev2 = err;

                    err = hs.establishKeys();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                {
                    var err__prev2 = err;

                    err = hs.sendFinished(c.clientFinished[..]);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                {
                    var err__prev2 = err;

                    (_, err) = c.flush();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                c.clientFinishedIsFirst = true;
                {
                    var err__prev2 = err;

                    err = hs.readSessionTicket();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                {
                    var err__prev2 = err;

                    err = hs.readFinished(c.serverFinished[..]);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            c.didResume = isResume;
            c.handshakeComplete = true;

            return error.As(null);
        }

        private static error pickTLSVersion(this ref clientHandshakeState hs)
        {
            var (vers, ok) = hs.c.config.mutualVersion(hs.serverHello.vers);
            if (!ok || vers < VersionTLS10)
            { 
                // TLS 1.0 is the minimum version supported as a client.
                hs.c.sendAlert(alertProtocolVersion);
                return error.As(fmt.Errorf("tls: server selected unsupported protocol version %x", hs.serverHello.vers));
            }
            hs.c.vers = vers;
            hs.c.haveVers = true;

            return error.As(null);
        }

        private static error pickCipherSuite(this ref clientHandshakeState hs)
        {
            hs.suite = mutualCipherSuite(hs.hello.cipherSuites, hs.serverHello.cipherSuite);

            if (hs.suite == null)
            {
                hs.c.sendAlert(alertHandshakeFailure);
                return error.As(errors.New("tls: server chose an unconfigured cipher suite"));
            }
            hs.c.cipherSuite = hs.suite.id;
            return error.As(null);
        }

        private static error doFullHandshake(this ref clientHandshakeState hs)
        {
            var c = hs.c;

            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err);
            }
            ref certificateMsg (certMsg, ok) = msg._<ref certificateMsg>();
            if (!ok || len(certMsg.certificates) == 0L)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(certMsg, msg));
            }
            hs.finishedHash.Write(certMsg.marshal());

            if (c.handshakes == 0L)
            { 
                // If this is the first handshake on a connection, process and
                // (optionally) verify the server's certificates.
                var certs = make_slice<ref x509.Certificate>(len(certMsg.certificates));
                {
                    var i__prev1 = i;

                    foreach (var (__i, __asn1Data) in certMsg.certificates)
                    {
                        i = __i;
                        asn1Data = __asn1Data;
                        var (cert, err) = x509.ParseCertificate(asn1Data);
                        if (err != null)
                        {
                            c.sendAlert(alertBadCertificate);
                            return error.As(errors.New("tls: failed to parse certificate from server: " + err.Error()));
                        }
                        certs[i] = cert;
                    }
            else


                    i = i__prev1;
                }

                if (!c.config.InsecureSkipVerify)
                {
                    x509.VerifyOptions opts = new x509.VerifyOptions(Roots:c.config.RootCAs,CurrentTime:c.config.time(),DNSName:c.config.ServerName,Intermediates:x509.NewCertPool(),);

                    {
                        var i__prev1 = i;
                        var cert__prev1 = cert;

                        foreach (var (__i, __cert) in certs)
                        {
                            i = __i;
                            cert = __cert;
                            if (i == 0L)
                            {
                                continue;
                            }
                            opts.Intermediates.AddCert(cert);
                        }

                        i = i__prev1;
                        cert = cert__prev1;
                    }

                    c.verifiedChains, err = certs[0L].Verify(opts);
                    if (err != null)
                    {
                        c.sendAlert(alertBadCertificate);
                        return error.As(err);
                    }
                }
                if (c.config.VerifyPeerCertificate != null)
                {
                    {
                        var err__prev3 = err;

                        var err = c.config.VerifyPeerCertificate(certMsg.certificates, c.verifiedChains);

                        if (err != null)
                        {
                            c.sendAlert(alertBadCertificate);
                            return error.As(err);
                        }

                        err = err__prev3;

                    }
                }
                switch (certs[0L].PublicKey.type())
                {
                    case ref rsa.PublicKey _:
                        break;
                        break;
                    case ref ecdsa.PublicKey _:
                        break;
                        break;
                    default:
                    {
                        c.sendAlert(alertUnsupportedCertificate);
                        return error.As(fmt.Errorf("tls: server's certificate contains an unsupported type of public key: %T", certs[0L].PublicKey));
                        break;
                    }

                }

                c.peerCertificates = certs;
            }            { 
                // This is a renegotiation handshake. We require that the
                // server's identity (i.e. leaf certificate) is unchanged and
                // thus any previous trust decision is still valid.
                //
                // See https://mitls.org/pages/attacks/3SHAKE for the
                // motivation behind this requirement.
                if (!bytes.Equal(c.peerCertificates[0L].Raw, certMsg.certificates[0L]))
                {
                    c.sendAlert(alertBadCertificate);
                    return error.As(errors.New("tls: server's identity changed during renegotiation"));
                }
            }
            msg, err = c.readHandshake();
            if (err != null)
            {
                return error.As(err);
            }
            ref certificateStatusMsg (cs, ok) = msg._<ref certificateStatusMsg>();
            if (ok)
            { 
                // RFC4366 on Certificate Status Request:
                // The server MAY return a "certificate_status" message.

                if (!hs.serverHello.ocspStapling)
                { 
                    // If a server returns a "CertificateStatus" message, then the
                    // server MUST have included an extension of type "status_request"
                    // with empty "extension_data" in the extended server hello.

                    c.sendAlert(alertUnexpectedMessage);
                    return error.As(errors.New("tls: received unexpected CertificateStatus message"));
                }
                hs.finishedHash.Write(cs.marshal());

                if (cs.statusType == statusTypeOCSP)
                {
                    c.ocspResponse = cs.response;
                }
                msg, err = c.readHandshake();
                if (err != null)
                {
                    return error.As(err);
                }
            }
            var keyAgreement = hs.suite.ka(c.vers);

            ref serverKeyExchangeMsg (skx, ok) = msg._<ref serverKeyExchangeMsg>();
            if (ok)
            {
                hs.finishedHash.Write(skx.marshal());
                err = keyAgreement.processServerKeyExchange(c.config, hs.hello, hs.serverHello, c.peerCertificates[0L], skx);
                if (err != null)
                {
                    c.sendAlert(alertUnexpectedMessage);
                    return error.As(err);
                }
                msg, err = c.readHandshake();
                if (err != null)
                {
                    return error.As(err);
                }
            }
            ref Certificate chainToSend = default;
            bool certRequested = default;
            ref certificateRequestMsg (certReq, ok) = msg._<ref certificateRequestMsg>();
            if (ok)
            {
                certRequested = true;
                hs.finishedHash.Write(certReq.marshal());

                chainToSend, err = hs.getCertificate(certReq);

                if (err != null)
                {
                    c.sendAlert(alertInternalError);
                    return error.As(err);
                }
                msg, err = c.readHandshake();
                if (err != null)
                {
                    return error.As(err);
                }
            }
            ref serverHelloDoneMsg (shd, ok) = msg._<ref serverHelloDoneMsg>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(shd, msg));
            }
            hs.finishedHash.Write(shd.marshal()); 

            // If the server requested a certificate then we have to send a
            // Certificate message, even if it's empty because we don't have a
            // certificate to send.
            if (certRequested)
            {
                certMsg = @new<certificateMsg>();
                certMsg.certificates = chainToSend.Certificate;
                hs.finishedHash.Write(certMsg.marshal());
                {
                    var err__prev2 = err;

                    var (_, err) = c.writeRecord(recordTypeHandshake, certMsg.marshal());

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            var (preMasterSecret, ckx, err) = keyAgreement.generateClientKeyExchange(c.config, hs.hello, c.peerCertificates[0L]);
            if (err != null)
            {
                c.sendAlert(alertInternalError);
                return error.As(err);
            }
            if (ckx != null)
            {
                hs.finishedHash.Write(ckx.marshal());
                {
                    var err__prev2 = err;

                    (_, err) = c.writeRecord(recordTypeHandshake, ckx.marshal());

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            if (chainToSend != null && len(chainToSend.Certificate) > 0L)
            {
                certificateVerifyMsg certVerify = ref new certificateVerifyMsg(hasSignatureAndHash:c.vers>=VersionTLS12,);

                crypto.Signer (key, ok) = chainToSend.PrivateKey._<crypto.Signer>();
                if (!ok)
                {
                    c.sendAlert(alertInternalError);
                    return error.As(fmt.Errorf("tls: client certificate private key of type %T does not implement crypto.Signer", chainToSend.PrivateKey));
                }
                byte signatureType = default;
                switch (key.Public().type())
                {
                    case ref ecdsa.PublicKey _:
                        signatureType = signatureECDSA;
                        break;
                    case ref rsa.PublicKey _:
                        signatureType = signatureRSA;
                        break;
                    default:
                    {
                        c.sendAlert(alertInternalError);
                        return error.As(fmt.Errorf("tls: failed to sign handshake with client certificate: unknown client certificate key type: %T", key));
                        break;
                    } 

                    // SignatureAndHashAlgorithm was introduced in TLS 1.2.
                } 

                // SignatureAndHashAlgorithm was introduced in TLS 1.2.
                if (certVerify.hasSignatureAndHash)
                {
                    certVerify.signatureAlgorithm, err = hs.finishedHash.selectClientCertSignatureAlgorithm(certReq.supportedSignatureAlgorithms, signatureType);
                    if (err != null)
                    {
                        c.sendAlert(alertInternalError);
                        return error.As(err);
                    }
                }
                var (digest, hashFunc, err) = hs.finishedHash.hashForClientCertificate(signatureType, certVerify.signatureAlgorithm, hs.masterSecret);
                if (err != null)
                {
                    c.sendAlert(alertInternalError);
                    return error.As(err);
                }
                certVerify.signature, err = key.Sign(c.config.rand(), digest, hashFunc);
                if (err != null)
                {
                    c.sendAlert(alertInternalError);
                    return error.As(err);
                }
                hs.finishedHash.Write(certVerify.marshal());
                {
                    var err__prev2 = err;

                    (_, err) = c.writeRecord(recordTypeHandshake, certVerify.marshal());

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            hs.masterSecret = masterFromPreMasterSecret(c.vers, hs.suite, preMasterSecret, hs.hello.random, hs.serverHello.random);
            {
                var err__prev1 = err;

                err = c.config.writeKeyLog(hs.hello.random, hs.masterSecret);

                if (err != null)
                {
                    c.sendAlert(alertInternalError);
                    return error.As(errors.New("tls: failed to write to key log: " + err.Error()));
                }

                err = err__prev1;

            }

            hs.finishedHash.discardHandshakeBuffer();

            return error.As(null);
        }

        private static error establishKeys(this ref clientHandshakeState hs)
        {
            var c = hs.c;

            var (clientMAC, serverMAC, clientKey, serverKey, clientIV, serverIV) = keysFromMasterSecret(c.vers, hs.suite, hs.masterSecret, hs.hello.random, hs.serverHello.random, hs.suite.macLen, hs.suite.keyLen, hs.suite.ivLen);
            var clientCipher = default;            var serverCipher = default;

            macFunction clientHash = default;            macFunction serverHash = default;

            if (hs.suite.cipher != null)
            {
                clientCipher = hs.suite.cipher(clientKey, clientIV, false);
                clientHash = hs.suite.mac(c.vers, clientMAC);
                serverCipher = hs.suite.cipher(serverKey, serverIV, true);
                serverHash = hs.suite.mac(c.vers, serverMAC);
            }
            else
            {
                clientCipher = hs.suite.aead(clientKey, clientIV);
                serverCipher = hs.suite.aead(serverKey, serverIV);
            }
            c.@in.prepareCipherSpec(c.vers, serverCipher, serverHash);
            c.@out.prepareCipherSpec(c.vers, clientCipher, clientHash);
            return error.As(null);
        }

        private static bool serverResumedSession(this ref clientHandshakeState hs)
        { 
            // If the server responded with the same sessionId then it means the
            // sessionTicket is being used to resume a TLS session.
            return hs.session != null && hs.hello.sessionId != null && bytes.Equal(hs.serverHello.sessionId, hs.hello.sessionId);
        }

        private static (bool, error) processServerHello(this ref clientHandshakeState hs)
        {
            var c = hs.c;

            if (hs.serverHello.compressionMethod != compressionNone)
            {
                c.sendAlert(alertUnexpectedMessage);
                return (false, errors.New("tls: server selected unsupported compression format"));
            }
            if (c.handshakes == 0L && hs.serverHello.secureRenegotiationSupported)
            {
                c.secureRenegotiation = true;
                if (len(hs.serverHello.secureRenegotiation) != 0L)
                {
                    c.sendAlert(alertHandshakeFailure);
                    return (false, errors.New("tls: initial handshake had non-empty renegotiation extension"));
                }
            }
            if (c.handshakes > 0L && c.secureRenegotiation)
            {
                array<byte> expectedSecureRenegotiation = new array<byte>(24L);
                copy(expectedSecureRenegotiation[..], c.clientFinished[..]);
                copy(expectedSecureRenegotiation[12L..], c.serverFinished[..]);
                if (!bytes.Equal(hs.serverHello.secureRenegotiation, expectedSecureRenegotiation[..]))
                {
                    c.sendAlert(alertHandshakeFailure);
                    return (false, errors.New("tls: incorrect renegotiation extension contents"));
                }
            }
            var clientDidNPN = hs.hello.nextProtoNeg;
            var clientDidALPN = len(hs.hello.alpnProtocols) > 0L;
            var serverHasNPN = hs.serverHello.nextProtoNeg;
            var serverHasALPN = len(hs.serverHello.alpnProtocol) > 0L;

            if (!clientDidNPN && serverHasNPN)
            {
                c.sendAlert(alertHandshakeFailure);
                return (false, errors.New("tls: server advertised unrequested NPN extension"));
            }
            if (!clientDidALPN && serverHasALPN)
            {
                c.sendAlert(alertHandshakeFailure);
                return (false, errors.New("tls: server advertised unrequested ALPN extension"));
            }
            if (serverHasNPN && serverHasALPN)
            {
                c.sendAlert(alertHandshakeFailure);
                return (false, errors.New("tls: server advertised both NPN and ALPN extensions"));
            }
            if (serverHasALPN)
            {
                c.clientProtocol = hs.serverHello.alpnProtocol;
                c.clientProtocolFallback = false;
            }
            c.scts = hs.serverHello.scts;

            if (!hs.serverResumedSession())
            {
                return (false, null);
            }
            if (hs.session.vers != c.vers)
            {
                c.sendAlert(alertHandshakeFailure);
                return (false, errors.New("tls: server resumed a session with a different version"));
            }
            if (hs.session.cipherSuite != hs.suite.id)
            {
                c.sendAlert(alertHandshakeFailure);
                return (false, errors.New("tls: server resumed a session with a different cipher suite"));
            } 

            // Restore masterSecret and peerCerts from previous state
            hs.masterSecret = hs.session.masterSecret;
            c.peerCertificates = hs.session.serverCertificates;
            c.verifiedChains = hs.session.verifiedChains;
            return (true, null);
        }

        private static error readFinished(this ref clientHandshakeState hs, slice<byte> @out)
        {
            var c = hs.c;

            c.readRecord(recordTypeChangeCipherSpec);
            if (c.@in.err != null)
            {
                return error.As(c.@in.err);
            }
            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err);
            }
            ref finishedMsg (serverFinished, ok) = msg._<ref finishedMsg>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(serverFinished, msg));
            }
            var verify = hs.finishedHash.serverSum(hs.masterSecret);
            if (len(verify) != len(serverFinished.verifyData) || subtle.ConstantTimeCompare(verify, serverFinished.verifyData) != 1L)
            {
                c.sendAlert(alertHandshakeFailure);
                return error.As(errors.New("tls: server's Finished message was incorrect"));
            }
            hs.finishedHash.Write(serverFinished.marshal());
            copy(out, verify);
            return error.As(null);
        }

        private static error readSessionTicket(this ref clientHandshakeState hs)
        {
            if (!hs.serverHello.ticketSupported)
            {
                return error.As(null);
            }
            var c = hs.c;
            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err);
            }
            ref newSessionTicketMsg (sessionTicketMsg, ok) = msg._<ref newSessionTicketMsg>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(sessionTicketMsg, msg));
            }
            hs.finishedHash.Write(sessionTicketMsg.marshal());

            hs.session = ref new ClientSessionState(sessionTicket:sessionTicketMsg.ticket,vers:c.vers,cipherSuite:hs.suite.id,masterSecret:hs.masterSecret,serverCertificates:c.peerCertificates,verifiedChains:c.verifiedChains,);

            return error.As(null);
        }

        private static error sendFinished(this ref clientHandshakeState hs, slice<byte> @out)
        {
            var c = hs.c;

            {
                var (_, err) = c.writeRecord(recordTypeChangeCipherSpec, new slice<byte>(new byte[] { 1 }));

                if (err != null)
                {
                    return error.As(err);
                }

            }
            if (hs.serverHello.nextProtoNeg)
            {
                ptr<object> nextProto = @new<nextProtoMsg>();
                var (proto, fallback) = mutualProtocol(c.config.NextProtos, hs.serverHello.nextProtos);
                nextProto.proto = proto;
                c.clientProtocol = proto;
                c.clientProtocolFallback = fallback;

                hs.finishedHash.Write(nextProto.marshal());
                {
                    (_, err) = c.writeRecord(recordTypeHandshake, nextProto.marshal());

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }
            ptr<finishedMsg> finished = @new<finishedMsg>();
            finished.verifyData = hs.finishedHash.clientSum(hs.masterSecret);
            hs.finishedHash.Write(finished.marshal());
            {
                (_, err) = c.writeRecord(recordTypeHandshake, finished.marshal());

                if (err != null)
                {
                    return error.As(err);
                }

            }
            copy(out, finished.verifyData);
            return error.As(null);
        }

        // tls11SignatureSchemes contains the signature schemes that we synthesise for
        // a TLS <= 1.1 connection, based on the supported certificate types.
        private static SignatureScheme tls11SignatureSchemes = new slice<SignatureScheme>(new SignatureScheme[] { ECDSAWithP256AndSHA256, ECDSAWithP384AndSHA384, ECDSAWithP521AndSHA512, PKCS1WithSHA256, PKCS1WithSHA384, PKCS1WithSHA512, PKCS1WithSHA1 });

 
        // tls11SignatureSchemesNumECDSA is the number of initial elements of
        // tls11SignatureSchemes that use ECDSA.
        private static readonly long tls11SignatureSchemesNumECDSA = 3L; 
        // tls11SignatureSchemesNumRSA is the number of trailing elements of
        // tls11SignatureSchemes that use RSA.
        private static readonly long tls11SignatureSchemesNumRSA = 4L;

        private static (ref Certificate, error) getCertificate(this ref clientHandshakeState hs, ref certificateRequestMsg certReq)
        {
            var c = hs.c;

            bool rsaAvail = default;            bool ecdsaAvail = default;

            foreach (var (_, certType) in certReq.certificateTypes)
            {

                if (certType == certTypeRSASign) 
                    rsaAvail = true;
                else if (certType == certTypeECDSASign) 
                    ecdsaAvail = true;
                            }
            if (c.config.GetClientCertificate != null)
            {
                slice<SignatureScheme> signatureSchemes = default;

                if (!certReq.hasSignatureAndHash)
                { 
                    // Prior to TLS 1.2, the signature schemes were not
                    // included in the certificate request message. In this
                    // case we use a plausible list based on the acceptable
                    // certificate types.
                    signatureSchemes = tls11SignatureSchemes;
                    if (!ecdsaAvail)
                    {
                        signatureSchemes = signatureSchemes[tls11SignatureSchemesNumECDSA..];
                    }
                    if (!rsaAvail)
                    {
                        signatureSchemes = signatureSchemes[..len(signatureSchemes) - tls11SignatureSchemesNumRSA];
                    }
                }
                else
                {
                    signatureSchemes = certReq.supportedSignatureAlgorithms;
                }
                return c.config.GetClientCertificate(ref new CertificateRequestInfo(AcceptableCAs:certReq.certificateAuthorities,SignatureSchemes:signatureSchemes,));
            } 

            // RFC 4346 on the certificateAuthorities field: A list of the
            // distinguished names of acceptable certificate authorities.
            // These distinguished names may specify a desired
            // distinguished name for a root CA or for a subordinate CA;
            // thus, this message can be used to describe both known roots
            // and a desired authorization space. If the
            // certificate_authorities list is empty then the client MAY
            // send any certificate of the appropriate
            // ClientCertificateType, unless there is some external
            // arrangement to the contrary.

            // We need to search our list of client certs for one
            // where SignatureAlgorithm is acceptable to the server and the
            // Issuer is in certReq.certificateAuthorities
findCert: 

            // No acceptable certificate found. Don't send a certificate.
            foreach (var (i, chain) in c.config.Certificates)
            {
                if (!rsaAvail && !ecdsaAvail)
                {
                    continue;
                }
                foreach (var (j, cert) in chain.Certificate)
                {
                    var x509Cert = chain.Leaf; 
                    // parse the certificate if this isn't the leaf
                    // node, or if chain.Leaf was nil
                    if (j != 0L || x509Cert == null)
                    {
                        error err = default;
                        x509Cert, err = x509.ParseCertificate(cert);

                        if (err != null)
                        {
                            c.sendAlert(alertInternalError);
                            return (null, errors.New("tls: failed to parse client certificate #" + strconv.Itoa(i) + ": " + err.Error()));
                        }
                    }

                    if (rsaAvail && x509Cert.PublicKeyAlgorithm == x509.RSA)                     else if (ecdsaAvail && x509Cert.PublicKeyAlgorithm == x509.ECDSA)                     else 
                        _continuefindCert = true;
                        break;
                                        if (len(certReq.certificateAuthorities) == 0L)
                    { 
                        // they gave us an empty list, so just take the
                        // first cert from c.config.Certificates
                        return (ref chain, null);
                    }
                    foreach (var (_, ca) in certReq.certificateAuthorities)
                    {
                        if (bytes.Equal(x509Cert.RawIssuer, ca))
                        {
                            return (ref chain, null);
                        }
                    }
                }
            } 

            // No acceptable certificate found. Don't send a certificate.
            return (@new<Certificate>(), null);
        }

        // clientSessionCacheKey returns a key used to cache sessionTickets that could
        // be used to resume previously negotiated TLS sessions with a server.
        private static @string clientSessionCacheKey(net.Addr serverAddr, ref Config config)
        {
            if (len(config.ServerName) > 0L)
            {
                return config.ServerName;
            }
            return serverAddr.String();
        }

        // mutualProtocol finds the mutual Next Protocol Negotiation or ALPN protocol
        // given list of possible protocols and a list of the preference order. The
        // first list must not be empty. It returns the resulting protocol and flag
        // indicating if the fallback case was reached.
        private static (@string, bool) mutualProtocol(slice<@string> protos, slice<@string> preferenceProtos)
        {
            foreach (var (_, s) in preferenceProtos)
            {
                foreach (var (_, c) in protos)
                {
                    if (s == c)
                    {
                        return (s, false);
                    }
                }
            }
            return (protos[0L], true);
        }

        // hostnameInSNI converts name into an approriate hostname for SNI.
        // Literal IP addresses and absolute FQDNs are not permitted as SNI values.
        // See https://tools.ietf.org/html/rfc6066#section-3.
        private static @string hostnameInSNI(@string name)
        {
            var host = name;
            if (len(host) > 0L && host[0L] == '[' && host[len(host) - 1L] == ']')
            {
                host = host[1L..len(host) - 1L];
            }
            {
                var i = strings.LastIndex(host, "%");

                if (i > 0L)
                {
                    host = host[..i];
                }

            }
            if (net.ParseIP(host) != null)
            {
                return "";
            }
            while (len(name) > 0L && name[len(name) - 1L] == '.')
            {
                name = name[..len(name) - 1L];
            }

            return name;
        }
    }
}}
