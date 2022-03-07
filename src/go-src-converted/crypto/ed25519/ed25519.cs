// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ed25519 implements the Ed25519 signature algorithm. See
// https://ed25519.cr.yp.to/.
//
// These functions are also compatible with the “Ed25519” function defined in
// RFC 8032. However, unlike RFC 8032's formulation, this package's private key
// representation includes a public key suffix to make multiple signing
// operations with the same key more efficient. This package refers to the RFC
// 8032 private key as the “seed”.
// package ed25519 -- go2cs converted at 2022 March 06 22:17:09 UTC
// import "crypto/ed25519" ==> using ed25519 = go.crypto.ed25519_package
// Original source: C:\Program Files\Go\src\crypto\ed25519\ed25519.go
using bytes = go.bytes_package;
using crypto = go.crypto_package;
using edwards25519 = go.crypto.ed25519.@internal.edwards25519_package;
using cryptorand = go.crypto.rand_package;
using sha512 = go.crypto.sha512_package;
using errors = go.errors_package;
using io = go.io_package;
using strconv = go.strconv_package;

namespace go.crypto;

public static partial class ed25519_package {

 
// PublicKeySize is the size, in bytes, of public keys as used in this package.
public static readonly nint PublicKeySize = 32; 
// PrivateKeySize is the size, in bytes, of private keys as used in this package.
public static readonly nint PrivateKeySize = 64; 
// SignatureSize is the size, in bytes, of signatures generated and verified by this package.
public static readonly nint SignatureSize = 64; 
// SeedSize is the size, in bytes, of private key seeds. These are the private key representations used by RFC 8032.
public static readonly nint SeedSize = 32;


// PublicKey is the type of Ed25519 public keys.
public partial struct PublicKey { // : slice<byte>
}

// Any methods implemented on PublicKey might need to also be implemented on
// PrivateKey, as the latter embeds the former and will expose its methods.

// Equal reports whether pub and x have the same value.
public static bool Equal(this PublicKey pub, crypto.PublicKey x) {
    PublicKey (xx, ok) = x._<PublicKey>();
    if (!ok) {
        return false;
    }
    return bytes.Equal(pub, xx);

}

// PrivateKey is the type of Ed25519 private keys. It implements crypto.Signer.
public partial struct PrivateKey { // : slice<byte>
}

// Public returns the PublicKey corresponding to priv.
public static crypto.PublicKey Public(this PrivateKey priv) {
    var publicKey = make_slice<byte>(PublicKeySize);
    copy(publicKey, priv[(int)32..]);
    return PublicKey(publicKey);
}

// Equal reports whether priv and x have the same value.
public static bool Equal(this PrivateKey priv, crypto.PrivateKey x) {
    PrivateKey (xx, ok) = x._<PrivateKey>();
    if (!ok) {
        return false;
    }
    return bytes.Equal(priv, xx);

}

// Seed returns the private key seed corresponding to priv. It is provided for
// interoperability with RFC 8032. RFC 8032's private keys correspond to seeds
// in this package.
public static slice<byte> Seed(this PrivateKey priv) {
    var seed = make_slice<byte>(SeedSize);
    copy(seed, priv[..(int)32]);
    return seed;
}

// Sign signs the given message with priv.
// Ed25519 performs two passes over messages to be signed and therefore cannot
// handle pre-hashed messages. Thus opts.HashFunc() must return zero to
// indicate the message hasn't been hashed. This can be achieved by passing
// crypto.Hash(0) as the value for opts.
public static (slice<byte>, error) Sign(this PrivateKey priv, io.Reader rand, slice<byte> message, crypto.SignerOpts opts) {
    slice<byte> signature = default;
    error err = default!;

    if (opts.HashFunc() != crypto.Hash(0)) {
        return (null, error.As(errors.New("ed25519: cannot sign hashed message"))!);
    }
    return (Sign(priv, message), error.As(null!)!);

}

// GenerateKey generates a public/private key pair using entropy from rand.
// If rand is nil, crypto/rand.Reader will be used.
public static (PublicKey, PrivateKey, error) GenerateKey(io.Reader rand) {
    PublicKey _p0 = default;
    PrivateKey _p0 = default;
    error _p0 = default!;

    if (rand == null) {
        rand = cryptorand.Reader;
    }
    var seed = make_slice<byte>(SeedSize);
    {
        var (_, err) = io.ReadFull(rand, seed);

        if (err != null) {
            return (null, null, error.As(err)!);
        }
    }


    var privateKey = NewKeyFromSeed(seed);
    var publicKey = make_slice<byte>(PublicKeySize);
    copy(publicKey, privateKey[(int)32..]);

    return (publicKey, privateKey, error.As(null!)!);

}

// NewKeyFromSeed calculates a private key from a seed. It will panic if
// len(seed) is not SeedSize. This function is provided for interoperability
// with RFC 8032. RFC 8032's private keys correspond to seeds in this
// package.
public static PrivateKey NewKeyFromSeed(slice<byte> seed) { 
    // Outline the function body so that the returned key can be stack-allocated.
    var privateKey = make_slice<byte>(PrivateKeySize);
    newKeyFromSeed(privateKey, seed);
    return privateKey;

}

private static void newKeyFromSeed(slice<byte> privateKey, slice<byte> seed) => func((_, panic, _) => {
    {
        var l = len(seed);

        if (l != SeedSize) {
            panic("ed25519: bad seed length: " + strconv.Itoa(l));
        }
    }


    var h = sha512.Sum512(seed);
    var s = edwards25519.NewScalar().SetBytesWithClamping(h[..(int)32]);
    ptr<edwards25519.Point> A = (addr(new edwards25519.Point())).ScalarBaseMult(s);

    var publicKey = A.Bytes();

    copy(privateKey, seed);
    copy(privateKey[(int)32..], publicKey);

});

// Sign signs the message with privateKey and returns a signature. It will
// panic if len(privateKey) is not PrivateKeySize.
public static slice<byte> Sign(PrivateKey privateKey, slice<byte> message) { 
    // Outline the function body so that the returned signature can be
    // stack-allocated.
    var signature = make_slice<byte>(SignatureSize);
    sign(signature, privateKey, message);
    return signature;

}

private static void sign(slice<byte> signature, slice<byte> privateKey, slice<byte> message) => func((_, panic, _) => {
    {
        var l = len(privateKey);

        if (l != PrivateKeySize) {
            panic("ed25519: bad private key length: " + strconv.Itoa(l));
        }
    }

    var seed = privateKey[..(int)SeedSize];
    var publicKey = privateKey[(int)SeedSize..];

    var h = sha512.Sum512(seed);
    var s = edwards25519.NewScalar().SetBytesWithClamping(h[..(int)32]);
    var prefix = h[(int)32..];

    var mh = sha512.New();
    mh.Write(prefix);
    mh.Write(message);
    var messageDigest = make_slice<byte>(0, sha512.Size);
    messageDigest = mh.Sum(messageDigest);
    var r = edwards25519.NewScalar().SetUniformBytes(messageDigest);

    ptr<edwards25519.Point> R = (addr(new edwards25519.Point())).ScalarBaseMult(r);

    var kh = sha512.New();
    kh.Write(R.Bytes());
    kh.Write(publicKey);
    kh.Write(message);
    var hramDigest = make_slice<byte>(0, sha512.Size);
    hramDigest = kh.Sum(hramDigest);
    var k = edwards25519.NewScalar().SetUniformBytes(hramDigest);

    var S = edwards25519.NewScalar().MultiplyAdd(k, s, r);

    copy(signature[..(int)32], R.Bytes());
    copy(signature[(int)32..], S.Bytes());

});

// Verify reports whether sig is a valid signature of message by publicKey. It
// will panic if len(publicKey) is not PublicKeySize.
public static bool Verify(PublicKey publicKey, slice<byte> message, slice<byte> sig) => func((_, panic, _) => {
    {
        var l = len(publicKey);

        if (l != PublicKeySize) {
            panic("ed25519: bad public key length: " + strconv.Itoa(l));
        }
    }


    if (len(sig) != SignatureSize || sig[63] & 224 != 0) {
        return false;
    }
    ptr<edwards25519.Point> (A, err) = (addr(new edwards25519.Point())).SetBytes(publicKey);
    if (err != null) {
        return false;
    }
    var kh = sha512.New();
    kh.Write(sig[..(int)32]);
    kh.Write(publicKey);
    kh.Write(message);
    var hramDigest = make_slice<byte>(0, sha512.Size);
    hramDigest = kh.Sum(hramDigest);
    var k = edwards25519.NewScalar().SetUniformBytes(hramDigest);

    var (S, err) = edwards25519.NewScalar().SetCanonicalBytes(sig[(int)32..]);
    if (err != null) {
        return false;
    }
    ptr<edwards25519.Point> minusA = (addr(new edwards25519.Point())).Negate(A);
    ptr<edwards25519.Point> R = (addr(new edwards25519.Point())).VarTimeDoubleScalarBaseMult(k, minusA, S);

    return bytes.Equal(sig[..(int)32], R.Bytes());

});

} // end ed25519_package
