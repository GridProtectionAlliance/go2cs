// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 August 29 08:31:30 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\handshake_server.go
using crypto = go.crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using rsa = go.crypto.rsa_package;
using subtle = go.crypto.subtle_package;
using x509 = go.crypto.x509_package;
using asn1 = go.encoding.asn1_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using static go.builtin;
using System;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        // serverHandshakeState contains details of a server handshake in progress.
        // It's discarded once the handshake has completed.
        private partial struct serverHandshakeState
        {
            public ptr<Conn> c;
            public ptr<clientHelloMsg> clientHello;
            public ptr<serverHelloMsg> hello;
            public ptr<cipherSuite> suite;
            public bool ellipticOk;
            public bool ecdsaOk;
            public bool rsaDecryptOk;
            public bool rsaSignOk;
            public ptr<sessionState> sessionState;
            public finishedHash finishedHash;
            public slice<byte> masterSecret;
            public slice<slice<byte>> certsFromClient;
            public ptr<Certificate> cert;
            public ptr<ClientHelloInfo> cachedClientHelloInfo;
        }

        // serverHandshake performs a TLS handshake as a server.
        // c.out.Mutex <= L; c.handshakeMutex <= L.
        private static error serverHandshake(this ref Conn c)
        { 
            // If this is the first server handshake, we generate a random key to
            // encrypt the tickets with.
            c.config.serverInitOnce.Do(() =>
            {
                c.config.serverInit(null);

            });

            serverHandshakeState hs = new serverHandshakeState(c:c,);
            var (isResume, err) = hs.readClientHello();
            if (err != null)
            {
                return error.As(err);
            } 

            // For an overview of TLS handshaking, see https://tools.ietf.org/html/rfc5246#section-7.3
            c.buffering = true;
            if (isResume)
            { 
                // The client has included a session ticket and so we do an abbreviated handshake.
                {
                    var err__prev2 = err;

                    var err = hs.doResumeHandshake();

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
                    // ticketSupported is set in a resumption handshake if the
                    // ticket from the client was encrypted with an old session
                    // ticket key and thus a refreshed ticket should be sent.

                    err = err__prev2;

                } 
                // ticketSupported is set in a resumption handshake if the
                // ticket from the client was encrypted with an old session
                // ticket key and thus a refreshed ticket should be sent.
                if (hs.hello.ticketSupported)
                {
                    {
                        var err__prev3 = err;

                        err = hs.sendSessionTicket();

                        if (err != null)
                        {
                            return error.As(err);
                        }

                        err = err__prev3;

                    }
                }
                {
                    var err__prev2 = err;

                    err = hs.sendFinished(c.serverFinished[..]);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                {
                    var err__prev2 = err;

                    var (_, err) = c.flush();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                c.clientFinishedIsFirst = false;
                {
                    var err__prev2 = err;

                    err = hs.readFinished(null);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                c.didResume = true;
            }
            else
            { 
                // The client didn't include a session ticket, or it wasn't
                // valid so we do a full handshake.
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

                    err = hs.readFinished(c.clientFinished[..]);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                c.clientFinishedIsFirst = true;
                c.buffering = true;
                {
                    var err__prev2 = err;

                    err = hs.sendSessionTicket();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                {
                    var err__prev2 = err;

                    err = hs.sendFinished(null);

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
            c.handshakeComplete = true;

            return error.As(null);
        }

        // readClientHello reads a ClientHello message from the client and decides
        // whether we will perform session resumption.
        private static (bool, error) readClientHello(this ref serverHandshakeState hs)
        {
            var c = hs.c;

            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return (false, err);
            }
            bool ok = default;
            hs.clientHello, ok = msg._<ref clientHelloMsg>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return (false, unexpectedMessageError(hs.clientHello, msg));
            }
            if (c.config.GetConfigForClient != null)
            {
                {
                    var (newConfig, err) = c.config.GetConfigForClient(hs.clientHelloInfo());

                    if (err != null)
                    {
                        c.sendAlert(alertInternalError);
                        return (false, err);
                    }
                    else if (newConfig != null)
                    {
                        newConfig.serverInitOnce.Do(() =>
                        {
                            newConfig.serverInit(c.config);

                        });
                        c.config = newConfig;
                    }

                }
            }
            c.vers, ok = c.config.mutualVersion(hs.clientHello.vers);
            if (!ok)
            {
                c.sendAlert(alertProtocolVersion);
                return (false, fmt.Errorf("tls: client offered an unsupported, maximum protocol version of %x", hs.clientHello.vers));
            }
            c.haveVers = true;

            hs.hello = @new<serverHelloMsg>();

            var supportedCurve = false;
            var preferredCurves = c.config.curvePreferences();
Curves:

            foreach (var (_, curve) in hs.clientHello.supportedCurves)
            {
                foreach (var (_, supported) in preferredCurves)
                {
                    if (supported == curve)
                    {
                        supportedCurve = true;
                        _breakCurves = true;
                        break;
                    }
                }
            }
            var supportedPointFormat = false;
            foreach (var (_, pointFormat) in hs.clientHello.supportedPoints)
            {
                if (pointFormat == pointFormatUncompressed)
                {
                    supportedPointFormat = true;
                    break;
                }
            }
            hs.ellipticOk = supportedCurve && supportedPointFormat;

            var foundCompression = false; 
            // We only support null compression, so check that the client offered it.
            foreach (var (_, compression) in hs.clientHello.compressionMethods)
            {
                if (compression == compressionNone)
                {
                    foundCompression = true;
                    break;
                }
            }
            if (!foundCompression)
            {
                c.sendAlert(alertHandshakeFailure);
                return (false, errors.New("tls: client does not support uncompressed connections"));
            }
            hs.hello.vers = c.vers;
            hs.hello.random = make_slice<byte>(32L);
            _, err = io.ReadFull(c.config.rand(), hs.hello.random);
            if (err != null)
            {
                c.sendAlert(alertInternalError);
                return (false, err);
            }
            if (len(hs.clientHello.secureRenegotiation) != 0L)
            {
                c.sendAlert(alertHandshakeFailure);
                return (false, errors.New("tls: initial handshake had non-empty renegotiation extension"));
            }
            hs.hello.secureRenegotiationSupported = hs.clientHello.secureRenegotiationSupported;
            hs.hello.compressionMethod = compressionNone;
            if (len(hs.clientHello.serverName) > 0L)
            {
                c.serverName = hs.clientHello.serverName;
            }
            if (len(hs.clientHello.alpnProtocols) > 0L)
            {
                {
                    var (selectedProto, fallback) = mutualProtocol(hs.clientHello.alpnProtocols, c.config.NextProtos);

                    if (!fallback)
                    {
                        hs.hello.alpnProtocol = selectedProto;
                        c.clientProtocol = selectedProto;
                    }

                }
            }
            else
            { 
                // Although sending an empty NPN extension is reasonable, Firefox has
                // had a bug around this. Best to send nothing at all if
                // c.config.NextProtos is empty. See
                // https://golang.org/issue/5445.
                if (hs.clientHello.nextProtoNeg && len(c.config.NextProtos) > 0L)
                {
                    hs.hello.nextProtoNeg = true;
                    hs.hello.nextProtos = c.config.NextProtos;
                }
            }
            hs.cert, err = c.config.getCertificate(hs.clientHelloInfo());
            if (err != null)
            {
                c.sendAlert(alertInternalError);
                return (false, err);
            }
            if (hs.clientHello.scts)
            {
                hs.hello.scts = hs.cert.SignedCertificateTimestamps;
            }
            {
                crypto.Signer priv__prev1 = priv;

                crypto.Signer (priv, ok) = hs.cert.PrivateKey._<crypto.Signer>();

                if (ok)
                {
                    switch (priv.Public().type())
                    {
                        case ref ecdsa.PublicKey _:
                            hs.ecdsaOk = true;
                            break;
                        case ref rsa.PublicKey _:
                            hs.rsaSignOk = true;
                            break;
                        default:
                        {
                            c.sendAlert(alertInternalError);
                            return (false, fmt.Errorf("tls: unsupported signing key type (%T)", priv.Public()));
                            break;
                        }
                    }
                }

                priv = priv__prev1;

            }
            {
                crypto.Signer priv__prev1 = priv;

                (priv, ok) = hs.cert.PrivateKey._<crypto.Decrypter>();

                if (ok)
                {
                    switch (priv.Public().type())
                    {
                        case ref rsa.PublicKey _:
                            hs.rsaDecryptOk = true;
                            break;
                        default:
                        {
                            c.sendAlert(alertInternalError);
                            return (false, fmt.Errorf("tls: unsupported decryption key type (%T)", priv.Public()));
                            break;
                        }
                    }
                }

                priv = priv__prev1;

            }

            if (hs.checkForResumption())
            {
                return (true, null);
            }
            slice<ushort> preferenceList = default;            slice<ushort> supportedList = default;

            if (c.config.PreferServerCipherSuites)
            {
                preferenceList = c.config.cipherSuites();
                supportedList = hs.clientHello.cipherSuites;
            }
            else
            {
                preferenceList = hs.clientHello.cipherSuites;
                supportedList = c.config.cipherSuites();
            }
            {
                var id__prev1 = id;

                foreach (var (_, __id) in preferenceList)
                {
                    id = __id;
                    if (hs.setCipherSuite(id, supportedList, c.vers))
                    {
                        break;
                    }
                }

                id = id__prev1;
            }

            if (hs.suite == null)
            {
                c.sendAlert(alertHandshakeFailure);
                return (false, errors.New("tls: no cipher suite supported by both client and server"));
            } 

            // See https://tools.ietf.org/html/rfc7507.
            {
                var id__prev1 = id;

                foreach (var (_, __id) in hs.clientHello.cipherSuites)
                {
                    id = __id;
                    if (id == TLS_FALLBACK_SCSV)
                    { 
                        // The client is doing a fallback connection.
                        if (hs.clientHello.vers < c.config.maxVersion())
                        {
                            c.sendAlert(alertInappropriateFallback);
                            return (false, errors.New("tls: client using inappropriate protocol fallback"));
                        }
                        break;
                    }
                }

                id = id__prev1;
            }

            return (false, null);
        }

        // checkForResumption reports whether we should perform resumption on this connection.
        private static bool checkForResumption(this ref serverHandshakeState hs)
        {
            var c = hs.c;

            if (c.config.SessionTicketsDisabled)
            {
                return false;
            }
            bool ok = default;
            var sessionTicket = append(new slice<byte>(new byte[] {  }), hs.clientHello.sessionTicket);
            hs.sessionState, ok = c.decryptTicket(sessionTicket);

            if (!ok)
            {
                return false;
            } 

            // Never resume a session for a different TLS version.
            if (c.vers != hs.sessionState.vers)
            {
                return false;
            }
            var cipherSuiteOk = false; 
            // Check that the client is still offering the ciphersuite in the session.
            foreach (var (_, id) in hs.clientHello.cipherSuites)
            {
                if (id == hs.sessionState.cipherSuite)
                {
                    cipherSuiteOk = true;
                    break;
                }
            }
            if (!cipherSuiteOk)
            {
                return false;
            } 

            // Check that we also support the ciphersuite from the session.
            if (!hs.setCipherSuite(hs.sessionState.cipherSuite, c.config.cipherSuites(), hs.sessionState.vers))
            {
                return false;
            }
            var sessionHasClientCerts = len(hs.sessionState.certificates) != 0L;
            var needClientCerts = c.config.ClientAuth == RequireAnyClientCert || c.config.ClientAuth == RequireAndVerifyClientCert;
            if (needClientCerts && !sessionHasClientCerts)
            {
                return false;
            }
            if (sessionHasClientCerts && c.config.ClientAuth == NoClientCert)
            {
                return false;
            }
            return true;
        }

        private static error doResumeHandshake(this ref serverHandshakeState hs)
        {
            var c = hs.c;

            hs.hello.cipherSuite = hs.suite.id; 
            // We echo the client's session ID in the ServerHello to let it know
            // that we're doing a resumption.
            hs.hello.sessionId = hs.clientHello.sessionId;
            hs.hello.ticketSupported = hs.sessionState.usedOldKey;
            hs.finishedHash = newFinishedHash(c.vers, hs.suite);
            hs.finishedHash.discardHandshakeBuffer();
            hs.finishedHash.Write(hs.clientHello.marshal());
            hs.finishedHash.Write(hs.hello.marshal());
            {
                var (_, err) = c.writeRecord(recordTypeHandshake, hs.hello.marshal());

                if (err != null)
                {
                    return error.As(err);
                }

            }

            if (len(hs.sessionState.certificates) > 0L)
            {
                {
                    (_, err) = hs.processCertsFromClient(hs.sessionState.certificates);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }
            hs.masterSecret = hs.sessionState.masterSecret;

            return error.As(null);
        }

        private static error doFullHandshake(this ref serverHandshakeState hs)
        {
            var c = hs.c;

            if (hs.clientHello.ocspStapling && len(hs.cert.OCSPStaple) > 0L)
            {
                hs.hello.ocspStapling = true;
            }
            hs.hello.ticketSupported = hs.clientHello.ticketSupported && !c.config.SessionTicketsDisabled;
            hs.hello.cipherSuite = hs.suite.id;

            hs.finishedHash = newFinishedHash(hs.c.vers, hs.suite);
            if (c.config.ClientAuth == NoClientCert)
            { 
                // No need to keep a full record of the handshake if client
                // certificates won't be used.
                hs.finishedHash.discardHandshakeBuffer();
            }
            hs.finishedHash.Write(hs.clientHello.marshal());
            hs.finishedHash.Write(hs.hello.marshal());
            {
                var (_, err) = c.writeRecord(recordTypeHandshake, hs.hello.marshal());

                if (err != null)
                {
                    return error.As(err);
                }

            }

            ptr<object> certMsg = @new<certificateMsg>();
            certMsg.certificates = hs.cert.Certificate;
            hs.finishedHash.Write(certMsg.marshal());
            {
                (_, err) = c.writeRecord(recordTypeHandshake, certMsg.marshal());

                if (err != null)
                {
                    return error.As(err);
                }

            }

            if (hs.hello.ocspStapling)
            {
                ptr<object> certStatus = @new<certificateStatusMsg>();
                certStatus.statusType = statusTypeOCSP;
                certStatus.response = hs.cert.OCSPStaple;
                hs.finishedHash.Write(certStatus.marshal());
                {
                    (_, err) = c.writeRecord(recordTypeHandshake, certStatus.marshal());

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }
            var keyAgreement = hs.suite.ka(c.vers);
            var (skx, err) = keyAgreement.generateServerKeyExchange(c.config, hs.cert, hs.clientHello, hs.hello);
            if (err != null)
            {
                c.sendAlert(alertHandshakeFailure);
                return error.As(err);
            }
            if (skx != null)
            {
                hs.finishedHash.Write(skx.marshal());
                {
                    (_, err) = c.writeRecord(recordTypeHandshake, skx.marshal());

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }
            if (c.config.ClientAuth >= RequestClientCert)
            { 
                // Request a client certificate
                ptr<object> certReq = @new<certificateRequestMsg>();
                certReq.certificateTypes = new slice<byte>(new byte[] { byte(certTypeRSASign), byte(certTypeECDSASign) });
                if (c.vers >= VersionTLS12)
                {
                    certReq.hasSignatureAndHash = true;
                    certReq.supportedSignatureAlgorithms = supportedSignatureAlgorithms;
                } 

                // An empty list of certificateAuthorities signals to
                // the client that it may send any certificate in response
                // to our request. When we know the CAs we trust, then
                // we can send them down, so that the client can choose
                // an appropriate certificate to give to us.
                if (c.config.ClientCAs != null)
                {
                    certReq.certificateAuthorities = c.config.ClientCAs.Subjects();
                }
                hs.finishedHash.Write(certReq.marshal());
                {
                    (_, err) = c.writeRecord(recordTypeHandshake, certReq.marshal());

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }
            ptr<object> helloDone = @new<serverHelloDoneMsg>();
            hs.finishedHash.Write(helloDone.marshal());
            {
                (_, err) = c.writeRecord(recordTypeHandshake, helloDone.marshal());

                if (err != null)
                {
                    return error.As(err);
                }

            }

            {
                (_, err) = c.flush();

                if (err != null)
                {
                    return error.As(err);
                }

            }

            crypto.PublicKey pub = default; // public key for client auth, if any

            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err);
            }
            bool ok = default; 
            // If we requested a client certificate, then the client must send a
            // certificate message, even if it's empty.
            if (c.config.ClientAuth >= RequestClientCert)
            {
                certMsg, ok = msg._<ref certificateMsg>();

                if (!ok)
                {
                    c.sendAlert(alertUnexpectedMessage);
                    return error.As(unexpectedMessageError(certMsg, msg));
                }
                hs.finishedHash.Write(certMsg.marshal());

                if (len(certMsg.certificates) == 0L)
                { 
                    // The client didn't actually send a certificate

                    if (c.config.ClientAuth == RequireAnyClientCert || c.config.ClientAuth == RequireAndVerifyClientCert) 
                        c.sendAlert(alertBadCertificate);
                        return error.As(errors.New("tls: client didn't provide a certificate"));
                                    }
                pub, err = hs.processCertsFromClient(certMsg.certificates);
                if (err != null)
                {
                    return error.As(err);
                }
                msg, err = c.readHandshake();
                if (err != null)
                {
                    return error.As(err);
                }
            } 

            // Get client key exchange
            ref clientKeyExchangeMsg (ckx, ok) = msg._<ref clientKeyExchangeMsg>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(ckx, msg));
            }
            hs.finishedHash.Write(ckx.marshal());

            var (preMasterSecret, err) = keyAgreement.processClientKeyExchange(c.config, hs.cert, ckx, c.vers);
            if (err != null)
            {
                c.sendAlert(alertHandshakeFailure);
                return error.As(err);
            }
            hs.masterSecret = masterFromPreMasterSecret(c.vers, hs.suite, preMasterSecret, hs.clientHello.random, hs.hello.random);
            {
                var err = c.config.writeKeyLog(hs.clientHello.random, hs.masterSecret);

                if (err != null)
                {
                    c.sendAlert(alertInternalError);
                    return error.As(err);
                } 

                // If we received a client cert in response to our certificate request message,
                // the client will send us a certificateVerifyMsg immediately after the
                // clientKeyExchangeMsg. This message is a digest of all preceding
                // handshake-layer messages that is signed using the private key corresponding
                // to the client's certificate. This allows us to verify that the client is in
                // possession of the private key of the certificate.

            } 

            // If we received a client cert in response to our certificate request message,
            // the client will send us a certificateVerifyMsg immediately after the
            // clientKeyExchangeMsg. This message is a digest of all preceding
            // handshake-layer messages that is signed using the private key corresponding
            // to the client's certificate. This allows us to verify that the client is in
            // possession of the private key of the certificate.
            if (len(c.peerCertificates) > 0L)
            {
                msg, err = c.readHandshake();
                if (err != null)
                {
                    return error.As(err);
                }
                ref certificateVerifyMsg (certVerify, ok) = msg._<ref certificateVerifyMsg>();
                if (!ok)
                {
                    c.sendAlert(alertUnexpectedMessage);
                    return error.As(unexpectedMessageError(certVerify, msg));
                } 

                // Determine the signature type.
                SignatureScheme signatureAlgorithm = default;
                byte sigType = default;
                if (certVerify.hasSignatureAndHash)
                {
                    signatureAlgorithm = certVerify.signatureAlgorithm;
                    if (!isSupportedSignatureAlgorithm(signatureAlgorithm, supportedSignatureAlgorithms))
                    {
                        return error.As(errors.New("tls: unsupported hash function for client certificate"));
                    }
                    sigType = signatureFromSignatureScheme(signatureAlgorithm);
                }
                else
                { 
                    // Before TLS 1.2 the signature algorithm was implicit
                    // from the key type, and only one hash per signature
                    // algorithm was possible. Leave signatureAlgorithm
                    // unset.
                    switch (pub.type())
                    {
                        case ref ecdsa.PublicKey _:
                            sigType = signatureECDSA;
                            break;
                        case ref rsa.PublicKey _:
                            sigType = signatureRSA;
                            break;
                    }
                }
                switch (pub.type())
                {
                    case ref ecdsa.PublicKey key:
                        if (sigType != signatureECDSA)
                        {
                            err = errors.New("tls: bad signature type for client's ECDSA certificate");
                            break;
                        }
                        ptr<object> ecdsaSig = @new<ecdsaSignature>();
                        _, err = asn1.Unmarshal(certVerify.signature, ecdsaSig);

                        if (err != null)
                        {
                            break;
                        }
                        if (ecdsaSig.R.Sign() <= 0L || ecdsaSig.S.Sign() <= 0L)
                        {
                            err = errors.New("tls: ECDSA signature contained zero or negative values");
                            break;
                        }
                        slice<byte> digest = default;
                        digest, _, err = hs.finishedHash.hashForClientCertificate(sigType, signatureAlgorithm, hs.masterSecret);

                        if (err != null)
                        {
                            break;
                        }
                        if (!ecdsa.Verify(key, digest, ecdsaSig.R, ecdsaSig.S))
                        {
                            err = errors.New("tls: ECDSA verification failure");
                        }
                        break;
                    case ref rsa.PublicKey key:
                        if (sigType != signatureRSA)
                        {
                            err = errors.New("tls: bad signature type for client's RSA certificate");
                            break;
                        }
                        digest = default;
                        crypto.Hash hashFunc = default;
                        digest, hashFunc, err = hs.finishedHash.hashForClientCertificate(sigType, signatureAlgorithm, hs.masterSecret);

                        if (err != null)
                        {
                            break;
                        }
                        err = rsa.VerifyPKCS1v15(key, hashFunc, digest, certVerify.signature);
                        break;
                }
                if (err != null)
                {
                    c.sendAlert(alertBadCertificate);
                    return error.As(errors.New("tls: could not validate signature of connection nonces: " + err.Error()));
                }
                hs.finishedHash.Write(certVerify.marshal());
            }
            hs.finishedHash.discardHandshakeBuffer();

            return error.As(null);
        }

        private static error establishKeys(this ref serverHandshakeState hs)
        {
            var c = hs.c;

            var (clientMAC, serverMAC, clientKey, serverKey, clientIV, serverIV) = keysFromMasterSecret(c.vers, hs.suite, hs.masterSecret, hs.clientHello.random, hs.hello.random, hs.suite.macLen, hs.suite.keyLen, hs.suite.ivLen);

            var clientCipher = default;            var serverCipher = default;

            macFunction clientHash = default;            macFunction serverHash = default;



            if (hs.suite.aead == null)
            {
                clientCipher = hs.suite.cipher(clientKey, clientIV, true);
                clientHash = hs.suite.mac(c.vers, clientMAC);
                serverCipher = hs.suite.cipher(serverKey, serverIV, false);
                serverHash = hs.suite.mac(c.vers, serverMAC);
            }
            else
            {
                clientCipher = hs.suite.aead(clientKey, clientIV);
                serverCipher = hs.suite.aead(serverKey, serverIV);
            }
            c.@in.prepareCipherSpec(c.vers, clientCipher, clientHash);
            c.@out.prepareCipherSpec(c.vers, serverCipher, serverHash);

            return error.As(null);
        }

        private static error readFinished(this ref serverHandshakeState hs, slice<byte> @out)
        {
            var c = hs.c;

            c.readRecord(recordTypeChangeCipherSpec);
            if (c.@in.err != null)
            {
                return error.As(c.@in.err);
            }
            if (hs.hello.nextProtoNeg)
            {
                var (msg, err) = c.readHandshake();
                if (err != null)
                {
                    return error.As(err);
                }
                ref nextProtoMsg (nextProto, ok) = msg._<ref nextProtoMsg>();
                if (!ok)
                {
                    c.sendAlert(alertUnexpectedMessage);
                    return error.As(unexpectedMessageError(nextProto, msg));
                }
                hs.finishedHash.Write(nextProto.marshal());
                c.clientProtocol = nextProto.proto;
            }
            (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err);
            }
            ref finishedMsg (clientFinished, ok) = msg._<ref finishedMsg>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(clientFinished, msg));
            }
            var verify = hs.finishedHash.clientSum(hs.masterSecret);
            if (len(verify) != len(clientFinished.verifyData) || subtle.ConstantTimeCompare(verify, clientFinished.verifyData) != 1L)
            {
                c.sendAlert(alertHandshakeFailure);
                return error.As(errors.New("tls: client's Finished message is incorrect"));
            }
            hs.finishedHash.Write(clientFinished.marshal());
            copy(out, verify);
            return error.As(null);
        }

        private static error sendSessionTicket(this ref serverHandshakeState hs)
        {
            if (!hs.hello.ticketSupported)
            {
                return error.As(null);
            }
            var c = hs.c;
            ptr<object> m = @new<newSessionTicketMsg>();

            error err = default;
            sessionState state = new sessionState(vers:c.vers,cipherSuite:hs.suite.id,masterSecret:hs.masterSecret,certificates:hs.certsFromClient,);
            m.ticket, err = c.encryptTicket(ref state);
            if (err != null)
            {
                return error.As(err);
            }
            hs.finishedHash.Write(m.marshal());
            {
                var (_, err) = c.writeRecord(recordTypeHandshake, m.marshal());

                if (err != null)
                {
                    return error.As(err);
                }

            }

            return error.As(null);
        }

        private static error sendFinished(this ref serverHandshakeState hs, slice<byte> @out)
        {
            var c = hs.c;

            {
                var (_, err) = c.writeRecord(recordTypeChangeCipherSpec, new slice<byte>(new byte[] { 1 }));

                if (err != null)
                {
                    return error.As(err);
                }

            }

            ptr<finishedMsg> finished = @new<finishedMsg>();
            finished.verifyData = hs.finishedHash.serverSum(hs.masterSecret);
            hs.finishedHash.Write(finished.marshal());
            {
                (_, err) = c.writeRecord(recordTypeHandshake, finished.marshal());

                if (err != null)
                {
                    return error.As(err);
                }

            }

            c.cipherSuite = hs.suite.id;
            copy(out, finished.verifyData);

            return error.As(null);
        }

        // processCertsFromClient takes a chain of client certificates either from a
        // Certificates message or from a sessionState and verifies them. It returns
        // the public key of the leaf certificate.
        private static (crypto.PublicKey, error) processCertsFromClient(this ref serverHandshakeState hs, slice<slice<byte>> certificates)
        {
            var c = hs.c;

            hs.certsFromClient = certificates;
            var certs = make_slice<ref x509.Certificate>(len(certificates));
            error err = default;
            foreach (var (i, asn1Data) in certificates)
            {
                certs[i], err = x509.ParseCertificate(asn1Data);

                if (err != null)
                {
                    c.sendAlert(alertBadCertificate);
                    return (null, errors.New("tls: failed to parse client certificate: " + err.Error()));
                }
            }
            if (c.config.ClientAuth >= VerifyClientCertIfGiven && len(certs) > 0L)
            {
                x509.VerifyOptions opts = new x509.VerifyOptions(Roots:c.config.ClientCAs,CurrentTime:c.config.time(),Intermediates:x509.NewCertPool(),KeyUsages:[]x509.ExtKeyUsage{x509.ExtKeyUsageClientAuth},);

                foreach (var (_, cert) in certs[1L..])
                {
                    opts.Intermediates.AddCert(cert);
                }
                var (chains, err) = certs[0L].Verify(opts);
                if (err != null)
                {
                    c.sendAlert(alertBadCertificate);
                    return (null, errors.New("tls: failed to verify client's certificate: " + err.Error()));
                }
                c.verifiedChains = chains;
            }
            if (c.config.VerifyPeerCertificate != null)
            {
                {
                    error err__prev2 = err;

                    err = c.config.VerifyPeerCertificate(certificates, c.verifiedChains);

                    if (err != null)
                    {
                        c.sendAlert(alertBadCertificate);
                        return (null, err);
                    }

                    err = err__prev2;

                }
            }
            if (len(certs) == 0L)
            {
                return (null, null);
            }
            crypto.PublicKey pub = default;
            switch (certs[0L].PublicKey.type())
            {
                case ref ecdsa.PublicKey key:
                    pub = key;
                    break;
                case ref rsa.PublicKey key:
                    pub = key;
                    break;
                default:
                {
                    var key = certs[0L].PublicKey.type();
                    c.sendAlert(alertUnsupportedCertificate);
                    return (null, fmt.Errorf("tls: client's certificate contains an unsupported public key of type %T", certs[0L].PublicKey));
                    break;
                }
            }
            c.peerCertificates = certs;
            return (pub, null);
        }

        // setCipherSuite sets a cipherSuite with the given id as the serverHandshakeState
        // suite if that cipher suite is acceptable to use.
        // It returns a bool indicating if the suite was set.
        private static bool setCipherSuite(this ref serverHandshakeState hs, ushort id, slice<ushort> supportedCipherSuites, ushort version)
        {
            foreach (var (_, supported) in supportedCipherSuites)
            {
                if (id == supported)
                {
                    ref cipherSuite candidate = default;

                    foreach (var (_, s) in cipherSuites)
                    {
                        if (s.id == id)
                        {
                            candidate = s;
                            break;
                        }
                    }
                    if (candidate == null)
                    {
                        continue;
                    } 
                    // Don't select a ciphersuite which we can't
                    // support for this client.
                    if (candidate.flags & suiteECDHE != 0L)
                    {
                        if (!hs.ellipticOk)
                        {
                            continue;
                        }
                        if (candidate.flags & suiteECDSA != 0L)
                        {
                            if (!hs.ecdsaOk)
                            {
                                continue;
                            }
                        }
                        else if (!hs.rsaSignOk)
                        {
                            continue;
                        }
                    }
                    else if (!hs.rsaDecryptOk)
                    {
                        continue;
                    }
                    if (version < VersionTLS12 && candidate.flags & suiteTLS12 != 0L)
                    {
                        continue;
                    }
                    hs.suite = candidate;
                    return true;
                }
            }
            return false;
        }

        // suppVersArray is the backing array of ClientHelloInfo.SupportedVersions
        private static array<ushort> suppVersArray = new array<ushort>(new ushort[] { VersionTLS12, VersionTLS11, VersionTLS10, VersionSSL30 });

        private static ref ClientHelloInfo clientHelloInfo(this ref serverHandshakeState hs)
        {
            if (hs.cachedClientHelloInfo != null)
            {
                return hs.cachedClientHelloInfo;
            }
            slice<ushort> supportedVersions = default;
            if (hs.clientHello.vers > VersionTLS12)
            {
                supportedVersions = suppVersArray[..];
            }
            else if (hs.clientHello.vers >= VersionSSL30)
            {
                supportedVersions = suppVersArray[VersionTLS12 - hs.clientHello.vers..];
            }
            hs.cachedClientHelloInfo = ref new ClientHelloInfo(CipherSuites:hs.clientHello.cipherSuites,ServerName:hs.clientHello.serverName,SupportedCurves:hs.clientHello.supportedCurves,SupportedPoints:hs.clientHello.supportedPoints,SignatureSchemes:hs.clientHello.supportedSignatureAlgorithms,SupportedProtos:hs.clientHello.alpnProtocols,SupportedVersions:supportedVersions,Conn:hs.c.conn,);

            return hs.cachedClientHelloInfo;
        }
    }
}}
