// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !(boringcrypto && linux && (amd64 || arm64) && !android && !msan && cgo)
namespace go.crypto.@internal;

using crypto = crypto_package;
using cipher = crypto.cipher_package;
using sig = crypto.@internal.boring.sig_package;
using hash = hash_package;
using crypto;
using crypto.@internal.boring;

partial class boring_package {

internal const bool available = false;

// Unreachable marks code that should be unreachable
// when BoringCrypto is in use. It is a no-op without BoringCrypto.
public static void Unreachable() {
    // Code that's unreachable when using BoringCrypto
    // is exactly the code we want to detect for reporting
    // standard Go crypto.
    sig.StandardCrypto();
}

// UnreachableExceptTests marks code that should be unreachable
// when BoringCrypto is in use. It is a no-op without BoringCrypto.
public static void UnreachableExceptTests() {
}

[GoType("num:nint")] partial struct randReader;

internal static (nint, error) Read(this randReader _, slice<byte> b) {
    throw panic("boringcrypto: not available");
}

public static readonly randReader RandReader = /* randReader(0) */ 0;

public static hash.Hash NewSHA1() {
    throw panic("boringcrypto: not available");
}

public static hash.Hash NewSHA224() {
    throw panic("boringcrypto: not available");
}

public static hash.Hash NewSHA256() {
    throw panic("boringcrypto: not available");
}

public static hash.Hash NewSHA384() {
    throw panic("boringcrypto: not available");
}

public static hash.Hash NewSHA512() {
    throw panic("boringcrypto: not available");
}

public static array<byte> SHA1(slice<byte> _) {
    throw panic("boringcrypto: not available");
}

public static array<byte> SHA224(slice<byte> _) {
    throw panic("boringcrypto: not available");
}

public static array<byte> SHA256(slice<byte> _) {
    throw panic("boringcrypto: not available");
}

public static array<byte> SHA384(slice<byte> _) {
    throw panic("boringcrypto: not available");
}

public static array<byte> SHA512(slice<byte> _) {
    throw panic("boringcrypto: not available");
}

public static hash.Hash NewHMAC(Func<hash.Hash> h, slice<byte> key) {
    throw panic("boringcrypto: not available");
}

public static (cipher.Block, error) NewAESCipher(slice<byte> key) {
    throw panic("boringcrypto: not available");
}

public static (cipher.AEAD, error) NewGCMTLS(cipher.Block _) {
    throw panic("boringcrypto: not available");
}

[GoType] partial struct PublicKeyECDSA {
    internal nint _;
}

[GoType] partial struct PrivateKeyECDSA {
    internal nint _;
}

public static (BigInt X, BigInt Y, BigInt D, error err) GenerateKeyECDSA(@string curve) {
    BigInt X = default!;
    BigInt Y = default!;
    BigInt D = default!;
    error err = default!;

    throw panic("boringcrypto: not available");
}

public static (ж<PrivateKeyECDSA>, error) NewPrivateKeyECDSA(@string curve, BigInt X, BigInt Y, BigInt D) {
    throw panic("boringcrypto: not available");
}

public static (ж<PublicKeyECDSA>, error) NewPublicKeyECDSA(@string curve, BigInt X, BigInt Y) {
    throw panic("boringcrypto: not available");
}

public static (slice<byte>, error) SignMarshalECDSA(ж<PrivateKeyECDSA> Ꮡpriv, slice<byte> hash) {
    ref var priv = ref Ꮡpriv.val;

    throw panic("boringcrypto: not available");
}

public static bool VerifyECDSA(ж<PublicKeyECDSA> Ꮡpub, slice<byte> hash, slice<byte> sig) {
    ref var pub = ref Ꮡpub.val;

    throw panic("boringcrypto: not available");
}

[GoType] partial struct PublicKeyRSA {
    internal nint _;
}

[GoType] partial struct PrivateKeyRSA {
    internal nint _;
}

public static (slice<byte>, error) DecryptRSAOAEP(hash.Hash h, hash.Hash mgfHash, ж<PrivateKeyRSA> Ꮡpriv, slice<byte> ciphertext, slice<byte> label) {
    ref var priv = ref Ꮡpriv.val;

    throw panic("boringcrypto: not available");
}

public static (slice<byte>, error) DecryptRSAPKCS1(ж<PrivateKeyRSA> Ꮡpriv, slice<byte> ciphertext) {
    ref var priv = ref Ꮡpriv.val;

    throw panic("boringcrypto: not available");
}

public static (slice<byte>, error) DecryptRSANoPadding(ж<PrivateKeyRSA> Ꮡpriv, slice<byte> ciphertext) {
    ref var priv = ref Ꮡpriv.val;

    throw panic("boringcrypto: not available");
}

public static (slice<byte>, error) EncryptRSAOAEP(hash.Hash h, hash.Hash mgfHash, ж<PublicKeyRSA> Ꮡpub, slice<byte> msg, slice<byte> label) {
    ref var pub = ref Ꮡpub.val;

    throw panic("boringcrypto: not available");
}

public static (slice<byte>, error) EncryptRSAPKCS1(ж<PublicKeyRSA> Ꮡpub, slice<byte> msg) {
    ref var pub = ref Ꮡpub.val;

    throw panic("boringcrypto: not available");
}

public static (slice<byte>, error) EncryptRSANoPadding(ж<PublicKeyRSA> Ꮡpub, slice<byte> msg) {
    ref var pub = ref Ꮡpub.val;

    throw panic("boringcrypto: not available");
}

public static (BigInt N, BigInt E, BigInt D, BigInt P, BigInt Q, BigInt Dp, BigInt Dq, BigInt Qinv, error err) GenerateKeyRSA(nint bits) {
    BigInt N = default!;
    BigInt E = default!;
    BigInt D = default!;
    BigInt P = default!;
    BigInt Q = default!;
    BigInt Dp = default!;
    BigInt Dq = default!;
    BigInt Qinv = default!;
    error err = default!;

    throw panic("boringcrypto: not available");
}

public static (ж<PrivateKeyRSA>, error) NewPrivateKeyRSA(BigInt N, BigInt E, BigInt D, BigInt P, BigInt Q, BigInt Dp, BigInt Dq, BigInt Qinv) {
    throw panic("boringcrypto: not available");
}

public static (ж<PublicKeyRSA>, error) NewPublicKeyRSA(BigInt N, BigInt E) {
    throw panic("boringcrypto: not available");
}

public static (slice<byte>, error) SignRSAPKCS1v15(ж<PrivateKeyRSA> Ꮡpriv, crypto.Hash h, slice<byte> hashed) {
    ref var priv = ref Ꮡpriv.val;

    throw panic("boringcrypto: not available");
}

public static (slice<byte>, error) SignRSAPSS(ж<PrivateKeyRSA> Ꮡpriv, crypto.Hash h, slice<byte> hashed, nint saltLen) {
    ref var priv = ref Ꮡpriv.val;

    throw panic("boringcrypto: not available");
}

public static error VerifyRSAPKCS1v15(ж<PublicKeyRSA> Ꮡpub, crypto.Hash h, slice<byte> hashed, slice<byte> sig) {
    ref var pub = ref Ꮡpub.val;

    throw panic("boringcrypto: not available");
}

public static error VerifyRSAPSS(ж<PublicKeyRSA> Ꮡpub, crypto.Hash h, slice<byte> hashed, slice<byte> sig, nint saltLen) {
    ref var pub = ref Ꮡpub.val;

    throw panic("boringcrypto: not available");
}

[GoType] partial struct PublicKeyECDH {
}

[GoType] partial struct PrivateKeyECDH {
}

public static (slice<byte>, error) ECDH(ж<PrivateKeyECDH> Ꮡ, ж<PublicKeyECDH> Ꮡ) {
    ref var  = ref Ꮡ.val;
    ref var  = ref Ꮡ.val;

    throw panic("boringcrypto: not available");
}

public static (ж<PrivateKeyECDH>, slice<byte>, error) GenerateKeyECDH(@string _) {
    throw panic("boringcrypto: not available");
}

public static (ж<PrivateKeyECDH>, error) NewPrivateKeyECDH(@string _, slice<byte> _) {
    throw panic("boringcrypto: not available");
}

public static (ж<PublicKeyECDH>, error) NewPublicKeyECDH(@string _, slice<byte> _) {
    throw panic("boringcrypto: not available");
}

[GoRecv] public static slice<byte> Bytes(this ref PublicKeyECDH _) {
    throw panic("boringcrypto: not available");
}

[GoRecv] public static (ж<PublicKeyECDH>, error) PublicKey(this ref PrivateKeyECDH _) {
    throw panic("boringcrypto: not available");
}

} // end boring_package
