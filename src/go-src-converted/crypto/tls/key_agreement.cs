// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 August 29 08:31:33 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\key_agreement.go
using crypto = go.crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using md5 = go.crypto.md5_package;
using rsa = go.crypto.rsa_package;
using sha1 = go.crypto.sha1_package;
using x509 = go.crypto.x509_package;
using asn1 = go.encoding.asn1_package;
using errors = go.errors_package;
using io = go.io_package;
using big = go.math.big_package;

using curve25519 = go.golang_org.x.crypto.curve25519_package;
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

        private static (ref serverKeyExchangeMsg, error) generateServerKeyExchange(this rsaKeyAgreement ka, ref Config config, ref Certificate cert, ref clientHelloMsg clientHello, ref serverHelloMsg hello)
        {
            return (null, null);
        }

        private static (slice<byte>, error) processClientKeyExchange(this rsaKeyAgreement ka, ref Config config, ref Certificate cert, ref clientKeyExchangeMsg ckx, ushort version)
        {
            if (len(ckx.ciphertext) < 2L)
            {
                return (null, errClientKeyExchange);
            }
            var ciphertext = ckx.ciphertext;
            if (version != VersionSSL30)
            {
                var ciphertextLen = int(ckx.ciphertext[0L]) << (int)(8L) | int(ckx.ciphertext[1L]);
                if (ciphertextLen != len(ckx.ciphertext) - 2L)
                {
                    return (null, errClientKeyExchange);
                }
                ciphertext = ckx.ciphertext[2L..];
            }
            crypto.Decrypter (priv, ok) = cert.PrivateKey._<crypto.Decrypter>();
            if (!ok)
            {
                return (null, errors.New("tls: certificate private key does not implement crypto.Decrypter"));
            } 
            // Perform constant time RSA PKCS#1 v1.5 decryption
            var (preMasterSecret, err) = priv.Decrypt(config.rand(), ciphertext, ref new rsa.PKCS1v15DecryptOptions(SessionKeyLen:48));
            if (err != null)
            {
                return (null, err);
            } 
            // We don't check the version number in the premaster secret. For one,
            // by checking it, we would leak information about the validity of the
            // encrypted pre-master secret. Secondly, it provides only a small
            // benefit against a downgrade attack and some implementations send the
            // wrong version anyway. See the discussion at the end of section
            // 7.4.7.1 of RFC 4346.
            return (preMasterSecret, null);
        }

        private static error processServerKeyExchange(this rsaKeyAgreement ka, ref Config config, ref clientHelloMsg clientHello, ref serverHelloMsg serverHello, ref x509.Certificate cert, ref serverKeyExchangeMsg skx)
        {
            return error.As(errors.New("tls: unexpected ServerKeyExchange"));
        }

        private static (slice<byte>, ref clientKeyExchangeMsg, error) generateClientKeyExchange(this rsaKeyAgreement ka, ref Config config, ref clientHelloMsg clientHello, ref x509.Certificate cert)
        {
            var preMasterSecret = make_slice<byte>(48L);
            preMasterSecret[0L] = byte(clientHello.vers >> (int)(8L));
            preMasterSecret[1L] = byte(clientHello.vers);
            var (_, err) = io.ReadFull(config.rand(), preMasterSecret[2L..]);
            if (err != null)
            {
                return (null, null, err);
            }
            var (encrypted, err) = rsa.EncryptPKCS1v15(config.rand(), cert.PublicKey._<ref rsa.PublicKey>(), preMasterSecret);
            if (err != null)
            {
                return (null, null, err);
            }
            ptr<clientKeyExchangeMsg> ckx = @new<clientKeyExchangeMsg>();
            ckx.ciphertext = make_slice<byte>(len(encrypted) + 2L);
            ckx.ciphertext[0L] = byte(len(encrypted) >> (int)(8L));
            ckx.ciphertext[1L] = byte(len(encrypted));
            copy(ckx.ciphertext[2L..], encrypted);
            return (preMasterSecret, ckx, null);
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
        // and the identifier of the hash function used. The signatureAlgorithm argument
        // is only used for >= TLS 1.2 and identifies the hash function to use.
        private static (slice<byte>, crypto.Hash, error) hashForServerKeyExchange(byte sigType, SignatureScheme signatureAlgorithm, ushort version, params slice<byte>[] slices)
        {
            slices = slices.Clone();

            if (version >= VersionTLS12)
            {
                if (!isSupportedSignatureAlgorithm(signatureAlgorithm, supportedSignatureAlgorithms))
                {
                    return (null, crypto.Hash(0L), errors.New("tls: unsupported hash function used by peer"));
                }
                var (hashFunc, err) = lookupTLSHash(signatureAlgorithm);
                if (err != null)
                {
                    return (null, crypto.Hash(0L), err);
                }
                var h = hashFunc.New();
                foreach (var (_, slice) in slices)
                {
                    h.Write(slice);
                }
                var digest = h.Sum(null);
                return (digest, hashFunc, null);
            }
            if (sigType == signatureECDSA)
            {
                return (sha1Hash(slices), crypto.SHA1, null);
            }
            return (md5SHA1Hash(slices), crypto.MD5SHA1, null);
        }

        // pickTLS12HashForSignature returns a TLS 1.2 hash identifier for signing a
        // ServerKeyExchange given the signature type being used and the client's
        // advertised list of supported signature and hash combinations.
        private static (SignatureScheme, error) pickTLS12HashForSignature(byte sigType, slice<SignatureScheme> clientList)
        {
            if (len(clientList) == 0L)
            { 
                // If the client didn't specify any signature_algorithms
                // extension then we can assume that it supports SHA1. See
                // http://tools.ietf.org/html/rfc5246#section-7.4.1.4.1

                if (sigType == signatureRSA) 
                    return (PKCS1WithSHA1, null);
                else if (sigType == signatureECDSA) 
                    return (ECDSAWithSHA1, null);
                else 
                    return (0L, errors.New("tls: unknown signature algorithm"));
                            }
            foreach (var (_, sigAlg) in clientList)
            {
                if (signatureFromSignatureScheme(sigAlg) != sigType)
                {
                    continue;
                }
                if (isSupportedSignatureAlgorithm(sigAlg, supportedSignatureAlgorithms))
                {
                    return (sigAlg, null);
                }
            }
            return (0L, errors.New("tls: client doesn't support any common hash functions"));
        }

        private static (elliptic.Curve, bool) curveForCurveID(CurveID id)
        {

            if (id == CurveP256) 
                return (elliptic.P256(), true);
            else if (id == CurveP384) 
                return (elliptic.P384(), true);
            else if (id == CurveP521) 
                return (elliptic.P521(), true);
            else 
                return (null, false);
                    }

        // ecdheRSAKeyAgreement implements a TLS key agreement where the server
        // generates an ephemeral EC public/private key pair and signs it. The
        // pre-master secret is then calculated using ECDH. The signature may
        // either be ECDSA or RSA.
        private partial struct ecdheKeyAgreement
        {
            public ushort version;
            public byte sigType;
            public slice<byte> privateKey;
            public CurveID curveid; // publicKey is used to store the peer's public value when X25519 is
// being used.
            public slice<byte> publicKey; // x and y are used to store the peer's public value when one of the
// NIST curves is being used.
            public ptr<big.Int> x;
            public ptr<big.Int> y;
        }

        private static (ref serverKeyExchangeMsg, error) generateServerKeyExchange(this ref ecdheKeyAgreement ka, ref Config config, ref Certificate cert, ref clientHelloMsg clientHello, ref serverHelloMsg hello)
        {
            var preferredCurves = config.curvePreferences();

NextCandidate:

            foreach (var (_, candidate) in preferredCurves)
            {
                foreach (var (_, c) in clientHello.supportedCurves)
                {
                    if (candidate == c)
                    {
                        ka.curveid = c;
                        _breakNextCandidate = true;
                        break;
                    }
                }
            }
            if (ka.curveid == 0L)
            {
                return (null, errors.New("tls: no supported elliptic curves offered"));
            }
            slice<byte> ecdhePublic = default;

            if (ka.curveid == X25519)
            {
                array<byte> scalar = new array<byte>(32L);                array<byte> @public = new array<byte>(32L);

                {
                    error err__prev2 = err;

                    var (_, err) = io.ReadFull(config.rand(), scalar[..]);

                    if (err != null)
                    {
                        return (null, err);
                    }

                    err = err__prev2;

                }

                curve25519.ScalarBaseMult(ref public, ref scalar);
                ka.privateKey = scalar[..];
                ecdhePublic = public[..];
            }
            else
            {
                var (curve, ok) = curveForCurveID(ka.curveid);
                if (!ok)
                {
                    return (null, errors.New("tls: preferredCurves includes unsupported curve"));
                }
                ref big.Int x = default;                ref big.Int y = default;

                error err = default;
                ka.privateKey, x, y, err = elliptic.GenerateKey(curve, config.rand());
                if (err != null)
                {
                    return (null, err);
                }
                ecdhePublic = elliptic.Marshal(curve, x, y);
            } 

            // http://tools.ietf.org/html/rfc4492#section-5.4
            var serverECDHParams = make_slice<byte>(1L + 2L + 1L + len(ecdhePublic));
            serverECDHParams[0L] = 3L; // named curve
            serverECDHParams[1L] = byte(ka.curveid >> (int)(8L));
            serverECDHParams[2L] = byte(ka.curveid);
            serverECDHParams[3L] = byte(len(ecdhePublic));
            copy(serverECDHParams[4L..], ecdhePublic);

            SignatureScheme signatureAlgorithm = default;

            if (ka.version >= VersionTLS12)
            {
                err = default;
                signatureAlgorithm, err = pickTLS12HashForSignature(ka.sigType, clientHello.supportedSignatureAlgorithms);
                if (err != null)
                {
                    return (null, err);
                }
            }
            var (digest, hashFunc, err) = hashForServerKeyExchange(ka.sigType, signatureAlgorithm, ka.version, clientHello.random, hello.random, serverECDHParams);
            if (err != null)
            {
                return (null, err);
            }
            crypto.Signer (priv, ok) = cert.PrivateKey._<crypto.Signer>();
            if (!ok)
            {
                return (null, errors.New("tls: certificate private key does not implement crypto.Signer"));
            }
            slice<byte> sig = default;

            if (ka.sigType == signatureECDSA) 
                ref ecdsa.PublicKey (_, ok) = priv.Public()._<ref ecdsa.PublicKey>();
                if (!ok)
                {
                    return (null, errors.New("tls: ECDHE ECDSA requires an ECDSA server key"));
                }
            else if (ka.sigType == signatureRSA) 
                (_, ok) = priv.Public()._<ref rsa.PublicKey>();
                if (!ok)
                {
                    return (null, errors.New("tls: ECDHE RSA requires a RSA server key"));
                }
            else 
                return (null, errors.New("tls: unknown ECDHE signature algorithm"));
                        sig, err = priv.Sign(config.rand(), digest, hashFunc);
            if (err != null)
            {
                return (null, errors.New("tls: failed to sign ECDHE parameters: " + err.Error()));
            }
            ptr<serverKeyExchangeMsg> skx = @new<serverKeyExchangeMsg>();
            long sigAndHashLen = 0L;
            if (ka.version >= VersionTLS12)
            {
                sigAndHashLen = 2L;
            }
            skx.key = make_slice<byte>(len(serverECDHParams) + sigAndHashLen + 2L + len(sig));
            copy(skx.key, serverECDHParams);
            var k = skx.key[len(serverECDHParams)..];
            if (ka.version >= VersionTLS12)
            {
                k[0L] = byte(signatureAlgorithm >> (int)(8L));
                k[1L] = byte(signatureAlgorithm);
                k = k[2L..];
            }
            k[0L] = byte(len(sig) >> (int)(8L));
            k[1L] = byte(len(sig));
            copy(k[2L..], sig);

            return (skx, null);
        }

        private static (slice<byte>, error) processClientKeyExchange(this ref ecdheKeyAgreement _ka, ref Config _config, ref Certificate _cert, ref clientKeyExchangeMsg _ckx, ushort version) => func(_ka, _config, _cert, _ckx, (ref ecdheKeyAgreement ka, ref Config config, ref Certificate cert, ref clientKeyExchangeMsg ckx, Defer _, Panic panic, Recover __) =>
        {
            if (len(ckx.ciphertext) == 0L || int(ckx.ciphertext[0L]) != len(ckx.ciphertext) - 1L)
            {
                return (null, errClientKeyExchange);
            }
            if (ka.curveid == X25519)
            {
                if (len(ckx.ciphertext) != 1L + 32L)
                {
                    return (null, errClientKeyExchange);
                }
                array<byte> theirPublic = new array<byte>(32L);                array<byte> sharedKey = new array<byte>(32L);                array<byte> scalar = new array<byte>(32L);

                copy(theirPublic[..], ckx.ciphertext[1L..]);
                copy(scalar[..], ka.privateKey);
                curve25519.ScalarMult(ref sharedKey, ref scalar, ref theirPublic);
                return (sharedKey[..], null);
            }
            var (curve, ok) = curveForCurveID(ka.curveid);
            if (!ok)
            {
                panic("internal error");
            }
            var (x, y) = elliptic.Unmarshal(curve, ckx.ciphertext[1L..]); // Unmarshal also checks whether the given point is on the curve
            if (x == null)
            {
                return (null, errClientKeyExchange);
            }
            x, _ = curve.ScalarMult(x, y, ka.privateKey);
            var preMasterSecret = make_slice<byte>((curve.Params().BitSize + 7L) >> (int)(3L));
            var xBytes = x.Bytes();
            copy(preMasterSecret[len(preMasterSecret) - len(xBytes)..], xBytes);

            return (preMasterSecret, null);
        });

        private static error processServerKeyExchange(this ref ecdheKeyAgreement ka, ref Config config, ref clientHelloMsg clientHello, ref serverHelloMsg serverHello, ref x509.Certificate cert, ref serverKeyExchangeMsg skx)
        {
            if (len(skx.key) < 4L)
            {
                return error.As(errServerKeyExchange);
            }
            if (skx.key[0L] != 3L)
            { // named curve
                return error.As(errors.New("tls: server selected unsupported curve"));
            }
            ka.curveid = CurveID(skx.key[1L]) << (int)(8L) | CurveID(skx.key[2L]);

            var publicLen = int(skx.key[3L]);
            if (publicLen + 4L > len(skx.key))
            {
                return error.As(errServerKeyExchange);
            }
            var serverECDHParams = skx.key[..4L + publicLen];
            var publicKey = serverECDHParams[4L..];

            var sig = skx.key[4L + publicLen..];
            if (len(sig) < 2L)
            {
                return error.As(errServerKeyExchange);
            }
            if (ka.curveid == X25519)
            {
                if (len(publicKey) != 32L)
                {
                    return error.As(errors.New("tls: bad X25519 public value"));
                }
                ka.publicKey = publicKey;
            }
            else
            {
                var (curve, ok) = curveForCurveID(ka.curveid);
                if (!ok)
                {
                    return error.As(errors.New("tls: server selected unsupported curve"));
                }
                ka.x, ka.y = elliptic.Unmarshal(curve, publicKey); // Unmarshal also checks whether the given point is on the curve
                if (ka.x == null)
                {
                    return error.As(errServerKeyExchange);
                }
            }
            SignatureScheme signatureAlgorithm = default;
            if (ka.version >= VersionTLS12)
            { 
                // handle SignatureAndHashAlgorithm
                signatureAlgorithm = SignatureScheme(sig[0L]) << (int)(8L) | SignatureScheme(sig[1L]);
                if (signatureFromSignatureScheme(signatureAlgorithm) != ka.sigType)
                {
                    return error.As(errServerKeyExchange);
                }
                sig = sig[2L..];
                if (len(sig) < 2L)
                {
                    return error.As(errServerKeyExchange);
                }
            }
            var sigLen = int(sig[0L]) << (int)(8L) | int(sig[1L]);
            if (sigLen + 2L != len(sig))
            {
                return error.As(errServerKeyExchange);
            }
            sig = sig[2L..];

            var (digest, hashFunc, err) = hashForServerKeyExchange(ka.sigType, signatureAlgorithm, ka.version, clientHello.random, serverHello.random, serverECDHParams);
            if (err != null)
            {
                return error.As(err);
            }

            if (ka.sigType == signatureECDSA) 
                ref ecdsa.PublicKey (pubKey, ok) = cert.PublicKey._<ref ecdsa.PublicKey>();
                if (!ok)
                {
                    return error.As(errors.New("tls: ECDHE ECDSA requires a ECDSA server public key"));
                }
                ptr<object> ecdsaSig = @new<ecdsaSignature>();
                {
                    var (_, err) = asn1.Unmarshal(sig, ecdsaSig);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
                if (ecdsaSig.R.Sign() <= 0L || ecdsaSig.S.Sign() <= 0L)
                {
                    return error.As(errors.New("tls: ECDSA signature contained zero or negative values"));
                }
                if (!ecdsa.Verify(pubKey, digest, ecdsaSig.R, ecdsaSig.S))
                {
                    return error.As(errors.New("tls: ECDSA verification failure"));
                }
            else if (ka.sigType == signatureRSA) 
                (pubKey, ok) = cert.PublicKey._<ref rsa.PublicKey>();
                if (!ok)
                {
                    return error.As(errors.New("tls: ECDHE RSA requires a RSA server public key"));
                }
                {
                    var err = rsa.VerifyPKCS1v15(pubKey, hashFunc, digest, sig);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            else 
                return error.As(errors.New("tls: unknown ECDHE signature algorithm"));
                        return error.As(null);
        }

        private static (slice<byte>, ref clientKeyExchangeMsg, error) generateClientKeyExchange(this ref ecdheKeyAgreement _ka, ref Config _config, ref clientHelloMsg _clientHello, ref x509.Certificate _cert) => func(_ka, _config, _clientHello, _cert, (ref ecdheKeyAgreement ka, ref Config config, ref clientHelloMsg clientHello, ref x509.Certificate cert, Defer _, Panic panic, Recover __) =>
        {
            if (ka.curveid == 0L)
            {
                return (null, null, errors.New("tls: missing ServerKeyExchange message"));
            }
            slice<byte> serialized = default;            slice<byte> preMasterSecret = default;



            if (ka.curveid == X25519)
            {
                array<byte> ourPublic = new array<byte>(32L);                array<byte> theirPublic = new array<byte>(32L);                array<byte> sharedKey = new array<byte>(32L);                array<byte> scalar = new array<byte>(32L);



                {
                    var (_, err) = io.ReadFull(config.rand(), scalar[..]);

                    if (err != null)
                    {
                        return (null, null, err);
                    }

                }

                copy(theirPublic[..], ka.publicKey);
                curve25519.ScalarBaseMult(ref ourPublic, ref scalar);
                curve25519.ScalarMult(ref sharedKey, ref scalar, ref theirPublic);
                serialized = ourPublic[..];
                preMasterSecret = sharedKey[..];
            }
            else
            {
                var (curve, ok) = curveForCurveID(ka.curveid);
                if (!ok)
                {
                    panic("internal error");
                }
                var (priv, mx, my, err) = elliptic.GenerateKey(curve, config.rand());
                if (err != null)
                {
                    return (null, null, err);
                }
                var (x, _) = curve.ScalarMult(ka.x, ka.y, priv);
                preMasterSecret = make_slice<byte>((curve.Params().BitSize + 7L) >> (int)(3L));
                var xBytes = x.Bytes();
                copy(preMasterSecret[len(preMasterSecret) - len(xBytes)..], xBytes);

                serialized = elliptic.Marshal(curve, mx, my);
            }
            ptr<clientKeyExchangeMsg> ckx = @new<clientKeyExchangeMsg>();
            ckx.ciphertext = make_slice<byte>(1L + len(serialized));
            ckx.ciphertext[0L] = byte(len(serialized));
            copy(ckx.ciphertext[1L..], serialized);

            return (preMasterSecret, ckx, null);
        });
    }
}}
