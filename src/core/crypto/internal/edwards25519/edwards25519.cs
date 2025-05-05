// Copyright (c) 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using field = crypto.@internal.edwards25519.field_package;
using errors = errors_package;
using crypto.@internal.edwards25519;
using ꓸꓸꓸж<Point> = Span<ж<Point>>;

partial class edwards25519_package {

// Point types.
[GoType] partial struct projP1xP1 {
    public crypto.@internal.edwards25519.field_package.Element X;
    public crypto.@internal.edwards25519.field_package.Element Y;
    public crypto.@internal.edwards25519.field_package.Element Z;
    public crypto.@internal.edwards25519.field_package.Element T;
}

[GoType] partial struct projP2 {
    public crypto.@internal.edwards25519.field_package.Element X;
    public crypto.@internal.edwards25519.field_package.Element Y;
    public crypto.@internal.edwards25519.field_package.Element Z;
}

// Point represents a point on the edwards25519 curve.
//
// This type works similarly to math/big.Int, and all arguments and receivers
// are allowed to alias.
//
// The zero value is NOT valid, and it may be used only as a receiver.
[GoType] partial struct Point {
    // Make the type not comparable (i.e. used with == or as a map key), as
    // equivalent points can be represented by different Go values.
    internal incomparable _;
    // The point is internally represented in extended coordinates (X, Y, Z, T)
    // where x = X/Z, y = Y/Z, and xy = T/Z per https://eprint.iacr.org/2008/522.
    internal crypto.@internal.edwards25519.field_package.Element x;
    internal crypto.@internal.edwards25519.field_package.Element y;
    internal crypto.@internal.edwards25519.field_package.Element z;
    internal crypto.@internal.edwards25519.field_package.Element t;
}
// [...]func()

internal static void checkInitialized(params ꓸꓸꓸж<Point> pointsʗp) {
    var points = pointsʗp.slice();

    foreach (var (_, p) in points) {
        if ((~p).x == (new field.Element(nil)) && (~p).y == (new field.Element(nil))) {
            throw panic("edwards25519: use of uninitialized Point");
        }
    }
}

[GoType] partial struct projCached {
    public crypto.@internal.edwards25519.field_package.Element YplusX;
    public crypto.@internal.edwards25519.field_package.Element YminusX;
    public crypto.@internal.edwards25519.field_package.Element Z;
    public crypto.@internal.edwards25519.field_package.Element T2d;
}

[GoType] partial struct affineCached {
    public crypto.@internal.edwards25519.field_package.Element YplusX;
    public crypto.@internal.edwards25519.field_package.Element YminusX;
    public crypto.@internal.edwards25519.field_package.Element T2d;
}

// Constructors.
[GoRecv("capture")] internal static ж<projP2> Zero(this ref projP2 v) {
    v.X.Zero();
    v.Y.One();
    v.Z.One();
    return ZeroꓸᏑv;
}

// identity is the point at infinity.
internal static ж<Point> identity = @new<Point>().SetBytes(new byte[]{
    1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice());
internal static error _;

// NewIdentityPoint returns a new Point set to the identity.
public static ж<Point> NewIdentityPoint() {
    return @new<Point>().Set(identity);
}

// generator is the canonical curve basepoint. See TestGenerator for the
// correspondence of this encoding with the values in RFC 8032.
internal static ж<Point> generator = @new<Point>().SetBytes(new byte[]{
    88, 102, 102, 102, 102, 102, 102, 102,
    102, 102, 102, 102, 102, 102, 102, 102,
    102, 102, 102, 102, 102, 102, 102, 102,
    102, 102, 102, 102, 102, 102, 102, 102}.slice());
internal static error _;

// NewGeneratorPoint returns a new Point set to the canonical generator.
public static ж<Point> NewGeneratorPoint() {
    return @new<Point>().Set(generator);
}

[GoRecv("capture")] internal static ж<projCached> Zero(this ref projCached v) {
    v.YplusX.One();
    v.YminusX.One();
    v.Z.One();
    v.T2d.Zero();
    return ZeroꓸᏑv;
}

[GoRecv("capture")] internal static ж<affineCached> Zero(this ref affineCached v) {
    v.YplusX.One();
    v.YminusX.One();
    v.T2d.Zero();
    return ZeroꓸᏑv;
}

// Assignments.

// Set sets v = u, and returns v.
[GoRecv("capture")] public static ж<Point> Set(this ref Point v, ж<Point> Ꮡu) {
    ref var u = ref Ꮡu.val;

    v = u;
    return SetꓸᏑv;
}

// Encoding.

// Bytes returns the canonical 32-byte encoding of v, according to RFC 8032,
// Section 5.1.2.
[GoRecv] public static slice<byte> Bytes(this ref Point v) {
    // This function is outlined to make the allocations inline in the caller
    // rather than happen on the heap.
    ref var buf = ref heap(new array<byte>(32), out var Ꮡbuf);
    return v.bytes(Ꮡbuf);
}

[GoRecv] public static slice<byte> bytes(this ref Point v, ж<array<byte>> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.val;

    checkInitialized(v);
    ref var zInv = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑzInv);
    field.Element x = default!;
    ref var y = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var Ꮡy);
    zInv.Invert(Ꮡ(v.z));
    // zInv = 1 / Z
    x.Multiply(Ꮡ(v.x), ᏑzInv);
    // x = X / Z
    y.Multiply(Ꮡ(v.y), ᏑzInv);
    // y = Y / Z
    var @out = copyFieldElement(Ꮡbuf, Ꮡy);
    @out[31] |= (byte)(((byte)(x.IsNegative() << (int)(7))));
    return @out;
}

internal static ж<field.Element> feOne = @new<field.Element>().One();

// SetBytes sets v = x, where x is a 32-byte encoding of v. If x does not
// represent a valid point on the curve, SetBytes returns nil and an error and
// the receiver is unchanged. Otherwise, SetBytes returns v.
//
// Note that SetBytes accepts all non-canonical encodings of valid points.
// That is, it follows decoding rules that match most implementations in
// the ecosystem rather than RFC 8032.
[GoRecv("capture")] public static (ж<Point>, error) SetBytes(this ref Point v, slice<byte> x) {
    // Specifically, the non-canonical encodings that are accepted are
    //   1) the ones where the field element is not reduced (see the
    //      (*field.Element).SetBytes docs) and
    //   2) the ones where the x-coordinate is zero and the sign bit is set.
    //
    // Read more at https://hdevalence.ca/blog/2020-10-04-its-25519am,
    // specifically the "Canonical A, R" section.
    (y, err) = @new<field.Element>().SetBytes(x);
    if (err != default!) {
        return (default!, errors.New("edwards25519: invalid point encoding length"u8));
    }
    // -x² + y² = 1 + dx²y²
    // x² + dx²y² = x²(dy² + 1) = y² - 1
    // x² = (y² - 1) / (dy² + 1)
    // u = y² - 1
    var y2 = @new<field.Element>().Square(y);
    var u = @new<field.Element>().Subtract(y2, feOne);
    // v = dy² + 1
    var vv = @new<field.Element>().Multiply(y2, d);
    vv = vv.Add(vv, feOne);
    // x = +√(u/v)
    var (xx, wasSquare) = @new<field.Element>().SqrtRatio(u, vv);
    if (wasSquare == 0) {
        return (default!, errors.New("edwards25519: invalid point encoding"u8));
    }
    // Select the negative square root if the sign bit is set.
    var xxNeg = @new<field.Element>().Negate(xx);
    xx = xx.Select(xxNeg, xx, ((nint)(x[31] >> (int)(7))));
    v.x.Set(xx);
    v.y.Set(y);
    v.z.One();
    v.t.Multiply(xx, y);
    // xy = T / Z
    return (SetBytesꓸᏑv, default!);
}

internal static slice<byte> copyFieldElement(ж<array<byte>> Ꮡbuf, ж<field.Element> Ꮡv) {
    ref var buf = ref Ꮡbuf.val;
    ref var v = ref Ꮡv.val;

    copy(buf[..], v.Bytes());
    return buf[..];
}

// Conversions.
[GoRecv("capture")] internal static ж<projP2> FromP1xP1(this ref projP2 v, ж<projP1xP1> Ꮡp) {
    ref var p = ref Ꮡp.val;

    v.X.Multiply(Ꮡ(p.X), Ꮡ(p.T));
    v.Y.Multiply(Ꮡ(p.Y), Ꮡ(p.Z));
    v.Z.Multiply(Ꮡ(p.Z), Ꮡ(p.T));
    return FromP1xP1ꓸᏑv;
}

[GoRecv("capture")] internal static ж<projP2> FromP3(this ref projP2 v, ж<Point> Ꮡp) {
    ref var p = ref Ꮡp.val;

    v.X.Set(Ꮡ(p.x));
    v.Y.Set(Ꮡ(p.y));
    v.Z.Set(Ꮡ(p.z));
    return FromP3ꓸᏑv;
}

[GoRecv("capture")] public static ж<Point> fromP1xP1(this ref Point v, ж<projP1xP1> Ꮡp) {
    ref var p = ref Ꮡp.val;

    v.x.Multiply(Ꮡ(p.X), Ꮡ(p.T));
    v.y.Multiply(Ꮡ(p.Y), Ꮡ(p.Z));
    v.z.Multiply(Ꮡ(p.Z), Ꮡ(p.T));
    v.t.Multiply(Ꮡ(p.X), Ꮡ(p.Y));
    return fromP1xP1ꓸᏑv;
}

[GoRecv("capture")] public static ж<Point> fromP2(this ref Point v, ж<projP2> Ꮡp) {
    ref var p = ref Ꮡp.val;

    v.x.Multiply(Ꮡ(p.X), Ꮡ(p.Z));
    v.y.Multiply(Ꮡ(p.Y), Ꮡ(p.Z));
    v.z.Square(Ꮡ(p.Z));
    v.t.Multiply(Ꮡ(p.X), Ꮡ(p.Y));
    return fromP2ꓸᏑv;
}

// d is a constant in the curve equation.
internal static ж<field.Element> d = @new<field.Element>().SetBytes(new byte[]{
    163, 120, 89, 19, 202, 77, 235, 117,
    171, 216, 65, 65, 77, 10, 112, 0,
    152, 232, 121, 119, 121, 64, 199, 140,
    115, 254, 111, 43, 238, 108, 3, 82}.slice());
internal static error _;

internal static ж<field.Element> d2 = @new<field.Element>().Add(d, d);

[GoRecv("capture")] internal static ж<projCached> FromP3(this ref projCached v, ж<Point> Ꮡp) {
    ref var p = ref Ꮡp.val;

    v.YplusX.Add(Ꮡ(p.y), Ꮡ(p.x));
    v.YminusX.Subtract(Ꮡ(p.y), Ꮡ(p.x));
    v.Z.Set(Ꮡ(p.z));
    v.T2d.Multiply(Ꮡ(p.t), d2);
    return FromP3ꓸᏑv;
}

[GoRecv("capture")] internal static ж<affineCached> FromP3(this ref affineCached v, ж<Point> Ꮡp) {
    ref var p = ref Ꮡp.val;

    v.YplusX.Add(Ꮡ(p.y), Ꮡ(p.x));
    v.YminusX.Subtract(Ꮡ(p.y), Ꮡ(p.x));
    v.T2d.Multiply(Ꮡ(p.t), d2);
    ref var invZ = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑinvZ);
    invZ.Invert(Ꮡ(p.z));
    v.YplusX.Multiply(Ꮡ(v.YplusX), ᏑinvZ);
    v.YminusX.Multiply(Ꮡ(v.YminusX), ᏑinvZ);
    v.T2d.Multiply(Ꮡ(v.T2d), ᏑinvZ);
    return FromP3ꓸᏑv;
}

// (Re)addition and subtraction.

// Add sets v = p + q, and returns v.
[GoRecv] public static ж<Point> Add(this ref Point v, ж<Point> Ꮡp, ж<Point> Ꮡq) {
    ref var p = ref Ꮡp.val;
    ref var q = ref Ꮡq.val;

    checkInitialized(Ꮡp, q);
    var qCached = @new<projCached>().FromP3(Ꮡq);
    var result = @new<projP1xP1>().Add(Ꮡp, qCached);
    return v.fromP1xP1(result);
}

// Subtract sets v = p - q, and returns v.
[GoRecv] public static ж<Point> Subtract(this ref Point v, ж<Point> Ꮡp, ж<Point> Ꮡq) {
    ref var p = ref Ꮡp.val;
    ref var q = ref Ꮡq.val;

    checkInitialized(Ꮡp, q);
    var qCached = @new<projCached>().FromP3(Ꮡq);
    var result = @new<projP1xP1>().Sub(Ꮡp, qCached);
    return v.fromP1xP1(result);
}

[GoRecv("capture")] internal static ж<projP1xP1> Add(this ref projP1xP1 v, ж<Point> Ꮡp, ж<projCached> Ꮡq) {
    ref var p = ref Ꮡp.val;
    ref var q = ref Ꮡq.val;

    ref var YplusX = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑYplusX);
    ref var YminusX = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑYminusX);
    ref var PP = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑPP);
    ref var MM = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑMM);
    ref var TT2d = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑTT2d);
    ref var ZZ2 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑZZ2);
    YplusX.Add(Ꮡ(p.y), Ꮡ(p.x));
    YminusX.Subtract(Ꮡ(p.y), Ꮡ(p.x));
    PP.Multiply(ᏑYplusX, Ꮡ(q.YplusX));
    MM.Multiply(ᏑYminusX, Ꮡ(q.YminusX));
    TT2d.Multiply(Ꮡ(p.t), Ꮡ(q.T2d));
    ZZ2.Multiply(Ꮡ(p.z), Ꮡ(q.Z));
    ZZ2.Add(ᏑZZ2, ᏑZZ2);
    v.X.Subtract(ᏑPP, ᏑMM);
    v.Y.Add(ᏑPP, ᏑMM);
    v.Z.Add(ᏑZZ2, ᏑTT2d);
    v.T.Subtract(ᏑZZ2, ᏑTT2d);
    return AddꓸᏑv;
}

[GoRecv("capture")] internal static ж<projP1xP1> Sub(this ref projP1xP1 v, ж<Point> Ꮡp, ж<projCached> Ꮡq) {
    ref var p = ref Ꮡp.val;
    ref var q = ref Ꮡq.val;

    ref var YplusX = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑYplusX);
    ref var YminusX = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑYminusX);
    ref var PP = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑPP);
    ref var MM = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑMM);
    ref var TT2d = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑTT2d);
    ref var ZZ2 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑZZ2);
    YplusX.Add(Ꮡ(p.y), Ꮡ(p.x));
    YminusX.Subtract(Ꮡ(p.y), Ꮡ(p.x));
    PP.Multiply(ᏑYplusX, Ꮡ(q.YminusX));
    // flipped sign
    MM.Multiply(ᏑYminusX, Ꮡ(q.YplusX));
    // flipped sign
    TT2d.Multiply(Ꮡ(p.t), Ꮡ(q.T2d));
    ZZ2.Multiply(Ꮡ(p.z), Ꮡ(q.Z));
    ZZ2.Add(ᏑZZ2, ᏑZZ2);
    v.X.Subtract(ᏑPP, ᏑMM);
    v.Y.Add(ᏑPP, ᏑMM);
    v.Z.Subtract(ᏑZZ2, ᏑTT2d);
    // flipped sign
    v.T.Add(ᏑZZ2, ᏑTT2d);
    // flipped sign
    return SubꓸᏑv;
}

[GoRecv("capture")] internal static ж<projP1xP1> AddAffine(this ref projP1xP1 v, ж<Point> Ꮡp, ж<affineCached> Ꮡq) {
    ref var p = ref Ꮡp.val;
    ref var q = ref Ꮡq.val;

    ref var YplusX = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑYplusX);
    ref var YminusX = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑYminusX);
    ref var PP = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑPP);
    ref var MM = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑMM);
    ref var TT2d = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑTT2d);
    ref var Z2 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑZ2);
    YplusX.Add(Ꮡ(p.y), Ꮡ(p.x));
    YminusX.Subtract(Ꮡ(p.y), Ꮡ(p.x));
    PP.Multiply(ᏑYplusX, Ꮡ(q.YplusX));
    MM.Multiply(ᏑYminusX, Ꮡ(q.YminusX));
    TT2d.Multiply(Ꮡ(p.t), Ꮡ(q.T2d));
    Z2.Add(Ꮡ(p.z), Ꮡ(p.z));
    v.X.Subtract(ᏑPP, ᏑMM);
    v.Y.Add(ᏑPP, ᏑMM);
    v.Z.Add(ᏑZ2, ᏑTT2d);
    v.T.Subtract(ᏑZ2, ᏑTT2d);
    return AddAffineꓸᏑv;
}

[GoRecv("capture")] internal static ж<projP1xP1> SubAffine(this ref projP1xP1 v, ж<Point> Ꮡp, ж<affineCached> Ꮡq) {
    ref var p = ref Ꮡp.val;
    ref var q = ref Ꮡq.val;

    ref var YplusX = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑYplusX);
    ref var YminusX = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑYminusX);
    ref var PP = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑPP);
    ref var MM = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑMM);
    ref var TT2d = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑTT2d);
    ref var Z2 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑZ2);
    YplusX.Add(Ꮡ(p.y), Ꮡ(p.x));
    YminusX.Subtract(Ꮡ(p.y), Ꮡ(p.x));
    PP.Multiply(ᏑYplusX, Ꮡ(q.YminusX));
    // flipped sign
    MM.Multiply(ᏑYminusX, Ꮡ(q.YplusX));
    // flipped sign
    TT2d.Multiply(Ꮡ(p.t), Ꮡ(q.T2d));
    Z2.Add(Ꮡ(p.z), Ꮡ(p.z));
    v.X.Subtract(ᏑPP, ᏑMM);
    v.Y.Add(ᏑPP, ᏑMM);
    v.Z.Subtract(ᏑZ2, ᏑTT2d);
    // flipped sign
    v.T.Add(ᏑZ2, ᏑTT2d);
    // flipped sign
    return SubAffineꓸᏑv;
}

// Doubling.
[GoRecv("capture")] internal static ж<projP1xP1> Double(this ref projP1xP1 v, ж<projP2> Ꮡp) {
    ref var p = ref Ꮡp.val;

    ref var XX = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑXX);
    ref var YY = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑYY);
    ref var ZZ2 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑZZ2);
    ref var XplusYsq = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var ᏑXplusYsq);
    XX.Square(Ꮡ(p.X));
    YY.Square(Ꮡ(p.Y));
    ZZ2.Square(Ꮡ(p.Z));
    ZZ2.Add(ᏑZZ2, ᏑZZ2);
    XplusYsq.Add(Ꮡ(p.X), Ꮡ(p.Y));
    XplusYsq.Square(ᏑXplusYsq);
    v.Y.Add(ᏑYY, ᏑXX);
    v.Z.Subtract(ᏑYY, ᏑXX);
    v.X.Subtract(ᏑXplusYsq, Ꮡ(v.Y));
    v.T.Subtract(ᏑZZ2, Ꮡ(v.Z));
    return DoubleꓸᏑv;
}

// Negation.

// Negate sets v = -p, and returns v.
[GoRecv("capture")] public static ж<Point> Negate(this ref Point v, ж<Point> Ꮡp) {
    ref var p = ref Ꮡp.val;

    checkInitialized(Ꮡp);
    v.x.Negate(Ꮡ(p.x));
    v.y.Set(Ꮡ(p.y));
    v.z.Set(Ꮡ(p.z));
    v.t.Negate(Ꮡ(p.t));
    return NegateꓸᏑv;
}

// Equal returns 1 if v is equivalent to u, and 0 otherwise.
[GoRecv] public static nint Equal(this ref Point v, ж<Point> Ꮡu) {
    ref var u = ref Ꮡu.val;

    checkInitialized(v, u);
    field.Element t1 = default!;
    ref var t2 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var Ꮡt2);
    field.Element t3 = default!;
    ref var t4 = ref heap(new crypto.@internal.edwards25519.field_package.Element(), out var Ꮡt4);
    t1.Multiply(Ꮡ(v.x), Ꮡ(u.z));
    t2.Multiply(Ꮡ(u.x), Ꮡ(v.z));
    t3.Multiply(Ꮡ(v.y), Ꮡ(u.z));
    t4.Multiply(Ꮡ(u.y), Ꮡ(v.z));
    return (nint)(t1.Equal(Ꮡt2) & t3.Equal(Ꮡt4));
}

// Constant-time operations

// Select sets v to a if cond == 1 and to b if cond == 0.
[GoRecv("capture")] internal static ж<projCached> Select(this ref projCached v, ж<projCached> Ꮡa, ж<projCached> Ꮡb, nint cond) {
    ref var a = ref Ꮡa.val;
    ref var b = ref Ꮡb.val;

    v.YplusX.Select(Ꮡ(a.YplusX), Ꮡ(b.YplusX), cond);
    v.YminusX.Select(Ꮡ(a.YminusX), Ꮡ(b.YminusX), cond);
    v.Z.Select(Ꮡ(a.Z), Ꮡ(b.Z), cond);
    v.T2d.Select(Ꮡ(a.T2d), Ꮡ(b.T2d), cond);
    return SelectꓸᏑv;
}

// Select sets v to a if cond == 1 and to b if cond == 0.
[GoRecv("capture")] internal static ж<affineCached> Select(this ref affineCached v, ж<affineCached> Ꮡa, ж<affineCached> Ꮡb, nint cond) {
    ref var a = ref Ꮡa.val;
    ref var b = ref Ꮡb.val;

    v.YplusX.Select(Ꮡ(a.YplusX), Ꮡ(b.YplusX), cond);
    v.YminusX.Select(Ꮡ(a.YminusX), Ꮡ(b.YminusX), cond);
    v.T2d.Select(Ꮡ(a.T2d), Ꮡ(b.T2d), cond);
    return SelectꓸᏑv;
}

// CondNeg negates v if cond == 1 and leaves it unchanged if cond == 0.
[GoRecv("capture")] internal static ж<projCached> CondNeg(this ref projCached v, nint cond) {
    v.YplusX.Swap(Ꮡ(v.YminusX), cond);
    v.T2d.Select(@new<field.Element>().Negate(Ꮡ(v.T2d)), Ꮡ(v.T2d), cond);
    return CondNegꓸᏑv;
}

// CondNeg negates v if cond == 1 and leaves it unchanged if cond == 0.
[GoRecv("capture")] internal static ж<affineCached> CondNeg(this ref affineCached v, nint cond) {
    v.YplusX.Swap(Ꮡ(v.YminusX), cond);
    v.T2d.Select(@new<field.Element>().Negate(Ꮡ(v.T2d)), Ꮡ(v.T2d), cond);
    return CondNegꓸᏑv;
}

} // end edwards25519_package
