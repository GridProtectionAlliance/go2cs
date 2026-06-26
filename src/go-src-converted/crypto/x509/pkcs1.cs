// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using rsa = crypto.rsa_package;
using asn1 = encoding.asn1_package;
using errors = errors_package;
using big = math.big_package;
using encoding;
using math;

partial class x509_package {

// pkcs1PrivateKey is a structure which mirrors the PKCS #1 ASN.1 for an RSA private key.
[GoType] partial struct pkcs1PrivateKey {
    public nint Version;
    public ж<math.big_package.ΔInt> N;
    public nint E;
    public ж<math.big_package.ΔInt> D;
    public ж<math.big_package.ΔInt> P;
    public ж<math.big_package.ΔInt> Q;
    // We ignore these values, if present, because rsa will calculate them.
    [GoTag(@"asn1:""optional""")]
    public ж<math.big_package.ΔInt> Dp;
    [GoTag(@"asn1:""optional""")]
    public ж<math.big_package.ΔInt> Dq;
    [GoTag(@"asn1:""optional""")]
    public ж<math.big_package.ΔInt> Qinv;
    [GoTag(@"asn1:""optional,omitempty""")]
    public slice<pkcs1AdditionalRSAPrime> AdditionalPrimes;
}

[GoType] partial struct pkcs1AdditionalRSAPrime {
    public ж<math.big_package.ΔInt> Prime;
    // We ignore these values because rsa will calculate them.
    public ж<math.big_package.ΔInt> Exp;
    public ж<math.big_package.ΔInt> Coeff;
}

// pkcs1PublicKey reflects the ASN.1 structure of a PKCS #1 public key.
[GoType] partial struct pkcs1PublicKey {
    public ж<math.big_package.ΔInt> N;
    public nint E;
}

// ParsePKCS1PrivateKey parses an [RSA] private key in PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PRIVATE KEY".
public static (ж<rsa.PrivateKey>, error) ParsePKCS1PrivateKey(slice<byte> der) {
    ref var priv = ref heap(new pkcs1PrivateKey(), out var Ꮡpriv);
    (rest, err) = asn1.Unmarshal(der, Ꮡpriv);
    if (len(rest) > 0) {
        return (default!, new asn1.SyntaxError(Msg: "trailing data"u8));
    }
    if (err != default!) {
        {
            (_, errΔ1) = asn1.Unmarshal(der, Ꮡ(new ecPrivateKey(nil))); if (errΔ1 == default!) {
                return (default!, errors.New("x509: failed to parse private key (use ParseECPrivateKey instead for this key format)"u8));
            }
        }
        {
            (_, errΔ2) = asn1.Unmarshal(der, Ꮡ(new pkcs8(nil))); if (errΔ2 == default!) {
                return (default!, errors.New("x509: failed to parse private key (use ParsePKCS8PrivateKey instead for this key format)"u8));
            }
        }
        return (default!, err);
    }
    if (priv.Version > 1) {
        return (default!, errors.New("x509: unsupported private key version"u8));
    }
    if (priv.N.Sign() <= 0 || priv.D.Sign() <= 0 || priv.P.Sign() <= 0 || priv.Q.Sign() <= 0) {
        return (default!, errors.New("x509: private key contains zero or negative value"u8));
    }
    var key = @new<rsa.PrivateKey>();
    key.val.PublicKey = new rsa.PublicKey(
        E: priv.E,
        N: priv.N
    );
    key.val.D = priv.D;
    key.val.Primes = new slice<bigꓸInt>(2 + len(priv.AdditionalPrimes));
    (~key).Primes[0] = priv.P;
    (~key).Primes[1] = priv.Q;
    foreach (var (i, a) in priv.AdditionalPrimes) {
        if (a.Prime.Sign() <= 0) {
            return (default!, errors.New("x509: private key contains zero or negative prime"u8));
        }
        (~key).Primes[i + 2] = a.Prime;
    }
    // We ignore the other two values because rsa will calculate
    // them as needed.
    err = key.Validate();
    if (err != default!) {
        return (default!, err);
    }
    key.Precompute();
    return (key, default!);
}

// MarshalPKCS1PrivateKey converts an [RSA] private key to PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PRIVATE KEY".
// For a more flexible key format which is not [RSA] specific, use
// [MarshalPKCS8PrivateKey].
public static slice<byte> MarshalPKCS1PrivateKey(ж<rsa.PrivateKey> Ꮡkey) {
    ref var key = ref Ꮡkey.val;

    key.Precompute();
    nint version = 0;
    if (len(key.Primes) > 2) {
        version = 1;
    }
    var priv = new pkcs1PrivateKey(
        Version: version,
        N: key.N,
        E: key.PublicKey.E,
        D: key.D,
        P: key.Primes[0],
        Q: key.Primes[1],
        Dp: key.Precomputed.Dp,
        Dq: key.Precomputed.Dq,
        Qinv: key.Precomputed.Qinv
    );
    priv.AdditionalPrimes = new slice<pkcs1AdditionalRSAPrime>(len(key.Precomputed.CRTValues));
    foreach (var (i, values) in key.Precomputed.CRTValues) {
        priv.AdditionalPrimes[i].Prime = key.Primes[2 + i];
        priv.AdditionalPrimes[i].Exp = values.Exp;
        priv.AdditionalPrimes[i].Coeff = values.Coeff;
    }
    (b, _) = asn1.Marshal(priv);
    return b;
}

// ParsePKCS1PublicKey parses an [RSA] public key in PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PUBLIC KEY".
public static (ж<rsa.PublicKey>, error) ParsePKCS1PublicKey(slice<byte> der) {
    ref var pub = ref heap(new pkcs1PublicKey(), out var Ꮡpub);
    (rest, err) = asn1.Unmarshal(der, Ꮡpub);
    if (err != default!) {
        {
            (_, errΔ1) = asn1.Unmarshal(der, Ꮡ(new publicKeyInfo(nil))); if (errΔ1 == default!) {
                return (default!, errors.New("x509: failed to parse public key (use ParsePKIXPublicKey instead for this key format)"u8));
            }
        }
        return (default!, err);
    }
    if (len(rest) > 0) {
        return (default!, new asn1.SyntaxError(Msg: "trailing data"u8));
    }
    if (pub.N.Sign() <= 0 || pub.E <= 0) {
        return (default!, errors.New("x509: public key contains zero or negative value"u8));
    }
    if (pub.E > 1 << (int)(31) - 1) {
        return (default!, errors.New("x509: public key contains large public exponent"u8));
    }
    return (Ꮡ(new rsa.PublicKey(
        E: pub.E,
        N: pub.N
    )), default!);
}

// MarshalPKCS1PublicKey converts an [RSA] public key to PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PUBLIC KEY".
public static slice<byte> MarshalPKCS1PublicKey(ж<rsa.PublicKey> Ꮡkey) {
    ref var key = ref Ꮡkey.val;

    (derBytes, _) = asn1.Marshal(new pkcs1PublicKey(
        N: key.N,
        E: key.E
    ));
    return derBytes;
}

} // end x509_package
