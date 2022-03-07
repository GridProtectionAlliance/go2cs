// Copyright (c) 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package edwards25519 -- go2cs converted at 2022 March 06 22:17:24 UTC
// import "crypto/ed25519/internal/edwards25519" ==> using edwards25519 = go.crypto.ed25519.@internal.edwards25519_package
// Original source: C:\Program Files\Go\src\crypto\ed25519\internal\edwards25519\edwards25519.go
using field = go.crypto.ed25519.@internal.edwards25519.field_package;
using errors = go.errors_package;
using System;


namespace go.crypto.ed25519.@internal;

public static partial class edwards25519_package {

    // Point types.
private partial struct projP1xP1 {
    public field.Element X;
    public field.Element Y;
    public field.Element Z;
    public field.Element T;
}

private partial struct projP2 {
    public field.Element X;
    public field.Element Y;
    public field.Element Z;
}

// Point represents a point on the edwards25519 curve.
//
// This type works similarly to math/big.Int, and all arguments and receivers
// are allowed to alias.
//
// The zero value is NOT valid, and it may be used only as a receiver.
public partial struct Point {
    public field.Element x; // Make the type not comparable (i.e. used with == or as a map key), as
// equivalent points can be represented by different Go values.
    public field.Element y; // Make the type not comparable (i.e. used with == or as a map key), as
// equivalent points can be represented by different Go values.
    public field.Element z; // Make the type not comparable (i.e. used with == or as a map key), as
// equivalent points can be represented by different Go values.
    public field.Element t; // Make the type not comparable (i.e. used with == or as a map key), as
// equivalent points can be represented by different Go values.
    public incomparable _;
}

private partial struct incomparable { // : array<Action>
}

private static void checkInitialized(params ptr<ptr<Point>>[] _addr_points) => func((_, panic, _) => {
    points = points.Clone();
    ref Point points = ref _addr_points.val;

    foreach (var (_, p) in points) {
        if (p.x == (new field.Element()) && p.y == (new field.Element())) {
            panic("edwards25519: use of uninitialized Point");
        }
    }
});

private partial struct projCached {
    public field.Element YplusX;
    public field.Element YminusX;
    public field.Element Z;
    public field.Element T2d;
}

private partial struct affineCached {
    public field.Element YplusX;
    public field.Element YminusX;
    public field.Element T2d;
}

// Constructors.

private static ptr<projP2> Zero(this ptr<projP2> _addr_v) {
    ref projP2 v = ref _addr_v.val;

    v.X.Zero();
    v.Y.One();
    v.Z.One();
    return _addr_v!;
}

// identity is the point at infinity.


// NewIdentityPoint returns a new Point set to the identity.
public static ptr<Point> NewIdentityPoint() {
    return @new<Point>().Set(identity);
}

// generator is the canonical curve basepoint. See TestGenerator for the
// correspondence of this encoding with the values in RFC 8032.


// NewGeneratorPoint returns a new Point set to the canonical generator.
public static ptr<Point> NewGeneratorPoint() {
    return @new<Point>().Set(generator);
}

private static ptr<projCached> Zero(this ptr<projCached> _addr_v) {
    ref projCached v = ref _addr_v.val;

    v.YplusX.One();
    v.YminusX.One();
    v.Z.One();
    v.T2d.Zero();
    return _addr_v!;
}

private static ptr<affineCached> Zero(this ptr<affineCached> _addr_v) {
    ref affineCached v = ref _addr_v.val;

    v.YplusX.One();
    v.YminusX.One();
    v.T2d.Zero();
    return _addr_v!;
}

// Assignments.

// Set sets v = u, and returns v.
private static ptr<Point> Set(this ptr<Point> _addr_v, ptr<Point> _addr_u) {
    ref Point v = ref _addr_v.val;
    ref Point u = ref _addr_u.val;

    v.val = u;
    return _addr_v!;
}

// Encoding.

// Bytes returns the canonical 32-byte encoding of v, according to RFC 8032,
// Section 5.1.2.
private static slice<byte> Bytes(this ptr<Point> _addr_v) {
    ref Point v = ref _addr_v.val;
 
    // This function is outlined to make the allocations inline in the caller
    // rather than happen on the heap.
    ref array<byte> buf = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_buf);
    return v.bytes(_addr_buf);

}

private static slice<byte> bytes(this ptr<Point> _addr_v, ptr<array<byte>> _addr_buf) {
    ref Point v = ref _addr_v.val;
    ref array<byte> buf = ref _addr_buf.val;

    checkInitialized(_addr_v);

    ref field.Element zInv = ref heap(out ptr<field.Element> _addr_zInv);    field.Element x = default;    ref field.Element y = ref heap(out ptr<field.Element> _addr_y);

    zInv.Invert(_addr_v.z); // zInv = 1 / Z
    x.Multiply(_addr_v.x, _addr_zInv); // x = X / Z
    y.Multiply(_addr_v.y, _addr_zInv); // y = Y / Z

    var @out = copyFieldElement(_addr_buf, _addr_y);
    out[31] |= byte(x.IsNegative() << 7);
    return out;

}

private static ptr<field.Element> feOne = @new<field.Element>().One();

// SetBytes sets v = x, where x is a 32-byte encoding of v. If x does not
// represent a valid point on the curve, SetBytes returns nil and an error and
// the receiver is unchanged. Otherwise, SetBytes returns v.
//
// Note that SetBytes accepts all non-canonical encodings of valid points.
// That is, it follows decoding rules that match most implementations in
// the ecosystem rather than RFC 8032.
private static (ptr<Point>, error) SetBytes(this ptr<Point> _addr_v, slice<byte> x) {
    ptr<Point> _p0 = default!;
    error _p0 = default!;
    ref Point v = ref _addr_v.val;
 
    // Specifically, the non-canonical encodings that are accepted are
    //   1) the ones where the field element is not reduced (see the
    //      (*field.Element).SetBytes docs) and
    //   2) the ones where the x-coordinate is zero and the sign bit is set.
    //
    // This is consistent with crypto/ed25519/internal/edwards25519. Read more
    // at https://hdevalence.ca/blog/2020-10-04-its-25519am, specifically the
    // "Canonical A, R" section.

    if (len(x) != 32) {
        return (_addr_null!, error.As(errors.New("edwards25519: invalid point encoding length"))!);
    }
    ptr<field.Element> y = @new<field.Element>().SetBytes(x); 

    // -x² + y² = 1 + dx²y²
    // x² + dx²y² = x²(dy² + 1) = y² - 1
    // x² = (y² - 1) / (dy² + 1)

    // u = y² - 1
    ptr<field.Element> y2 = @new<field.Element>().Square(y);
    ptr<field.Element> u = @new<field.Element>().Subtract(y2, feOne); 

    // v = dy² + 1
    ptr<field.Element> vv = @new<field.Element>().Multiply(y2, d);
    vv = vv.Add(vv, feOne); 

    // x = +√(u/v)
    ptr<field.Element> (xx, wasSquare) = @new<field.Element>().SqrtRatio(u, vv);
    if (wasSquare == 0) {
        return (_addr_null!, error.As(errors.New("edwards25519: invalid point encoding"))!);
    }
    ptr<field.Element> xxNeg = @new<field.Element>().Negate(xx);
    xx = xx.Select(xxNeg, xx, int(x[31] >> 7));

    v.x.Set(xx);
    v.y.Set(y);
    v.z.One();
    v.t.Multiply(xx, y); // xy = T / Z

    return (_addr_v!, error.As(null!)!);

}

private static slice<byte> copyFieldElement(ptr<array<byte>> _addr_buf, ptr<field.Element> _addr_v) {
    ref array<byte> buf = ref _addr_buf.val;
    ref field.Element v = ref _addr_v.val;

    copy(buf[..], v.Bytes());
    return buf[..];
}

// Conversions.

private static ptr<projP2> FromP1xP1(this ptr<projP2> _addr_v, ptr<projP1xP1> _addr_p) {
    ref projP2 v = ref _addr_v.val;
    ref projP1xP1 p = ref _addr_p.val;

    v.X.Multiply(_addr_p.X, _addr_p.T);
    v.Y.Multiply(_addr_p.Y, _addr_p.Z);
    v.Z.Multiply(_addr_p.Z, _addr_p.T);
    return _addr_v!;
}

private static ptr<projP2> FromP3(this ptr<projP2> _addr_v, ptr<Point> _addr_p) {
    ref projP2 v = ref _addr_v.val;
    ref Point p = ref _addr_p.val;

    v.X.Set(_addr_p.x);
    v.Y.Set(_addr_p.y);
    v.Z.Set(_addr_p.z);
    return _addr_v!;
}

private static ptr<Point> fromP1xP1(this ptr<Point> _addr_v, ptr<projP1xP1> _addr_p) {
    ref Point v = ref _addr_v.val;
    ref projP1xP1 p = ref _addr_p.val;

    v.x.Multiply(_addr_p.X, _addr_p.T);
    v.y.Multiply(_addr_p.Y, _addr_p.Z);
    v.z.Multiply(_addr_p.Z, _addr_p.T);
    v.t.Multiply(_addr_p.X, _addr_p.Y);
    return _addr_v!;
}

private static ptr<Point> fromP2(this ptr<Point> _addr_v, ptr<projP2> _addr_p) {
    ref Point v = ref _addr_v.val;
    ref projP2 p = ref _addr_p.val;

    v.x.Multiply(_addr_p.X, _addr_p.Z);
    v.y.Multiply(_addr_p.Y, _addr_p.Z);
    v.z.Square(_addr_p.Z);
    v.t.Multiply(_addr_p.X, _addr_p.Y);
    return _addr_v!;
}

// d is a constant in the curve equation.
private static ptr<field.Element> d = @new<field.Element>().SetBytes(new slice<byte>(new byte[] { 0xa3, 0x78, 0x59, 0x13, 0xca, 0x4d, 0xeb, 0x75, 0xab, 0xd8, 0x41, 0x41, 0x4d, 0x0a, 0x70, 0x00, 0x98, 0xe8, 0x79, 0x77, 0x79, 0x40, 0xc7, 0x8c, 0x73, 0xfe, 0x6f, 0x2b, 0xee, 0x6c, 0x03, 0x52 }));
private static ptr<field.Element> d2 = @new<field.Element>().Add(d, d);

private static ptr<projCached> FromP3(this ptr<projCached> _addr_v, ptr<Point> _addr_p) {
    ref projCached v = ref _addr_v.val;
    ref Point p = ref _addr_p.val;

    v.YplusX.Add(_addr_p.y, _addr_p.x);
    v.YminusX.Subtract(_addr_p.y, _addr_p.x);
    v.Z.Set(_addr_p.z);
    v.T2d.Multiply(_addr_p.t, d2);
    return _addr_v!;
}

private static ptr<affineCached> FromP3(this ptr<affineCached> _addr_v, ptr<Point> _addr_p) {
    ref affineCached v = ref _addr_v.val;
    ref Point p = ref _addr_p.val;

    v.YplusX.Add(_addr_p.y, _addr_p.x);
    v.YminusX.Subtract(_addr_p.y, _addr_p.x);
    v.T2d.Multiply(_addr_p.t, d2);

    ref field.Element invZ = ref heap(out ptr<field.Element> _addr_invZ);
    invZ.Invert(_addr_p.z);
    v.YplusX.Multiply(_addr_v.YplusX, _addr_invZ);
    v.YminusX.Multiply(_addr_v.YminusX, _addr_invZ);
    v.T2d.Multiply(_addr_v.T2d, _addr_invZ);
    return _addr_v!;
}

// (Re)addition and subtraction.

// Add sets v = p + q, and returns v.
private static ptr<Point> Add(this ptr<Point> _addr_v, ptr<Point> _addr_p, ptr<Point> _addr_q) {
    ref Point v = ref _addr_v.val;
    ref Point p = ref _addr_p.val;
    ref Point q = ref _addr_q.val;

    checkInitialized(_addr_p, q);
    ptr<projCached> qCached = @new<projCached>().FromP3(q);
    ptr<projP1xP1> result = @new<projP1xP1>().Add(p, qCached);
    return _addr_v.fromP1xP1(result)!;
}

// Subtract sets v = p - q, and returns v.
private static ptr<Point> Subtract(this ptr<Point> _addr_v, ptr<Point> _addr_p, ptr<Point> _addr_q) {
    ref Point v = ref _addr_v.val;
    ref Point p = ref _addr_p.val;
    ref Point q = ref _addr_q.val;

    checkInitialized(_addr_p, q);
    ptr<projCached> qCached = @new<projCached>().FromP3(q);
    ptr<projP1xP1> result = @new<projP1xP1>().Sub(p, qCached);
    return _addr_v.fromP1xP1(result)!;
}

private static ptr<projP1xP1> Add(this ptr<projP1xP1> _addr_v, ptr<Point> _addr_p, ptr<projCached> _addr_q) {
    ref projP1xP1 v = ref _addr_v.val;
    ref Point p = ref _addr_p.val;
    ref projCached q = ref _addr_q.val;

    ref field.Element YplusX = ref heap(out ptr<field.Element> _addr_YplusX);    ref field.Element YminusX = ref heap(out ptr<field.Element> _addr_YminusX);    ref field.Element PP = ref heap(out ptr<field.Element> _addr_PP);    ref field.Element MM = ref heap(out ptr<field.Element> _addr_MM);    ref field.Element TT2d = ref heap(out ptr<field.Element> _addr_TT2d);    ref field.Element ZZ2 = ref heap(out ptr<field.Element> _addr_ZZ2);



    YplusX.Add(_addr_p.y, _addr_p.x);
    YminusX.Subtract(_addr_p.y, _addr_p.x);

    PP.Multiply(_addr_YplusX, _addr_q.YplusX);
    MM.Multiply(_addr_YminusX, _addr_q.YminusX);
    TT2d.Multiply(_addr_p.t, _addr_q.T2d);
    ZZ2.Multiply(_addr_p.z, _addr_q.Z);

    ZZ2.Add(_addr_ZZ2, _addr_ZZ2);

    v.X.Subtract(_addr_PP, _addr_MM);
    v.Y.Add(_addr_PP, _addr_MM);
    v.Z.Add(_addr_ZZ2, _addr_TT2d);
    v.T.Subtract(_addr_ZZ2, _addr_TT2d);
    return _addr_v!;
}

private static ptr<projP1xP1> Sub(this ptr<projP1xP1> _addr_v, ptr<Point> _addr_p, ptr<projCached> _addr_q) {
    ref projP1xP1 v = ref _addr_v.val;
    ref Point p = ref _addr_p.val;
    ref projCached q = ref _addr_q.val;

    ref field.Element YplusX = ref heap(out ptr<field.Element> _addr_YplusX);    ref field.Element YminusX = ref heap(out ptr<field.Element> _addr_YminusX);    ref field.Element PP = ref heap(out ptr<field.Element> _addr_PP);    ref field.Element MM = ref heap(out ptr<field.Element> _addr_MM);    ref field.Element TT2d = ref heap(out ptr<field.Element> _addr_TT2d);    ref field.Element ZZ2 = ref heap(out ptr<field.Element> _addr_ZZ2);



    YplusX.Add(_addr_p.y, _addr_p.x);
    YminusX.Subtract(_addr_p.y, _addr_p.x);

    PP.Multiply(_addr_YplusX, _addr_q.YminusX); // flipped sign
    MM.Multiply(_addr_YminusX, _addr_q.YplusX); // flipped sign
    TT2d.Multiply(_addr_p.t, _addr_q.T2d);
    ZZ2.Multiply(_addr_p.z, _addr_q.Z);

    ZZ2.Add(_addr_ZZ2, _addr_ZZ2);

    v.X.Subtract(_addr_PP, _addr_MM);
    v.Y.Add(_addr_PP, _addr_MM);
    v.Z.Subtract(_addr_ZZ2, _addr_TT2d); // flipped sign
    v.T.Add(_addr_ZZ2, _addr_TT2d); // flipped sign
    return _addr_v!;

}

private static ptr<projP1xP1> AddAffine(this ptr<projP1xP1> _addr_v, ptr<Point> _addr_p, ptr<affineCached> _addr_q) {
    ref projP1xP1 v = ref _addr_v.val;
    ref Point p = ref _addr_p.val;
    ref affineCached q = ref _addr_q.val;

    ref field.Element YplusX = ref heap(out ptr<field.Element> _addr_YplusX);    ref field.Element YminusX = ref heap(out ptr<field.Element> _addr_YminusX);    ref field.Element PP = ref heap(out ptr<field.Element> _addr_PP);    ref field.Element MM = ref heap(out ptr<field.Element> _addr_MM);    ref field.Element TT2d = ref heap(out ptr<field.Element> _addr_TT2d);    ref field.Element Z2 = ref heap(out ptr<field.Element> _addr_Z2);



    YplusX.Add(_addr_p.y, _addr_p.x);
    YminusX.Subtract(_addr_p.y, _addr_p.x);

    PP.Multiply(_addr_YplusX, _addr_q.YplusX);
    MM.Multiply(_addr_YminusX, _addr_q.YminusX);
    TT2d.Multiply(_addr_p.t, _addr_q.T2d);

    Z2.Add(_addr_p.z, _addr_p.z);

    v.X.Subtract(_addr_PP, _addr_MM);
    v.Y.Add(_addr_PP, _addr_MM);
    v.Z.Add(_addr_Z2, _addr_TT2d);
    v.T.Subtract(_addr_Z2, _addr_TT2d);
    return _addr_v!;
}

private static ptr<projP1xP1> SubAffine(this ptr<projP1xP1> _addr_v, ptr<Point> _addr_p, ptr<affineCached> _addr_q) {
    ref projP1xP1 v = ref _addr_v.val;
    ref Point p = ref _addr_p.val;
    ref affineCached q = ref _addr_q.val;

    ref field.Element YplusX = ref heap(out ptr<field.Element> _addr_YplusX);    ref field.Element YminusX = ref heap(out ptr<field.Element> _addr_YminusX);    ref field.Element PP = ref heap(out ptr<field.Element> _addr_PP);    ref field.Element MM = ref heap(out ptr<field.Element> _addr_MM);    ref field.Element TT2d = ref heap(out ptr<field.Element> _addr_TT2d);    ref field.Element Z2 = ref heap(out ptr<field.Element> _addr_Z2);



    YplusX.Add(_addr_p.y, _addr_p.x);
    YminusX.Subtract(_addr_p.y, _addr_p.x);

    PP.Multiply(_addr_YplusX, _addr_q.YminusX); // flipped sign
    MM.Multiply(_addr_YminusX, _addr_q.YplusX); // flipped sign
    TT2d.Multiply(_addr_p.t, _addr_q.T2d);

    Z2.Add(_addr_p.z, _addr_p.z);

    v.X.Subtract(_addr_PP, _addr_MM);
    v.Y.Add(_addr_PP, _addr_MM);
    v.Z.Subtract(_addr_Z2, _addr_TT2d); // flipped sign
    v.T.Add(_addr_Z2, _addr_TT2d); // flipped sign
    return _addr_v!;

}

// Doubling.

private static ptr<projP1xP1> Double(this ptr<projP1xP1> _addr_v, ptr<projP2> _addr_p) {
    ref projP1xP1 v = ref _addr_v.val;
    ref projP2 p = ref _addr_p.val;

    ref field.Element XX = ref heap(out ptr<field.Element> _addr_XX);    ref field.Element YY = ref heap(out ptr<field.Element> _addr_YY);    ref field.Element ZZ2 = ref heap(out ptr<field.Element> _addr_ZZ2);    ref field.Element XplusYsq = ref heap(out ptr<field.Element> _addr_XplusYsq);



    XX.Square(_addr_p.X);
    YY.Square(_addr_p.Y);
    ZZ2.Square(_addr_p.Z);
    ZZ2.Add(_addr_ZZ2, _addr_ZZ2);
    XplusYsq.Add(_addr_p.X, _addr_p.Y);
    XplusYsq.Square(_addr_XplusYsq);

    v.Y.Add(_addr_YY, _addr_XX);
    v.Z.Subtract(_addr_YY, _addr_XX);

    v.X.Subtract(_addr_XplusYsq, _addr_v.Y);
    v.T.Subtract(_addr_ZZ2, _addr_v.Z);
    return _addr_v!;
}

// Negation.

// Negate sets v = -p, and returns v.
private static ptr<Point> Negate(this ptr<Point> _addr_v, ptr<Point> _addr_p) {
    ref Point v = ref _addr_v.val;
    ref Point p = ref _addr_p.val;

    checkInitialized(_addr_p);
    v.x.Negate(_addr_p.x);
    v.y.Set(_addr_p.y);
    v.z.Set(_addr_p.z);
    v.t.Negate(_addr_p.t);
    return _addr_v!;
}

// Equal returns 1 if v is equivalent to u, and 0 otherwise.
private static nint Equal(this ptr<Point> _addr_v, ptr<Point> _addr_u) {
    ref Point v = ref _addr_v.val;
    ref Point u = ref _addr_u.val;

    checkInitialized(_addr_v, u);

    field.Element t1 = default;    ref field.Element t2 = ref heap(out ptr<field.Element> _addr_t2);    field.Element t3 = default;    ref field.Element t4 = ref heap(out ptr<field.Element> _addr_t4);

    t1.Multiply(_addr_v.x, _addr_u.z);
    t2.Multiply(_addr_u.x, _addr_v.z);
    t3.Multiply(_addr_v.y, _addr_u.z);
    t4.Multiply(_addr_u.y, _addr_v.z);

    return t1.Equal(_addr_t2) & t3.Equal(_addr_t4);
}

// Constant-time operations

// Select sets v to a if cond == 1 and to b if cond == 0.
private static ptr<projCached> Select(this ptr<projCached> _addr_v, ptr<projCached> _addr_a, ptr<projCached> _addr_b, nint cond) {
    ref projCached v = ref _addr_v.val;
    ref projCached a = ref _addr_a.val;
    ref projCached b = ref _addr_b.val;

    v.YplusX.Select(_addr_a.YplusX, _addr_b.YplusX, cond);
    v.YminusX.Select(_addr_a.YminusX, _addr_b.YminusX, cond);
    v.Z.Select(_addr_a.Z, _addr_b.Z, cond);
    v.T2d.Select(_addr_a.T2d, _addr_b.T2d, cond);
    return _addr_v!;
}

// Select sets v to a if cond == 1 and to b if cond == 0.
private static ptr<affineCached> Select(this ptr<affineCached> _addr_v, ptr<affineCached> _addr_a, ptr<affineCached> _addr_b, nint cond) {
    ref affineCached v = ref _addr_v.val;
    ref affineCached a = ref _addr_a.val;
    ref affineCached b = ref _addr_b.val;

    v.YplusX.Select(_addr_a.YplusX, _addr_b.YplusX, cond);
    v.YminusX.Select(_addr_a.YminusX, _addr_b.YminusX, cond);
    v.T2d.Select(_addr_a.T2d, _addr_b.T2d, cond);
    return _addr_v!;
}

// CondNeg negates v if cond == 1 and leaves it unchanged if cond == 0.
private static ptr<projCached> CondNeg(this ptr<projCached> _addr_v, nint cond) {
    ref projCached v = ref _addr_v.val;

    v.YplusX.Swap(_addr_v.YminusX, cond);
    v.T2d.Select(@new<field.Element>().Negate(_addr_v.T2d), _addr_v.T2d, cond);
    return _addr_v!;
}

// CondNeg negates v if cond == 1 and leaves it unchanged if cond == 0.
private static ptr<affineCached> CondNeg(this ptr<affineCached> _addr_v, nint cond) {
    ref affineCached v = ref _addr_v.val;

    v.YplusX.Swap(_addr_v.YminusX, cond);
    v.T2d.Select(@new<field.Element>().Negate(_addr_v.T2d), _addr_v.T2d, cond);
    return _addr_v!;
}

} // end edwards25519_package
