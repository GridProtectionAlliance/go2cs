// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 October 09 04:55:21 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\handshake_client.go
using bytes = go.bytes_package;
using crypto = go.crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using rsa = go.crypto.rsa_package;
using subtle = go.crypto.subtle_package;
using x509 = go.crypto.x509_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using net = go.net_package;
using strings = go.strings_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using static go.builtin;
using System;

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

        private static (ptr<clientHelloMsg>, ecdheParameters, error) makeClientHello(this ptr<Conn> _addr_c)
        {
            ptr<clientHelloMsg> _p0 = default!;
            ecdheParameters _p0 = default;
            error _p0 = default!;
            ref Conn c = ref _addr_c.val;

            var config = c.config;
            if (len(config.ServerName) == 0L && !config.InsecureSkipVerify)
            {
                return (_addr_null!, null, error.As(errors.New("tls: either ServerName or InsecureSkipVerify must be specified in the tls.Config"))!);
            }

            long nextProtosLength = 0L;
            foreach (var (_, proto) in config.NextProtos)
            {
                {
                    var l = len(proto);

                    if (l == 0L || l > 255L)
                    {
                        return (_addr_null!, null, error.As(errors.New("tls: invalid NextProtos value"))!);
                    }
                    else
                    {
                        nextProtosLength += 1L + l;
                    }

                }

            }
            if (nextProtosLength > 0xffffUL)
            {
                return (_addr_null!, null, error.As(errors.New("tls: NextProtos values too large"))!);
            }

            var supportedVersions = config.supportedVersions();
            if (len(supportedVersions) == 0L)
            {
                return (_addr_null!, null, error.As(errors.New("tls: no supported versions satisfy MinVersion and MaxVersion"))!);
            }

            var clientHelloVersion = config.maxSupportedVersion(); 
            // The version at the beginning of the ClientHello was capped at TLS 1.2
            // for compatibility reasons. The supported_versions extension is used
            // to negotiate versions now. See RFC 8446, Section 4.2.1.
            if (clientHelloVersion > VersionTLS12)
            {
                clientHelloVersion = VersionTLS12;
            }

            ptr<clientHelloMsg> hello = addr(new clientHelloMsg(vers:clientHelloVersion,compressionMethods:[]uint8{compressionNone},random:make([]byte,32),sessionId:make([]byte,32),ocspStapling:true,scts:true,serverName:hostnameInSNI(config.ServerName),supportedCurves:config.curvePreferences(),supportedPoints:[]uint8{pointFormatUncompressed},secureRenegotiationSupported:true,alpnProtocols:config.NextProtos,supportedVersions:supportedVersions,));

            if (c.handshakes > 0L)
            {
                hello.secureRenegotiation = c.clientFinished[..];
            }

            var possibleCipherSuites = config.cipherSuites();
            hello.cipherSuites = make_slice<ushort>(0L, len(possibleCipherSuites));

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
                        break;
                    }

                    hello.cipherSuites = append(hello.cipherSuites, suiteId);
                    break;

                }

            }
            var (_, err) = io.ReadFull(config.rand(), hello.random);
            if (err != null)
            {
                return (_addr_null!, null, error.As(errors.New("tls: short read from Rand: " + err.Error()))!);
            } 

            // A random session ID is used to detect when the server accepted a ticket
            // and is resuming a session (see RFC 5077). In TLS 1.3, it's always set as
            // a compatibility measure (see RFC 8446, Section 4.1.2).
            {
                (_, err) = io.ReadFull(config.rand(), hello.sessionId);

                if (err != null)
                {
                    return (_addr_null!, null, error.As(errors.New("tls: short read from Rand: " + err.Error()))!);
                }

            }


            if (hello.vers >= VersionTLS12)
            {
                hello.supportedSignatureAlgorithms = supportedSignatureAlgorithms;
            }

            ecdheParameters @params = default;
            if (hello.supportedVersions[0L] == VersionTLS13)
            {
                hello.cipherSuites = append(hello.cipherSuites, defaultCipherSuitesTLS13());

                var curveID = config.curvePreferences()[0L];
                {
                    var (_, ok) = curveForCurveID(curveID);

                    if (curveID != X25519 && !ok)
                    {
                        return (_addr_null!, null, error.As(errors.New("tls: CurvePreferences includes unsupported curve"))!);
                    }

                }

                params, err = generateECDHEParameters(config.rand(), curveID);
                if (err != null)
                {
                    return (_addr_null!, null, error.As(err)!);
                }

                hello.keyShares = new slice<keyShare>(new keyShare[] { {group:curveID,data:params.PublicKey()} });

            }

            return (_addr_hello!, params, error.As(null!)!);

        }

        private static error clientHandshake(this ptr<Conn> _addr_c) => func((defer, _, __) =>
        {
            error err = default!;
            ref Conn c = ref _addr_c.val;

            if (c.config == null)
            {
                c.config = defaultConfig();
            } 

            // This may be a renegotiation handshake, in which case some fields
            // need to be reset.
            c.didResume = false;

            var (hello, ecdheParams, err) = c.makeClientHello();
            if (err != null)
            {
                return error.As(err)!;
            }

            c.serverName = hello.serverName;

            var (cacheKey, session, earlySecret, binderKey) = c.loadSession(hello);
            if (cacheKey != "" && session != null)
            {
                defer(() =>
                { 
                    // If we got a handshake failure when resuming a session, throw away
                    // the session ticket. See RFC 5077, Section 3.2.
                    //
                    // RFC 8446 makes no mention of dropping tickets on failure, but it
                    // does require servers to abort on invalid binders, so we need to
                    // delete tickets to recover from a corrupted PSK.
                    if (err != null)
                    {
                        c.config.ClientSessionCache.Put(cacheKey, null);
                    }

                }());

            }

            {
                var err__prev1 = err;

                var (_, err) = c.writeRecord(recordTypeHandshake, hello.marshal());

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

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

            {
                var err__prev1 = err;

                var err = c.pickTLSVersion(serverHello);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // If we are negotiating a protocol version that's lower than what we
                // support, check for the server downgrade canaries.
                // See RFC 8446, Section 4.1.3.

                err = err__prev1;

            } 

            // If we are negotiating a protocol version that's lower than what we
            // support, check for the server downgrade canaries.
            // See RFC 8446, Section 4.1.3.
            var maxVers = c.config.maxSupportedVersion();
            var tls12Downgrade = string(serverHello.random[24L..]) == downgradeCanaryTLS12;
            var tls11Downgrade = string(serverHello.random[24L..]) == downgradeCanaryTLS11;
            if (maxVers == VersionTLS13 && c.vers <= VersionTLS12 && (tls12Downgrade || tls11Downgrade) || maxVers == VersionTLS12 && c.vers <= VersionTLS11 && tls11Downgrade)
            {
                c.sendAlert(alertIllegalParameter);
                return error.As(errors.New("tls: downgrade attempt detected, possibly due to a MitM attack or a broken middlebox"))!;
            }

            if (c.vers == VersionTLS13)
            {
                ptr<clientHandshakeStateTLS13> hs = addr(new clientHandshakeStateTLS13(c:c,serverHello:serverHello,hello:hello,ecdheParams:ecdheParams,session:session,earlySecret:earlySecret,binderKey:binderKey,)); 

                // In TLS 1.3, session tickets are delivered after the handshake.
                return error.As(hs.handshake())!;

            }

            hs = addr(new clientHandshakeState(c:c,serverHello:serverHello,hello:hello,session:session,));

            {
                var err__prev1 = err;

                err = hs.handshake();

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // If we had a successful handshake and hs.session is different from
                // the one already cached - cache a new one.

                err = err__prev1;

            } 

            // If we had a successful handshake and hs.session is different from
            // the one already cached - cache a new one.
            if (cacheKey != "" && hs.session != null && session != hs.session)
            {
                c.config.ClientSessionCache.Put(cacheKey, hs.session);
            }

            return error.As(null!)!;

        });

        private static (@string, ptr<ClientSessionState>, slice<byte>, slice<byte>) loadSession(this ptr<Conn> _addr_c, ptr<clientHelloMsg> _addr_hello)
        {
            @string cacheKey = default;
            ptr<ClientSessionState> session = default!;
            slice<byte> earlySecret = default;
            slice<byte> binderKey = default;
            ref Conn c = ref _addr_c.val;
            ref clientHelloMsg hello = ref _addr_hello.val;

            if (c.config.SessionTicketsDisabled || c.config.ClientSessionCache == null)
            {
                return ("", _addr_null!, null, null);
            }

            hello.ticketSupported = true;

            if (hello.supportedVersions[0L] == VersionTLS13)
            { 
                // Require DHE on resumption as it guarantees forward secrecy against
                // compromise of the session ticket key. See RFC 8446, Section 4.2.9.
                hello.pskModes = new slice<byte>(new byte[] { pskModeDHE });

            } 

            // Session resumption is not allowed if renegotiating because
            // renegotiation is primarily used to allow a client to send a client
            // certificate, which would be skipped if session resumption occurred.
            if (c.handshakes != 0L)
            {
                return ("", _addr_null!, null, null);
            } 

            // Try to resume a previously negotiated TLS session, if available.
            cacheKey = clientSessionCacheKey(c.conn.RemoteAddr(), _addr_c.config);
            var (session, ok) = c.config.ClientSessionCache.Get(cacheKey);
            if (!ok || session == null)
            {
                return (cacheKey, _addr_null!, null, null);
            } 

            // Check that version used for the previous session is still valid.
            var versOk = false;
            foreach (var (_, v) in hello.supportedVersions)
            {
                if (v == session.vers)
                {
                    versOk = true;
                    break;
                }

            }
            if (!versOk)
            {
                return (cacheKey, _addr_null!, null, null);
            } 

            // Check that the cached server certificate is not expired, and that it's
            // valid for the ServerName. This should be ensured by the cache key, but
            // protect the application from a faulty ClientSessionCache implementation.
            if (!c.config.InsecureSkipVerify)
            {
                if (len(session.verifiedChains) == 0L)
                { 
                    // The original connection had InsecureSkipVerify, while this doesn't.
                    return (cacheKey, _addr_null!, null, null);

                }

                var serverCert = session.serverCertificates[0L];
                if (c.config.time().After(serverCert.NotAfter))
                { 
                    // Expired certificate, delete the entry.
                    c.config.ClientSessionCache.Put(cacheKey, null);
                    return (cacheKey, _addr_null!, null, null);

                }

                {
                    var err = serverCert.VerifyHostname(c.config.ServerName);

                    if (err != null)
                    {
                        return (cacheKey, _addr_null!, null, null);
                    }

                }

            }

            if (session.vers != VersionTLS13)
            { 
                // In TLS 1.2 the cipher suite must match the resumed session. Ensure we
                // are still offering it.
                if (mutualCipherSuite(hello.cipherSuites, session.cipherSuite) == null)
                {
                    return (cacheKey, _addr_null!, null, null);
                }

                hello.sessionTicket = session.sessionTicket;
                return ;

            } 

            // Check that the session ticket is not expired.
            if (c.config.time().After(session.useBy))
            {
                c.config.ClientSessionCache.Put(cacheKey, null);
                return (cacheKey, _addr_null!, null, null);
            } 

            // In TLS 1.3 the KDF hash must match the resumed session. Ensure we
            // offer at least one cipher suite with that hash.
            var cipherSuite = cipherSuiteTLS13ByID(session.cipherSuite);
            if (cipherSuite == null)
            {
                return (cacheKey, _addr_null!, null, null);
            }

            var cipherSuiteOk = false;
            foreach (var (_, offeredID) in hello.cipherSuites)
            {
                var offeredSuite = cipherSuiteTLS13ByID(offeredID);
                if (offeredSuite != null && offeredSuite.hash == cipherSuite.hash)
                {
                    cipherSuiteOk = true;
                    break;
                }

            }
            if (!cipherSuiteOk)
            {
                return (cacheKey, _addr_null!, null, null);
            } 

            // Set the pre_shared_key extension. See RFC 8446, Section 4.2.11.1.
            var ticketAge = uint32(c.config.time().Sub(session.receivedAt) / time.Millisecond);
            pskIdentity identity = new pskIdentity(label:session.sessionTicket,obfuscatedTicketAge:ticketAge+session.ageAdd,);
            hello.pskIdentities = new slice<pskIdentity>(new pskIdentity[] { identity });
            hello.pskBinders = new slice<slice<byte>>(new slice<byte>[] { make([]byte,cipherSuite.hash.Size()) }); 

            // Compute the PSK binders. See RFC 8446, Section 4.2.11.2.
            var psk = cipherSuite.expandLabel(session.masterSecret, "resumption", session.nonce, cipherSuite.hash.Size());
            earlySecret = cipherSuite.extract(psk, null);
            binderKey = cipherSuite.deriveSecret(earlySecret, resumptionBinderLabel, null);
            var transcript = cipherSuite.hash.New();
            transcript.Write(hello.marshalWithoutBinders());
            slice<byte> pskBinders = new slice<slice<byte>>(new slice<byte>[] { cipherSuite.finishedHash(binderKey,transcript) });
            hello.updateBinders(pskBinders);

            return ;

        }

        private static error pickTLSVersion(this ptr<Conn> _addr_c, ptr<serverHelloMsg> _addr_serverHello)
        {
            ref Conn c = ref _addr_c.val;
            ref serverHelloMsg serverHello = ref _addr_serverHello.val;

            var peerVersion = serverHello.vers;
            if (serverHello.supportedVersion != 0L)
            {
                peerVersion = serverHello.supportedVersion;
            }

            var (vers, ok) = c.config.mutualVersion(new slice<ushort>(new ushort[] { peerVersion }));
            if (!ok)
            {
                c.sendAlert(alertProtocolVersion);
                return error.As(fmt.Errorf("tls: server selected unsupported protocol version %x", peerVersion))!;
            }

            c.vers = vers;
            c.haveVers = true;
            c.@in.version = vers;
            c.@out.version = vers;

            return error.As(null!)!;

        }

        // Does the handshake, either a full one or resumes old session. Requires hs.c,
        // hs.hello, hs.serverHello, and, optionally, hs.session to be set.
        private static error handshake(this ptr<clientHandshakeState> _addr_hs)
        {
            ref clientHandshakeState hs = ref _addr_hs.val;

            var c = hs.c;

            var (isResume, err) = hs.processServerHello();
            if (err != null)
            {
                return error.As(err)!;
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
            c.didResume = isResume;
            if (isResume)
            {
                {
                    var err__prev2 = err;

                    var err = hs.establishKeys();

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                {
                    var err__prev2 = err;

                    err = hs.readSessionTicket();

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                {
                    var err__prev2 = err;

                    err = hs.readFinished(c.serverFinished[..]);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                c.clientFinishedIsFirst = false; 
                // Make sure the connection is still being verified whether or not this
                // is a resumption. Resumptions currently don't reverify certificates so
                // they don't call verifyServerCertificate. See Issue 31641.
                if (c.config.VerifyConnection != null)
                {
                    {
                        var err__prev3 = err;

                        err = c.config.VerifyConnection(c.connectionStateLocked());

                        if (err != null)
                        {
                            c.sendAlert(alertBadCertificate);
                            return error.As(err)!;
                        }

                        err = err__prev3;

                    }

                }

                {
                    var err__prev2 = err;

                    err = hs.sendFinished(c.clientFinished[..]);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                {
                    var err__prev2 = err;

                    var (_, err) = c.flush();

                    if (err != null)
                    {
                        return error.As(err)!;
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
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                {
                    var err__prev2 = err;

                    err = hs.establishKeys();

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                {
                    var err__prev2 = err;

                    err = hs.sendFinished(c.clientFinished[..]);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                {
                    var err__prev2 = err;

                    (_, err) = c.flush();

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                c.clientFinishedIsFirst = true;
                {
                    var err__prev2 = err;

                    err = hs.readSessionTicket();

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                {
                    var err__prev2 = err;

                    err = hs.readFinished(c.serverFinished[..]);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            c.ekm = ekmFromMasterSecret(c.vers, hs.suite, hs.masterSecret, hs.hello.random, hs.serverHello.random);
            atomic.StoreUint32(_addr_c.handshakeStatus, 1L);

            return error.As(null!)!;

        }

        private static error pickCipherSuite(this ptr<clientHandshakeState> _addr_hs)
        {
            ref clientHandshakeState hs = ref _addr_hs.val;

            hs.suite = mutualCipherSuite(hs.hello.cipherSuites, hs.serverHello.cipherSuite);

            if (hs.suite == null)
            {
                hs.c.sendAlert(alertHandshakeFailure);
                return error.As(errors.New("tls: server chose an unconfigured cipher suite"))!;
            }

            hs.c.cipherSuite = hs.suite.id;
            return error.As(null!)!;

        }

        private static error doFullHandshake(this ptr<clientHandshakeState> _addr_hs)
        {
            ref clientHandshakeState hs = ref _addr_hs.val;

            var c = hs.c;

            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err)!;
            }

            ptr<certificateMsg> (certMsg, ok) = msg._<ptr<certificateMsg>>();
            if (!ok || len(certMsg.certificates) == 0L)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(certMsg, msg))!;
            }

            hs.finishedHash.Write(certMsg.marshal());

            msg, err = c.readHandshake();
            if (err != null)
            {
                return error.As(err)!;
            }

            ptr<certificateStatusMsg> (cs, ok) = msg._<ptr<certificateStatusMsg>>();
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
                    return error.As(errors.New("tls: received unexpected CertificateStatus message"))!;

                }

                hs.finishedHash.Write(cs.marshal());

                c.ocspResponse = cs.response;

                msg, err = c.readHandshake();
                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            if (c.handshakes == 0L)
            { 
                // If this is the first handshake on a connection, process and
                // (optionally) verify the server's certificates.
                {
                    var err__prev2 = err;

                    var err = c.verifyServerCertificate(certMsg.certificates);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }
            else
            { 
                // This is a renegotiation handshake. We require that the
                // server's identity (i.e. leaf certificate) is unchanged and
                // thus any previous trust decision is still valid.
                //
                // See https://mitls.org/pages/attacks/3SHAKE for the
                // motivation behind this requirement.
                if (!bytes.Equal(c.peerCertificates[0L].Raw, certMsg.certificates[0L]))
                {
                    c.sendAlert(alertBadCertificate);
                    return error.As(errors.New("tls: server's identity changed during renegotiation"))!;
                }

            }

            var keyAgreement = hs.suite.ka(c.vers);

            ptr<serverKeyExchangeMsg> (skx, ok) = msg._<ptr<serverKeyExchangeMsg>>();
            if (ok)
            {
                hs.finishedHash.Write(skx.marshal());
                err = keyAgreement.processServerKeyExchange(c.config, hs.hello, hs.serverHello, c.peerCertificates[0L], skx);
                if (err != null)
                {
                    c.sendAlert(alertUnexpectedMessage);
                    return error.As(err)!;
                }

                msg, err = c.readHandshake();
                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            ptr<Certificate> chainToSend;
            bool certRequested = default;
            ptr<certificateRequestMsg> (certReq, ok) = msg._<ptr<certificateRequestMsg>>();
            if (ok)
            {
                certRequested = true;
                hs.finishedHash.Write(certReq.marshal());

                var cri = certificateRequestInfoFromMsg(c.vers, certReq);
                chainToSend, err = c.getClientCertificate(cri);

                if (err != null)
                {
                    c.sendAlert(alertInternalError);
                    return error.As(err)!;
                }

                msg, err = c.readHandshake();
                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            ptr<serverHelloDoneMsg> (shd, ok) = msg._<ptr<serverHelloDoneMsg>>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(shd, msg))!;
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
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            var (preMasterSecret, ckx, err) = keyAgreement.generateClientKeyExchange(c.config, hs.hello, c.peerCertificates[0L]);
            if (err != null)
            {
                c.sendAlert(alertInternalError);
                return error.As(err)!;
            }

            if (ckx != null)
            {
                hs.finishedHash.Write(ckx.marshal());
                {
                    var err__prev2 = err;

                    (_, err) = c.writeRecord(recordTypeHandshake, ckx.marshal());

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            if (chainToSend != null && len(chainToSend.Certificate) > 0L)
            {
                ptr<certificateVerifyMsg> certVerify = addr(new certificateVerifyMsg());

                crypto.Signer (key, ok) = chainToSend.PrivateKey._<crypto.Signer>();
                if (!ok)
                {
                    c.sendAlert(alertInternalError);
                    return error.As(fmt.Errorf("tls: client certificate private key of type %T does not implement crypto.Signer", chainToSend.PrivateKey))!;
                }

                byte sigType = default;
                crypto.Hash sigHash = default;
                if (c.vers >= VersionTLS12)
                {
                    var (signatureAlgorithm, err) = selectSignatureScheme(c.vers, chainToSend, certReq.supportedSignatureAlgorithms);
                    if (err != null)
                    {
                        c.sendAlert(alertIllegalParameter);
                        return error.As(err)!;
                    }

                    sigType, sigHash, err = typeAndHashFromSignatureScheme(signatureAlgorithm);
                    if (err != null)
                    {
                        return error.As(c.sendAlert(alertInternalError))!;
                    }

                    certVerify.hasSignatureAlgorithm = true;
                    certVerify.signatureAlgorithm = signatureAlgorithm;

                }
                else
                {
                    sigType, sigHash, err = legacyTypeAndHashFromPublicKey(key.Public());
                    if (err != null)
                    {
                        c.sendAlert(alertIllegalParameter);
                        return error.As(err)!;
                    }

                }

                var signed = hs.finishedHash.hashForClientCertificate(sigType, sigHash, hs.masterSecret);
                var signOpts = crypto.SignerOpts(sigHash);
                if (sigType == signatureRSAPSS)
                {
                    signOpts = addr(new rsa.PSSOptions(SaltLength:rsa.PSSSaltLengthEqualsHash,Hash:sigHash));
                }

                certVerify.signature, err = key.Sign(c.config.rand(), signed, signOpts);
                if (err != null)
                {
                    c.sendAlert(alertInternalError);
                    return error.As(err)!;
                }

                hs.finishedHash.Write(certVerify.marshal());
                {
                    var err__prev2 = err;

                    (_, err) = c.writeRecord(recordTypeHandshake, certVerify.marshal());

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            hs.masterSecret = masterFromPreMasterSecret(c.vers, hs.suite, preMasterSecret, hs.hello.random, hs.serverHello.random);
            {
                var err__prev1 = err;

                err = c.config.writeKeyLog(keyLogLabelTLS12, hs.hello.random, hs.masterSecret);

                if (err != null)
                {
                    c.sendAlert(alertInternalError);
                    return error.As(errors.New("tls: failed to write to key log: " + err.Error()))!;
                }

                err = err__prev1;

            }


            hs.finishedHash.discardHandshakeBuffer();

            return error.As(null!)!;

        }

        private static error establishKeys(this ptr<clientHandshakeState> _addr_hs)
        {
            ref clientHandshakeState hs = ref _addr_hs.val;

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
            return error.As(null!)!;

        }

        private static bool serverResumedSession(this ptr<clientHandshakeState> _addr_hs)
        {
            ref clientHandshakeState hs = ref _addr_hs.val;
 
            // If the server responded with the same sessionId then it means the
            // sessionTicket is being used to resume a TLS session.
            return hs.session != null && hs.hello.sessionId != null && bytes.Equal(hs.serverHello.sessionId, hs.hello.sessionId);

        }

        private static (bool, error) processServerHello(this ptr<clientHandshakeState> _addr_hs)
        {
            bool _p0 = default;
            error _p0 = default!;
            ref clientHandshakeState hs = ref _addr_hs.val;

            var c = hs.c;

            {
                var err = hs.pickCipherSuite();

                if (err != null)
                {
                    return (false, error.As(err)!);
                }

            }


            if (hs.serverHello.compressionMethod != compressionNone)
            {
                c.sendAlert(alertUnexpectedMessage);
                return (false, error.As(errors.New("tls: server selected unsupported compression format"))!);
            }

            if (c.handshakes == 0L && hs.serverHello.secureRenegotiationSupported)
            {
                c.secureRenegotiation = true;
                if (len(hs.serverHello.secureRenegotiation) != 0L)
                {
                    c.sendAlert(alertHandshakeFailure);
                    return (false, error.As(errors.New("tls: initial handshake had non-empty renegotiation extension"))!);
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
                    return (false, error.As(errors.New("tls: incorrect renegotiation extension contents"))!);
                }

            }

            var clientDidALPN = len(hs.hello.alpnProtocols) > 0L;
            var serverHasALPN = len(hs.serverHello.alpnProtocol) > 0L;

            if (!clientDidALPN && serverHasALPN)
            {
                c.sendAlert(alertHandshakeFailure);
                return (false, error.As(errors.New("tls: server advertised unrequested ALPN extension"))!);
            }

            if (serverHasALPN)
            {
                c.clientProtocol = hs.serverHello.alpnProtocol;
                c.clientProtocolFallback = false;
            }

            c.scts = hs.serverHello.scts;

            if (!hs.serverResumedSession())
            {
                return (false, error.As(null!)!);
            }

            if (hs.session.vers != c.vers)
            {
                c.sendAlert(alertHandshakeFailure);
                return (false, error.As(errors.New("tls: server resumed a session with a different version"))!);
            }

            if (hs.session.cipherSuite != hs.suite.id)
            {
                c.sendAlert(alertHandshakeFailure);
                return (false, error.As(errors.New("tls: server resumed a session with a different cipher suite"))!);
            } 

            // Restore masterSecret, peerCerts, and ocspResponse from previous state
            hs.masterSecret = hs.session.masterSecret;
            c.peerCertificates = hs.session.serverCertificates;
            c.verifiedChains = hs.session.verifiedChains;
            c.ocspResponse = hs.session.ocspResponse; 
            // Let the ServerHello SCTs override the session SCTs from the original
            // connection, if any are provided
            if (len(c.scts) == 0L && len(hs.session.scts) != 0L)
            {
                c.scts = hs.session.scts;
            }

            return (true, error.As(null!)!);

        }

        private static error readFinished(this ptr<clientHandshakeState> _addr_hs, slice<byte> @out)
        {
            ref clientHandshakeState hs = ref _addr_hs.val;

            var c = hs.c;

            {
                var err = c.readChangeCipherSpec();

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

            ptr<finishedMsg> (serverFinished, ok) = msg._<ptr<finishedMsg>>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(serverFinished, msg))!;
            }

            var verify = hs.finishedHash.serverSum(hs.masterSecret);
            if (len(verify) != len(serverFinished.verifyData) || subtle.ConstantTimeCompare(verify, serverFinished.verifyData) != 1L)
            {
                c.sendAlert(alertHandshakeFailure);
                return error.As(errors.New("tls: server's Finished message was incorrect"))!;
            }

            hs.finishedHash.Write(serverFinished.marshal());
            copy(out, verify);
            return error.As(null!)!;

        }

        private static error readSessionTicket(this ptr<clientHandshakeState> _addr_hs)
        {
            ref clientHandshakeState hs = ref _addr_hs.val;

            if (!hs.serverHello.ticketSupported)
            {
                return error.As(null!)!;
            }

            var c = hs.c;
            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err)!;
            }

            ptr<newSessionTicketMsg> (sessionTicketMsg, ok) = msg._<ptr<newSessionTicketMsg>>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(unexpectedMessageError(sessionTicketMsg, msg))!;
            }

            hs.finishedHash.Write(sessionTicketMsg.marshal());

            hs.session = addr(new ClientSessionState(sessionTicket:sessionTicketMsg.ticket,vers:c.vers,cipherSuite:hs.suite.id,masterSecret:hs.masterSecret,serverCertificates:c.peerCertificates,verifiedChains:c.verifiedChains,receivedAt:c.config.time(),ocspResponse:c.ocspResponse,scts:c.scts,));

            return error.As(null!)!;

        }

        private static error sendFinished(this ptr<clientHandshakeState> _addr_hs, slice<byte> @out)
        {
            ref clientHandshakeState hs = ref _addr_hs.val;

            var c = hs.c;

            {
                var (_, err) = c.writeRecord(recordTypeChangeCipherSpec, new slice<byte>(new byte[] { 1 }));

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            ptr<finishedMsg> finished = @new<finishedMsg>();
            finished.verifyData = hs.finishedHash.clientSum(hs.masterSecret);
            hs.finishedHash.Write(finished.marshal());
            {
                (_, err) = c.writeRecord(recordTypeHandshake, finished.marshal());

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            copy(out, finished.verifyData);
            return error.As(null!)!;

        }

        // verifyServerCertificate parses and verifies the provided chain, setting
        // c.verifiedChains and c.peerCertificates or sending the appropriate alert.
        private static error verifyServerCertificate(this ptr<Conn> _addr_c, slice<slice<byte>> certificates)
        {
            ref Conn c = ref _addr_c.val;

            var certs = make_slice<ptr<x509.Certificate>>(len(certificates));
            foreach (var (i, asn1Data) in certificates)
            {
                var (cert, err) = x509.ParseCertificate(asn1Data);
                if (err != null)
                {
                    c.sendAlert(alertBadCertificate);
                    return error.As(errors.New("tls: failed to parse certificate from server: " + err.Error()))!;
                }

                certs[i] = cert;

            }
            if (!c.config.InsecureSkipVerify)
            {
                x509.VerifyOptions opts = new x509.VerifyOptions(Roots:c.config.RootCAs,CurrentTime:c.config.time(),DNSName:c.config.ServerName,Intermediates:x509.NewCertPool(),);
                {
                    var cert__prev1 = cert;

                    foreach (var (_, __cert) in certs[1L..])
                    {
                        cert = __cert;
                        opts.Intermediates.AddCert(cert);
                    }

                    cert = cert__prev1;
                }

                error err = default!;
                c.verifiedChains, err = certs[0L].Verify(opts);
                if (err != null)
                {
                    c.sendAlert(alertBadCertificate);
                    return error.As(err)!;
                }

            }

            switch (certs[0L].PublicKey.type())
            {
                case ptr<rsa.PublicKey> _:
                    break;
                    break;
                case ptr<ecdsa.PublicKey> _:
                    break;
                    break;
                case ed25519.PublicKey _:
                    break;
                    break;
                default:
                {
                    c.sendAlert(alertUnsupportedCertificate);
                    return error.As(fmt.Errorf("tls: server's certificate contains an unsupported type of public key: %T", certs[0L].PublicKey))!;
                    break;
                }

            }

            c.peerCertificates = certs;

            if (c.config.VerifyPeerCertificate != null)
            {
                {
                    error err__prev2 = err;

                    err = c.config.VerifyPeerCertificate(certificates, c.verifiedChains);

                    if (err != null)
                    {
                        c.sendAlert(alertBadCertificate);
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            if (c.config.VerifyConnection != null)
            {
                {
                    error err__prev2 = err;

                    err = c.config.VerifyConnection(c.connectionStateLocked());

                    if (err != null)
                    {
                        c.sendAlert(alertBadCertificate);
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            return error.As(null!)!;

        }

        // certificateRequestInfoFromMsg generates a CertificateRequestInfo from a TLS
        // <= 1.2 CertificateRequest, making an effort to fill in missing information.
        private static ptr<CertificateRequestInfo> certificateRequestInfoFromMsg(ushort vers, ptr<certificateRequestMsg> _addr_certReq)
        {
            ref certificateRequestMsg certReq = ref _addr_certReq.val;

            ptr<CertificateRequestInfo> cri = addr(new CertificateRequestInfo(AcceptableCAs:certReq.certificateAuthorities,Version:vers,));

            bool rsaAvail = default;            bool ecAvail = default;

            foreach (var (_, certType) in certReq.certificateTypes)
            {

                if (certType == certTypeRSASign) 
                    rsaAvail = true;
                else if (certType == certTypeECDSASign) 
                    ecAvail = true;
                
            }
            if (!certReq.hasSignatureAlgorithm)
            { 
                // Prior to TLS 1.2, signature schemes did not exist. In this case we
                // make up a list based on the acceptable certificate types, to help
                // GetClientCertificate and SupportsCertificate select the right certificate.
                // The hash part of the SignatureScheme is a lie here, because
                // TLS 1.0 and 1.1 always use MD5+SHA1 for RSA and SHA1 for ECDSA.

                if (rsaAvail && ecAvail) 
                    cri.SignatureSchemes = new slice<SignatureScheme>(new SignatureScheme[] { ECDSAWithP256AndSHA256, ECDSAWithP384AndSHA384, ECDSAWithP521AndSHA512, PKCS1WithSHA256, PKCS1WithSHA384, PKCS1WithSHA512, PKCS1WithSHA1 });
                else if (rsaAvail) 
                    cri.SignatureSchemes = new slice<SignatureScheme>(new SignatureScheme[] { PKCS1WithSHA256, PKCS1WithSHA384, PKCS1WithSHA512, PKCS1WithSHA1 });
                else if (ecAvail) 
                    cri.SignatureSchemes = new slice<SignatureScheme>(new SignatureScheme[] { ECDSAWithP256AndSHA256, ECDSAWithP384AndSHA384, ECDSAWithP521AndSHA512 });
                                return _addr_cri!;

            } 

            // Filter the signature schemes based on the certificate types.
            // See RFC 5246, Section 7.4.4 (where it calls this "somewhat complicated").
            cri.SignatureSchemes = make_slice<SignatureScheme>(0L, len(certReq.supportedSignatureAlgorithms));
            foreach (var (_, sigScheme) in certReq.supportedSignatureAlgorithms)
            {
                var (sigType, _, err) = typeAndHashFromSignatureScheme(sigScheme);
                if (err != null)
                {
                    continue;
                }


                if (sigType == signatureECDSA || sigType == signatureEd25519) 
                    if (ecAvail)
                    {
                        cri.SignatureSchemes = append(cri.SignatureSchemes, sigScheme);
                    }

                else if (sigType == signatureRSAPSS || sigType == signaturePKCS1v15) 
                    if (rsaAvail)
                    {
                        cri.SignatureSchemes = append(cri.SignatureSchemes, sigScheme);
                    }

                            }
            return _addr_cri!;

        }

        private static (ptr<Certificate>, error) getClientCertificate(this ptr<Conn> _addr_c, ptr<CertificateRequestInfo> _addr_cri)
        {
            ptr<Certificate> _p0 = default!;
            error _p0 = default!;
            ref Conn c = ref _addr_c.val;
            ref CertificateRequestInfo cri = ref _addr_cri.val;

            if (c.config.GetClientCertificate != null)
            {
                return _addr_c.config.GetClientCertificate(cri)!;
            }

            foreach (var (_, chain) in c.config.Certificates)
            {
                {
                    var err = cri.SupportsCertificate(_addr_chain);

                    if (err != null)
                    {
                        continue;
                    }

                }

                return (_addr__addr_chain!, error.As(null!)!);

            } 

            // No acceptable certificate found. Don't send a certificate.
            return (@new<Certificate>(), error.As(null!)!);

        }

        // clientSessionCacheKey returns a key used to cache sessionTickets that could
        // be used to resume previously negotiated TLS sessions with a server.
        private static @string clientSessionCacheKey(net.Addr serverAddr, ptr<Config> _addr_config)
        {
            ref Config config = ref _addr_config.val;

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
            @string _p0 = default;
            bool _p0 = default;

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

        // hostnameInSNI converts name into an appropriate hostname for SNI.
        // Literal IP addresses and absolute FQDNs are not permitted as SNI values.
        // See RFC 6066, Section 3.
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
