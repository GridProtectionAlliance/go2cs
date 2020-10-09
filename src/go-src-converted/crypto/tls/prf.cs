// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 October 09 04:56:02 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\prf.go
using crypto = go.crypto_package;
using hmac = go.crypto.hmac_package;
using md5 = go.crypto.md5_package;
using sha1 = go.crypto.sha1_package;
using sha256 = go.crypto.sha256_package;
using sha512 = go.crypto.sha512_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using hash = go.hash_package;
using static go.builtin;
using System;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        // Split a premaster secret in two as specified in RFC 4346, Section 5.
        private static (slice<byte>, slice<byte>) splitPreMasterSecret(slice<byte> secret)
        {
            slice<byte> s1 = default;
            slice<byte> s2 = default;

            s1 = secret[0L..(len(secret) + 1L) / 2L];
            s2 = secret[len(secret) / 2L..];
            return ;
        }

        // pHash implements the P_hash function, as defined in RFC 4346, Section 5.
        private static hash.Hash pHash(slice<byte> result, slice<byte> secret, slice<byte> seed, Func<hash.Hash> hash)
        {
            var h = hmac.New(hash, secret);
            h.Write(seed);
            var a = h.Sum(null);

            long j = 0L;
            while (j < len(result))
            {
                h.Reset();
                h.Write(a);
                h.Write(seed);
                var b = h.Sum(null);
                copy(result[j..], b);
                j += len(b);

                h.Reset();
                h.Write(a);
                a = h.Sum(null);
            }


        }

        // prf10 implements the TLS 1.0 pseudo-random function, as defined in RFC 2246, Section 5.
        private static void prf10(slice<byte> result, slice<byte> secret, slice<byte> label, slice<byte> seed)
        {
            var hashSHA1 = sha1.New;
            var hashMD5 = md5.New;

            var labelAndSeed = make_slice<byte>(len(label) + len(seed));
            copy(labelAndSeed, label);
            copy(labelAndSeed[len(label)..], seed);

            var (s1, s2) = splitPreMasterSecret(secret);
            pHash(result, s1, labelAndSeed, hashMD5);
            var result2 = make_slice<byte>(len(result));
            pHash(result2, s2, labelAndSeed, hashSHA1);

            foreach (var (i, b) in result2)
            {
                result[i] ^= b;
            }

        }

        // prf12 implements the TLS 1.2 pseudo-random function, as defined in RFC 5246, Section 5.
        private static Action<slice<byte>, slice<byte>, slice<byte>, slice<byte>> prf12(Func<hash.Hash> hashFunc)
        {
            return (result, secret, label, seed) =>
            {
                var labelAndSeed = make_slice<byte>(len(label) + len(seed));
                copy(labelAndSeed, label);
                copy(labelAndSeed[len(label)..], seed);

                pHash(result, secret, labelAndSeed, hashFunc);
            };

        }

        private static readonly long masterSecretLength = (long)48L; // Length of a master secret in TLS 1.1.
        private static readonly long finishedVerifyLength = (long)12L; // Length of verify_data in a Finished message.

        private static slice<byte> masterSecretLabel = (slice<byte>)"master secret";
        private static slice<byte> keyExpansionLabel = (slice<byte>)"key expansion";
        private static slice<byte> clientFinishedLabel = (slice<byte>)"client finished";
        private static slice<byte> serverFinishedLabel = (slice<byte>)"server finished";

        private static (Action<slice<byte>, slice<byte>, slice<byte>, slice<byte>>, crypto.Hash) prfAndHashForVersion(ushort version, ptr<cipherSuite> _addr_suite) => func((_, panic, __) =>
        {
            Action<slice<byte>, slice<byte>, slice<byte>, slice<byte>> _p0 = default;
            crypto.Hash _p0 = default;
            ref cipherSuite suite = ref _addr_suite.val;


            if (version == VersionTLS10 || version == VersionTLS11) 
                return (prf10, crypto.Hash(0L));
            else if (version == VersionTLS12) 
                if (suite.flags & suiteSHA384 != 0L)
                {
                    return (prf12(sha512.New384), crypto.SHA384);
                }

                return (prf12(sha256.New), crypto.SHA256);
            else 
                panic("unknown version");
            
        });

        private static Action<slice<byte>, slice<byte>, slice<byte>, slice<byte>> prfForVersion(ushort version, ptr<cipherSuite> _addr_suite)
        {
            ref cipherSuite suite = ref _addr_suite.val;

            var (prf, _) = prfAndHashForVersion(version, _addr_suite);
            return prf;
        }

        // masterFromPreMasterSecret generates the master secret from the pre-master
        // secret. See RFC 5246, Section 8.1.
        private static slice<byte> masterFromPreMasterSecret(ushort version, ptr<cipherSuite> _addr_suite, slice<byte> preMasterSecret, slice<byte> clientRandom, slice<byte> serverRandom)
        {
            ref cipherSuite suite = ref _addr_suite.val;

            var seed = make_slice<byte>(0L, len(clientRandom) + len(serverRandom));
            seed = append(seed, clientRandom);
            seed = append(seed, serverRandom);

            var masterSecret = make_slice<byte>(masterSecretLength);
            prfForVersion(version, _addr_suite)(masterSecret, preMasterSecret, masterSecretLabel, seed);
            return masterSecret;
        }

        // keysFromMasterSecret generates the connection keys from the master
        // secret, given the lengths of the MAC key, cipher key and IV, as defined in
        // RFC 2246, Section 6.3.
        private static (slice<byte>, slice<byte>, slice<byte>, slice<byte>, slice<byte>, slice<byte>) keysFromMasterSecret(ushort version, ptr<cipherSuite> _addr_suite, slice<byte> masterSecret, slice<byte> clientRandom, slice<byte> serverRandom, long macLen, long keyLen, long ivLen)
        {
            slice<byte> clientMAC = default;
            slice<byte> serverMAC = default;
            slice<byte> clientKey = default;
            slice<byte> serverKey = default;
            slice<byte> clientIV = default;
            slice<byte> serverIV = default;
            ref cipherSuite suite = ref _addr_suite.val;

            var seed = make_slice<byte>(0L, len(serverRandom) + len(clientRandom));
            seed = append(seed, serverRandom);
            seed = append(seed, clientRandom);

            long n = 2L * macLen + 2L * keyLen + 2L * ivLen;
            var keyMaterial = make_slice<byte>(n);
            prfForVersion(version, _addr_suite)(keyMaterial, masterSecret, keyExpansionLabel, seed);
            clientMAC = keyMaterial[..macLen];
            keyMaterial = keyMaterial[macLen..];
            serverMAC = keyMaterial[..macLen];
            keyMaterial = keyMaterial[macLen..];
            clientKey = keyMaterial[..keyLen];
            keyMaterial = keyMaterial[keyLen..];
            serverKey = keyMaterial[..keyLen];
            keyMaterial = keyMaterial[keyLen..];
            clientIV = keyMaterial[..ivLen];
            keyMaterial = keyMaterial[ivLen..];
            serverIV = keyMaterial[..ivLen];
            return ;
        }

        private static finishedHash newFinishedHash(ushort version, ptr<cipherSuite> _addr_cipherSuite)
        {
            ref cipherSuite cipherSuite = ref _addr_cipherSuite.val;

            slice<byte> buffer = default;
            if (version >= VersionTLS12)
            {
                buffer = new slice<byte>(new byte[] {  });
            }

            var (prf, hash) = prfAndHashForVersion(version, _addr_cipherSuite);
            if (hash != 0L)
            {
                return new finishedHash(hash.New(),hash.New(),nil,nil,buffer,version,prf);
            }

            return new finishedHash(sha1.New(),sha1.New(),md5.New(),md5.New(),buffer,version,prf);

        }

        // A finishedHash calculates the hash of a set of handshake messages suitable
        // for including in a Finished message.
        private partial struct finishedHash
        {
            public hash.Hash client;
            public hash.Hash server; // Prior to TLS 1.2, an additional MD5 hash is required.
            public hash.Hash clientMD5;
            public hash.Hash serverMD5; // In TLS 1.2, a full buffer is sadly required.
            public slice<byte> buffer;
            public ushort version;
            public Action<slice<byte>, slice<byte>, slice<byte>, slice<byte>> prf;
        }

        private static (long, error) Write(this ptr<finishedHash> _addr_h, slice<byte> msg)
        {
            long n = default;
            error err = default!;
            ref finishedHash h = ref _addr_h.val;

            h.client.Write(msg);
            h.server.Write(msg);

            if (h.version < VersionTLS12)
            {
                h.clientMD5.Write(msg);
                h.serverMD5.Write(msg);
            }

            if (h.buffer != null)
            {
                h.buffer = append(h.buffer, msg);
            }

            return (len(msg), error.As(null!)!);

        }

        private static slice<byte> Sum(this finishedHash h)
        {
            if (h.version >= VersionTLS12)
            {
                return h.client.Sum(null);
            }

            var @out = make_slice<byte>(0L, md5.Size + sha1.Size);
            out = h.clientMD5.Sum(out);
            return h.client.Sum(out);

        }

        // clientSum returns the contents of the verify_data member of a client's
        // Finished message.
        private static slice<byte> clientSum(this finishedHash h, slice<byte> masterSecret)
        {
            var @out = make_slice<byte>(finishedVerifyLength);
            h.prf(out, masterSecret, clientFinishedLabel, h.Sum());
            return out;
        }

        // serverSum returns the contents of the verify_data member of a server's
        // Finished message.
        private static slice<byte> serverSum(this finishedHash h, slice<byte> masterSecret)
        {
            var @out = make_slice<byte>(finishedVerifyLength);
            h.prf(out, masterSecret, serverFinishedLabel, h.Sum());
            return out;
        }

        // hashForClientCertificate returns the handshake messages so far, pre-hashed if
        // necessary, suitable for signing by a TLS client certificate.
        private static slice<byte> hashForClientCertificate(this finishedHash h, byte sigType, crypto.Hash hashAlg, slice<byte> masterSecret) => func((_, panic, __) =>
        {
            if ((h.version >= VersionTLS12 || sigType == signatureEd25519) && h.buffer == null)
            {
                panic("tls: handshake hash for a client certificate requested after discarding the handshake buffer");
            }

            if (sigType == signatureEd25519)
            {
                return h.buffer;
            }

            if (h.version >= VersionTLS12)
            {
                var hash = hashAlg.New();
                hash.Write(h.buffer);
                return hash.Sum(null);
            }

            if (sigType == signatureECDSA)
            {
                return h.server.Sum(null);
            }

            return h.Sum();

        });

        // discardHandshakeBuffer is called when there is no more need to
        // buffer the entirety of the handshake messages.
        private static void discardHandshakeBuffer(this ptr<finishedHash> _addr_h)
        {
            ref finishedHash h = ref _addr_h.val;

            h.buffer = null;
        }

        // noExportedKeyingMaterial is used as a value of
        // ConnectionState.ekm when renegotiation is enabled and thus
        // we wish to fail all key-material export requests.
        private static (slice<byte>, error) noExportedKeyingMaterial(@string label, slice<byte> context, long length)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            return (null, error.As(errors.New("crypto/tls: ExportKeyingMaterial is unavailable when renegotiation is enabled"))!);
        }

        // ekmFromMasterSecret generates exported keying material as defined in RFC 5705.
        private static Func<@string, slice<byte>, long, (slice<byte>, error)> ekmFromMasterSecret(ushort version, ptr<cipherSuite> _addr_suite, slice<byte> masterSecret, slice<byte> clientRandom, slice<byte> serverRandom)
        {
            ref cipherSuite suite = ref _addr_suite.val;

            return (label, context, length) =>
            {
                switch (label)
                {
                    case "client finished": 
                        // These values are reserved and may not be used.

                    case "server finished": 
                        // These values are reserved and may not be used.

                    case "master secret": 
                        // These values are reserved and may not be used.

                    case "key expansion": 
                        // These values are reserved and may not be used.
                        return (null, fmt.Errorf("crypto/tls: reserved ExportKeyingMaterial label: %s", label));
                        break;
                }

                var seedLen = len(serverRandom) + len(clientRandom);
                if (context != null)
                {
                    seedLen += 2L + len(context);
                }

                var seed = make_slice<byte>(0L, seedLen);

                seed = append(seed, clientRandom);
                seed = append(seed, serverRandom);

                if (context != null)
                {
                    if (len(context) >= 1L << (int)(16L))
                    {
                        return (null, fmt.Errorf("crypto/tls: ExportKeyingMaterial context too long"));
                    }

                    seed = append(seed, byte(len(context) >> (int)(8L)), byte(len(context)));
                    seed = append(seed, context);

                }

                var keyMaterial = make_slice<byte>(length);
                prfForVersion(version, _addr_suite)(keyMaterial, masterSecret, (slice<byte>)label, seed);
                return (keyMaterial, null);

            };

        }
    }
}}
