// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using ecdh = go.crypto.ecdh_package;
using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using asn1 = encoding.asn1_package;
using errors = errors_package;
using fmt = fmt_package;
using big = go.math.big_package;
using encoding;
using go.crypto;
using go.math;

partial class x509_package {

internal static readonly UntypedInt ecPrivKeyVersion = 1;

// ecPrivateKey reflects an ASN.1 Elliptic Curve Private Key Structure.
// References:
//
//	RFC 5915
//	SEC1 - http://www.secg.org/sec1-v2.pdf
//
// Per RFC 5915 the NamedCurveOID is marked as ASN.1 OPTIONAL, however in
// most cases it is not.
[GoType] partial struct ecPrivateKey {
    public nint Version;
    public slice<byte> PrivateKey;
    [GoTag(@"asn1:""optional,explicit,tag:0""")]
    public asn1.ObjectIdentifier NamedCurveOID;
    [GoTag(@"asn1:""optional,explicit,tag:1""")]
    public asn1.BitString PublicKey;
}

// ParseECPrivateKey parses an EC private key in SEC 1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "EC PRIVATE KEY".
public static (ж<ecdsa.PrivateKey>, error) ParseECPrivateKey(slice<byte> der) {
    return parseECPrivateKey(nil, der);
}

// MarshalECPrivateKey converts an EC private key to SEC 1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "EC PRIVATE KEY".
// For a more flexible key format which is not EC specific, use
// [MarshalPKCS8PrivateKey].
public static (slice<byte>, error) MarshalECPrivateKey(ж<ecdsa.PrivateKey> Ꮡkey) {
    ref var key = ref Ꮡkey.Value;

    var (oid, ok) = oidFromNamedCurve(key.Curve);
    if (!ok) {
        return (default!, errors.New("x509: unknown elliptic curve"u8));
    }
    return marshalECPrivateKeyWithOID(Ꮡkey, oid);
}

// marshalECPrivateKeyWithOID marshals an EC private key into ASN.1, DER format and
// sets the curve ID to the given OID, or omits it if OID is nil.
internal static (slice<byte>, error) marshalECPrivateKeyWithOID(ж<ecdsa.PrivateKey> Ꮡkey, asn1.ObjectIdentifier oid) {
    ref var key = ref Ꮡkey.Value;

    if (!key.Curve.IsOnCurve(key.X, key.Y)) {
        return (default!, errors.New("invalid elliptic key public key"u8));
    }
    var privateKey = new slice<byte>(((~key.Curve.Params()).N.BitLen() + 7) / 8);
    return asn1.Marshal(new ecPrivateKey(
        Version: 1,
        PrivateKey: key.D.FillBytes(privateKey),
        NamedCurveOID: oid,
        PublicKey: new asn1.BitString(Bytes: elliptic.Marshal(key.Curve, key.X, key.Y))
    ));
}

// marshalECDHPrivateKey marshals an EC private key into ASN.1, DER format
// suitable for NIST curves.
internal static (slice<byte>, error) marshalECDHPrivateKey(ж<ecdh.PrivateKey> Ꮡkey) {
    ref var key = ref Ꮡkey.Value;

    return asn1.Marshal(new ecPrivateKey(
        Version: 1,
        PrivateKey: key.Bytes(),
        PublicKey: new asn1.BitString(Bytes: Ꮡkey.PublicKey().Bytes())
    ));
}

// parseECPrivateKey parses an ASN.1 Elliptic Curve Private Key Structure.
// The OID for the named curve may be provided from another source (such as
// the PKCS8 container) - if it is provided then use this instead of the OID
// that may exist in the EC private key structure.
internal static (ж<ecdsa.PrivateKey> key, error err) parseECPrivateKey(ж<asn1.ObjectIdentifier> ᏑnamedCurveOID, slice<byte> der) {
    ж<ecdsa.PrivateKey> key = default!;
    error err = default!;

    ref var namedCurveOID = ref ᏑnamedCurveOID.DerefOrNil();
    ref var privKey = ref heap(new ecPrivateKey(), out var ᏑprivKey);
    {
        var (_, errΔ1) = asn1.Unmarshal(der, ᏑprivKey); if (errΔ1 != default!) {
            {
                var (_, errΔ2) = asn1.Unmarshal(der, Ꮡ(new pkcs8(nil))); if (errΔ2 == default!) {
                    return (default!, errors.New("x509: failed to parse private key (use ParsePKCS8PrivateKey instead for this key format)"u8));
                }
            }
            {
                var (_, errΔ3) = asn1.Unmarshal(der, Ꮡ(new pkcs1PrivateKey(nil))); if (errΔ3 == default!) {
                    return (default!, errors.New("x509: failed to parse private key (use ParsePKCS1PrivateKey instead for this key format)"u8));
                }
            }
            return (default!, errors.New("x509: failed to parse EC private key: "u8 + errΔ1.Error()));
        }
    }
    if (privKey.Version != ecPrivKeyVersion) {
        return (default!, fmt.Errorf("x509: unknown EC private key version %d"u8, privKey.Version));
    }
    elliptic.Curve curve = default!;
    if (ᏑnamedCurveOID != nil){
        curve = namedCurveFromOID(namedCurveOID);
    } else {
        curve = namedCurveFromOID(privKey.NamedCurveOID);
    }
    if (curve == default!) {
        return (default!, errors.New("x509: unknown elliptic curve"u8));
    }
    var k = @new<bigꓸInt>().SetBytes(privKey.PrivateKey);
    var curveOrder = curve.Params().Value.N;
    if (k.Cmp(curveOrder) >= 0) {
        return (default!, errors.New("x509: invalid elliptic curve private key value"u8));
    }
    var priv = @new<ecdsa.PrivateKey>();
    priv.Value.Curve = curve;
    priv.Value.D = k;
    var privateKey = new slice<byte>((curveOrder.BitLen() + 7) / 8);
    // Some private keys have leading zero padding. This is invalid
    // according to [SEC1], but this code will ignore it.
    while (builtin.len(privKey.PrivateKey) > builtin.len(privateKey)) {
        if (privKey.PrivateKey[0] != 0) {
            return (default!, errors.New("x509: invalid private key length"u8));
        }
        privKey.PrivateKey = privKey.PrivateKey[1..];
    }
    // Some private keys remove all leading zeros, this is also invalid
    // according to [SEC1] but since OpenSSL used to do this, we ignore
    // this too.
    copy(privateKey[(int)(builtin.len(privateKey) - builtin.len(privKey.PrivateKey))..], privKey.PrivateKey);
    (priv.Value.X, priv.Value.Y) = curve.ScalarBaseMult(privateKey);
    return (priv, default!);
}

} // end x509_package
