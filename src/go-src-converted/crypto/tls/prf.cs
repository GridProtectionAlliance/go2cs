// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 August 29 08:31:36 UTC
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
        // Split a premaster secret in two as specified in RFC 4346, section 5.
        private static (slice<byte>, slice<byte>) splitPreMasterSecret(slice<byte> secret)
        {
            s1 = secret[0L..(len(secret) + 1L) / 2L];
            s2 = secret[len(secret) / 2L..];
            return;
        }

        // pHash implements the P_hash function, as defined in RFC 4346, section 5.
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

        // prf10 implements the TLS 1.0 pseudo-random function, as defined in RFC 2246, section 5.
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

        // prf12 implements the TLS 1.2 pseudo-random function, as defined in RFC 5246, section 5.
        private static Action<slice<byte>, slice<byte>, slice<byte>, slice<byte>> prf12(Func<hash.Hash> hashFunc)
        {
            return (result, secret, label, seed) =>
            {
                var labelAndSeed = make_slice<byte>(len(label) + len(seed));
                copy(labelAndSeed, label);
                copy(labelAndSeed[len(label)..], seed);

                pHash(result, secret, labelAndSeed, hashFunc);
            }
;
        }

        // prf30 implements the SSL 3.0 pseudo-random function, as defined in
        // www.mozilla.org/projects/security/pki/nss/ssl/draft302.txt section 6.
        private static void prf30(slice<byte> result, slice<byte> secret, slice<byte> label, slice<byte> seed)
        {
            var hashSHA1 = sha1.New();
            var hashMD5 = md5.New();

            long done = 0L;
            long i = 0L; 
            // RFC 5246 section 6.3 says that the largest PRF output needed is 128
            // bytes. Since no more ciphersuites will be added to SSLv3, this will
            // remain true. Each iteration gives us 16 bytes so 10 iterations will
            // be sufficient.
            array<byte> b = new array<byte>(11L);
            while (done < len(result))
            {
                for (long j = 0L; j <= i; j++)
                {
                    b[j] = 'A' + byte(i);
                }


                hashSHA1.Reset();
                hashSHA1.Write(b[..i + 1L]);
                hashSHA1.Write(secret);
                hashSHA1.Write(seed);
                var digest = hashSHA1.Sum(null);

                hashMD5.Reset();
                hashMD5.Write(secret);
                hashMD5.Write(digest);

                done += copy(result[done..], hashMD5.Sum(null));
                i++;
            }

        }

        private static readonly long tlsRandomLength = 32L; // Length of a random nonce in TLS 1.1.
        private static readonly long masterSecretLength = 48L; // Length of a master secret in TLS 1.1.
        private static readonly long finishedVerifyLength = 12L; // Length of verify_data in a Finished message.

        private static slice<byte> masterSecretLabel = (slice<byte>)"master secret";
        private static slice<byte> keyExpansionLabel = (slice<byte>)"key expansion";
        private static slice<byte> clientFinishedLabel = (slice<byte>)"client finished";
        private static slice<byte> serverFinishedLabel = (slice<byte>)"server finished";

        private static (Action<slice<byte>, slice<byte>, slice<byte>, slice<byte>>, crypto.Hash) prfAndHashForVersion(ushort version, ref cipherSuite _suite) => func(_suite, (ref cipherSuite suite, Defer _, Panic panic, Recover __) =>
        {

            if (version == VersionSSL30) 
                return (prf30, crypto.Hash(0L));
            else if (version == VersionTLS10 || version == VersionTLS11) 
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

        private static Action<slice<byte>, slice<byte>, slice<byte>, slice<byte>> prfForVersion(ushort version, ref cipherSuite suite)
        {
            var (prf, _) = prfAndHashForVersion(version, suite);
            return prf;
        }

        // masterFromPreMasterSecret generates the master secret from the pre-master
        // secret. See http://tools.ietf.org/html/rfc5246#section-8.1
        private static slice<byte> masterFromPreMasterSecret(ushort version, ref cipherSuite suite, slice<byte> preMasterSecret, slice<byte> clientRandom, slice<byte> serverRandom)
        {
            var seed = make_slice<byte>(0L, len(clientRandom) + len(serverRandom));
            seed = append(seed, clientRandom);
            seed = append(seed, serverRandom);

            var masterSecret = make_slice<byte>(masterSecretLength);
            prfForVersion(version, suite)(masterSecret, preMasterSecret, masterSecretLabel, seed);
            return masterSecret;
        }

        // keysFromMasterSecret generates the connection keys from the master
        // secret, given the lengths of the MAC key, cipher key and IV, as defined in
        // RFC 2246, section 6.3.
        private static (slice<byte>, slice<byte>, slice<byte>, slice<byte>, slice<byte>, slice<byte>) keysFromMasterSecret(ushort version, ref cipherSuite suite, slice<byte> masterSecret, slice<byte> clientRandom, slice<byte> serverRandom, long macLen, long keyLen, long ivLen)
        {
            var seed = make_slice<byte>(0L, len(serverRandom) + len(clientRandom));
            seed = append(seed, serverRandom);
            seed = append(seed, clientRandom);

            long n = 2L * macLen + 2L * keyLen + 2L * ivLen;
            var keyMaterial = make_slice<byte>(n);
            prfForVersion(version, suite)(keyMaterial, masterSecret, keyExpansionLabel, seed);
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
            return;
        }

        // lookupTLSHash looks up the corresponding crypto.Hash for a given
        // hash from a TLS SignatureScheme.
        private static (crypto.Hash, error) lookupTLSHash(SignatureScheme signatureAlgorithm)
        {

            if (signatureAlgorithm == PKCS1WithSHA1 || signatureAlgorithm == ECDSAWithSHA1) 
                return (crypto.SHA1, null);
            else if (signatureAlgorithm == PKCS1WithSHA256 || signatureAlgorithm == PSSWithSHA256 || signatureAlgorithm == ECDSAWithP256AndSHA256) 
                return (crypto.SHA256, null);
            else if (signatureAlgorithm == PKCS1WithSHA384 || signatureAlgorithm == PSSWithSHA384 || signatureAlgorithm == ECDSAWithP384AndSHA384) 
                return (crypto.SHA384, null);
            else if (signatureAlgorithm == PKCS1WithSHA512 || signatureAlgorithm == PSSWithSHA512 || signatureAlgorithm == ECDSAWithP521AndSHA512) 
                return (crypto.SHA512, null);
            else 
                return (0L, fmt.Errorf("tls: unsupported signature algorithm: %#04x", signatureAlgorithm));
                    }

        private static finishedHash newFinishedHash(ushort version, ref cipherSuite cipherSuite)
        {
            slice<byte> buffer = default;
            if (version == VersionSSL30 || version >= VersionTLS12)
            {
                buffer = new slice<byte>(new byte[] {  });
            }
            var (prf, hash) = prfAndHashForVersion(version, cipherSuite);
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

        private static (long, error) Write(this ref finishedHash h, slice<byte> msg)
        {
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
            return (len(msg), null);
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

        // finishedSum30 calculates the contents of the verify_data member of a SSLv3
        // Finished message given the MD5 and SHA1 hashes of a set of handshake
        // messages.
        private static slice<byte> finishedSum30(hash.Hash md5, hash.Hash sha1, slice<byte> masterSecret, slice<byte> magic)
        {
            md5.Write(magic);
            md5.Write(masterSecret);
            md5.Write(ssl30Pad1[..]);
            var md5Digest = md5.Sum(null);

            md5.Reset();
            md5.Write(masterSecret);
            md5.Write(ssl30Pad2[..]);
            md5.Write(md5Digest);
            md5Digest = md5.Sum(null);

            sha1.Write(magic);
            sha1.Write(masterSecret);
            sha1.Write(ssl30Pad1[..40L]);
            var sha1Digest = sha1.Sum(null);

            sha1.Reset();
            sha1.Write(masterSecret);
            sha1.Write(ssl30Pad2[..40L]);
            sha1.Write(sha1Digest);
            sha1Digest = sha1.Sum(null);

            var ret = make_slice<byte>(len(md5Digest) + len(sha1Digest));
            copy(ret, md5Digest);
            copy(ret[len(md5Digest)..], sha1Digest);
            return ret;
        }

        private static array<byte> ssl3ClientFinishedMagic = new array<byte>(new byte[] { 0x43, 0x4c, 0x4e, 0x54 });
        private static array<byte> ssl3ServerFinishedMagic = new array<byte>(new byte[] { 0x53, 0x52, 0x56, 0x52 });

        // clientSum returns the contents of the verify_data member of a client's
        // Finished message.
        private static slice<byte> clientSum(this finishedHash h, slice<byte> masterSecret)
        {
            if (h.version == VersionSSL30)
            {
                return finishedSum30(h.clientMD5, h.client, masterSecret, ssl3ClientFinishedMagic[..]);
            }
            var @out = make_slice<byte>(finishedVerifyLength);
            h.prf(out, masterSecret, clientFinishedLabel, h.Sum());
            return out;
        }

        // serverSum returns the contents of the verify_data member of a server's
        // Finished message.
        private static slice<byte> serverSum(this finishedHash h, slice<byte> masterSecret)
        {
            if (h.version == VersionSSL30)
            {
                return finishedSum30(h.serverMD5, h.server, masterSecret, ssl3ServerFinishedMagic[..]);
            }
            var @out = make_slice<byte>(finishedVerifyLength);
            h.prf(out, masterSecret, serverFinishedLabel, h.Sum());
            return out;
        }

        // selectClientCertSignatureAlgorithm returns a SignatureScheme to sign a
        // client's CertificateVerify with, or an error if none can be found.
        private static (SignatureScheme, error) selectClientCertSignatureAlgorithm(this finishedHash h, slice<SignatureScheme> serverList, byte sigType)
        {
            foreach (var (_, v) in serverList)
            {
                if (signatureFromSignatureScheme(v) == sigType && isSupportedSignatureAlgorithm(v, supportedSignatureAlgorithms))
                {
                    return (v, null);
                }
            }
            return (0L, errors.New("tls: no supported signature algorithm found for signing client certificate"));
        }

        // hashForClientCertificate returns a digest, hash function, and TLS 1.2 hash
        // id suitable for signing by a TLS client certificate.
        private static (slice<byte>, crypto.Hash, error) hashForClientCertificate(this finishedHash h, byte sigType, SignatureScheme signatureAlgorithm, slice<byte> masterSecret) => func((_, panic, __) =>
        {
            if ((h.version == VersionSSL30 || h.version >= VersionTLS12) && h.buffer == null)
            {
                panic("a handshake hash for a client-certificate was requested after discarding the handshake buffer");
            }
            if (h.version == VersionSSL30)
            {
                if (sigType != signatureRSA)
                {
                    return (null, 0L, errors.New("tls: unsupported signature type for client certificate"));
                }
                var md5Hash = md5.New();
                md5Hash.Write(h.buffer);
                var sha1Hash = sha1.New();
                sha1Hash.Write(h.buffer);
                return (finishedSum30(md5Hash, sha1Hash, masterSecret, null), crypto.MD5SHA1, null);
            }
            if (h.version >= VersionTLS12)
            {
                var (hashAlg, err) = lookupTLSHash(signatureAlgorithm);
                if (err != null)
                {
                    return (null, 0L, err);
                }
                var hash = hashAlg.New();
                hash.Write(h.buffer);
                return (hash.Sum(null), hashAlg, null);
            }
            if (sigType == signatureECDSA)
            {
                return (h.server.Sum(null), crypto.SHA1, null);
            }
            return (h.Sum(), crypto.MD5SHA1, null);
        });

        // discardHandshakeBuffer is called when there is no more need to
        // buffer the entirety of the handshake messages.
        private static void discardHandshakeBuffer(this ref finishedHash h)
        {
            h.buffer = null;
        }
    }
}}
