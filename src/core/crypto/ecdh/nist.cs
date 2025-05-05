// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using boring = crypto.@internal.boring_package;
using nistec = crypto.@internal.nistec_package;
using randutil = crypto.@internal.randutil_package;
using errors = errors_package;
using byteorder = @internal.byteorder_package;
using io = io_package;
using bits = math.bits_package;
using @internal;
using crypto.@internal;
using math;

partial class ecdh_package {

[GoType] partial struct nistCurve<Point>
    where Point : nistPoint[Point]<Point>, new()
{
    internal @string name;
    internal Func<Point> newPoint;
    internal slice<byte> scalarOrder;
}

// nistPoint is a generic constraint for the nistec Point types.
[GoType] partial interface nistPoint {
    slice<byte> Bytes();
    (slice<byte>, error) BytesX();
    (T, error) SetBytes(slice<byte> _);
    (T, error) ScalarMult(T _, slice<byte> _);
    (T, error) ScalarBaseMult(slice<byte> _);
}

[GoRecv] internal static @string String<Point>(this ref nistCurve<Point> c)
    where Point : nistPoint[Point]<Point>, new()
{
    return c.name;
}

internal static error errInvalidPrivateKey = errors.New("crypto/ecdh: invalid private key"u8);

[GoRecv] internal static (ж<PrivateKey>, error) GenerateKey<Point>(this ref nistCurve<Point> c, io.Reader rand)
    where Point : nistPoint[Point]<Point>, new()
{
    if (boring.Enabled && rand == boring.RandReader) {
        (keyΔ1, bytes, err) = boring.GenerateKeyECDH(c.name);
        if (err != default!) {
            return (default!, err);
        }
        return newBoringPrivateKey(~c, keyΔ1, bytes);
    }
    var key = new slice<byte>(len(c.scalarOrder));
    randutil.MaybeReadByte(rand);
    while (ᐧ) {
        {
            var (_, err) = io.ReadFull(rand, key); if (err != default!) {
                return (default!, err);
            }
        }
        // Mask off any excess bits if the size of the underlying field is not a
        // whole number of bytes, which is only the case for P-521. We use a
        // pointer to the scalarOrder field because comparing generic and
        // instantiated types is not supported.
        if (Ꮡ(c.scalarOrder[0]) == Ꮡ(p521Order, 0)) {
            key[0] &= (byte)(1);
        }
        // In tests, rand will return all zeros and NewPrivateKey will reject
        // the zero key as it generates the identity as a public key. This also
        // makes this function consistent with crypto/elliptic.GenerateKey.
        key[1] ^= (byte)(66);
        (k, err) = c.NewPrivateKey(key);
        if (AreEqual(err, errInvalidPrivateKey)) {
            continue;
        }
        return (k, err);
    }
}

[GoRecv] internal static (ж<PrivateKey>, error) NewPrivateKey<Point>(this ref nistCurve<Point> c, slice<byte> key)
    where Point : nistPoint[Point]<Point>, new()
{
    if (len(key) != len(c.scalarOrder)) {
        return (default!, errors.New("crypto/ecdh: invalid private key size"u8));
    }
    if (isZero(key) || !isLess(key, c.scalarOrder)) {
        return (default!, errInvalidPrivateKey);
    }
    if (boring.Enabled) {
        (bk, err) = boring.NewPrivateKeyECDH(c.name, key);
        if (err != default!) {
            return (default!, err);
        }
        return newBoringPrivateKey(~c, bk, key);
    }
    var k = Ꮡ(new PrivateKey(
        curve: c,
        privateKey: append(new byte[]{}.slice(), key.ꓸꓸꓸ)
    ));
    return (k, default!);
}

internal static (ж<PrivateKey>, error) newBoringPrivateKey(ΔCurve c, ж<boring.PrivateKeyECDH> Ꮡbk, slice<byte> privateKey) {
    ref var bk = ref Ꮡbk.val;

    var k = Ꮡ(new PrivateKey(
        curve: c,
        boring: bk,
        privateKey: append(slice<byte>(default!), privateKey.ꓸꓸꓸ)
    ));
    return (k, default!);
}

[GoRecv] internal static ж<ΔPublicKey> privateKeyToPublicKey<Point>(this ref nistCurve<Point> c, ж<PrivateKey> Ꮡkey)
    where Point : nistPoint[Point]<Point>, new()
{
    ref var key = ref Ꮡkey.val;

    boring.Unreachable();
    if (key.curve != ~c) {
        throw panic("crypto/ecdh: internal error: converting the wrong key type");
    }
    (p, err) = c.newPoint().ScalarBaseMult(key.privateKey);
    if (err != default!) {
        // This is unreachable because the only error condition of
        // ScalarBaseMult is if the input is not the right size.
        throw panic("crypto/ecdh: internal error: nistec ScalarBaseMult failed for a fixed-size input");
    }
    var publicKey = p.Bytes();
    if (len(publicKey) == 1) {
        // The encoding of the identity is a single 0x00 byte. This is
        // unreachable because the only scalar that generates the identity is
        // zero, which is rejected by NewPrivateKey.
        throw panic("crypto/ecdh: internal error: nistec ScalarBaseMult returned the identity");
    }
    return Ꮡ(new ΔPublicKey(
        curve: key.curve,
        publicKey: publicKey
    ));
}

// isZero returns whether a is all zeroes in constant time.
internal static bool isZero(slice<byte> a) {
    byte acc = default!;
    foreach (var (_, b) in a) {
        acc |= (byte)(b);
    }
    return acc == 0;
}

// isLess returns whether a < b, where a and b are big-endian buffers of the
// same length and shorter than 72 bytes.
internal static bool isLess(slice<byte> a, slice<byte> b) {
    if (len(a) != len(b)) {
        throw panic("crypto/ecdh: internal error: mismatched isLess inputs");
    }
    // Copy the values into a fixed-size preallocated little-endian buffer.
    // 72 bytes is enough for every scalar in this package, and having a fixed
    // size lets us avoid heap allocations.
    if (len(a) > 72) {
        throw panic("crypto/ecdh: internal error: isLess input too large");
    }
    var bufA = new slice<byte>(72);
    var bufB = new slice<byte>(72);
    foreach (var (iΔ1, _) in a) {
        (bufA[iΔ1], bufB[iΔ1]) = (a[len(a) - iΔ1 - 1], b[len(b) - iΔ1 - 1]);
    }
    // Perform a subtraction with borrow.
    uint64 borrow = default!;
    for (nint i = 0; i < len(bufA); i += 8) {
        var (limbA, limbB) = (byteorder.LeUint64(bufA[(int)(i)..]), byteorder.LeUint64(bufB[(int)(i)..]));
        (_, borrow) = bits.Sub64(limbA, limbB, borrow);
    }
    // If there is a borrow at the end of the operation, then a < b.
    return borrow == 1;
}

[GoRecv] internal static (ж<ΔPublicKey>, error) NewPublicKey<Point>(this ref nistCurve<Point> c, slice<byte> key)
    where Point : nistPoint[Point]<Point>, new()
{
    // Reject the point at infinity and compressed encodings.
    if (len(key) == 0 || key[0] != 4) {
        return (default!, errors.New("crypto/ecdh: invalid public key"u8));
    }
    var k = Ꮡ(new ΔPublicKey(
        curve: c,
        publicKey: append(new byte[]{}.slice(), key.ꓸꓸꓸ)
    ));
    if (boring.Enabled){
        (bk, err) = boring.NewPublicKeyECDH(c.name, (~k).publicKey);
        if (err != default!) {
            return (default!, err);
        }
        k.val.boring = bk;
    } else {
        // SetBytes also checks that the point is on the curve.
        {
            (_, err) = c.newPoint().SetBytes(key); if (err != default!) {
                return (default!, err);
            }
        }
    }
    return (k, default!);
}

[GoRecv] internal static (slice<byte>, error) ecdh<Point>(this ref nistCurve<Point> c, ж<PrivateKey> Ꮡlocal, ж<ΔPublicKey> Ꮡremote)
    where Point : nistPoint[Point]<Point>, new()
{
    ref var local = ref Ꮡlocal.val;
    ref var remote = ref Ꮡremote.val;

    // Note that this function can't return an error, as NewPublicKey rejects
    // invalid points and the point at infinity, and NewPrivateKey rejects
    // invalid scalars and the zero value. BytesX returns an error for the point
    // at infinity, but in a prime order group such as the NIST curves that can
    // only be the result of a scalar multiplication if one of the inputs is the
    // zero scalar or the point at infinity.
    if (boring.Enabled) {
        return boring.ECDH(local.boring, remote.boring);
    }
    boring.Unreachable();
    (p, err) = c.newPoint().SetBytes(remote.publicKey);
    if (err != default!) {
        return (default!, err);
    }
    {
        (_, errΔ1) = p.ScalarMult(p, local.privateKey); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    return p.BytesX();
}

// P256 returns a [Curve] which implements NIST P-256 (FIPS 186-3, section D.2.3),
// also known as secp256r1 or prime256v1.
//
// Multiple invocations of this function will return the same value, which can
// be used for equality checks and switch statements.
public static ΔCurve P256() {
    return ~p256;
}

internal static ж<nistec.P256Point>> p256 = Ꮡ(new nistCurve[ж<nistec.P256Point>](
    name: "P-256"u8,
    newPoint: nistec.NewP256Point,
    scalarOrder: p256Order
));

internal static slice<byte> p256Order = new byte[]{
    255, 255, 255, 255, 0, 0, 0, 0,
    255, 255, 255, 255, 255, 255, 255, 255,
    188, 230, 250, 173, 167, 23, 158, 132,
    243, 185, 202, 194, 252, 99, 37, 81}.slice();

// P384 returns a [Curve] which implements NIST P-384 (FIPS 186-3, section D.2.4),
// also known as secp384r1.
//
// Multiple invocations of this function will return the same value, which can
// be used for equality checks and switch statements.
public static ΔCurve P384() {
    return ~p384;
}

internal static ж<nistec.P384Point>> p384 = Ꮡ(new nistCurve[ж<nistec.P384Point>](
    name: "P-384"u8,
    newPoint: nistec.NewP384Point,
    scalarOrder: p384Order
));

internal static slice<byte> p384Order = new byte[]{
    255, 255, 255, 255, 255, 255, 255, 255,
    255, 255, 255, 255, 255, 255, 255, 255,
    255, 255, 255, 255, 255, 255, 255, 255,
    199, 99, 77, 129, 244, 55, 45, 223,
    88, 26, 13, 178, 72, 176, 167, 122,
    236, 236, 25, 106, 204, 197, 41, 115}.slice();

// P521 returns a [Curve] which implements NIST P-521 (FIPS 186-3, section D.2.5),
// also known as secp521r1.
//
// Multiple invocations of this function will return the same value, which can
// be used for equality checks and switch statements.
public static ΔCurve P521() {
    return ~p521;
}

internal static ж<nistec.P521Point>> p521 = Ꮡ(new nistCurve[ж<nistec.P521Point>](
    name: "P-521"u8,
    newPoint: nistec.NewP521Point,
    scalarOrder: p521Order
));

internal static slice<byte> p521Order = new byte[]{1, 255,
    255, 255, 255, 255, 255, 255, 255, 255,
    255, 255, 255, 255, 255, 255, 255, 255,
    255, 255, 255, 255, 255, 255, 255, 255,
    255, 255, 255, 255, 255, 255, 255, 250,
    81, 134, 135, 131, 191, 47, 150, 107,
    127, 204, 1, 72, 247, 9, 165, 208,
    59, 181, 201, 184, 137, 156, 71, 174,
    187, 111, 183, 30, 145, 56, 100, 9}.slice();

} // end ecdh_package
