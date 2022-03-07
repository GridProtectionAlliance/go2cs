// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2022 March 06 22:19:49 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Program Files\Go\src\crypto\x509\pkcs8.go
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using rsa = go.crypto.rsa_package;
using pkix = go.crypto.x509.pkix_package;
using asn1 = go.encoding.asn1_package;
using errors = go.errors_package;
using fmt = go.fmt_package;

namespace go.crypto;

public static partial class x509_package {

    // pkcs8 reflects an ASN.1, PKCS #8 PrivateKey. See
    // ftp://ftp.rsasecurity.com/pub/pkcs/pkcs-8/pkcs-8v1_2.asn
    // and RFC 5208.
private partial struct pkcs8 {
    public nint Version;
    public pkix.AlgorithmIdentifier Algo;
    public slice<byte> PrivateKey; // optional attributes omitted.
}

// ParsePKCS8PrivateKey parses an unencrypted private key in PKCS #8, ASN.1 DER form.
//
// It returns a *rsa.PrivateKey, a *ecdsa.PrivateKey, or a ed25519.PrivateKey.
// More types might be supported in the future.
//
// This kind of key is commonly encoded in PEM blocks of type "PRIVATE KEY".
public static (object, error) ParsePKCS8PrivateKey(slice<byte> der) {
    object key = default;
    error err = default!;

    ref pkcs8 privKey = ref heap(out ptr<pkcs8> _addr_privKey);
    {
        var (_, err) = asn1.Unmarshal(der, _addr_privKey);

        if (err != null) {
            {
                (_, err) = asn1.Unmarshal(der, addr(new ecPrivateKey()));

                if (err == null) {
                    return (null, error.As(errors.New("x509: failed to parse private key (use ParseECPrivateKey instead for this key format)"))!);
                }

            }

            {
                (_, err) = asn1.Unmarshal(der, addr(new pkcs1PrivateKey()));

                if (err == null) {
                    return (null, error.As(errors.New("x509: failed to parse private key (use ParsePKCS1PrivateKey instead for this key format)"))!);
                }

            }

            return (null, error.As(err)!);

        }
    }


    if (privKey.Algo.Algorithm.Equal(oidPublicKeyRSA)) 
        key, err = ParsePKCS1PrivateKey(privKey.PrivateKey);
        if (err != null) {
            return (null, error.As(errors.New("x509: failed to parse RSA private key embedded in PKCS#8: " + err.Error()))!);
        }
        return (key, error.As(null!)!);
    else if (privKey.Algo.Algorithm.Equal(oidPublicKeyECDSA)) 
        var bytes = privKey.Algo.Parameters.FullBytes;
        ptr<object> namedCurveOID = @new<asn1.ObjectIdentifier>();
        {
            (_, err) = asn1.Unmarshal(bytes, namedCurveOID);

            if (err != null) {
                namedCurveOID = null;
            }

        }

        key, err = parseECPrivateKey(namedCurveOID, privKey.PrivateKey);
        if (err != null) {
            return (null, error.As(errors.New("x509: failed to parse EC private key embedded in PKCS#8: " + err.Error()))!);
        }
        return (key, error.As(null!)!);
    else if (privKey.Algo.Algorithm.Equal(oidPublicKeyEd25519)) 
        {
            var l__prev1 = l;

            var l = len(privKey.Algo.Parameters.FullBytes);

            if (l != 0) {
                return (null, error.As(errors.New("x509: invalid Ed25519 private key parameters"))!);
            }

            l = l__prev1;

        }

        ref slice<byte> curvePrivateKey = ref heap(out ptr<slice<byte>> _addr_curvePrivateKey);
        {
            (_, err) = asn1.Unmarshal(privKey.PrivateKey, _addr_curvePrivateKey);

            if (err != null) {
                return (null, error.As(fmt.Errorf("x509: invalid Ed25519 private key: %v", err))!);
            }

        }

        {
            var l__prev1 = l;

            l = len(curvePrivateKey);

            if (l != ed25519.SeedSize) {
                return (null, error.As(fmt.Errorf("x509: invalid Ed25519 private key length: %d", l))!);
            }

            l = l__prev1;

        }

        return (ed25519.NewKeyFromSeed(curvePrivateKey), error.As(null!)!);
    else 
        return (null, error.As(fmt.Errorf("x509: PKCS#8 wrapping contained private key with unknown algorithm: %v", privKey.Algo.Algorithm))!);
    
}

// MarshalPKCS8PrivateKey converts a private key to PKCS #8, ASN.1 DER form.
//
// The following key types are currently supported: *rsa.PrivateKey, *ecdsa.PrivateKey
// and ed25519.PrivateKey. Unsupported key types result in an error.
//
// This kind of key is commonly encoded in PEM blocks of type "PRIVATE KEY".
public static (slice<byte>, error) MarshalPKCS8PrivateKey(object key) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    pkcs8 privKey = default;

    switch (key.type()) {
        case ptr<rsa.PrivateKey> k:
            privKey.Algo = new pkix.AlgorithmIdentifier(Algorithm:oidPublicKeyRSA,Parameters:asn1.NullRawValue,);
            privKey.PrivateKey = MarshalPKCS1PrivateKey(k);
            break;
        case ptr<ecdsa.PrivateKey> k:
            var (oid, ok) = oidFromNamedCurve(k.Curve);
            if (!ok) {
                return (null, error.As(errors.New("x509: unknown curve while marshaling to PKCS#8"))!);
            }
            var (oidBytes, err) = asn1.Marshal(oid);
            if (err != null) {
                return (null, error.As(errors.New("x509: failed to marshal curve OID: " + err.Error()))!);
            }
            privKey.Algo = new pkix.AlgorithmIdentifier(Algorithm:oidPublicKeyECDSA,Parameters:asn1.RawValue{FullBytes:oidBytes,},);

            privKey.PrivateKey, err = marshalECPrivateKeyWithOID(k, null);

            if (err != null) {
                return (null, error.As(errors.New("x509: failed to marshal EC private key while building PKCS#8: " + err.Error()))!);
            }

            break;
        case ed25519.PrivateKey k:
            privKey.Algo = new pkix.AlgorithmIdentifier(Algorithm:oidPublicKeyEd25519,);
            var (curvePrivateKey, err) = asn1.Marshal(k.Seed());
            if (err != null) {
                return (null, error.As(fmt.Errorf("x509: failed to marshal private key: %v", err))!);
            }
            privKey.PrivateKey = curvePrivateKey;
            break;
        default:
        {
            var k = key.type();
            return (null, error.As(fmt.Errorf("x509: unknown key type while marshaling PKCS#8: %T", key))!);
            break;
        }

    }

    return asn1.Marshal(privKey);

}

} // end x509_package
