// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using field = go.crypto.@internal.edwards25519.field_package;
using randutil = go.crypto.@internal.randutil_package;
using errors = errors_package;
using io = io_package;
using go.crypto.@internal;
using go.crypto.@internal.edwards25519;

partial class ecdh_package {

internal static nint x25519PublicKeySize = 32;
internal static nint x25519PrivateKeySize = 32;
internal static nint x25519SharedSecretSize = 32;

// X25519 returns a [Curve] which implements the X25519 function over Curve25519
// (RFC 7748, Section 5).
//
// Multiple invocations of this function will return the same value, so it can
// be used for equality checks and switch statements.
public static ΔCurve X25519() {
    return new x25519CurveжΔCurve(x25519);
}

internal static ж<x25519Curve> x25519 = Ꮡ(new x25519Curve(nil));

[GoType] partial struct x25519Curve {
}

[GoRecv] internal static @string String(this ref x25519Curve c) {
    return "X25519"u8;
}

internal static (ж<PrivateKey>, error) GenerateKey(this ж<x25519Curve> Ꮡc, io.Reader rand) {
    var key = new slice<byte>(x25519PrivateKeySize);
    randutil.MaybeReadByte(rand);
    {
        var (_, err) = io.ReadFull(rand, key); if (err != default!) {
            return (default!, err);
        }
    }
    return Ꮡc.NewPrivateKey(key);
}

internal static (ж<PrivateKey>, error) NewPrivateKey(this ж<x25519Curve> Ꮡc, slice<byte> key) {
    if (len(key) != x25519PrivateKeySize) {
        return (default!, errors.New("crypto/ecdh: invalid private key size"u8));
    }
    return (Ꮡ(new PrivateKey(
        curve: new x25519CurveжΔCurve(Ꮡc),
        privateKey: append(new byte[]{}.slice(), key.ꓸꓸꓸ)
    )), default!);
}

internal static ж<ΔPublicKey> privateKeyToPublicKey(this ж<x25519Curve> Ꮡc, ж<PrivateKey> Ꮡkey) {
    ref var key = ref Ꮡkey.Value;

    if (!AreEqual(key.curve, Ꮡc)) {
        throw panic("crypto/ecdh: internal error: converting the wrong key type");
    }
    var k = Ꮡ(new ΔPublicKey(
        curve: key.curve,
        publicKey: new slice<byte>(x25519PublicKeySize)
    ));
    var x25519Basepoint = new byte[]{9}.array();
    x25519ScalarMult((~k).publicKey, key.privateKey, x25519Basepoint[..]);
    return k;
}

internal static (ж<ΔPublicKey>, error) NewPublicKey(this ж<x25519Curve> Ꮡc, slice<byte> key) {
    if (len(key) != x25519PublicKeySize) {
        return (default!, errors.New("crypto/ecdh: invalid public key"u8));
    }
    return (Ꮡ(new ΔPublicKey(
        curve: new x25519CurveжΔCurve(Ꮡc),
        publicKey: append(new byte[]{}.slice(), key.ꓸꓸꓸ)
    )), default!);
}

[GoRecv] internal static (slice<byte>, error) ecdh(this ref x25519Curve c, ж<PrivateKey> Ꮡlocal, ж<ΔPublicKey> Ꮡremote) {
    ref var local = ref Ꮡlocal.Value;
    ref var remote = ref Ꮡremote.Value;

    var @out = new slice<byte>(x25519SharedSecretSize);
    x25519ScalarMult(@out, local.privateKey, remote.publicKey);
    if (isZero(@out)) {
        return (default!, errors.New("crypto/ecdh: bad X25519 remote ECDH input: low order point"u8));
    }
    return (@out, default!);
}

internal static void x25519ScalarMult(slice<byte> dst, slice<byte> scalar, slice<byte> point) {
    array<byte> e = new(32);
    copy(e[..], scalar[..]);
    e[0] &= (byte)(248);
    e[31] &= (byte)(127);
    e[31] |= (byte)(64);
    ref var x1 = ref heap(new field.Element(), out var Ꮡx1);
    ref var x2 = ref heap(new field.Element(), out var Ꮡx2);
    ref var z2 = ref heap(new field.Element(), out var Ꮡz2);
    ref var x3 = ref heap(new field.Element(), out var Ꮡx3);
    ref var z3 = ref heap(new field.Element(), out var Ꮡz3);
    ref var tmp0 = ref heap(new field.Element(), out var Ꮡtmp0);
    ref var tmp1 = ref heap(new field.Element(), out var Ꮡtmp1);
    Ꮡx1.SetBytes(point[..]);
    Ꮡx2.One();
    Ꮡx3.Set(Ꮡx1);
    Ꮡz3.One();
    nint swap = 0;
    for (nint pos = 254; pos >= 0; pos--) {
        var b = (byte)((e[pos / 8] >> (int)((nuint)((nint)(pos & 7)))));
        b &= (byte)(1);
        swap ^= (nint)((nint)b);
        x2.Swap(Ꮡx3, swap);
        z2.Swap(Ꮡz3, swap);
        swap = (nint)b;
        Ꮡtmp0.Subtract(Ꮡx3, Ꮡz3);
        Ꮡtmp1.Subtract(Ꮡx2, Ꮡz2);
        Ꮡx2.Add(Ꮡx2, Ꮡz2);
        Ꮡz2.Add(Ꮡx3, Ꮡz3);
        Ꮡz3.Multiply(Ꮡtmp0, Ꮡx2);
        Ꮡz2.Multiply(Ꮡz2, Ꮡtmp1);
        Ꮡtmp0.Square(Ꮡtmp1);
        Ꮡtmp1.Square(Ꮡx2);
        Ꮡx3.Add(Ꮡz3, Ꮡz2);
        Ꮡz2.Subtract(Ꮡz3, Ꮡz2);
        Ꮡx2.Multiply(Ꮡtmp1, Ꮡtmp0);
        Ꮡtmp1.Subtract(Ꮡtmp1, Ꮡtmp0);
        Ꮡz2.Square(Ꮡz2);
        Ꮡz3.Mult32(Ꮡtmp1, 121666);
        Ꮡx3.Square(Ꮡx3);
        Ꮡtmp0.Add(Ꮡtmp0, Ꮡz3);
        Ꮡz3.Multiply(Ꮡx1, Ꮡz2);
        Ꮡz2.Multiply(Ꮡtmp1, Ꮡtmp0);
    }
    x2.Swap(Ꮡx3, swap);
    z2.Swap(Ꮡz3, swap);
    Ꮡz2.Invert(Ꮡz2);
    Ꮡx2.Multiply(Ꮡx2, Ꮡz2);
    copy(dst[..], x2.Bytes());
}

} // end ecdh_package
