// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2022 March 06 22:19:48 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Program Files\Go\src\crypto\x509\pkcs1.go
using rsa = go.crypto.rsa_package;
using asn1 = go.encoding.asn1_package;
using errors = go.errors_package;
using big = go.math.big_package;
using System.ComponentModel;


namespace go.crypto;

public static partial class x509_package {

    // pkcs1PrivateKey is a structure which mirrors the PKCS #1 ASN.1 for an RSA private key.
private partial struct pkcs1PrivateKey {
    public nint Version;
    public ptr<big.Int> N;
    public nint E;
    public ptr<big.Int> D;
    public ptr<big.Int> P;
    public ptr<big.Int> Q; // We ignore these values, if present, because rsa will calculate them.
    [Description("asn1:\"optional\"")]
    public ptr<big.Int> Dp;
    [Description("asn1:\"optional\"")]
    public ptr<big.Int> Dq;
    [Description("asn1:\"optional\"")]
    public ptr<big.Int> Qinv;
    [Description("asn1:\"optional,omitempty\"")]
    public slice<pkcs1AdditionalRSAPrime> AdditionalPrimes;
}

private partial struct pkcs1AdditionalRSAPrime {
    public ptr<big.Int> Prime; // We ignore these values because rsa will calculate them.
    public ptr<big.Int> Exp;
    public ptr<big.Int> Coeff;
}

// pkcs1PublicKey reflects the ASN.1 structure of a PKCS #1 public key.
private partial struct pkcs1PublicKey {
    public ptr<big.Int> N;
    public nint E;
}

// ParsePKCS1PrivateKey parses an RSA private key in PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PRIVATE KEY".
public static (ptr<rsa.PrivateKey>, error) ParsePKCS1PrivateKey(slice<byte> der) {
    ptr<rsa.PrivateKey> _p0 = default!;
    error _p0 = default!;

    ref pkcs1PrivateKey priv = ref heap(out ptr<pkcs1PrivateKey> _addr_priv);
    var (rest, err) = asn1.Unmarshal(der, _addr_priv);
    if (len(rest) > 0) {
        return (_addr_null!, error.As(new asn1.SyntaxError(Msg:"trailing data"))!);
    }
    if (err != null) {
        {
            var (_, err) = asn1.Unmarshal(der, addr(new ecPrivateKey()));

            if (err == null) {
                return (_addr_null!, error.As(errors.New("x509: failed to parse private key (use ParseECPrivateKey instead for this key format)"))!);
            }

        }

        {
            (_, err) = asn1.Unmarshal(der, addr(new pkcs8()));

            if (err == null) {
                return (_addr_null!, error.As(errors.New("x509: failed to parse private key (use ParsePKCS8PrivateKey instead for this key format)"))!);
            }

        }

        return (_addr_null!, error.As(err)!);

    }
    if (priv.Version > 1) {
        return (_addr_null!, error.As(errors.New("x509: unsupported private key version"))!);
    }
    if (priv.N.Sign() <= 0 || priv.D.Sign() <= 0 || priv.P.Sign() <= 0 || priv.Q.Sign() <= 0) {
        return (_addr_null!, error.As(errors.New("x509: private key contains zero or negative value"))!);
    }
    ptr<rsa.PrivateKey> key = @new<rsa.PrivateKey>();
    key.PublicKey = new rsa.PublicKey(E:priv.E,N:priv.N,);

    key.D = priv.D;
    key.Primes = make_slice<ptr<big.Int>>(2 + len(priv.AdditionalPrimes));
    key.Primes[0] = priv.P;
    key.Primes[1] = priv.Q;
    foreach (var (i, a) in priv.AdditionalPrimes) {
        if (a.Prime.Sign() <= 0) {
            return (_addr_null!, error.As(errors.New("x509: private key contains zero or negative prime"))!);
        }
        key.Primes[i + 2] = a.Prime; 
        // We ignore the other two values because rsa will calculate
        // them as needed.
    }    err = key.Validate();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    key.Precompute();

    return (_addr_key!, error.As(null!)!);

}

// MarshalPKCS1PrivateKey converts an RSA private key to PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PRIVATE KEY".
// For a more flexible key format which is not RSA specific, use
// MarshalPKCS8PrivateKey.
public static slice<byte> MarshalPKCS1PrivateKey(ptr<rsa.PrivateKey> _addr_key) {
    ref rsa.PrivateKey key = ref _addr_key.val;

    key.Precompute();

    nint version = 0;
    if (len(key.Primes) > 2) {
        version = 1;
    }
    pkcs1PrivateKey priv = new pkcs1PrivateKey(Version:version,N:key.N,E:key.PublicKey.E,D:key.D,P:key.Primes[0],Q:key.Primes[1],Dp:key.Precomputed.Dp,Dq:key.Precomputed.Dq,Qinv:key.Precomputed.Qinv,);

    priv.AdditionalPrimes = make_slice<pkcs1AdditionalRSAPrime>(len(key.Precomputed.CRTValues));
    foreach (var (i, values) in key.Precomputed.CRTValues) {
        priv.AdditionalPrimes[i].Prime = key.Primes[2 + i];
        priv.AdditionalPrimes[i].Exp = values.Exp;
        priv.AdditionalPrimes[i].Coeff = values.Coeff;
    }    var (b, _) = asn1.Marshal(priv);
    return b;

}

// ParsePKCS1PublicKey parses an RSA public key in PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PUBLIC KEY".
public static (ptr<rsa.PublicKey>, error) ParsePKCS1PublicKey(slice<byte> der) {
    ptr<rsa.PublicKey> _p0 = default!;
    error _p0 = default!;

    ref pkcs1PublicKey pub = ref heap(out ptr<pkcs1PublicKey> _addr_pub);
    var (rest, err) = asn1.Unmarshal(der, _addr_pub);
    if (err != null) {
        {
            var (_, err) = asn1.Unmarshal(der, addr(new publicKeyInfo()));

            if (err == null) {
                return (_addr_null!, error.As(errors.New("x509: failed to parse public key (use ParsePKIXPublicKey instead for this key format)"))!);
            }

        }

        return (_addr_null!, error.As(err)!);

    }
    if (len(rest) > 0) {
        return (_addr_null!, error.As(new asn1.SyntaxError(Msg:"trailing data"))!);
    }
    if (pub.N.Sign() <= 0 || pub.E <= 0) {
        return (_addr_null!, error.As(errors.New("x509: public key contains zero or negative value"))!);
    }
    if (pub.E > 1 << 31 - 1) {
        return (_addr_null!, error.As(errors.New("x509: public key contains large public exponent"))!);
    }
    return (addr(new rsa.PublicKey(E:pub.E,N:pub.N,)), error.As(null!)!);

}

// MarshalPKCS1PublicKey converts an RSA public key to PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PUBLIC KEY".
public static slice<byte> MarshalPKCS1PublicKey(ptr<rsa.PublicKey> _addr_key) {
    ref rsa.PublicKey key = ref _addr_key.val;

    var (derBytes, _) = asn1.Marshal(new pkcs1PublicKey(N:key.N,E:key.E,));
    return derBytes;
}

} // end x509_package
