// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ecdh implements Elliptic Curve Diffie-Hellman over
// NIST curves and Curve25519.
namespace go.crypto;

using crypto = crypto_package;
using boring = crypto.@internal.boring_package;
using subtle = crypto.subtle_package;
using errors = errors_package;
using io = io_package;
using sync = sync_package;
using crypto.@internal;

partial class ecdh_package {

[GoType] partial interface ΔCurve {
    // GenerateKey generates a random PrivateKey.
    //
    // Most applications should use [crypto/rand.Reader] as rand. Note that the
    // returned key does not depend deterministically on the bytes read from rand,
    // and may change between calls and/or between versions.
    (ж<PrivateKey>, error) GenerateKey(io.Reader rand);
    // NewPrivateKey checks that key is valid and returns a PrivateKey.
    //
    // For NIST curves, this follows SEC 1, Version 2.0, Section 2.3.6, which
    // amounts to decoding the bytes as a fixed length big endian integer and
    // checking that the result is lower than the order of the curve. The zero
    // private key is also rejected, as the encoding of the corresponding public
    // key would be irregular.
    //
    // For X25519, this only checks the scalar length.
    (ж<PrivateKey>, error) NewPrivateKey(slice<byte> key);
    // NewPublicKey checks that key is valid and returns a PublicKey.
    //
    // For NIST curves, this decodes an uncompressed point according to SEC 1,
    // Version 2.0, Section 2.3.4. Compressed encodings and the point at
    // infinity are rejected.
    //
    // For X25519, this only checks the u-coordinate length. Adversarially
    // selected public keys can cause ECDH to return an error.
    (ж<ΔPublicKey>, error) NewPublicKey(slice<byte> key);
    // ecdh performs an ECDH exchange and returns the shared secret. It's exposed
    // as the PrivateKey.ECDH method.
    //
    // The private method also allow us to expand the ECDH interface with more
    // methods in the future without breaking backwards compatibility.
    (slice<byte>, error) ecdh(ж<PrivateKey> local, ж<ΔPublicKey> remote);
    // privateKeyToPublicKey converts a PrivateKey to a PublicKey. It's exposed
    // as the PrivateKey.PublicKey method.
    //
    // This method always succeeds: for X25519, the zero key can't be
    // constructed due to clamping; for NIST curves, it is rejected by
    // NewPrivateKey.
    ж<ΔPublicKey> privateKeyToPublicKey(ж<PrivateKey> _);
}

// PublicKey is an ECDH public key, usually a peer's ECDH share sent over the wire.
//
// These keys can be parsed with [crypto/x509.ParsePKIXPublicKey] and encoded
// with [crypto/x509.MarshalPKIXPublicKey]. For NIST curves, they then need to
// be converted with [crypto/ecdsa.PublicKey.ECDH] after parsing.
[GoType] partial struct ΔPublicKey {
    internal ΔCurve curve;
    internal slice<byte> publicKey;
    internal ж<crypto.@internal.boring_package.PublicKeyECDH> boring;
}

// Bytes returns a copy of the encoding of the public key.
[GoRecv] public static slice<byte> Bytes(this ref ΔPublicKey k) {
    // Copy the public key to a fixed size buffer that can get allocated on the
    // caller's stack after inlining.
    array<byte> buf = new(133);
    return append(buf[..0], k.publicKey.ꓸꓸꓸ);
}

// Equal returns whether x represents the same public key as k.
//
// Note that there can be equivalent public keys with different encodings which
// would return false from this check but behave the same way as inputs to ECDH.
//
// This check is performed in constant time as long as the key types and their
// curve match.
[GoRecv] public static bool Equal(this ref ΔPublicKey k, crypto.PublicKey x) {
    var (xx, ok) = x._<ΔPublicKey.val>(ᐧ);
    if (!ok) {
        return false;
    }
    return AreEqual(k.curve, (~xx).curve) && subtle.ConstantTimeCompare(k.publicKey, (~xx).publicKey) == 1;
}

[GoRecv] public static ΔCurve Curve(this ref ΔPublicKey k) {
    return k.curve;
}

// PrivateKey is an ECDH private key, usually kept secret.
//
// These keys can be parsed with [crypto/x509.ParsePKCS8PrivateKey] and encoded
// with [crypto/x509.MarshalPKCS8PrivateKey]. For NIST curves, they then need to
// be converted with [crypto/ecdsa.PrivateKey.ECDH] after parsing.
[GoType] partial struct PrivateKey {
    internal ΔCurve curve;
    internal slice<byte> privateKey;
    internal ж<crypto.@internal.boring_package.PrivateKeyECDH> boring;
    // publicKey is set under publicKeyOnce, to allow loading private keys with
    // NewPrivateKey without having to perform a scalar multiplication.
    internal ж<ΔPublicKey> publicKey;
    internal sync_package.Once publicKeyOnce;
}

// ECDH performs an ECDH exchange and returns the shared secret. The [PrivateKey]
// and [PublicKey] must use the same curve.
//
// For NIST curves, this performs ECDH as specified in SEC 1, Version 2.0,
// Section 3.3.1, and returns the x-coordinate encoded according to SEC 1,
// Version 2.0, Section 2.3.5. The result is never the point at infinity.
//
// For [X25519], this performs ECDH as specified in RFC 7748, Section 6.1. If
// the result is the all-zero value, ECDH returns an error.
[GoRecv] public static (slice<byte>, error) ECDH(this ref PrivateKey k, ж<ΔPublicKey> Ꮡremote) {
    ref var remote = ref Ꮡremote.val;

    if (!AreEqual(k.curve, remote.curve)) {
        return (default!, errors.New("crypto/ecdh: private key and public key curves do not match"u8));
    }
    return k.curve.ecdh(k, Ꮡremote);
}

// Bytes returns a copy of the encoding of the private key.
[GoRecv] public static slice<byte> Bytes(this ref PrivateKey k) {
    // Copy the private key to a fixed size buffer that can get allocated on the
    // caller's stack after inlining.
    array<byte> buf = new(66);
    return append(buf[..0], k.privateKey.ꓸꓸꓸ);
}

// Equal returns whether x represents the same private key as k.
//
// Note that there can be equivalent private keys with different encodings which
// would return false from this check but behave the same way as inputs to [ECDH].
//
// This check is performed in constant time as long as the key types and their
// curve match.
[GoRecv] public static bool Equal(this ref PrivateKey k, crypto.PrivateKey x) {
    var (xx, ok) = x._<PrivateKey.val>(ᐧ);
    if (!ok) {
        return false;
    }
    return AreEqual(k.curve, (~xx).curve) && subtle.ConstantTimeCompare(k.privateKey, (~xx).privateKey) == 1;
}

[GoRecv] public static ΔCurve Curve(this ref PrivateKey k) {
    return k.curve;
}

[GoRecv] public static ж<ΔPublicKey> PublicKey(this ref PrivateKey k) {
    k.publicKeyOnce.Do(() => {
        if (k.boring != nil){
            (kpub, err) = k.boring.PublicKey();
            if (err != default!) {
                throw panic("boringcrypto: "u8 + err.Error());
            }
            k.publicKey = Ꮡ(new ΔPublicKey(
                curve: k.curve,
                publicKey: kpub.Bytes(),
                boring: kpub
            ));
        } else {
            k.publicKey = k.curve.privateKeyToPublicKey(k);
        }
    });
    return k.publicKey;
}

// Public implements the implicit interface of all standard library private
// keys. See the docs of [crypto.PrivateKey].
[GoRecv] public static crypto.PublicKey Public(this ref PrivateKey k) {
    return k.PublicKey();
}

} // end ecdh_package
