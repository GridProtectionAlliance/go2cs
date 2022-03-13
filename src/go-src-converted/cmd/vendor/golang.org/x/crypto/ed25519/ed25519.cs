// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// In Go 1.13, the ed25519 package was promoted to the standard library as
// crypto/ed25519, and this package became a wrapper for the standard library one.
//
//go:build !go1.13
// +build !go1.13

// Package ed25519 implements the Ed25519 signature algorithm. See
// https://ed25519.cr.yp.to/.
//
// These functions are also compatible with the “Ed25519” function defined in
// RFC 8032. However, unlike RFC 8032's formulation, this package's private key
// representation includes a public key suffix to make multiple signing
// operations with the same key more efficient. This package refers to the RFC
// 8032 private key as the “seed”.

// package ed25519 -- go2cs converted at 2022 March 13 06:38:57 UTC
// import "cmd/vendor/golang.org/x/crypto/ed25519" ==> using ed25519 = go.cmd.vendor.golang.org.x.crypto.ed25519_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\crypto\ed25519\ed25519.go
namespace go.cmd.vendor.golang.org.x.crypto;
// This code is a port of the public domain, “ref10” implementation of ed25519
// from SUPERCOP.


using bytes = bytes_package;
using crypto = crypto_package;
using cryptorand = crypto.rand_package;
using sha512 = crypto.sha512_package;
using errors = errors_package;
using io = io_package;
using strconv = strconv_package;

using edwards25519 = golang.org.x.crypto.ed25519.@internal.edwards25519_package;

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

// PrivateKey is the type of Ed25519 private keys. It implements crypto.Signer.
public partial struct PrivateKey { // : slice<byte>
}

// Public returns the PublicKey corresponding to priv.
public static crypto.PublicKey Public(this PrivateKey priv) {
    var publicKey = make_slice<byte>(PublicKeySize);
    copy(publicKey, priv[(int)32..]);
    return PublicKey(publicKey);
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
public static PrivateKey NewKeyFromSeed(slice<byte> seed) => func((_, panic, _) => {
    {
        var l = len(seed);

        if (l != SeedSize) {
            panic("ed25519: bad seed length: " + strconv.Itoa(l));
        }
    }

    var digest = sha512.Sum512(seed);
    digest[0] &= 248;
    digest[31] &= 127;
    digest[31] |= 64;

    ref edwards25519.ExtendedGroupElement A = ref heap(out ptr<edwards25519.ExtendedGroupElement> _addr_A);
    ref array<byte> hBytes = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_hBytes);
    copy(hBytes[..], digest[..]);
    edwards25519.GeScalarMultBase(_addr_A, _addr_hBytes);
    ref array<byte> publicKeyBytes = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_publicKeyBytes);
    A.ToBytes(_addr_publicKeyBytes);

    var privateKey = make_slice<byte>(PrivateKeySize);
    copy(privateKey, seed);
    copy(privateKey[(int)32..], publicKeyBytes[..]);

    return privateKey;
});

// Sign signs the message with privateKey and returns a signature. It will
// panic if len(privateKey) is not PrivateKeySize.
public static slice<byte> Sign(PrivateKey privateKey, slice<byte> message) => func((_, panic, _) => {
    {
        var l = len(privateKey);

        if (l != PrivateKeySize) {
            panic("ed25519: bad private key length: " + strconv.Itoa(l));
        }
    }

    var h = sha512.New();
    h.Write(privateKey[..(int)32]);

    array<byte> digest1 = new array<byte>(64);    ref array<byte> messageDigest = ref heap(new array<byte>(64), out ptr<array<byte>> _addr_messageDigest);    ref array<byte> hramDigest = ref heap(new array<byte>(64), out ptr<array<byte>> _addr_hramDigest);

    ref array<byte> expandedSecretKey = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_expandedSecretKey);
    h.Sum(digest1[..(int)0]);
    copy(expandedSecretKey[..], digest1[..]);
    expandedSecretKey[0] &= 248;
    expandedSecretKey[31] &= 63;
    expandedSecretKey[31] |= 64;

    h.Reset();
    h.Write(digest1[(int)32..]);
    h.Write(message);
    h.Sum(messageDigest[..(int)0]);

    ref array<byte> messageDigestReduced = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_messageDigestReduced);
    edwards25519.ScReduce(_addr_messageDigestReduced, _addr_messageDigest);
    ref edwards25519.ExtendedGroupElement R = ref heap(out ptr<edwards25519.ExtendedGroupElement> _addr_R);
    edwards25519.GeScalarMultBase(_addr_R, _addr_messageDigestReduced);

    ref array<byte> encodedR = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_encodedR);
    R.ToBytes(_addr_encodedR);

    h.Reset();
    h.Write(encodedR[..]);
    h.Write(privateKey[(int)32..]);
    h.Write(message);
    h.Sum(hramDigest[..(int)0]);
    ref array<byte> hramDigestReduced = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_hramDigestReduced);
    edwards25519.ScReduce(_addr_hramDigestReduced, _addr_hramDigest);

    ref array<byte> s = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_s);
    edwards25519.ScMulAdd(_addr_s, _addr_hramDigestReduced, _addr_expandedSecretKey, _addr_messageDigestReduced);

    var signature = make_slice<byte>(SignatureSize);
    copy(signature[..], encodedR[..]);
    copy(signature[(int)32..], s[..]);

    return signature;
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
    ref edwards25519.ExtendedGroupElement A = ref heap(out ptr<edwards25519.ExtendedGroupElement> _addr_A);
    ref array<byte> publicKeyBytes = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_publicKeyBytes);
    copy(publicKeyBytes[..], publicKey);
    if (!A.FromBytes(_addr_publicKeyBytes)) {
        return false;
    }
    edwards25519.FeNeg(_addr_A.X, _addr_A.X);
    edwards25519.FeNeg(_addr_A.T, _addr_A.T);

    var h = sha512.New();
    h.Write(sig[..(int)32]);
    h.Write(publicKey[..]);
    h.Write(message);
    ref array<byte> digest = ref heap(new array<byte>(64), out ptr<array<byte>> _addr_digest);
    h.Sum(digest[..(int)0]);

    ref array<byte> hReduced = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_hReduced);
    edwards25519.ScReduce(_addr_hReduced, _addr_digest);

    ref edwards25519.ProjectiveGroupElement R = ref heap(out ptr<edwards25519.ProjectiveGroupElement> _addr_R);
    ref array<byte> s = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_s);
    copy(s[..], sig[(int)32..]); 

    // https://tools.ietf.org/html/rfc8032#section-5.1.7 requires that s be in
    // the range [0, order) in order to prevent signature malleability.
    if (!edwards25519.ScMinimal(_addr_s)) {
        return false;
    }
    edwards25519.GeDoubleScalarMultVartime(_addr_R, _addr_hReduced, _addr_A, _addr_s);

    ref array<byte> checkR = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_checkR);
    R.ToBytes(_addr_checkR);
    return bytes.Equal(sig[..(int)32], checkR[..]);
});

} // end ed25519_package
