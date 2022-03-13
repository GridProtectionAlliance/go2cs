// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package elliptic -- go2cs converted at 2022 March 13 05:34:07 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Program Files\Go\src\crypto\elliptic\p521.go
namespace go.crypto;

using fiat = crypto.elliptic.@internal.fiat_package;
using big = math.big_package;

public static partial class elliptic_package {

private partial struct p521Curve {
    public ref ptr<CurveParams> ptr<CurveParams> => ref ptr<CurveParams>_ptr;
}

private static p521Curve p521 = default;
private static ptr<CurveParams> p521Params;

private static void initP521() { 
    // See FIPS 186-3, section D.2.5
    p521.CurveParams = addr(new CurveParams(Name:"P-521"));
    p521.P, _ = @new<big.Int>().SetString("6864797660130609714981900799081393217269435300143305409394463459185543183397656052122559640661454554977296311391480858037121987999716643812574028291115057151", 10);
    p521.N, _ = @new<big.Int>().SetString("6864797660130609714981900799081393217269435300143305409394463459185543183397655394245057746333217197532963996371363321113864768612440380340372808892707005449", 10);
    p521.B, _ = @new<big.Int>().SetString("051953eb9618e1c9a1f929a21a0b68540eea2da725b99b315f3b8b489918ef109e156193951ec7e937b1652c0bd3bb1bf073573df883d2c34f1ef451fd46b503f00", 16);
    p521.Gx, _ = @new<big.Int>().SetString("c6858e06b70404e9cd9e3ecb662395b4429c648139053fb521f828af606b4d3dbaa14b5e77efe75928fe1dc127a2ffa8de3348b3c1856a429bf97e7e31c2e5bd66", 16);
    p521.Gy, _ = @new<big.Int>().SetString("11839296a789a3bc0045c8a5fb42c7d1bd998f54449579b446817afbd17273e662c97ee72995ef42640c550b9013fad0761353c7086a272c24088be94769fd16650", 16);
    p521.BitSize = 521;
}

private static ptr<CurveParams> Params(this p521Curve curve) {
    return _addr_curve.CurveParams!;
}

private static bool IsOnCurve(this p521Curve curve, ptr<big.Int> _addr_x, ptr<big.Int> _addr_y) {
    ref big.Int x = ref _addr_x.val;
    ref big.Int y = ref _addr_y.val;

    var x1 = bigIntToFiatP521(_addr_x);
    var y1 = bigIntToFiatP521(_addr_y);
    var b = bigIntToFiatP521(_addr_curve.B); // TODO: precompute this value.

    // x³ - 3x + b.
    ptr<object> x3 = @new<fiat.P521Element>().Square(x1);
    x3.Mul(x3, x1);

    ptr<object> threeX = @new<fiat.P521Element>().Add(x1, x1);
    threeX.Add(threeX, x1);

    x3.Sub(x3, threeX);
    x3.Add(x3, b); 

    // y² = x³ - 3x + b
    ptr<object> y2 = @new<fiat.P521Element>().Square(y1);

    return x3.Equal(y2) == 1;
}

private partial struct p521Point {
    public ptr<fiat.P521Element> x;
    public ptr<fiat.P521Element> y;
    public ptr<fiat.P521Element> z;
}

private static ptr<big.Int> fiatP521ToBigInt(ptr<fiat.P521Element> _addr_x) {
    ref fiat.P521Element x = ref _addr_x.val;

    var xBytes = x.Bytes();
    foreach (var (i) in xBytes[..(int)len(xBytes) / 2]) {
        (xBytes[i], xBytes[len(xBytes) - i - 1]) = (xBytes[len(xBytes) - i - 1], xBytes[i]);
    }    return @new<big.Int>().SetBytes(xBytes);
}

// affineFromJacobian brings a point in Jacobian coordinates back to affine
// coordinates, with (0, 0) representing infinity by convention. It also goes
// back to big.Int values to match the exposed API.
private static (ptr<big.Int>, ptr<big.Int>) affineFromJacobian(this p521Curve curve, ptr<p521Point> _addr_p) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;
    ref p521Point p = ref _addr_p.val;

    if (p.z.IsZero() == 1) {
        return (@new<big.Int>(), @new<big.Int>());
    }
    ptr<fiat.P521Element> zinv = @new<fiat.P521Element>().Invert(p.z);
    ptr<fiat.P521Element> zinvsq = @new<fiat.P521Element>().Mul(zinv, zinv);

    ptr<fiat.P521Element> xx = @new<fiat.P521Element>().Mul(p.x, zinvsq);
    zinvsq.Mul(zinvsq, zinv);
    ptr<fiat.P521Element> yy = @new<fiat.P521Element>().Mul(p.y, zinvsq);

    return (_addr_fiatP521ToBigInt(xx)!, _addr_fiatP521ToBigInt(yy)!);
}

private static ptr<fiat.P521Element> bigIntToFiatP521(ptr<big.Int> _addr_x) => func((_, panic, _) => {
    ref big.Int x = ref _addr_x.val;

    ptr<big.Int> xBytes = @new<big.Int>().Mod(x, p521.P).FillBytes(make_slice<byte>(66));
    foreach (var (i) in xBytes[..(int)len(xBytes) / 2]) {
        (xBytes[i], xBytes[len(xBytes) - i - 1]) = (xBytes[len(xBytes) - i - 1], xBytes[i]);
    }    ptr<fiat.P521Element> (x1, err) = @new<fiat.P521Element>().SetBytes(xBytes);
    if (err != null) { 
        // The input is reduced modulo P and encoded in a fixed size bytes
        // slice, this should be impossible.
        panic("internal error: bigIntToFiatP521");
    }
    return _addr_x1!;
});

// jacobianFromAffine converts (x, y) affine coordinates into (x, y, z) Jacobian
// coordinates. It also converts from big.Int to fiat, which is necessarily a
// messy and variable-time operation, which we can't avoid due to the exposed API.
private static ptr<p521Point> jacobianFromAffine(this p521Curve curve, ptr<big.Int> _addr_x, ptr<big.Int> _addr_y) {
    ref big.Int x = ref _addr_x.val;
    ref big.Int y = ref _addr_y.val;
 
    // (0, 0) is by convention the point at infinity, which can't be represented
    // in affine coordinates, but is (0, 0, 0) in Jacobian.
    if (x.Sign() == 0 && y.Sign() == 0) {
        return addr(new p521Point(x:new(fiat.P521Element),y:new(fiat.P521Element),z:new(fiat.P521Element),));
    }
    return addr(new p521Point(x:bigIntToFiatP521(x),y:bigIntToFiatP521(y),z:new(fiat.P521Element).One(),));
}

private static (ptr<big.Int>, ptr<big.Int>) Add(this p521Curve curve, ptr<big.Int> _addr_x1, ptr<big.Int> _addr_y1, ptr<big.Int> _addr_x2, ptr<big.Int> _addr_y2) {
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;
    ref big.Int x1 = ref _addr_x1.val;
    ref big.Int y1 = ref _addr_y1.val;
    ref big.Int x2 = ref _addr_x2.val;
    ref big.Int y2 = ref _addr_y2.val;

    var p1 = curve.jacobianFromAffine(x1, y1);
    var p2 = curve.jacobianFromAffine(x2, y2);
    return _addr_curve.affineFromJacobian(p1.addJacobian(p1, p2))!;
}

// addJacobian sets q = p1 + p2, and returns q. The points may overlap.
private static ptr<p521Point> addJacobian(this ptr<p521Point> _addr_q, ptr<p521Point> _addr_p1, ptr<p521Point> _addr_p2) {
    ref p521Point q = ref _addr_q.val;
    ref p521Point p1 = ref _addr_p1.val;
    ref p521Point p2 = ref _addr_p2.val;
 
    // https://hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-3.html#addition-add-2007-bl
    var z1IsZero = p1.z.IsZero();
    var z2IsZero = p2.z.IsZero();

    ptr<fiat.P521Element> z1z1 = @new<fiat.P521Element>().Square(p1.z);
    ptr<fiat.P521Element> z2z2 = @new<fiat.P521Element>().Square(p2.z);

    ptr<fiat.P521Element> u1 = @new<fiat.P521Element>().Mul(p1.x, z2z2);
    ptr<fiat.P521Element> u2 = @new<fiat.P521Element>().Mul(p2.x, z1z1);
    ptr<fiat.P521Element> h = @new<fiat.P521Element>().Sub(u2, u1);
    var xEqual = h.IsZero() == 1;
    ptr<fiat.P521Element> i = @new<fiat.P521Element>().Add(h, h);
    i.Square(i);
    ptr<fiat.P521Element> j = @new<fiat.P521Element>().Mul(h, i);

    ptr<fiat.P521Element> s1 = @new<fiat.P521Element>().Mul(p1.y, p2.z);
    s1.Mul(s1, z2z2);
    ptr<fiat.P521Element> s2 = @new<fiat.P521Element>().Mul(p2.y, p1.z);
    s2.Mul(s2, z1z1);
    ptr<fiat.P521Element> r = @new<fiat.P521Element>().Sub(s2, s1);
    var yEqual = r.IsZero() == 1;
    if (xEqual && yEqual && z1IsZero == 0 && z2IsZero == 0) {
        return _addr_q.doubleJacobian(p1)!;
    }
    r.Add(r, r);
    ptr<fiat.P521Element> v = @new<fiat.P521Element>().Mul(u1, i);

    ptr<fiat.P521Element> x = @new<fiat.P521Element>().Set(r);
    x.Square(x);
    x.Sub(x, j);
    x.Sub(x, v);
    x.Sub(x, v);

    ptr<fiat.P521Element> y = @new<fiat.P521Element>().Set(r);
    v.Sub(v, x);
    y.Mul(y, v);
    s1.Mul(s1, j);
    s1.Add(s1, s1);
    y.Sub(y, s1);

    ptr<fiat.P521Element> z = @new<fiat.P521Element>().Add(p1.z, p2.z);
    z.Square(z);
    z.Sub(z, z1z1);
    z.Sub(z, z2z2);
    z.Mul(z, h);

    x.Select(p2.x, x, z1IsZero);
    x.Select(p1.x, x, z2IsZero);
    y.Select(p2.y, y, z1IsZero);
    y.Select(p1.y, y, z2IsZero);
    z.Select(p2.z, z, z1IsZero);
    z.Select(p1.z, z, z2IsZero);

    q.x.Set(x);
    q.y.Set(y);
    q.z.Set(z);
    return _addr_q!;
}

private static (ptr<big.Int>, ptr<big.Int>) Double(this p521Curve curve, ptr<big.Int> _addr_x1, ptr<big.Int> _addr_y1) {
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;
    ref big.Int x1 = ref _addr_x1.val;
    ref big.Int y1 = ref _addr_y1.val;

    var p = curve.jacobianFromAffine(x1, y1);
    return _addr_curve.affineFromJacobian(p.doubleJacobian(p))!;
}

// doubleJacobian sets q = p + p, and returns q. The points may overlap.
private static ptr<p521Point> doubleJacobian(this ptr<p521Point> _addr_q, ptr<p521Point> _addr_p) {
    ref p521Point q = ref _addr_q.val;
    ref p521Point p = ref _addr_p.val;
 
    // https://hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-3.html#doubling-dbl-2001-b
    ptr<fiat.P521Element> delta = @new<fiat.P521Element>().Square(p.z);
    ptr<fiat.P521Element> gamma = @new<fiat.P521Element>().Square(p.y);
    ptr<fiat.P521Element> alpha = @new<fiat.P521Element>().Sub(p.x, delta);
    ptr<fiat.P521Element> alpha2 = @new<fiat.P521Element>().Add(p.x, delta);
    alpha.Mul(alpha, alpha2);
    alpha2.Set(alpha);
    alpha.Add(alpha, alpha);
    alpha.Add(alpha, alpha2);

    var beta = alpha2.Mul(p.x, gamma);

    q.x.Square(alpha);
    ptr<fiat.P521Element> beta8 = @new<fiat.P521Element>().Add(beta, beta);
    beta8.Add(beta8, beta8);
    beta8.Add(beta8, beta8);
    q.x.Sub(q.x, beta8);

    q.z.Add(p.y, p.z);
    q.z.Square(q.z);
    q.z.Sub(q.z, gamma);
    q.z.Sub(q.z, delta);

    beta.Add(beta, beta);
    beta.Add(beta, beta);
    beta.Sub(beta, q.x);
    q.y.Mul(alpha, beta);

    gamma.Square(gamma);
    gamma.Add(gamma, gamma);
    gamma.Add(gamma, gamma);
    gamma.Add(gamma, gamma);

    q.y.Sub(q.y, gamma);

    return _addr_q!;
}

private static (ptr<big.Int>, ptr<big.Int>) ScalarMult(this p521Curve curve, ptr<big.Int> _addr_Bx, ptr<big.Int> _addr_By, slice<byte> scalar) {
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;
    ref big.Int Bx = ref _addr_Bx.val;
    ref big.Int By = ref _addr_By.val;

    var B = curve.jacobianFromAffine(Bx, By);
    ptr<p521Point> p = addr(new p521Point(x:new(fiat.P521Element),y:new(fiat.P521Element),z:new(fiat.P521Element),));
    ptr<p521Point> t = addr(new p521Point(x:new(fiat.P521Element),y:new(fiat.P521Element),z:new(fiat.P521Element),));

    foreach (var (_, byte) in scalar) {
        for (nint bitNum = 0; bitNum < 8; bitNum++) {
            p.doubleJacobian(p);
            var bit = (byte >> (int)((7 - bitNum))) & 1;
            t.addJacobian(p, B);
            p.x.Select(t.x, p.x, int(bit));
            p.y.Select(t.y, p.y, int(bit));
            p.z.Select(t.z, p.z, int(bit));
        }
    }    return _addr_curve.affineFromJacobian(p)!;
}

private static (ptr<big.Int>, ptr<big.Int>) ScalarBaseMult(this p521Curve curve, slice<byte> k) {
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;

    return _addr_curve.ScalarMult(curve.Gx, curve.Gy, k)!;
}

} // end elliptic_package
