// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using rsa = go.crypto.rsa_package;
using asn1 = encoding.asn1_package;
using errors = errors_package;
using big = go.math.big_package;
using encoding;
using go.crypto;
using go.math;

partial class x509_package {

// pkcs1PrivateKey is a structure which mirrors the PKCS #1 ASN.1 for an RSA private key.
[GoType] partial struct pkcs1PrivateKey {
    public nint Version;
    public ж<bigꓸInt> N;
    public nint E;
    public ж<bigꓸInt> D;
    public ж<bigꓸInt> P;
    public ж<bigꓸInt> Q;
    // We ignore these values, if present, because rsa will calculate them.
    [GoTag(@"asn1:""optional""")]
    public ж<bigꓸInt> Dp;
    [GoTag(@"asn1:""optional""")]
    public ж<bigꓸInt> Dq;
    [GoTag(@"asn1:""optional""")]
    public ж<bigꓸInt> Qinv;
    [GoTag(@"asn1:""optional,omitempty""")]
    public slice<pkcs1AdditionalRSAPrime> AdditionalPrimes;
}

[GoType] public partial struct pkcs1AdditionalRSAPrime {
    public ж<bigꓸInt> Prime;
    // We ignore these values because rsa will calculate them.
    public ж<bigꓸInt> Exp;
    public ж<bigꓸInt> Coeff;
}

// pkcs1PublicKey reflects the ASN.1 structure of a PKCS #1 public key.
[GoType] partial struct pkcs1PublicKey {
    public ж<bigꓸInt> N;
    public nint E;
}

// ParsePKCS1PrivateKey parses an [RSA] private key in PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PRIVATE KEY".
public static (ж<rsa.PrivateKey>, error) ParsePKCS1PrivateKey(slice<byte> der) {
    ref var priv = ref heap(new pkcs1PrivateKey(), out var Ꮡpriv);
    var (rest, err) = asn1.Unmarshal(der, Ꮡpriv);
    if (builtin.len(rest) > 0) {
        return (default!, new asn1.SyntaxError(Msg: "trailing data"u8));
    }
    if (err != default!) {
        {
            var (_, errΔ1) = asn1.Unmarshal(der, Ꮡ(new ecPrivateKey(nil))); if (errΔ1 == default!) {
                return (default!, errors.New("x509: failed to parse private key (use ParseECPrivateKey instead for this key format)"u8));
            }
        }
        {
            var (_, errΔ2) = asn1.Unmarshal(der, Ꮡ(new pkcs8(nil))); if (errΔ2 == default!) {
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
    key.Value.PublicKey = new rsa.PublicKey(
        E: priv.E,
        N: priv.N
    );
    key.Value.D = priv.D;
    key.Value.Primes = new slice<ж<bigꓸInt>>(2 + builtin.len(priv.AdditionalPrimes));
    key.Value.Primes[0] = priv.P;
    key.Value.Primes[1] = priv.Q;
    foreach (var (i, a) in priv.AdditionalPrimes) {
        if (a.Prime.Sign() <= 0) {
            return (default!, errors.New("x509: private key contains zero or negative prime"u8));
        }
        key.Value.Primes[i + 2] = a.Prime;
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
    ref var key = ref Ꮡkey.Value;

    key.Precompute();
    nint version = 0;
    if (builtin.len(key.Primes) > 2) {
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
    priv.AdditionalPrimes = new slice<pkcs1AdditionalRSAPrime>(builtin.len(key.Precomputed.CRTValues));
    foreach (var (i, values) in key.Precomputed.CRTValues) {
        priv.AdditionalPrimes[i].Prime = key.Primes[2 + i];
        priv.AdditionalPrimes[i].Exp = values.Exp;
        priv.AdditionalPrimes[i].Coeff = values.Coeff;
    }
    var (b, _) = asn1.Marshal(priv);
    return b;
}

// ParsePKCS1PublicKey parses an [RSA] public key in PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PUBLIC KEY".
public static (ж<rsa.PublicKey>, error) ParsePKCS1PublicKey(slice<byte> der) {
    ref var pub = ref heap(new pkcs1PublicKey(), out var Ꮡpub);
    var (rest, err) = asn1.Unmarshal(der, Ꮡpub);
    if (err != default!) {
        {
            var (_, errΔ1) = asn1.Unmarshal(der, Ꮡ(new publicKeyInfo(nil))); if (errΔ1 == default!) {
                return (default!, errors.New("x509: failed to parse public key (use ParsePKIXPublicKey instead for this key format)"u8));
            }
        }
        return (default!, err);
    }
    if (builtin.len(rest) > 0) {
        return (default!, new asn1.SyntaxError(Msg: "trailing data"u8));
    }
    if (pub.N.Sign() <= 0 || pub.E <= 0) {
        return (default!, errors.New("x509: public key contains zero or negative value"u8));
    }
    if (pub.E > 2147483648L - 1) {
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
    ref var key = ref Ꮡkey.Value;

    var (derBytes, _) = asn1.Marshal(new pkcs1PublicKey(
        N: key.N,
        E: key.E
    ));
    return derBytes;
}

} // end x509_package
