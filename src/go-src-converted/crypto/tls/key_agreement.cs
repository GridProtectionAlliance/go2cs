// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 October 08 03:38:21 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\key_agreement.go
using crypto = go.crypto_package;
using md5 = go.crypto.md5_package;
using rsa = go.crypto.rsa_package;
using sha1 = go.crypto.sha1_package;
using x509 = go.crypto.x509_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        private static var errClientKeyExchange = errors.New("tls: invalid ClientKeyExchange message");
        private static var errServerKeyExchange = errors.New("tls: invalid ServerKeyExchange message");

        // rsaKeyAgreement implements the standard TLS key agreement where the client
        // encrypts the pre-master secret to the server's public key.
        private partial struct rsaKeyAgreement
        {
        }

        private static (ptr<serverKeyExchangeMsg>, error) generateServerKeyExchange(this rsaKeyAgreement ka, ptr<Config> _addr_config, ptr<Certificate> _addr_cert, ptr<clientHelloMsg> _addr_clientHello, ptr<serverHelloMsg> _addr_hello)
        {
            ptr<serverKeyExchangeMsg> _p0 = default!;
            error _p0 = default!;
            ref Config config = ref _addr_config.val;
            ref Certificate cert = ref _addr_cert.val;
            ref clientHelloMsg clientHello = ref _addr_clientHello.val;
            ref serverHelloMsg hello = ref _addr_hello.val;

            return (_addr_null!, error.As(null!)!);
        }

        private static (slice<byte>, error) processClientKeyExchange(this rsaKeyAgreement ka, ptr<Config> _addr_config, ptr<Certificate> _addr_cert, ptr<clientKeyExchangeMsg> _addr_ckx, ushort version)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Config config = ref _addr_config.val;
            ref Certificate cert = ref _addr_cert.val;
            ref clientKeyExchangeMsg ckx = ref _addr_ckx.val;

            if (len(ckx.ciphertext) < 2L)
            {
                return (null, error.As(errClientKeyExchange)!);
            }

            var ciphertextLen = int(ckx.ciphertext[0L]) << (int)(8L) | int(ckx.ciphertext[1L]);
            if (ciphertextLen != len(ckx.ciphertext) - 2L)
            {
                return (null, error.As(errClientKeyExchange)!);
            }

            var ciphertext = ckx.ciphertext[2L..];

            crypto.Decrypter (priv, ok) = cert.PrivateKey._<crypto.Decrypter>();
            if (!ok)
            {
                return (null, error.As(errors.New("tls: certificate private key does not implement crypto.Decrypter"))!);
            } 
            // Perform constant time RSA PKCS #1 v1.5 decryption
            var (preMasterSecret, err) = priv.Decrypt(config.rand(), ciphertext, addr(new rsa.PKCS1v15DecryptOptions(SessionKeyLen:48)));
            if (err != null)
            {
                return (null, error.As(err)!);
            } 
            // We don't check the version number in the premaster secret. For one,
            // by checking it, we would leak information about the validity of the
            // encrypted pre-master secret. Secondly, it provides only a small
            // benefit against a downgrade attack and some implementations send the
            // wrong version anyway. See the discussion at the end of section
            // 7.4.7.1 of RFC 4346.
            return (preMasterSecret, error.As(null!)!);

        }

        private static error processServerKeyExchange(this rsaKeyAgreement ka, ptr<Config> _addr_config, ptr<clientHelloMsg> _addr_clientHello, ptr<serverHelloMsg> _addr_serverHello, ptr<x509.Certificate> _addr_cert, ptr<serverKeyExchangeMsg> _addr_skx)
        {
            ref Config config = ref _addr_config.val;
            ref clientHelloMsg clientHello = ref _addr_clientHello.val;
            ref serverHelloMsg serverHello = ref _addr_serverHello.val;
            ref x509.Certificate cert = ref _addr_cert.val;
            ref serverKeyExchangeMsg skx = ref _addr_skx.val;

            return error.As(errors.New("tls: unexpected ServerKeyExchange"))!;
        }

        private static (slice<byte>, ptr<clientKeyExchangeMsg>, error) generateClientKeyExchange(this rsaKeyAgreement ka, ptr<Config> _addr_config, ptr<clientHelloMsg> _addr_clientHello, ptr<x509.Certificate> _addr_cert)
        {
            slice<byte> _p0 = default;
            ptr<clientKeyExchangeMsg> _p0 = default!;
            error _p0 = default!;
            ref Config config = ref _addr_config.val;
            ref clientHelloMsg clientHello = ref _addr_clientHello.val;
            ref x509.Certificate cert = ref _addr_cert.val;

            var preMasterSecret = make_slice<byte>(48L);
            preMasterSecret[0L] = byte(clientHello.vers >> (int)(8L));
            preMasterSecret[1L] = byte(clientHello.vers);
            var (_, err) = io.ReadFull(config.rand(), preMasterSecret[2L..]);
            if (err != null)
            {
                return (null, _addr_null!, error.As(err)!);
            }

            var (encrypted, err) = rsa.EncryptPKCS1v15(config.rand(), cert.PublicKey._<ptr<rsa.PublicKey>>(), preMasterSecret);
            if (err != null)
            {
                return (null, _addr_null!, error.As(err)!);
            }

            ptr<clientKeyExchangeMsg> ckx = @new<clientKeyExchangeMsg>();
            ckx.ciphertext = make_slice<byte>(len(encrypted) + 2L);
            ckx.ciphertext[0L] = byte(len(encrypted) >> (int)(8L));
            ckx.ciphertext[1L] = byte(len(encrypted));
            copy(ckx.ciphertext[2L..], encrypted);
            return (preMasterSecret, _addr_ckx!, error.As(null!)!);

        }

        // sha1Hash calculates a SHA1 hash over the given byte slices.
        private static slice<byte> sha1Hash(slice<slice<byte>> slices)
        {
            var hsha1 = sha1.New();
            foreach (var (_, slice) in slices)
            {
                hsha1.Write(slice);
            }
            return hsha1.Sum(null);

        }

        // md5SHA1Hash implements TLS 1.0's hybrid hash function which consists of the
        // concatenation of an MD5 and SHA1 hash.
        private static slice<byte> md5SHA1Hash(slice<slice<byte>> slices)
        {
            var md5sha1 = make_slice<byte>(md5.Size + sha1.Size);
            var hmd5 = md5.New();
            foreach (var (_, slice) in slices)
            {
                hmd5.Write(slice);
            }
            copy(md5sha1, hmd5.Sum(null));
            copy(md5sha1[md5.Size..], sha1Hash(slices));
            return md5sha1;

        }

        // hashForServerKeyExchange hashes the given slices and returns their digest
        // using the given hash function (for >= TLS 1.2) or using a default based on
        // the sigType (for earlier TLS versions). For Ed25519 signatures, which don't
        // do pre-hashing, it returns the concatenation of the slices.
        private static slice<byte> hashForServerKeyExchange(byte sigType, crypto.Hash hashFunc, ushort version, params slice<byte>[] slices)
        {
            slices = slices.Clone();

            if (sigType == signatureEd25519)
            {
                slice<byte> signed = default;
                {
                    var slice__prev1 = slice;

                    foreach (var (_, __slice) in slices)
                    {
                        slice = __slice;
                        signed = append(signed, slice);
                    }

                    slice = slice__prev1;
                }

                return signed;

            }

            if (version >= VersionTLS12)
            {
                var h = hashFunc.New();
                {
                    var slice__prev1 = slice;

                    foreach (var (_, __slice) in slices)
                    {
                        slice = __slice;
                        h.Write(slice);
                    }

                    slice = slice__prev1;
                }

                var digest = h.Sum(null);
                return digest;

            }

            if (sigType == signatureECDSA)
            {
                return sha1Hash(slices);
            }

            return md5SHA1Hash(slices);

        }

        // ecdheKeyAgreement implements a TLS key agreement where the server
        // generates an ephemeral EC public/private key pair and signs it. The
        // pre-master secret is then calculated using ECDH. The signature may
        // be ECDSA, Ed25519 or RSA.
        private partial struct ecdheKeyAgreement
        {
            public ushort version;
            public bool isRSA;
            public ecdheParameters @params; // ckx and preMasterSecret are generated in processServerKeyExchange
// and returned in generateClientKeyExchange.
            public ptr<clientKeyExchangeMsg> ckx;
            public slice<byte> preMasterSecret;
        }

        private static (ptr<serverKeyExchangeMsg>, error) generateServerKeyExchange(this ptr<ecdheKeyAgreement> _addr_ka, ptr<Config> _addr_config, ptr<Certificate> _addr_cert, ptr<clientHelloMsg> _addr_clientHello, ptr<serverHelloMsg> _addr_hello)
        {
            ptr<serverKeyExchangeMsg> _p0 = default!;
            error _p0 = default!;
            ref ecdheKeyAgreement ka = ref _addr_ka.val;
            ref Config config = ref _addr_config.val;
            ref Certificate cert = ref _addr_cert.val;
            ref clientHelloMsg clientHello = ref _addr_clientHello.val;
            ref serverHelloMsg hello = ref _addr_hello.val;

            CurveID curveID = default;
            foreach (var (_, c) in clientHello.supportedCurves)
            {
                if (config.supportsCurve(c))
                {
                    curveID = c;
                    break;
                }

            }
            if (curveID == 0L)
            {
                return (_addr_null!, error.As(errors.New("tls: no supported elliptic curves offered"))!);
            }

            {
                var (_, ok) = curveForCurveID(curveID);

                if (curveID != X25519 && !ok)
                {
                    return (_addr_null!, error.As(errors.New("tls: CurvePreferences includes unsupported curve"))!);
                }

            }


            var (params, err) = generateECDHEParameters(config.rand(), curveID);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            ka.@params = params; 

            // See RFC 4492, Section 5.4.
            var ecdhePublic = @params.PublicKey();
            var serverECDHEParams = make_slice<byte>(1L + 2L + 1L + len(ecdhePublic));
            serverECDHEParams[0L] = 3L; // named curve
            serverECDHEParams[1L] = byte(curveID >> (int)(8L));
            serverECDHEParams[2L] = byte(curveID);
            serverECDHEParams[3L] = byte(len(ecdhePublic));
            copy(serverECDHEParams[4L..], ecdhePublic);

            crypto.Signer (priv, ok) = cert.PrivateKey._<crypto.Signer>();
            if (!ok)
            {
                return (_addr_null!, error.As(fmt.Errorf("tls: certificate private key of type %T does not implement crypto.Signer", cert.PrivateKey))!);
            }

            SignatureScheme signatureAlgorithm = default;
            byte sigType = default;
            crypto.Hash sigHash = default;
            if (ka.version >= VersionTLS12)
            {
                signatureAlgorithm, err = selectSignatureScheme(ka.version, cert, clientHello.supportedSignatureAlgorithms);
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

                sigType, sigHash, err = typeAndHashFromSignatureScheme(signatureAlgorithm);
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }
            else
            {
                sigType, sigHash, err = legacyTypeAndHashFromPublicKey(priv.Public());
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }

            if ((sigType == signaturePKCS1v15 || sigType == signatureRSAPSS) != ka.isRSA)
            {
                return (_addr_null!, error.As(errors.New("tls: certificate cannot be used with the selected cipher suite"))!);
            }

            var signed = hashForServerKeyExchange(sigType, sigHash, ka.version, clientHello.random, hello.random, serverECDHEParams);

            var signOpts = crypto.SignerOpts(sigHash);
            if (sigType == signatureRSAPSS)
            {
                signOpts = addr(new rsa.PSSOptions(SaltLength:rsa.PSSSaltLengthEqualsHash,Hash:sigHash));
            }

            var (sig, err) = priv.Sign(config.rand(), signed, signOpts);
            if (err != null)
            {
                return (_addr_null!, error.As(errors.New("tls: failed to sign ECDHE parameters: " + err.Error()))!);
            }

            ptr<serverKeyExchangeMsg> skx = @new<serverKeyExchangeMsg>();
            long sigAndHashLen = 0L;
            if (ka.version >= VersionTLS12)
            {
                sigAndHashLen = 2L;
            }

            skx.key = make_slice<byte>(len(serverECDHEParams) + sigAndHashLen + 2L + len(sig));
            copy(skx.key, serverECDHEParams);
            var k = skx.key[len(serverECDHEParams)..];
            if (ka.version >= VersionTLS12)
            {
                k[0L] = byte(signatureAlgorithm >> (int)(8L));
                k[1L] = byte(signatureAlgorithm);
                k = k[2L..];
            }

            k[0L] = byte(len(sig) >> (int)(8L));
            k[1L] = byte(len(sig));
            copy(k[2L..], sig);

            return (_addr_skx!, error.As(null!)!);

        }

        private static (slice<byte>, error) processClientKeyExchange(this ptr<ecdheKeyAgreement> _addr_ka, ptr<Config> _addr_config, ptr<Certificate> _addr_cert, ptr<clientKeyExchangeMsg> _addr_ckx, ushort version)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref ecdheKeyAgreement ka = ref _addr_ka.val;
            ref Config config = ref _addr_config.val;
            ref Certificate cert = ref _addr_cert.val;
            ref clientKeyExchangeMsg ckx = ref _addr_ckx.val;

            if (len(ckx.ciphertext) == 0L || int(ckx.ciphertext[0L]) != len(ckx.ciphertext) - 1L)
            {
                return (null, error.As(errClientKeyExchange)!);
            }

            var preMasterSecret = ka.@params.SharedKey(ckx.ciphertext[1L..]);
            if (preMasterSecret == null)
            {
                return (null, error.As(errClientKeyExchange)!);
            }

            return (preMasterSecret, error.As(null!)!);

        }

        private static error processServerKeyExchange(this ptr<ecdheKeyAgreement> _addr_ka, ptr<Config> _addr_config, ptr<clientHelloMsg> _addr_clientHello, ptr<serverHelloMsg> _addr_serverHello, ptr<x509.Certificate> _addr_cert, ptr<serverKeyExchangeMsg> _addr_skx)
        {
            ref ecdheKeyAgreement ka = ref _addr_ka.val;
            ref Config config = ref _addr_config.val;
            ref clientHelloMsg clientHello = ref _addr_clientHello.val;
            ref serverHelloMsg serverHello = ref _addr_serverHello.val;
            ref x509.Certificate cert = ref _addr_cert.val;
            ref serverKeyExchangeMsg skx = ref _addr_skx.val;

            if (len(skx.key) < 4L)
            {
                return error.As(errServerKeyExchange)!;
            }

            if (skx.key[0L] != 3L)
            { // named curve
                return error.As(errors.New("tls: server selected unsupported curve"))!;

            }

            var curveID = CurveID(skx.key[1L]) << (int)(8L) | CurveID(skx.key[2L]);

            var publicLen = int(skx.key[3L]);
            if (publicLen + 4L > len(skx.key))
            {
                return error.As(errServerKeyExchange)!;
            }

            var serverECDHEParams = skx.key[..4L + publicLen];
            var publicKey = serverECDHEParams[4L..];

            var sig = skx.key[4L + publicLen..];
            if (len(sig) < 2L)
            {
                return error.As(errServerKeyExchange)!;
            }

            {
                var (_, ok) = curveForCurveID(curveID);

                if (curveID != X25519 && !ok)
                {
                    return error.As(errors.New("tls: server selected unsupported curve"))!;
                }

            }


            var (params, err) = generateECDHEParameters(config.rand(), curveID);
            if (err != null)
            {
                return error.As(err)!;
            }

            ka.@params = params;

            ka.preMasterSecret = @params.SharedKey(publicKey);
            if (ka.preMasterSecret == null)
            {
                return error.As(errServerKeyExchange)!;
            }

            var ourPublicKey = @params.PublicKey();
            ka.ckx = @new<clientKeyExchangeMsg>();
            ka.ckx.ciphertext = make_slice<byte>(1L + len(ourPublicKey));
            ka.ckx.ciphertext[0L] = byte(len(ourPublicKey));
            copy(ka.ckx.ciphertext[1L..], ourPublicKey);

            byte sigType = default;
            crypto.Hash sigHash = default;
            if (ka.version >= VersionTLS12)
            {
                var signatureAlgorithm = SignatureScheme(sig[0L]) << (int)(8L) | SignatureScheme(sig[1L]);
                sig = sig[2L..];
                if (len(sig) < 2L)
                {
                    return error.As(errServerKeyExchange)!;
                }

                if (!isSupportedSignatureAlgorithm(signatureAlgorithm, clientHello.supportedSignatureAlgorithms))
                {
                    return error.As(errors.New("tls: certificate used with invalid signature algorithm"))!;
                }

                sigType, sigHash, err = typeAndHashFromSignatureScheme(signatureAlgorithm);
                if (err != null)
                {
                    return error.As(err)!;
                }

            }
            else
            {
                sigType, sigHash, err = legacyTypeAndHashFromPublicKey(cert.PublicKey);
                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            if ((sigType == signaturePKCS1v15 || sigType == signatureRSAPSS) != ka.isRSA)
            {
                return error.As(errServerKeyExchange)!;
            }

            var sigLen = int(sig[0L]) << (int)(8L) | int(sig[1L]);
            if (sigLen + 2L != len(sig))
            {
                return error.As(errServerKeyExchange)!;
            }

            sig = sig[2L..];

            var signed = hashForServerKeyExchange(sigType, sigHash, ka.version, clientHello.random, serverHello.random, serverECDHEParams);
            {
                var err = verifyHandshakeSignature(sigType, cert.PublicKey, sigHash, signed, sig);

                if (err != null)
                {
                    return error.As(errors.New("tls: invalid signature by the server certificate: " + err.Error()))!;
                }

            }

            return error.As(null!)!;

        }

        private static (slice<byte>, ptr<clientKeyExchangeMsg>, error) generateClientKeyExchange(this ptr<ecdheKeyAgreement> _addr_ka, ptr<Config> _addr_config, ptr<clientHelloMsg> _addr_clientHello, ptr<x509.Certificate> _addr_cert)
        {
            slice<byte> _p0 = default;
            ptr<clientKeyExchangeMsg> _p0 = default!;
            error _p0 = default!;
            ref ecdheKeyAgreement ka = ref _addr_ka.val;
            ref Config config = ref _addr_config.val;
            ref clientHelloMsg clientHello = ref _addr_clientHello.val;
            ref x509.Certificate cert = ref _addr_cert.val;

            if (ka.ckx == null)
            {
                return (null, _addr_null!, error.As(errors.New("tls: missing ServerKeyExchange message"))!);
            }

            return (ka.preMasterSecret, _addr_ka.ckx!, error.As(null!)!);

        }
    }
}}
