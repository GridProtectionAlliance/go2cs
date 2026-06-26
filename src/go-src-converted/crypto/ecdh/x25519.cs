// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using field = crypto.@internal.edwards25519.field_package;
using randutil = crypto.@internal.randutil_package;
using errors = errors_package;
using io = io_package;
using crypto.@internal;
using crypto.@internal.edwards25519;

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
    return ~x25519;
}

internal static ж<x25519Curve> x25519 = Ꮡ(new x25519Curve(nil));

[GoType] partial struct x25519Curve {
}

[GoRecv] internal static @string String(this ref x25519Curve c) {
    return "X25519"u8;
}

[GoRecv] internal static (ж<PrivateKey>, error) GenerateKey(this ref x25519Curve c, io.Reader rand) {
    var key = new slice<byte>(x25519PrivateKeySize);
    randutil.MaybeReadByte(rand);
    {
        var (_, err) = io.ReadFull(rand, key); if (err != default!) {
            return (default!, err);
        }
    }
    return c.NewPrivateKey(key);
}

[GoRecv] internal static (ж<PrivateKey>, error) NewPrivateKey(this ref x25519Curve c, slice<byte> key) {
    if (len(key) != x25519PrivateKeySize) {
        return (default!, errors.New("crypto/ecdh: invalid private key size"u8));
    }
    return (Ꮡ(new PrivateKey(
        curve: c,
        privateKey: append(new byte[]{}.slice(), key.ꓸꓸꓸ)
    )), default!);
}

[GoRecv] internal static ж<ΔPublicKey> privateKeyToPublicKey(this ref x25519Curve c, ж<PrivateKey> Ꮡkey) {
    ref var key = ref Ꮡkey.val;

    if (key.curve != ~c) {
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

[GoRecv] internal static (ж<ΔPublicKey>, error) NewPublicKey(this ref x25519Curve c, slice<byte> key) {
    if (len(key) != x25519PublicKeySize) {
        return (default!, errors.New("crypto/ecdh: invalid public key"u8));
    }
    return (Ꮡ(new ΔPublicKey(
        curve: c,
        publicKey: append(new byte[]{}.slice(), key.ꓸꓸꓸ)
    )), default!);
}

[GoRecv] internal static (slice<byte>, error) ecdh(this ref x25519Curve c, ж<PrivateKey> Ꮡlocal, ж<ΔPublicKey> Ꮡremote) {
    ref var local = ref Ꮡlocal.val;
    ref var remote = ref Ꮡremote.val;

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
    ref var x1 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var Ꮡx1);
    ref var x2 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var Ꮡx2);
    ref var z2 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var Ꮡz2);
    ref var x3 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var Ꮡx3);
    ref var z3 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var Ꮡz3);
    ref var tmp0 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var Ꮡtmp0);
    ref var tmp1 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var Ꮡtmp1);
    x1.SetBytes(point[..]);
    x2.One();
    x3.Set(Ꮡx1);
    z3.One();
    nint swap = 0;
    for (nint pos = 254; pos >= 0; pos--) {
        var b = e[pos / 8] >> (int)(((nuint)((nint)(pos & 7))));
        b &= (byte)(1);
        swap ^= (nint)(((nint)b));
        x2.Swap(Ꮡx3, swap);
        z2.Swap(Ꮡz3, swap);
        swap = ((nint)b);
        tmp0.Subtract(Ꮡx3, Ꮡz3);
        tmp1.Subtract(Ꮡx2, Ꮡz2);
        x2.Add(Ꮡx2, Ꮡz2);
        z2.Add(Ꮡx3, Ꮡz3);
        z3.Multiply(Ꮡtmp0, Ꮡx2);
        z2.Multiply(Ꮡz2, Ꮡtmp1);
        tmp0.Square(Ꮡtmp1);
        tmp1.Square(Ꮡx2);
        x3.Add(Ꮡz3, Ꮡz2);
        z2.Subtract(Ꮡz3, Ꮡz2);
        x2.Multiply(Ꮡtmp1, Ꮡtmp0);
        tmp1.Subtract(Ꮡtmp1, Ꮡtmp0);
        z2.Square(Ꮡz2);
        z3.Mult32(Ꮡtmp1, 121666);
        x3.Square(Ꮡx3);
        tmp0.Add(Ꮡtmp0, Ꮡz3);
        z3.Multiply(Ꮡx1, Ꮡz2);
        z2.Multiply(Ꮡtmp1, Ꮡtmp0);
    }
    x2.Swap(Ꮡx3, swap);
    z2.Swap(Ꮡz3, swap);
    z2.Invert(Ꮡz2);
    x2.Multiply(Ꮡx2, Ꮡz2);
    copy(dst[..], x2.Bytes());
}

} // end ecdh_package
