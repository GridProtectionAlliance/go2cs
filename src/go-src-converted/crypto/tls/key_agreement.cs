// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2022 March 06 22:21:03 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Program Files\Go\src\crypto\tls\key_agreement.go
using crypto = go.crypto_package;
using md5 = go.crypto.md5_package;
using rsa = go.crypto.rsa_package;
using sha1 = go.crypto.sha1_package;
using x509 = go.crypto.x509_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;

namespace go.crypto;

public static partial class tls_package {

    // a keyAgreement implements the client and server side of a TLS key agreement
    // protocol by generating and processing key exchange messages.
private partial interface keyAgreement {
    (slice<byte>, ptr<clientKeyExchangeMsg>, error) generateServerKeyExchange(ptr<Config> _p0, ptr<Certificate> _p0, ptr<clientHelloMsg> _p0, ptr<serverHelloMsg> _p0);
    (slice<byte>, ptr<clientKeyExchangeMsg>, error) processClientKeyExchange(ptr<Config> _p0, ptr<Certificate> _p0, ptr<clientKeyExchangeMsg> _p0, ushort _p0); // On the client side, the next two methods are called in order.

// This method may not be called if the server doesn't send a
// ServerKeyExchange message.
    (slice<byte>, ptr<clientKeyExchangeMsg>, error) processServerKeyExchange(ptr<Config> _p0, ptr<clientHelloMsg> _p0, ptr<serverHelloMsg> _p0, ptr<x509.Certificate> _p0, ptr<serverKeyExchangeMsg> _p0);
    (slice<byte>, ptr<clientKeyExchangeMsg>, error) generateClientKeyExchange(ptr<Config> _p0, ptr<clientHelloMsg> _p0, ptr<x509.Certificate> _p0);
}

private static var errClientKeyExchange = errors.New("tls: invalid ClientKeyExchange message");
private static var errServerKeyExchange = errors.New("tls: invalid ServerKeyExchange message");

// rsaKeyAgreement implements the standard TLS key agreement where the client
// encrypts the pre-master secret to the server's public key.
private partial struct rsaKeyAgreement {
}

private static (ptr<serverKeyExchangeMsg>, error) generateServerKeyExchange(this rsaKeyAgreement ka, ptr<Config> _addr_config, ptr<Certificate> _addr_cert, ptr<clientHelloMsg> _addr_clientHello, ptr<serverHelloMsg> _addr_hello) {
    ptr<serverKeyExchangeMsg> _p0 = default!;
    error _p0 = default!;
    ref Config config = ref _addr_config.val;
    ref Certificate cert = ref _addr_cert.val;
    ref clientHelloMsg clientHello = ref _addr_clientHello.val;
    ref serverHelloMsg hello = ref _addr_hello.val;

    return (_addr_null!, error.As(null!)!);
}

private static (slice<byte>, error) processClientKeyExchange(this rsaKeyAgreement ka, ptr<Config> _addr_config, ptr<Certificate> _addr_cert, ptr<clientKeyExchangeMsg> _addr_ckx, ushort version) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Config config = ref _addr_config.val;
    ref Certificate cert = ref _addr_cert.val;
    ref clientKeyExchangeMsg ckx = ref _addr_ckx.val;

    if (len(ckx.ciphertext) < 2) {
        return (null, error.As(errClientKeyExchange)!);
    }
    var ciphertextLen = int(ckx.ciphertext[0]) << 8 | int(ckx.ciphertext[1]);
    if (ciphertextLen != len(ckx.ciphertext) - 2) {
        return (null, error.As(errClientKeyExchange)!);
    }
    var ciphertext = ckx.ciphertext[(int)2..];

    crypto.Decrypter (priv, ok) = cert.PrivateKey._<crypto.Decrypter>();
    if (!ok) {
        return (null, error.As(errors.New("tls: certificate private key does not implement crypto.Decrypter"))!);
    }
    var (preMasterSecret, err) = priv.Decrypt(config.rand(), ciphertext, addr(new rsa.PKCS1v15DecryptOptions(SessionKeyLen:48)));
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (preMasterSecret, error.As(null!)!);

}

private static error processServerKeyExchange(this rsaKeyAgreement ka, ptr<Config> _addr_config, ptr<clientHelloMsg> _addr_clientHello, ptr<serverHelloMsg> _addr_serverHello, ptr<x509.Certificate> _addr_cert, ptr<serverKeyExchangeMsg> _addr_skx) {
    ref Config config = ref _addr_config.val;
    ref clientHelloMsg clientHello = ref _addr_clientHello.val;
    ref serverHelloMsg serverHello = ref _addr_serverHello.val;
    ref x509.Certificate cert = ref _addr_cert.val;
    ref serverKeyExchangeMsg skx = ref _addr_skx.val;

    return error.As(errors.New("tls: unexpected ServerKeyExchange"))!;
}

private static (slice<byte>, ptr<clientKeyExchangeMsg>, error) generateClientKeyExchange(this rsaKeyAgreement ka, ptr<Config> _addr_config, ptr<clientHelloMsg> _addr_clientHello, ptr<x509.Certificate> _addr_cert) {
    slice<byte> _p0 = default;
    ptr<clientKeyExchangeMsg> _p0 = default!;
    error _p0 = default!;
    ref Config config = ref _addr_config.val;
    ref clientHelloMsg clientHello = ref _addr_clientHello.val;
    ref x509.Certificate cert = ref _addr_cert.val;

    var preMasterSecret = make_slice<byte>(48);
    preMasterSecret[0] = byte(clientHello.vers >> 8);
    preMasterSecret[1] = byte(clientHello.vers);
    var (_, err) = io.ReadFull(config.rand(), preMasterSecret[(int)2..]);
    if (err != null) {
        return (null, _addr_null!, error.As(err)!);
    }
    ptr<rsa.PublicKey> (rsaKey, ok) = cert.PublicKey._<ptr<rsa.PublicKey>>();
    if (!ok) {
        return (null, _addr_null!, error.As(errors.New("tls: server certificate contains incorrect key type for selected ciphersuite"))!);
    }
    var (encrypted, err) = rsa.EncryptPKCS1v15(config.rand(), rsaKey, preMasterSecret);
    if (err != null) {
        return (null, _addr_null!, error.As(err)!);
    }
    ptr<clientKeyExchangeMsg> ckx = @new<clientKeyExchangeMsg>();
    ckx.ciphertext = make_slice<byte>(len(encrypted) + 2);
    ckx.ciphertext[0] = byte(len(encrypted) >> 8);
    ckx.ciphertext[1] = byte(len(encrypted));
    copy(ckx.ciphertext[(int)2..], encrypted);
    return (preMasterSecret, _addr_ckx!, error.As(null!)!);

}

// sha1Hash calculates a SHA1 hash over the given byte slices.
private static slice<byte> sha1Hash(slice<slice<byte>> slices) {
    var hsha1 = sha1.New();
    foreach (var (_, slice) in slices) {
        hsha1.Write(slice);
    }    return hsha1.Sum(null);
}

// md5SHA1Hash implements TLS 1.0's hybrid hash function which consists of the
// concatenation of an MD5 and SHA1 hash.
private static slice<byte> md5SHA1Hash(slice<slice<byte>> slices) {
    var md5sha1 = make_slice<byte>(md5.Size + sha1.Size);
    var hmd5 = md5.New();
    foreach (var (_, slice) in slices) {
        hmd5.Write(slice);
    }    copy(md5sha1, hmd5.Sum(null));
    copy(md5sha1[(int)md5.Size..], sha1Hash(slices));
    return md5sha1;
}

// hashForServerKeyExchange hashes the given slices and returns their digest
// using the given hash function (for >= TLS 1.2) or using a default based on
// the sigType (for earlier TLS versions). For Ed25519 signatures, which don't
// do pre-hashing, it returns the concatenation of the slices.
private static slice<byte> hashForServerKeyExchange(byte sigType, crypto.Hash hashFunc, ushort version, params slice<byte>[] slices) {
    slices = slices.Clone();

    if (sigType == signatureEd25519) {
        slice<byte> signed = default;
        {
            var slice__prev1 = slice;

            foreach (var (_, __slice) in slices) {
                slice = __slice;
                signed = append(signed, slice);
            }

            slice = slice__prev1;
        }

        return signed;

    }
    if (version >= VersionTLS12) {
        var h = hashFunc.New();
        {
            var slice__prev1 = slice;

            foreach (var (_, __slice) in slices) {
                slice = __slice;
                h.Write(slice);
            }

            slice = slice__prev1;
        }

        var digest = h.Sum(null);
        return digest;

    }
    if (sigType == signatureECDSA) {
        return sha1Hash(slices);
    }
    return md5SHA1Hash(slices);

}

// ecdheKeyAgreement implements a TLS key agreement where the server
// generates an ephemeral EC public/private key pair and signs it. The
// pre-master secret is then calculated using ECDH. The signature may
// be ECDSA, Ed25519 or RSA.
private partial struct ecdheKeyAgreement {
    public ushort version;
    public bool isRSA;
    public ecdheParameters @params; // ckx and preMasterSecret are generated in processServerKeyExchange
// and returned in generateClientKeyExchange.
    public ptr<clientKeyExchangeMsg> ckx;
    public slice<byte> preMasterSecret;
}

private static (ptr<serverKeyExchangeMsg>, error) generateServerKeyExchange(this ptr<ecdheKeyAgreement> _addr_ka, ptr<Config> _addr_config, ptr<Certificate> _addr_cert, ptr<clientHelloMsg> _addr_clientHello, ptr<serverHelloMsg> _addr_hello) {
    ptr<serverKeyExchangeMsg> _p0 = default!;
    error _p0 = default!;
    ref ecdheKeyAgreement ka = ref _addr_ka.val;
    ref Config config = ref _addr_config.val;
    ref Certificate cert = ref _addr_cert.val;
    ref clientHelloMsg clientHello = ref _addr_clientHello.val;
    ref serverHelloMsg hello = ref _addr_hello.val;

    CurveID curveID = default;
    foreach (var (_, c) in clientHello.supportedCurves) {
        if (config.supportsCurve(c)) {
            curveID = c;
            break;
        }
    }    if (curveID == 0) {
        return (_addr_null!, error.As(errors.New("tls: no supported elliptic curves offered"))!);
    }
    {
        var (_, ok) = curveForCurveID(curveID);

        if (curveID != X25519 && !ok) {
            return (_addr_null!, error.As(errors.New("tls: CurvePreferences includes unsupported curve"))!);
        }
    }


    var (params, err) = generateECDHEParameters(config.rand(), curveID);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ka.@params = params; 

    // See RFC 4492, Section 5.4.
    var ecdhePublic = @params.PublicKey();
    var serverECDHEParams = make_slice<byte>(1 + 2 + 1 + len(ecdhePublic));
    serverECDHEParams[0] = 3; // named curve
    serverECDHEParams[1] = byte(curveID >> 8);
    serverECDHEParams[2] = byte(curveID);
    serverECDHEParams[3] = byte(len(ecdhePublic));
    copy(serverECDHEParams[(int)4..], ecdhePublic);

    crypto.Signer (priv, ok) = cert.PrivateKey._<crypto.Signer>();
    if (!ok) {
        return (_addr_null!, error.As(fmt.Errorf("tls: certificate private key of type %T does not implement crypto.Signer", cert.PrivateKey))!);
    }
    SignatureScheme signatureAlgorithm = default;
    byte sigType = default;
    crypto.Hash sigHash = default;
    if (ka.version >= VersionTLS12) {
        signatureAlgorithm, err = selectSignatureScheme(ka.version, cert, clientHello.supportedSignatureAlgorithms);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        sigType, sigHash, err = typeAndHashFromSignatureScheme(signatureAlgorithm);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
    else
 {
        sigType, sigHash, err = legacyTypeAndHashFromPublicKey(priv.Public());
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
    if ((sigType == signaturePKCS1v15 || sigType == signatureRSAPSS) != ka.isRSA) {
        return (_addr_null!, error.As(errors.New("tls: certificate cannot be used with the selected cipher suite"))!);
    }
    var signed = hashForServerKeyExchange(sigType, sigHash, ka.version, clientHello.random, hello.random, serverECDHEParams);

    var signOpts = crypto.SignerOpts(sigHash);
    if (sigType == signatureRSAPSS) {
        signOpts = addr(new rsa.PSSOptions(SaltLength:rsa.PSSSaltLengthEqualsHash,Hash:sigHash));
    }
    var (sig, err) = priv.Sign(config.rand(), signed, signOpts);
    if (err != null) {
        return (_addr_null!, error.As(errors.New("tls: failed to sign ECDHE parameters: " + err.Error()))!);
    }
    ptr<serverKeyExchangeMsg> skx = @new<serverKeyExchangeMsg>();
    nint sigAndHashLen = 0;
    if (ka.version >= VersionTLS12) {
        sigAndHashLen = 2;
    }
    skx.key = make_slice<byte>(len(serverECDHEParams) + sigAndHashLen + 2 + len(sig));
    copy(skx.key, serverECDHEParams);
    var k = skx.key[(int)len(serverECDHEParams)..];
    if (ka.version >= VersionTLS12) {
        k[0] = byte(signatureAlgorithm >> 8);
        k[1] = byte(signatureAlgorithm);
        k = k[(int)2..];
    }
    k[0] = byte(len(sig) >> 8);
    k[1] = byte(len(sig));
    copy(k[(int)2..], sig);

    return (_addr_skx!, error.As(null!)!);

}

private static (slice<byte>, error) processClientKeyExchange(this ptr<ecdheKeyAgreement> _addr_ka, ptr<Config> _addr_config, ptr<Certificate> _addr_cert, ptr<clientKeyExchangeMsg> _addr_ckx, ushort version) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref ecdheKeyAgreement ka = ref _addr_ka.val;
    ref Config config = ref _addr_config.val;
    ref Certificate cert = ref _addr_cert.val;
    ref clientKeyExchangeMsg ckx = ref _addr_ckx.val;

    if (len(ckx.ciphertext) == 0 || int(ckx.ciphertext[0]) != len(ckx.ciphertext) - 1) {
        return (null, error.As(errClientKeyExchange)!);
    }
    var preMasterSecret = ka.@params.SharedKey(ckx.ciphertext[(int)1..]);
    if (preMasterSecret == null) {
        return (null, error.As(errClientKeyExchange)!);
    }
    return (preMasterSecret, error.As(null!)!);

}

private static error processServerKeyExchange(this ptr<ecdheKeyAgreement> _addr_ka, ptr<Config> _addr_config, ptr<clientHelloMsg> _addr_clientHello, ptr<serverHelloMsg> _addr_serverHello, ptr<x509.Certificate> _addr_cert, ptr<serverKeyExchangeMsg> _addr_skx) {
    ref ecdheKeyAgreement ka = ref _addr_ka.val;
    ref Config config = ref _addr_config.val;
    ref clientHelloMsg clientHello = ref _addr_clientHello.val;
    ref serverHelloMsg serverHello = ref _addr_serverHello.val;
    ref x509.Certificate cert = ref _addr_cert.val;
    ref serverKeyExchangeMsg skx = ref _addr_skx.val;

    if (len(skx.key) < 4) {
        return error.As(errServerKeyExchange)!;
    }
    if (skx.key[0] != 3) { // named curve
        return error.As(errors.New("tls: server selected unsupported curve"))!;

    }
    var curveID = CurveID(skx.key[1]) << 8 | CurveID(skx.key[2]);

    var publicLen = int(skx.key[3]);
    if (publicLen + 4 > len(skx.key)) {
        return error.As(errServerKeyExchange)!;
    }
    var serverECDHEParams = skx.key[..(int)4 + publicLen];
    var publicKey = serverECDHEParams[(int)4..];

    var sig = skx.key[(int)4 + publicLen..];
    if (len(sig) < 2) {
        return error.As(errServerKeyExchange)!;
    }
    {
        var (_, ok) = curveForCurveID(curveID);

        if (curveID != X25519 && !ok) {
            return error.As(errors.New("tls: server selected unsupported curve"))!;
        }
    }


    var (params, err) = generateECDHEParameters(config.rand(), curveID);
    if (err != null) {
        return error.As(err)!;
    }
    ka.@params = params;

    ka.preMasterSecret = @params.SharedKey(publicKey);
    if (ka.preMasterSecret == null) {
        return error.As(errServerKeyExchange)!;
    }
    var ourPublicKey = @params.PublicKey();
    ka.ckx = @new<clientKeyExchangeMsg>();
    ka.ckx.ciphertext = make_slice<byte>(1 + len(ourPublicKey));
    ka.ckx.ciphertext[0] = byte(len(ourPublicKey));
    copy(ka.ckx.ciphertext[(int)1..], ourPublicKey);

    byte sigType = default;
    crypto.Hash sigHash = default;
    if (ka.version >= VersionTLS12) {
        var signatureAlgorithm = SignatureScheme(sig[0]) << 8 | SignatureScheme(sig[1]);
        sig = sig[(int)2..];
        if (len(sig) < 2) {
            return error.As(errServerKeyExchange)!;
        }
        if (!isSupportedSignatureAlgorithm(signatureAlgorithm, clientHello.supportedSignatureAlgorithms)) {
            return error.As(errors.New("tls: certificate used with invalid signature algorithm"))!;
        }
        sigType, sigHash, err = typeAndHashFromSignatureScheme(signatureAlgorithm);
        if (err != null) {
            return error.As(err)!;
        }
    }
    else
 {
        sigType, sigHash, err = legacyTypeAndHashFromPublicKey(cert.PublicKey);
        if (err != null) {
            return error.As(err)!;
        }
    }
    if ((sigType == signaturePKCS1v15 || sigType == signatureRSAPSS) != ka.isRSA) {
        return error.As(errServerKeyExchange)!;
    }
    var sigLen = int(sig[0]) << 8 | int(sig[1]);
    if (sigLen + 2 != len(sig)) {
        return error.As(errServerKeyExchange)!;
    }
    sig = sig[(int)2..];

    var signed = hashForServerKeyExchange(sigType, sigHash, ka.version, clientHello.random, serverHello.random, serverECDHEParams);
    {
        var err = verifyHandshakeSignature(sigType, cert.PublicKey, sigHash, signed, sig);

        if (err != null) {
            return error.As(errors.New("tls: invalid signature by the server certificate: " + err.Error()))!;
        }
    }

    return error.As(null!)!;

}

private static (slice<byte>, ptr<clientKeyExchangeMsg>, error) generateClientKeyExchange(this ptr<ecdheKeyAgreement> _addr_ka, ptr<Config> _addr_config, ptr<clientHelloMsg> _addr_clientHello, ptr<x509.Certificate> _addr_cert) {
    slice<byte> _p0 = default;
    ptr<clientKeyExchangeMsg> _p0 = default!;
    error _p0 = default!;
    ref ecdheKeyAgreement ka = ref _addr_ka.val;
    ref Config config = ref _addr_config.val;
    ref clientHelloMsg clientHello = ref _addr_clientHello.val;
    ref x509.Certificate cert = ref _addr_cert.val;

    if (ka.ckx == null) {
        return (null, _addr_null!, error.As(errors.New("tls: missing ServerKeyExchange message"))!);
    }
    return (ka.preMasterSecret, _addr_ka.ckx!, error.As(null!)!);

}

} // end tls_package
