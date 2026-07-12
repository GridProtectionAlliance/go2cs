// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using boring = go.crypto.@internal.boring_package;
using nistec = go.crypto.@internal.nistec_package;
using randutil = go.crypto.@internal.randutil_package;
using errors = errors_package;
using byteorder = go.@internal.byteorder_package;
using io = io_package;
using bits = math.bits_package;
using go.@internal;
using go.crypto.@internal;
using math;

partial class ecdh_package {

[GoType] partial struct nistCurve<Point>
    where Point : nistPoint<Point>
{
    internal @string name;
    internal Func<Point> newPoint;
    internal slice<byte> scalarOrder;
}

// nistPoint is a generic constraint for the nistec Point types.
[GoType] partial interface nistPoint<T> {
    slice<byte> Bytes();
    (slice<byte>, error) BytesX();
    (T, error) SetBytes(slice<byte> _);
    (T, error) ScalarMult(T _Δp0, slice<byte> _Δp1);
    (T, error) ScalarBaseMult(slice<byte> _);
}

[GoRecv] internal static @string String<Point>(this ref nistCurve<Point> c)
    where Point : nistPoint<Point>
{
    return c.name;
}

internal static error errInvalidPrivateKey = errors.New("crypto/ecdh: invalid private key"u8);

internal static (ж<PrivateKey>, error) GenerateKey<Point>(this ж<nistCurve<Point>> Ꮡc, io.Reader rand)
    where Point : nistPoint<Point>
{
    ref var c = ref Ꮡc.Value;

    if (boring.Enabled && AreEqual(rand, boring.RandReader)) {
        var (keyΔ1, bytes, err) = boring.GenerateKeyECDH(c.name);
        if (err != default!) {
            return (default!, err);
        }
        return newBoringPrivateKey(new nistCurveжΔCurve<Point>(Ꮡc), keyΔ1, bytes);
    }
    var key = new slice<byte>(len(c.scalarOrder));
    randutil.MaybeReadByte(rand);
    while (ᐧ) {
        {
            var (_, errΔ1) = io.ReadFull(rand, key); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        // Mask off any excess bits if the size of the underlying field is not a
        // whole number of bytes, which is only the case for P-521. We use a
        // pointer to the scalarOrder field because comparing generic and
        // instantiated types is not supported.
        if (Ꮡ(c.scalarOrder[0]) == Ꮡ(p521Order, 0)) {
            key[0] &= (byte)(0b0000_0001);
        }
        // In tests, rand will return all zeros and NewPrivateKey will reject
        // the zero key as it generates the identity as a public key. This also
        // makes this function consistent with crypto/elliptic.GenerateKey.
        key[1] ^= (byte)(0x42);
        var (k, err) = Ꮡc.NewPrivateKey(key);
        if (AreEqual(err, errInvalidPrivateKey)) {
            continue;
        }
        return (k, err);
    }
}

internal static (ж<PrivateKey>, error) NewPrivateKey<Point>(this ж<nistCurve<Point>> Ꮡc, slice<byte> key)
    where Point : nistPoint<Point>
{
    ref var c = ref Ꮡc.Value;

    if (len(key) != len(c.scalarOrder)) {
        return (default!, errors.New("crypto/ecdh: invalid private key size"u8));
    }
    if (isZero(key) || !isLess(key, c.scalarOrder)) {
        return (default!, errInvalidPrivateKey);
    }
    if (boring.Enabled) {
        var (bk, err) = boring.NewPrivateKeyECDH(c.name, key);
        if (err != default!) {
            return (default!, err);
        }
        return newBoringPrivateKey(new nistCurveжΔCurve<Point>(Ꮡc), bk, key);
    }
    var k = Ꮡ(new PrivateKey(
        curve: new nistCurveжΔCurve<Point>(Ꮡc),
        privateKey: append(new byte[]{}.slice(), key.ꓸꓸꓸ)
    ));
    return (k, default!);
}

internal static (ж<PrivateKey>, error) newBoringPrivateKey(ΔCurve c, ж<boring.PrivateKeyECDH> Ꮡbk, slice<byte> privateKey) {
    var k = Ꮡ(new PrivateKey(
        curve: c,
        boring: Ꮡbk,
        privateKey: append(slice<byte>(default!), privateKey.ꓸꓸꓸ)
    ));
    return (k, default!);
}

internal static ж<ΔPublicKey> privateKeyToPublicKey<Point>(this ж<nistCurve<Point>> Ꮡc, ж<PrivateKey> Ꮡkey)
    where Point : nistPoint<Point>
{
    ref var c = ref Ꮡc.Value;
    ref var key = ref Ꮡkey.Value;

    boring.Unreachable();
    if (!AreEqual(key.curve, Ꮡc)) {
        throw panic("crypto/ecdh: internal error: converting the wrong key type");
    }
    var (p, err) = c.newPoint().ScalarBaseMult(key.privateKey);
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
    var (bufA, bufB) = (new slice<byte>(72), new slice<byte>(72));
    foreach (var (i, _) in a) {
        (bufA[i], bufB[i]) = (a[len(a) - i - 1], b[len(b) - i - 1]);
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

internal static (ж<ΔPublicKey>, error) NewPublicKey<Point>(this ж<nistCurve<Point>> Ꮡc, slice<byte> key)
    where Point : nistPoint<Point>
{
    ref var c = ref Ꮡc.Value;

    // Reject the point at infinity and compressed encodings.
    if (len(key) == 0 || key[0] != 4) {
        return (default!, errors.New("crypto/ecdh: invalid public key"u8));
    }
    var k = Ꮡ(new ΔPublicKey(
        curve: new nistCurveжΔCurve<Point>(Ꮡc),
        publicKey: append(new byte[]{}.slice(), key.ꓸꓸꓸ)
    ));
    if (boring.Enabled){
        var (bk, err) = boring.NewPublicKeyECDH(c.name, (~k).publicKey);
        if (err != default!) {
            return (default!, err);
        }
        k.Value.boring = bk;
    } else {
        // SetBytes also checks that the point is on the curve.
        {
            var (_, err) = c.newPoint().SetBytes(key); if (err != default!) {
                return (default!, err);
            }
        }
    }
    return (k, default!);
}

[GoRecv] internal static (slice<byte>, error) ecdh<Point>(this ref nistCurve<Point> c, ж<PrivateKey> Ꮡlocal, ж<ΔPublicKey> Ꮡremote)
    where Point : nistPoint<Point>
{
    ref var local = ref Ꮡlocal.Value;
    ref var remote = ref Ꮡremote.Value;

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
    var (p, err) = c.newPoint().SetBytes(remote.publicKey);
    if (err != default!) {
        return (default!, err);
    }
    {
        var (_, errΔ1) = p.ScalarMult(p, local.privateKey); if (errΔ1 != default!) {
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
    return new nistCurveжΔCurve<P256PointжnistPoint>(p256);
}

internal static ж<nistCurve<P256PointжnistPoint>> p256;
internal static void initᴛp256() { p256 = Ꮡ(new nistCurve<P256PointжnistPoint>(
    name: "P-256"u8,
    newPoint: () => nistec.NewP256Point(),
    scalarOrder: p256Order
)); }

internal static slice<byte> p256Order = new byte[]{
    0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xbc, 0xe6, 0xfa, 0xad, 0xa7, 0x17, 0x9e, 0x84,
    0xf3, 0xb9, 0xca, 0xc2, 0xfc, 0x63, 0x25, 0x51}.slice();

// P384 returns a [Curve] which implements NIST P-384 (FIPS 186-3, section D.2.4),
// also known as secp384r1.
//
// Multiple invocations of this function will return the same value, which can
// be used for equality checks and switch statements.
public static ΔCurve P384() {
    return new nistCurveжΔCurve<P384PointжnistPoint>(p384);
}

internal static ж<nistCurve<P384PointжnistPoint>> p384;
internal static void initᴛp384() { p384 = Ꮡ(new nistCurve<P384PointжnistPoint>(
    name: "P-384"u8,
    newPoint: () => nistec.NewP384Point(),
    scalarOrder: p384Order
)); }

internal static slice<byte> p384Order = new byte[]{
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xc7, 0x63, 0x4d, 0x81, 0xf4, 0x37, 0x2d, 0xdf,
    0x58, 0x1a, 0x0d, 0xb2, 0x48, 0xb0, 0xa7, 0x7a,
    0xec, 0xec, 0x19, 0x6a, 0xcc, 0xc5, 0x29, 0x73}.slice();

// P521 returns a [Curve] which implements NIST P-521 (FIPS 186-3, section D.2.5),
// also known as secp521r1.
//
// Multiple invocations of this function will return the same value, which can
// be used for equality checks and switch statements.
public static ΔCurve P521() {
    return new nistCurveжΔCurve<P521PointжnistPoint>(p521);
}

internal static ж<nistCurve<P521PointжnistPoint>> p521;
internal static void initᴛp521() { p521 = Ꮡ(new nistCurve<P521PointжnistPoint>(
    name: "P-521"u8,
    newPoint: () => nistec.NewP521Point(),
    scalarOrder: p521Order
)); }

internal static ж<slice<byte>> Ꮡp521Order = new(new byte[]{0x01, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfa,
    0x51, 0x86, 0x87, 0x83, 0xbf, 0x2f, 0x96, 0x6b,
    0x7f, 0xcc, 0x01, 0x48, 0xf7, 0x09, 0xa5, 0xd0,
    0x3b, 0xb5, 0xc9, 0xb8, 0x89, 0x9c, 0x47, 0xae,
    0xbb, 0x6f, 0xb7, 0x1e, 0x91, 0x38, 0x64, 0x09}.slice());
internal static ref slice<byte> p521Order => ref Ꮡp521Order.ValueSlot;

} // end ecdh_package
