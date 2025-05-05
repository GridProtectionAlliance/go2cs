// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using ecdh = crypto.ecdh_package;
using ecdsa = crypto.ecdsa_package;
using ed25519 = crypto.ed25519_package;
using rsa = crypto.rsa_package;
using pkix = crypto.x509.pkix_package;
using asn1 = encoding.asn1_package;
using errors = errors_package;
using fmt = fmt_package;
using crypto.x509;
using encoding;

partial class x509_package {

// pkcs8 reflects an ASN.1, PKCS #8 PrivateKey. See
// ftp://ftp.rsasecurity.com/pub/pkcs/pkcs-8/pkcs-8v1_2.asn
// and RFC 5208.
[GoType] partial struct pkcs8 {
    public nint Version;
    public crypto.x509.pkix_package.AlgorithmIdentifier Algo;
    public slice<byte> PrivateKey;
}

// optional attributes omitted.

// ParsePKCS8PrivateKey parses an unencrypted private key in PKCS #8, ASN.1 DER form.
//
// It returns a *[rsa.PrivateKey], an *[ecdsa.PrivateKey], an [ed25519.PrivateKey] (not
// a pointer), or an *[ecdh.PrivateKey] (for X25519). More types might be supported
// in the future.
//
// This kind of key is commonly encoded in PEM blocks of type "PRIVATE KEY".
public static (any key, error err) ParsePKCS8PrivateKey(slice<byte> der) {
    any key = default!;
    error err = default!;

    ref var privKey = ref heap(new pkcs8(), out var ᏑprivKey);
    {
        (_, errΔ1) = asn1.Unmarshal(der, ᏑprivKey); if (errΔ1 != default!) {
            {
                (_, errΔ2) = asn1.Unmarshal(der, Ꮡ(new ecPrivateKey(nil))); if (errΔ2 == default!) {
                    return (default!, errors.New("x509: failed to parse private key (use ParseECPrivateKey instead for this key format)"u8));
                }
            }
            {
                (_, errΔ3) = asn1.Unmarshal(der, Ꮡ(new pkcs1PrivateKey(nil))); if (errΔ3 == default!) {
                    return (default!, errors.New("x509: failed to parse private key (use ParsePKCS1PrivateKey instead for this key format)"u8));
                }
            }
            return (default!, errΔ1);
        }
    }
    switch (ᐧ) {
    case {} when privKey.Algo.Algorithm.Equal(oidPublicKeyRSA): {
        (key, err) = ParsePKCS1PrivateKey(privKey.PrivateKey);
        if (err != default!) {
            return (default!, errors.New("x509: failed to parse RSA private key embedded in PKCS#8: "u8 + err.Error()));
        }
        return (key, default!);
    }
    case {} when privKey.Algo.Algorithm.Equal(oidPublicKeyECDSA): {
        var bytes = privKey.Algo.Parameters.FullBytes;
        var namedCurveOID = @new<asn1.ObjectIdentifier>();
        {
            (_, errΔ7) = asn1.Unmarshal(bytes, namedCurveOID); if (errΔ7 != default!) {
                namedCurveOID = default!;
            }
        }
        (key, err) = parseECPrivateKey(namedCurveOID, privKey.PrivateKey);
        if (err != default!) {
            return (default!, errors.New("x509: failed to parse EC private key embedded in PKCS#8: "u8 + err.Error()));
        }
        return (key, default!);
    }
    case {} when privKey.Algo.Algorithm.Equal(oidPublicKeyEd25519): {
        {
            nint l = len(privKey.Algo.Parameters.FullBytes); if (l != 0) {
                return (default!, errors.New("x509: invalid Ed25519 private key parameters"u8));
            }
        }
        slice<byte> curvePrivateKeyΔ2 = default!;
        {
            (_, errΔ8) = asn1.Unmarshal(privKey.PrivateKey, Ꮡ(curvePrivateKeyΔ2)); if (errΔ8 != default!) {
                return (default!, fmt.Errorf("x509: invalid Ed25519 private key: %v"u8, errΔ8));
            }
        }
        {
            nint l = len(curvePrivateKeyΔ2); if (l != ed25519.SeedSize) {
                return (default!, fmt.Errorf("x509: invalid Ed25519 private key length: %d"u8, l));
            }
        }
        return (ed25519.NewKeyFromSeed(curvePrivateKeyΔ2), default!);
    }
    case {} when privKey.Algo.Algorithm.Equal(oidPublicKeyX25519): {
        {
            nint l = len(privKey.Algo.Parameters.FullBytes); if (l != 0) {
                return (default!, errors.New("x509: invalid X25519 private key parameters"u8));
            }
        }
        slice<byte> curvePrivateKey = default!;
        {
            (_, errΔ9) = asn1.Unmarshal(privKey.PrivateKey, Ꮡ(curvePrivateKey)); if (errΔ9 != default!) {
                return (default!, fmt.Errorf("x509: invalid X25519 private key: %v"u8, errΔ9));
            }
        }
        return ecdh.X25519().NewPrivateKey(curvePrivateKey);
    }
    default: {
        return (default!, fmt.Errorf("x509: PKCS#8 wrapping contained private key with unknown algorithm: %v"u8, privKey.Algo.Algorithm));
    }}

}

// MarshalPKCS8PrivateKey converts a private key to PKCS #8, ASN.1 DER form.
//
// The following key types are currently supported: *[rsa.PrivateKey],
// *[ecdsa.PrivateKey], [ed25519.PrivateKey] (not a pointer), and *[ecdh.PrivateKey].
// Unsupported key types result in an error.
//
// This kind of key is commonly encoded in PEM blocks of type "PRIVATE KEY".
public static (slice<byte>, error) MarshalPKCS8PrivateKey(any key) {
    pkcs8 privKey = default!;
    switch (key.type()) {
    case ж<rsa.PrivateKey> k: {
        privKey.Algo = new pkix.AlgorithmIdentifier(
            Algorithm: oidPublicKeyRSA,
            Parameters: asn1.NullRawValue
        );
        privKey.PrivateKey = MarshalPKCS1PrivateKey(k);
        break;
    }
    case ж<ecdsa.PrivateKey> k: {
        var (oid, ok) = oidFromNamedCurve(k.Curve);
        if (!ok) {
            return (default!, errors.New("x509: unknown curve while marshaling to PKCS#8"u8));
        }
        (oidBytes, errΔ1) = asn1.Marshal(oid);
        if (errΔ1 != default!) {
            return (default!, errors.New("x509: failed to marshal curve OID: "u8 + errΔ1.Error()));
        }
        privKey.Algo = new pkix.AlgorithmIdentifier(
            Algorithm: oidPublicKeyECDSA,
            Parameters: new asn1.RawValue(
                FullBytes: oidBytes
            )
        );
        {
            var (privKey.PrivateKey, errΔ1) = marshalECPrivateKeyWithOID(k, default!); if (errΔ1 != default!) {
                return (default!, errors.New("x509: failed to marshal EC private key while building PKCS#8: "u8 + errΔ1.Error()));
            }
        }
        break;
    }
    case ed25519.PrivateKey k: {
        privKey.Algo = new pkix.AlgorithmIdentifier(
            Algorithm: oidPublicKeyEd25519
        );
        (curvePrivateKey, errΔ1) = asn1.Marshal(k.Seed());
        if (err != default!) {
            return (default!, fmt.Errorf("x509: failed to marshal private key: %v"u8, err));
        }
        privKey.PrivateKey = curvePrivateKey;
        break;
    }
    case ж<ecdh.PrivateKey> k: {
        if (AreEqual(k.Curve(), ecdh.X25519())){
            privKey.Algo = new pkix.AlgorithmIdentifier(
                Algorithm: oidPublicKeyX25519
            );
            error errΔ2 = default!;
            {
                var (privKey.PrivateKey, errΔ2) = asn1.Marshal(k.Bytes()); if (errΔ2 != default!) {
                    return (default!, fmt.Errorf("x509: failed to marshal private key: %v"u8, errΔ2));
                }
            }
        } else {
            var (oidΔ1, okΔ1) = oidFromECDHCurve(k.Curve());
            if (!okΔ1) {
                return (default!, errors.New("x509: unknown curve while marshaling to PKCS#8"u8));
            }
            (oidBytesΔ1, err) = asn1.Marshal(oidΔ1);
            if (err != default!) {
                return (default!, errors.New("x509: failed to marshal curve OID: "u8 + err.Error()));
            }
            privKey.Algo = new pkix.AlgorithmIdentifier(
                Algorithm: oidPublicKeyECDSA,
                Parameters: new asn1.RawValue(
                    FullBytes: oidBytesΔ1
                )
            );
            {
                var (privKey.PrivateKey, err) = marshalECDHPrivateKey(k); if (err != default!) {
                    return (default!, errors.New("x509: failed to marshal EC private key while building PKCS#8: "u8 + err.Error()));
                }
            }
        }
        break;
    }
    default: {
        var k = key.type();
        return (default!, fmt.Errorf("x509: unknown key type while marshaling PKCS#8: %T"u8, key));
    }}
    return asn1.Marshal(privKey);
}

} // end x509_package
