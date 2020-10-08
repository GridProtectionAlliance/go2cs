// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 October 08 03:37:39 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\handshake_client_tls13.go
using bytes = go.bytes_package;
using crypto = go.crypto_package;
using hmac = go.crypto.hmac_package;
using rsa = go.crypto.rsa_package;
using errors = go.errors_package;
using hash = go.hash_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        private partial struct clientHandshakeStateTLS13
        {
            public ptr<Conn> c;
            public ptr<serverHelloMsg> serverHello;
            public ptr<clientHelloMsg> hello;
            public ecdheParameters ecdheParams;
            public ptr<ClientSessionState> session;
            public slice<byte> earlySecret;
            public slice<byte> binderKey;
            public ptr<certificateRequestMsgTLS13> certReq;
            public bool usingPSK;
            public bool sentDummyCCS;
            public ptr<cipherSuiteTLS13> suite;
            public hash.Hash transcript;
            public slice<byte> masterSecret;
            public slice<byte> trafficSecret; // client_application_traffic_secret_0
        }

        // handshake requires hs.c, hs.hello, hs.serverHello, hs.ecdheParams, and,
        // optionally, hs.session, hs.earlySecret and hs.binderKey to be set.
        private static error handshake(this ptr<clientHandshakeStateTLS13> _addr_hs)
        {
            ref clientHandshakeStateTLS13 hs = ref _addr_hs.val;

            var c = hs.c; 

            // The server must not select TLS 1.3 in a renegotiation. See RFC 8446,
            // sections 4.1.2 and 4.1.3.
            if (c.handshakes > 0L)
            {
                c.sendAlert(alertProtocolVersion);
                return error.As(errors.New("tls: server selected TLS 1.3 in a renegotiation"))!;
            } 

            // Consistency check on the presence of a keyShare and its parameters.
            if (hs.ecdheParams == null || len(hs.hello.keyShares) != 1L)
            {
                return error.As(c.sendAlert(alertInternalError))!;
            }

            {
                var err__prev1 = err;

                var err = hs.checkServerHelloOrHRR();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            hs.transcript = hs.suite.hash.New();
            hs.transcript.Write(hs.hello.marshal());

            if (bytes.Equal(hs.serverHello.random, helloRetryRequestRandom))
            {
                {
                    var err__prev2 = err;

                    err = hs.sendDummyChangeCipherSpec();

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                {
                    var err__prev2 = err;

                    err = hs.processHelloRetryRequest();

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            hs.transcript.Write(hs.serverHello.marshal());

            c.buffering = true;
            {
                var err__prev1 = err;

                err = hs.processServerHello();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = hs.sendDummyChangeCipherSpec();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = hs.establishHandshakeKeys();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = hs.readServerParameters();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = hs.readServerCertificate();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = hs.readServerFinished();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = hs.sendClientCertificate();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = hs.sendClientFinished();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                var (_, err) = c.flush();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            atomic.StoreUint32(_addr_c.handshakeStatus, 1L);

            return error.As(null!)!;

        }

        // checkServerHelloOrHRR does validity checks that apply to both ServerHello and
        // HelloRetryRequest messages. It sets hs.suite.
        private static error checkServerHelloOrHRR(this ptr<clientHandshakeStateTLS13> _addr_hs)
        {
            ref clientHandshakeStateTLS13 hs = ref _addr_hs.val;

            var c = hs.c;

            if (hs.serverHello.supportedVersion == 0L)
            {
                c.sendAlert(alertMissingExtension);
                return error.As(errors.New("tls: server selected TLS 1.3 using the legacy version field"))!;
            }

            if (hs.serverHello.supportedVersion != VersionTLS13)
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: server selected an invalid version after a HelloRetryRequest"))!;
            }

            if (hs.serverHello.vers != VersionTLS12)
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: server sent an incorrect legacy version"))!;
            }

            if (hs.serverHello.ocspStapling || hs.serverHello.ticketSupported || hs.serverHello.secureRenegotiationSupported || len(hs.serverHello.secureRenegotiation) != 0L || len(hs.serverHello.alpnProtocol) != 0L || len(hs.serverHello.scts) != 0L)
            {
                c.sendAlert(alertUnsupportedExtension);
                return error.As(errors.New("tls: server sent a ServerHello extension forbidden in TLS 1.3"))!;
            }

            if (!bytes.Equal(hs.hello.sessionId, hs.serverHello.sessionId))
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: server did not echo the legacy session ID"))!;
            }

            if (hs.serverHello.compressionMethod != compressionNone)
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: server selected unsupported compression format"))!;
            }

            var selectedSuite = mutualCipherSuiteTLS13(hs.hello.cipherSuites, hs.serverHello.cipherSuite);
            if (hs.suite != null && selectedSuite != hs.suite)
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: server changed cipher suite after a HelloRetryRequest"))!;
            }

            if (selectedSuite == null)
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: server chose an unconfigured cipher suite"))!;
            }

            hs.suite = selectedSuite;
            c.cipherSuite = hs.suite.id;

            return error.As(null!)!;

        }

        // sendDummyChangeCipherSpec sends a ChangeCipherSpec record for compatibility
        // with middleboxes that didn't implement TLS correctly. See RFC 8446, Appendix D.4.
        private static error sendDummyChangeCipherSpec(this ptr<clientHandshakeStateTLS13> _addr_hs)
        {
            ref clientHandshakeStateTLS13 hs = ref _addr_hs.val;

            if (hs.sentDummyCCS)
            {
                return error.As(null!)!;
            }

            hs.sentDummyCCS = true;

            var (_, err) = hs.c.writeRecord(recordTypeChangeCipherSpec, new slice<byte>(new byte[] { 1 }));
            return error.As(err)!;

        }

        // processHelloRetryRequest handles the HRR in hs.serverHello, modifies and
        // resends hs.hello, and reads the new ServerHello into hs.serverHello.
        private static error processHelloRetryRequest(this ptr<clientHandshakeStateTLS13> _addr_hs)
        {
            ref clientHandshakeStateTLS13 hs = ref _addr_hs.val;

            var c = hs.c; 

            // The first ClientHello gets double-hashed into the transcript upon a
            // HelloRetryRequest. (The idea is that the server might offload transcript
            // storage to the client in the cookie.) See RFC 8446, Section 4.4.1.
            var chHash = hs.transcript.Sum(null);
            hs.transcript.Reset();
            hs.transcript.Write(new slice<byte>(new byte[] { typeMessageHash, 0, 0, uint8(len(chHash)) }));
            hs.transcript.Write(chHash);
            hs.transcript.Write(hs.serverHello.marshal()); 

            // The only HelloRetryRequest extensions we support are key_share and
            // cookie, and clients must abort the handshake if the HRR would not result
            // in any change in the ClientHello.
            if (hs.serverHello.selectedGroup == 0L && hs.serverHello.cookie == null)
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: server sent an unnecessary HelloRetryRequest message"))!;
            }

            if (hs.serverHello.cookie != null)
            {
                hs.hello.cookie = hs.serverHello.cookie;
            }

            if (hs.serverHello.serverShare.group != 0L)
            {
                c.sendAlert(alertDecodeError);
                return error.As(errors.New("tls: received malformed key_share extension"))!;
            } 

            // If the server sent a key_share extension selecting a group, ensure it's
            // a group we advertised but did not send a key share for, and send a key
            // share for it this time.
            {
                var curveID = hs.serverHello.selectedGroup;

                if (curveID != 0L)
                {
                    var curveOK = false;
                    foreach (var (_, id) in hs.hello.supportedCurves)
                    {
                        if (id == curveID)
                        {
                            curveOK = true;
                            break;
                        }

                    }
                    if (!curveOK)
                    {
                        c.sendAlert(alertIllegalParameter);
                        return error.As(errors.New("tls: server selected unsupported group"))!;
                    }

                    if (hs.ecdheParams.CurveID() == curveID)
                    {
                        c.sendAlert(alertIllegalParameter);
                        return error.As(errors.New("tls: server sent an unnecessary HelloRetryRequest key_share"))!;
                    }

                    {
                        var (_, ok) = curveForCurveID(curveID);

                        if (curveID != X25519 && !ok)
                        {
                            c.sendAlert(alertInternalError);
                            return error.As(errors.New("tls: CurvePreferences includes unsupported curve"))!;
                        }

                    }

                    var (params, err) = generateECDHEParameters(c.config.rand(), curveID);
                    if (err != null)
                    {
                        c.sendAlert(alertInternalError);
                        return error.As(err)!;
                    }

                    hs.ecdheParams = params;
                    hs.hello.keyShares = new slice<keyShare>(new keyShare[] { {group:curveID,data:params.PublicKey()} });

                }

            }


            hs.hello.raw = null;
            if (len(hs.hello.pskIdentities) > 0L)
            {
                var pskSuite = cipherSuiteTLS13ByID(hs.session.cipherSuite);
                if (pskSuite == null)
                {
                    return error.As(c.sendAlert(alertInternalError))!;
                }

                if (pskSuite.hash == hs.suite.hash)
                { 
                    // Update binders and obfuscated_ticket_age.
                    var ticketAge = uint32(c.config.time().Sub(hs.session.receivedAt) / time.Millisecond);
                    hs.hello.pskIdentities[0L].obfuscatedTicketAge = ticketAge + hs.session.ageAdd;

                    var transcript = hs.suite.hash.New();
                    transcript.Write(new slice<byte>(new byte[] { typeMessageHash, 0, 0, uint8(len(chHash)) }));
                    transcript.Write(chHash);
                    transcript.Write(hs.serverHello.marshal());
                    transcript.Write(hs.hello.marshalWithoutBinders());
                    slice<byte> pskBinders = new slice<slice<byte>>(new slice<byte>[] { hs.suite.finishedHash(hs.binderKey,transcript) });
                    hs.hello.updateBinders(pskBinders);

                }
                else
                { 
                    // Server selected a cipher suite incompatible with the PSK.
                    hs.hello.pskIdentities = null;
                    hs.hello.pskBinders = null;

                }

            }

            hs.transcript.Write(hs.hello.marshal());
            {
                var (_, err) = c.writeRecord(recordTypeHandshake, hs.hello.marshal());

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err)!;
            }

            ptr<serverHelloMsg> (serverHello, ok) = msg._<ptr<serverHelloMsg>>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(serverHello, msg))!;
            }

            hs.serverHello = serverHello;

            {
                var err = hs.checkServerHelloOrHRR();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            return error.As(null!)!;

        }

        private static error processServerHello(this ptr<clientHandshakeStateTLS13> _addr_hs)
        {
            ref clientHandshakeStateTLS13 hs = ref _addr_hs.val;

            var c = hs.c;

            if (bytes.Equal(hs.serverHello.random, helloRetryRequestRandom))
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(errors.New("tls: server sent two HelloRetryRequest messages"))!;
            }

            if (len(hs.serverHello.cookie) != 0L)
            {
                c.sendAlert(alertUnsupportedExtension);
                return error.As(errors.New("tls: server sent a cookie in a normal ServerHello"))!;
            }

            if (hs.serverHello.selectedGroup != 0L)
            {
                c.sendAlert(alertDecodeError);
                return error.As(errors.New("tls: malformed key_share extension"))!;
            }

            if (hs.serverHello.serverShare.group == 0L)
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: server did not send a key share"))!;
            }

            if (hs.serverHello.serverShare.group != hs.ecdheParams.CurveID())
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: server selected unsupported group"))!;
            }

            if (!hs.serverHello.selectedIdentityPresent)
            {
                return error.As(null!)!;
            }

            if (int(hs.serverHello.selectedIdentity) >= len(hs.hello.pskIdentities))
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: server selected an invalid PSK"))!;
            }

            if (len(hs.hello.pskIdentities) != 1L || hs.session == null)
            {
                return error.As(c.sendAlert(alertInternalError))!;
            }

            var pskSuite = cipherSuiteTLS13ByID(hs.session.cipherSuite);
            if (pskSuite == null)
            {
                return error.As(c.sendAlert(alertInternalError))!;
            }

            if (pskSuite.hash != hs.suite.hash)
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: server selected an invalid PSK and cipher suite pair"))!;
            }

            hs.usingPSK = true;
            c.didResume = true;
            c.peerCertificates = hs.session.serverCertificates;
            c.verifiedChains = hs.session.verifiedChains;
            c.ocspResponse = hs.session.ocspResponse;
            c.scts = hs.session.scts;
            return error.As(null!)!;

        }

        private static error establishHandshakeKeys(this ptr<clientHandshakeStateTLS13> _addr_hs)
        {
            ref clientHandshakeStateTLS13 hs = ref _addr_hs.val;

            var c = hs.c;

            var sharedKey = hs.ecdheParams.SharedKey(hs.serverHello.serverShare.data);
            if (sharedKey == null)
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: invalid server key share"))!;
            }

            var earlySecret = hs.earlySecret;
            if (!hs.usingPSK)
            {
                earlySecret = hs.suite.extract(null, null);
            }

            var handshakeSecret = hs.suite.extract(sharedKey, hs.suite.deriveSecret(earlySecret, "derived", null));

            var clientSecret = hs.suite.deriveSecret(handshakeSecret, clientHandshakeTrafficLabel, hs.transcript);
            c.@out.setTrafficSecret(hs.suite, clientSecret);
            var serverSecret = hs.suite.deriveSecret(handshakeSecret, serverHandshakeTrafficLabel, hs.transcript);
            c.@in.setTrafficSecret(hs.suite, serverSecret);

            var err = c.config.writeKeyLog(keyLogLabelClientHandshake, hs.hello.random, clientSecret);
            if (err != null)
            {
                c.sendAlert(alertInternalError);
                return error.As(err)!;
            }

            err = c.config.writeKeyLog(keyLogLabelServerHandshake, hs.hello.random, serverSecret);
            if (err != null)
            {
                c.sendAlert(alertInternalError);
                return error.As(err)!;
            }

            hs.masterSecret = hs.suite.extract(null, hs.suite.deriveSecret(handshakeSecret, "derived", null));

            return error.As(null!)!;

        }

        private static error readServerParameters(this ptr<clientHandshakeStateTLS13> _addr_hs)
        {
            ref clientHandshakeStateTLS13 hs = ref _addr_hs.val;

            var c = hs.c;

            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err)!;
            }

            ptr<encryptedExtensionsMsg> (encryptedExtensions, ok) = msg._<ptr<encryptedExtensionsMsg>>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(encryptedExtensions, msg))!;
            }

            hs.transcript.Write(encryptedExtensions.marshal());

            if (len(encryptedExtensions.alpnProtocol) != 0L && len(hs.hello.alpnProtocols) == 0L)
            {
                c.sendAlert(alertUnsupportedExtension);
                return error.As(errors.New("tls: server advertised unrequested ALPN extension"))!;
            }

            c.clientProtocol = encryptedExtensions.alpnProtocol;

            return error.As(null!)!;

        }

        private static error readServerCertificate(this ptr<clientHandshakeStateTLS13> _addr_hs)
        {
            ref clientHandshakeStateTLS13 hs = ref _addr_hs.val;

            var c = hs.c; 

            // Either a PSK or a certificate is always used, but not both.
            // See RFC 8446, Section 4.1.1.
            if (hs.usingPSK)
            { 
                // Make sure the connection is still being verified whether or not this
                // is a resumption. Resumptions currently don't reverify certificates so
                // they don't call verifyServerCertificate. See Issue 31641.
                if (c.config.VerifyConnection != null)
                {
                    {
                        var err__prev3 = err;

                        var err = c.config.VerifyConnection(c.connectionStateLocked());

                        if (err != null)
                        {
                            c.sendAlert(alertBadCertificate);
                            return error.As(err)!;
                        }

                        err = err__prev3;

                    }

                }

                return error.As(null!)!;

            }

            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err)!;
            }

            ptr<certificateRequestMsgTLS13> (certReq, ok) = msg._<ptr<certificateRequestMsgTLS13>>();
            if (ok)
            {
                hs.transcript.Write(certReq.marshal());

                hs.certReq = certReq;

                msg, err = c.readHandshake();
                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            ptr<certificateMsgTLS13> (certMsg, ok) = msg._<ptr<certificateMsgTLS13>>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(certMsg, msg))!;
            }

            if (len(certMsg.certificate.Certificate) == 0L)
            {
                c.sendAlert(alertDecodeError);
                return error.As(errors.New("tls: received empty certificates message"))!;
            }

            hs.transcript.Write(certMsg.marshal());

            c.scts = certMsg.certificate.SignedCertificateTimestamps;
            c.ocspResponse = certMsg.certificate.OCSPStaple;

            {
                var err__prev1 = err;

                err = c.verifyServerCertificate(certMsg.certificate.Certificate);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            msg, err = c.readHandshake();
            if (err != null)
            {
                return error.As(err)!;
            }

            ptr<certificateVerifyMsg> (certVerify, ok) = msg._<ptr<certificateVerifyMsg>>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(certVerify, msg))!;
            } 

            // See RFC 8446, Section 4.4.3.
            if (!isSupportedSignatureAlgorithm(certVerify.signatureAlgorithm, supportedSignatureAlgorithms))
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: certificate used with invalid signature algorithm"))!;
            }

            var (sigType, sigHash, err) = typeAndHashFromSignatureScheme(certVerify.signatureAlgorithm);
            if (err != null)
            {
                return error.As(c.sendAlert(alertInternalError))!;
            }

            if (sigType == signaturePKCS1v15 || sigHash == crypto.SHA1)
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: certificate used with invalid signature algorithm"))!;
            }

            var signed = signedMessage(sigHash, serverSignatureContext, hs.transcript);
            {
                var err__prev1 = err;

                err = verifyHandshakeSignature(sigType, c.peerCertificates[0L].PublicKey, sigHash, signed, certVerify.signature);

                if (err != null)
                {
                    c.sendAlert(alertDecryptError);
                    return error.As(errors.New("tls: invalid signature by the server certificate: " + err.Error()))!;
                }

                err = err__prev1;

            }


            hs.transcript.Write(certVerify.marshal());

            return error.As(null!)!;

        }

        private static error readServerFinished(this ptr<clientHandshakeStateTLS13> _addr_hs)
        {
            ref clientHandshakeStateTLS13 hs = ref _addr_hs.val;

            var c = hs.c;

            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err)!;
            }

            ptr<finishedMsg> (finished, ok) = msg._<ptr<finishedMsg>>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(finished, msg))!;
            }

            var expectedMAC = hs.suite.finishedHash(c.@in.trafficSecret, hs.transcript);
            if (!hmac.Equal(expectedMAC, finished.verifyData))
            {
                c.sendAlert(alertDecryptError);
                return error.As(errors.New("tls: invalid server finished hash"))!;
            }

            hs.transcript.Write(finished.marshal()); 

            // Derive secrets that take context through the server Finished.

            hs.trafficSecret = hs.suite.deriveSecret(hs.masterSecret, clientApplicationTrafficLabel, hs.transcript);
            var serverSecret = hs.suite.deriveSecret(hs.masterSecret, serverApplicationTrafficLabel, hs.transcript);
            c.@in.setTrafficSecret(hs.suite, serverSecret);

            err = c.config.writeKeyLog(keyLogLabelClientTraffic, hs.hello.random, hs.trafficSecret);
            if (err != null)
            {
                c.sendAlert(alertInternalError);
                return error.As(err)!;
            }

            err = c.config.writeKeyLog(keyLogLabelServerTraffic, hs.hello.random, serverSecret);
            if (err != null)
            {
                c.sendAlert(alertInternalError);
                return error.As(err)!;
            }

            c.ekm = hs.suite.exportKeyingMaterial(hs.masterSecret, hs.transcript);

            return error.As(null!)!;

        }

        private static error sendClientCertificate(this ptr<clientHandshakeStateTLS13> _addr_hs)
        {
            ref clientHandshakeStateTLS13 hs = ref _addr_hs.val;

            var c = hs.c;

            if (hs.certReq == null)
            {
                return error.As(null!)!;
            }

            var (cert, err) = c.getClientCertificate(addr(new CertificateRequestInfo(AcceptableCAs:hs.certReq.certificateAuthorities,SignatureSchemes:hs.certReq.supportedSignatureAlgorithms,Version:c.vers,)));
            if (err != null)
            {
                return error.As(err)!;
            }

            ptr<certificateMsgTLS13> certMsg = @new<certificateMsgTLS13>();

            certMsg.certificate = cert.val;
            certMsg.scts = hs.certReq.scts && len(cert.SignedCertificateTimestamps) > 0L;
            certMsg.ocspStapling = hs.certReq.ocspStapling && len(cert.OCSPStaple) > 0L;

            hs.transcript.Write(certMsg.marshal());
            {
                var (_, err) = c.writeRecord(recordTypeHandshake, certMsg.marshal());

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // If we sent an empty certificate message, skip the CertificateVerify.

            } 

            // If we sent an empty certificate message, skip the CertificateVerify.
            if (len(cert.Certificate) == 0L)
            {
                return error.As(null!)!;
            }

            ptr<certificateVerifyMsg> certVerifyMsg = @new<certificateVerifyMsg>();
            certVerifyMsg.hasSignatureAlgorithm = true;

            certVerifyMsg.signatureAlgorithm, err = selectSignatureScheme(c.vers, cert, hs.certReq.supportedSignatureAlgorithms);
            if (err != null)
            { 
                // getClientCertificate returned a certificate incompatible with the
                // CertificateRequestInfo supported signature algorithms.
                c.sendAlert(alertHandshakeFailure);
                return error.As(err)!;

            }

            var (sigType, sigHash, err) = typeAndHashFromSignatureScheme(certVerifyMsg.signatureAlgorithm);
            if (err != null)
            {
                return error.As(c.sendAlert(alertInternalError))!;
            }

            var signed = signedMessage(sigHash, clientSignatureContext, hs.transcript);
            var signOpts = crypto.SignerOpts(sigHash);
            if (sigType == signatureRSAPSS)
            {
                signOpts = addr(new rsa.PSSOptions(SaltLength:rsa.PSSSaltLengthEqualsHash,Hash:sigHash));
            }

            crypto.Signer (sig, err) = cert.PrivateKey._<crypto.Signer>().Sign(c.config.rand(), signed, signOpts);
            if (err != null)
            {
                c.sendAlert(alertInternalError);
                return error.As(errors.New("tls: failed to sign handshake: " + err.Error()))!;
            }

            certVerifyMsg.signature = sig;

            hs.transcript.Write(certVerifyMsg.marshal());
            {
                (_, err) = c.writeRecord(recordTypeHandshake, certVerifyMsg.marshal());

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            return error.As(null!)!;

        }

        private static error sendClientFinished(this ptr<clientHandshakeStateTLS13> _addr_hs)
        {
            ref clientHandshakeStateTLS13 hs = ref _addr_hs.val;

            var c = hs.c;

            ptr<finishedMsg> finished = addr(new finishedMsg(verifyData:hs.suite.finishedHash(c.out.trafficSecret,hs.transcript),));

            hs.transcript.Write(finished.marshal());
            {
                var (_, err) = c.writeRecord(recordTypeHandshake, finished.marshal());

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            c.@out.setTrafficSecret(hs.suite, hs.trafficSecret);

            if (!c.config.SessionTicketsDisabled && c.config.ClientSessionCache != null)
            {
                c.resumptionSecret = hs.suite.deriveSecret(hs.masterSecret, resumptionLabel, hs.transcript);
            }

            return error.As(null!)!;

        }

        private static error handleNewSessionTicket(this ptr<Conn> _addr_c, ptr<newSessionTicketMsgTLS13> _addr_msg)
        {
            ref Conn c = ref _addr_c.val;
            ref newSessionTicketMsgTLS13 msg = ref _addr_msg.val;

            if (!c.isClient)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(errors.New("tls: received new session ticket from a client"))!;
            }

            if (c.config.SessionTicketsDisabled || c.config.ClientSessionCache == null)
            {
                return error.As(null!)!;
            } 

            // See RFC 8446, Section 4.6.1.
            if (msg.lifetime == 0L)
            {
                return error.As(null!)!;
            }

            var lifetime = time.Duration(msg.lifetime) * time.Second;
            if (lifetime > maxSessionTicketLifetime)
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: received a session ticket with invalid lifetime"))!;
            }

            var cipherSuite = cipherSuiteTLS13ByID(c.cipherSuite);
            if (cipherSuite == null || c.resumptionSecret == null)
            {
                return error.As(c.sendAlert(alertInternalError))!;
            } 

            // Save the resumption_master_secret and nonce instead of deriving the PSK
            // to do the least amount of work on NewSessionTicket messages before we
            // know if the ticket will be used. Forward secrecy of resumed connections
            // is guaranteed by the requirement for pskModeDHE.
            ptr<ClientSessionState> session = addr(new ClientSessionState(sessionTicket:msg.label,vers:c.vers,cipherSuite:c.cipherSuite,masterSecret:c.resumptionSecret,serverCertificates:c.peerCertificates,verifiedChains:c.verifiedChains,receivedAt:c.config.time(),nonce:msg.nonce,useBy:c.config.time().Add(lifetime),ageAdd:msg.ageAdd,ocspResponse:c.ocspResponse,scts:c.scts,));

            var cacheKey = clientSessionCacheKey(c.conn.RemoteAddr(), c.config);
            c.config.ClientSessionCache.Put(cacheKey, session);

            return error.As(null!)!;

        }
    }
}}
