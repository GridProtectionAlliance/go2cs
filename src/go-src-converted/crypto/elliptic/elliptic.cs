// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package elliptic implements several standard elliptic curves over prime
// fields.

// package elliptic -- go2cs converted at 2022 March 13 05:30:33 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Program Files\Go\src\crypto\elliptic\elliptic.go
namespace go.crypto;
// This package operates, internally, on Jacobian coordinates. For a given
// (x, y) position on the curve, the Jacobian coordinates are (x1, y1, z1)
// where x = x1/z1² and y = y1/z1³. The greatest speedups come when the whole
// calculation can be performed within the transform (as in ScalarMult and
// ScalarBaseMult). But even for Add and Double, it's faster to apply and
// reverse the transform than to operate in affine coordinates.


using io = io_package;
using big = math.big_package;
using sync = sync_package;


// A Curve represents a short-form Weierstrass curve with a=-3.
//
// Note that the point at infinity (0, 0) is not considered on the curve, and
// although it can be returned by Add, Double, ScalarMult, or ScalarBaseMult, it
// can't be marshaled or unmarshaled, and IsOnCurve will return false for it.

public static partial class elliptic_package {

public partial interface Curve {
    (ptr<big.Int>, ptr<big.Int>) Params(); // IsOnCurve reports whether the given (x,y) lies on the curve.
    (ptr<big.Int>, ptr<big.Int>) IsOnCurve(ptr<big.Int> x, ptr<big.Int> y); // Add returns the sum of (x1,y1) and (x2,y2)
    (ptr<big.Int>, ptr<big.Int>) Add(ptr<big.Int> x1, ptr<big.Int> y1, ptr<big.Int> x2, ptr<big.Int> y2); // Double returns 2*(x,y)
    (ptr<big.Int>, ptr<big.Int>) Double(ptr<big.Int> x1, ptr<big.Int> y1); // ScalarMult returns k*(Bx,By) where k is a number in big-endian form.
    (ptr<big.Int>, ptr<big.Int>) ScalarMult(ptr<big.Int> x1, ptr<big.Int> y1, slice<byte> k); // ScalarBaseMult returns k*G, where G is the base point of the group
// and k is an integer in big-endian form.
    (ptr<big.Int>, ptr<big.Int>) ScalarBaseMult(slice<byte> k);
}

private static (Curve, bool) matchesSpecificCurve(ptr<CurveParams> _addr_@params, params Curve[] available) {
    Curve _p0 = default;
    bool _p0 = default;
    available = available.Clone();
    ref CurveParams @params = ref _addr_@params.val;

    foreach (var (_, c) in available) {
        if (params == c.Params()) {
            return (c, true);
        }
    }    return (null, false);
}

// CurveParams contains the parameters of an elliptic curve and also provides
// a generic, non-constant time implementation of Curve.
public partial struct CurveParams {
    public ptr<big.Int> P; // the order of the underlying field
    public ptr<big.Int> N; // the order of the base point
    public ptr<big.Int> B; // the constant of the curve equation
    public ptr<big.Int> Gx; // (x,y) of the base point
    public ptr<big.Int> Gy; // (x,y) of the base point
    public nint BitSize; // the size of the underlying field
    public @string Name; // the canonical name of the curve
}

private static ptr<CurveParams> Params(this ptr<CurveParams> _addr_curve) {
    ref CurveParams curve = ref _addr_curve.val;

    return _addr_curve!;
}

// polynomial returns x³ - 3x + b.
private static ptr<big.Int> polynomial(this ptr<CurveParams> _addr_curve, ptr<big.Int> _addr_x) {
    ref CurveParams curve = ref _addr_curve.val;
    ref big.Int x = ref _addr_x.val;

    ptr<big.Int> x3 = @new<big.Int>().Mul(x, x);
    x3.Mul(x3, x);

    ptr<big.Int> threeX = @new<big.Int>().Lsh(x, 1);
    threeX.Add(threeX, x);

    x3.Sub(x3, threeX);
    x3.Add(x3, curve.B);
    x3.Mod(x3, curve.P);

    return _addr_x3!;
}

private static bool IsOnCurve(this ptr<CurveParams> _addr_curve, ptr<big.Int> _addr_x, ptr<big.Int> _addr_y) {
    ref CurveParams curve = ref _addr_curve.val;
    ref big.Int x = ref _addr_x.val;
    ref big.Int y = ref _addr_y.val;
 
    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    {
        var (specific, ok) = matchesSpecificCurve(_addr_curve, p224, p521);

        if (ok) {
            return specific.IsOnCurve(x, y);
        }
    } 

    // y² = x³ - 3x + b
    ptr<big.Int> y2 = @new<big.Int>().Mul(y, y);
    y2.Mod(y2, curve.P);

    return curve.polynomial(x).Cmp(y2) == 0;
}

// zForAffine returns a Jacobian Z value for the affine point (x, y). If x and
// y are zero, it assumes that they represent the point at infinity because (0,
// 0) is not on the any of the curves handled here.
private static ptr<big.Int> zForAffine(ptr<big.Int> _addr_x, ptr<big.Int> _addr_y) {
    ref big.Int x = ref _addr_x.val;
    ref big.Int y = ref _addr_y.val;

    ptr<big.Int> z = @new<big.Int>();
    if (x.Sign() != 0 || y.Sign() != 0) {
        z.SetInt64(1);
    }
    return _addr_z!;
}

// affineFromJacobian reverses the Jacobian transform. See the comment at the
// top of the file. If the point is ∞ it returns 0, 0.
private static (ptr<big.Int>, ptr<big.Int>) affineFromJacobian(this ptr<CurveParams> _addr_curve, ptr<big.Int> _addr_x, ptr<big.Int> _addr_y, ptr<big.Int> _addr_z) {
    ptr<big.Int> xOut = default!;
    ptr<big.Int> yOut = default!;
    ref CurveParams curve = ref _addr_curve.val;
    ref big.Int x = ref _addr_x.val;
    ref big.Int y = ref _addr_y.val;
    ref big.Int z = ref _addr_z.val;

    if (z.Sign() == 0) {
        return (@new<big.Int>(), @new<big.Int>());
    }
    ptr<big.Int> zinv = @new<big.Int>().ModInverse(z, curve.P);
    ptr<big.Int> zinvsq = @new<big.Int>().Mul(zinv, zinv);

    xOut = @new<big.Int>().Mul(x, zinvsq);
    xOut.Mod(xOut, curve.P);
    zinvsq.Mul(zinvsq, zinv);
    yOut = @new<big.Int>().Mul(y, zinvsq);
    yOut.Mod(yOut, curve.P);
    return ;
}

private static (ptr<big.Int>, ptr<big.Int>) Add(this ptr<CurveParams> _addr_curve, ptr<big.Int> _addr_x1, ptr<big.Int> _addr_y1, ptr<big.Int> _addr_x2, ptr<big.Int> _addr_y2) {
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;
    ref CurveParams curve = ref _addr_curve.val;
    ref big.Int x1 = ref _addr_x1.val;
    ref big.Int y1 = ref _addr_y1.val;
    ref big.Int x2 = ref _addr_x2.val;
    ref big.Int y2 = ref _addr_y2.val;
 
    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    {
        var (specific, ok) = matchesSpecificCurve(_addr_curve, p224, p521);

        if (ok) {
            return _addr_specific.Add(x1, y1, x2, y2)!;
        }
    }

    var z1 = zForAffine(_addr_x1, _addr_y1);
    var z2 = zForAffine(_addr_x2, _addr_y2);
    return _addr_curve.affineFromJacobian(curve.addJacobian(x1, y1, z1, x2, y2, z2))!;
}

// addJacobian takes two points in Jacobian coordinates, (x1, y1, z1) and
// (x2, y2, z2) and returns their sum, also in Jacobian form.
private static (ptr<big.Int>, ptr<big.Int>, ptr<big.Int>) addJacobian(this ptr<CurveParams> _addr_curve, ptr<big.Int> _addr_x1, ptr<big.Int> _addr_y1, ptr<big.Int> _addr_z1, ptr<big.Int> _addr_x2, ptr<big.Int> _addr_y2, ptr<big.Int> _addr_z2) {
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;
    ref CurveParams curve = ref _addr_curve.val;
    ref big.Int x1 = ref _addr_x1.val;
    ref big.Int y1 = ref _addr_y1.val;
    ref big.Int z1 = ref _addr_z1.val;
    ref big.Int x2 = ref _addr_x2.val;
    ref big.Int y2 = ref _addr_y2.val;
    ref big.Int z2 = ref _addr_z2.val;
 
    // See https://hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-3.html#addition-add-2007-bl
    ptr<big.Int> x3 = @new<big.Int>();
    ptr<big.Int> y3 = @new<big.Int>();
    ptr<big.Int> z3 = @new<big.Int>();
    if (z1.Sign() == 0) {
        x3.Set(x2);
        y3.Set(y2);
        z3.Set(z2);
        return (_addr_x3!, _addr_y3!, _addr_z3!);
    }
    if (z2.Sign() == 0) {
        x3.Set(x1);
        y3.Set(y1);
        z3.Set(z1);
        return (_addr_x3!, _addr_y3!, _addr_z3!);
    }
    ptr<big.Int> z1z1 = @new<big.Int>().Mul(z1, z1);
    z1z1.Mod(z1z1, curve.P);
    ptr<big.Int> z2z2 = @new<big.Int>().Mul(z2, z2);
    z2z2.Mod(z2z2, curve.P);

    ptr<big.Int> u1 = @new<big.Int>().Mul(x1, z2z2);
    u1.Mod(u1, curve.P);
    ptr<big.Int> u2 = @new<big.Int>().Mul(x2, z1z1);
    u2.Mod(u2, curve.P);
    ptr<big.Int> h = @new<big.Int>().Sub(u2, u1);
    var xEqual = h.Sign() == 0;
    if (h.Sign() == -1) {
        h.Add(h, curve.P);
    }
    ptr<big.Int> i = @new<big.Int>().Lsh(h, 1);
    i.Mul(i, i);
    ptr<big.Int> j = @new<big.Int>().Mul(h, i);

    ptr<big.Int> s1 = @new<big.Int>().Mul(y1, z2);
    s1.Mul(s1, z2z2);
    s1.Mod(s1, curve.P);
    ptr<big.Int> s2 = @new<big.Int>().Mul(y2, z1);
    s2.Mul(s2, z1z1);
    s2.Mod(s2, curve.P);
    ptr<big.Int> r = @new<big.Int>().Sub(s2, s1);
    if (r.Sign() == -1) {
        r.Add(r, curve.P);
    }
    var yEqual = r.Sign() == 0;
    if (xEqual && yEqual) {
        return _addr_curve.doubleJacobian(x1, y1, z1)!;
    }
    r.Lsh(r, 1);
    ptr<big.Int> v = @new<big.Int>().Mul(u1, i);

    x3.Set(r);
    x3.Mul(x3, x3);
    x3.Sub(x3, j);
    x3.Sub(x3, v);
    x3.Sub(x3, v);
    x3.Mod(x3, curve.P);

    y3.Set(r);
    v.Sub(v, x3);
    y3.Mul(y3, v);
    s1.Mul(s1, j);
    s1.Lsh(s1, 1);
    y3.Sub(y3, s1);
    y3.Mod(y3, curve.P);

    z3.Add(z1, z2);
    z3.Mul(z3, z3);
    z3.Sub(z3, z1z1);
    z3.Sub(z3, z2z2);
    z3.Mul(z3, h);
    z3.Mod(z3, curve.P);

    return (_addr_x3!, _addr_y3!, _addr_z3!);
}

private static (ptr<big.Int>, ptr<big.Int>) Double(this ptr<CurveParams> _addr_curve, ptr<big.Int> _addr_x1, ptr<big.Int> _addr_y1) {
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;
    ref CurveParams curve = ref _addr_curve.val;
    ref big.Int x1 = ref _addr_x1.val;
    ref big.Int y1 = ref _addr_y1.val;
 
    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    {
        var (specific, ok) = matchesSpecificCurve(_addr_curve, p224, p521);

        if (ok) {
            return _addr_specific.Double(x1, y1)!;
        }
    }

    var z1 = zForAffine(_addr_x1, _addr_y1);
    return _addr_curve.affineFromJacobian(curve.doubleJacobian(x1, y1, z1))!;
}

// doubleJacobian takes a point in Jacobian coordinates, (x, y, z), and
// returns its double, also in Jacobian form.
private static (ptr<big.Int>, ptr<big.Int>, ptr<big.Int>) doubleJacobian(this ptr<CurveParams> _addr_curve, ptr<big.Int> _addr_x, ptr<big.Int> _addr_y, ptr<big.Int> _addr_z) {
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;
    ref CurveParams curve = ref _addr_curve.val;
    ref big.Int x = ref _addr_x.val;
    ref big.Int y = ref _addr_y.val;
    ref big.Int z = ref _addr_z.val;
 
    // See https://hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-3.html#doubling-dbl-2001-b
    ptr<big.Int> delta = @new<big.Int>().Mul(z, z);
    delta.Mod(delta, curve.P);
    ptr<big.Int> gamma = @new<big.Int>().Mul(y, y);
    gamma.Mod(gamma, curve.P);
    ptr<big.Int> alpha = @new<big.Int>().Sub(x, delta);
    if (alpha.Sign() == -1) {
        alpha.Add(alpha, curve.P);
    }
    ptr<big.Int> alpha2 = @new<big.Int>().Add(x, delta);
    alpha.Mul(alpha, alpha2);
    alpha2.Set(alpha);
    alpha.Lsh(alpha, 1);
    alpha.Add(alpha, alpha2);

    var beta = alpha2.Mul(x, gamma);

    ptr<big.Int> x3 = @new<big.Int>().Mul(alpha, alpha);
    ptr<big.Int> beta8 = @new<big.Int>().Lsh(beta, 3);
    beta8.Mod(beta8, curve.P);
    x3.Sub(x3, beta8);
    if (x3.Sign() == -1) {
        x3.Add(x3, curve.P);
    }
    x3.Mod(x3, curve.P);

    ptr<big.Int> z3 = @new<big.Int>().Add(y, z);
    z3.Mul(z3, z3);
    z3.Sub(z3, gamma);
    if (z3.Sign() == -1) {
        z3.Add(z3, curve.P);
    }
    z3.Sub(z3, delta);
    if (z3.Sign() == -1) {
        z3.Add(z3, curve.P);
    }
    z3.Mod(z3, curve.P);

    beta.Lsh(beta, 2);
    beta.Sub(beta, x3);
    if (beta.Sign() == -1) {
        beta.Add(beta, curve.P);
    }
    var y3 = alpha.Mul(alpha, beta);

    gamma.Mul(gamma, gamma);
    gamma.Lsh(gamma, 3);
    gamma.Mod(gamma, curve.P);

    y3.Sub(y3, gamma);
    if (y3.Sign() == -1) {
        y3.Add(y3, curve.P);
    }
    y3.Mod(y3, curve.P);

    return (_addr_x3!, _addr_y3!, _addr_z3!);
}

private static (ptr<big.Int>, ptr<big.Int>) ScalarMult(this ptr<CurveParams> _addr_curve, ptr<big.Int> _addr_Bx, ptr<big.Int> _addr_By, slice<byte> k) {
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;
    ref CurveParams curve = ref _addr_curve.val;
    ref big.Int Bx = ref _addr_Bx.val;
    ref big.Int By = ref _addr_By.val;
 
    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    {
        var (specific, ok) = matchesSpecificCurve(_addr_curve, p224, p256, p521);

        if (ok) {
            return _addr_specific.ScalarMult(Bx, By, k)!;
        }
    }

    ptr<big.Int> Bz = @new<big.Int>().SetInt64(1);
    ptr<big.Int> x = @new<big.Int>();
    ptr<big.Int> y = @new<big.Int>();
    ptr<big.Int> z = @new<big.Int>();

    foreach (var (_, byte) in k) {
        for (nint bitNum = 0; bitNum < 8; bitNum++) {
            x, y, z = curve.doubleJacobian(x, y, z);
            if (byte & 0x80 == 0x80) {
                x, y, z = curve.addJacobian(Bx, By, Bz, x, y, z);
            }
            byte<<=1;
        }
    }    return _addr_curve.affineFromJacobian(x, y, z)!;
}

private static (ptr<big.Int>, ptr<big.Int>) ScalarBaseMult(this ptr<CurveParams> _addr_curve, slice<byte> k) {
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;
    ref CurveParams curve = ref _addr_curve.val;
 
    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    {
        var (specific, ok) = matchesSpecificCurve(_addr_curve, p224, p256, p521);

        if (ok) {
            return _addr_specific.ScalarBaseMult(k)!;
        }
    }

    return _addr_curve.ScalarMult(curve.Gx, curve.Gy, k)!;
}

private static byte mask = new slice<byte>(new byte[] { 0xff, 0x1, 0x3, 0x7, 0xf, 0x1f, 0x3f, 0x7f });

// GenerateKey returns a public/private key pair. The private key is
// generated using the given reader, which must return random data.
public static (slice<byte>, ptr<big.Int>, ptr<big.Int>, error) GenerateKey(Curve curve, io.Reader rand) {
    slice<byte> priv = default;
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;
    error err = default!;

    var N = curve.Params().N;
    var bitSize = N.BitLen();
    var byteLen = (bitSize + 7) / 8;
    priv = make_slice<byte>(byteLen);

    while (x == null) {
        _, err = io.ReadFull(rand, priv);
        if (err != null) {
            return ;
        }
        priv[0] &= mask[bitSize % 8]; 
        // This is because, in tests, rand will return all zeros and we don't
        // want to get the point at infinity and loop forever.
        priv[1] ^= 0x42; 

        // If the scalar is out of range, sample another random number.
        if (@new<big.Int>().SetBytes(priv).Cmp(N) >= 0) {
            continue;
        }
        x, y = curve.ScalarBaseMult(priv);
    }
    return ;
}

// Marshal converts a point on the curve into the uncompressed form specified in
// section 4.3.6 of ANSI X9.62.
public static slice<byte> Marshal(Curve curve, ptr<big.Int> _addr_x, ptr<big.Int> _addr_y) {
    ref big.Int x = ref _addr_x.val;
    ref big.Int y = ref _addr_y.val;

    var byteLen = (curve.Params().BitSize + 7) / 8;

    var ret = make_slice<byte>(1 + 2 * byteLen);
    ret[0] = 4; // uncompressed point

    x.FillBytes(ret[(int)1..(int)1 + byteLen]);
    y.FillBytes(ret[(int)1 + byteLen..(int)1 + 2 * byteLen]);

    return ret;
}

// MarshalCompressed converts a point on the curve into the compressed form
// specified in section 4.3.6 of ANSI X9.62.
public static slice<byte> MarshalCompressed(Curve curve, ptr<big.Int> _addr_x, ptr<big.Int> _addr_y) {
    ref big.Int x = ref _addr_x.val;
    ref big.Int y = ref _addr_y.val;

    var byteLen = (curve.Params().BitSize + 7) / 8;
    var compressed = make_slice<byte>(1 + byteLen);
    compressed[0] = byte(y.Bit(0)) | 2;
    x.FillBytes(compressed[(int)1..]);
    return compressed;
}

// Unmarshal converts a point, serialized by Marshal, into an x, y pair.
// It is an error if the point is not in uncompressed form or is not on the curve.
// On error, x = nil.
public static (ptr<big.Int>, ptr<big.Int>) Unmarshal(Curve curve, slice<byte> data) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;

    var byteLen = (curve.Params().BitSize + 7) / 8;
    if (len(data) != 1 + 2 * byteLen) {
        return (_addr_null!, _addr_null!);
    }
    if (data[0] != 4) { // uncompressed form
        return (_addr_null!, _addr_null!);
    }
    var p = curve.Params().P;
    x = @new<big.Int>().SetBytes(data[(int)1..(int)1 + byteLen]);
    y = @new<big.Int>().SetBytes(data[(int)1 + byteLen..]);
    if (x.Cmp(p) >= 0 || y.Cmp(p) >= 0) {
        return (_addr_null!, _addr_null!);
    }
    if (!curve.IsOnCurve(x, y)) {
        return (_addr_null!, _addr_null!);
    }
    return ;
}

// UnmarshalCompressed converts a point, serialized by MarshalCompressed, into an x, y pair.
// It is an error if the point is not in compressed form or is not on the curve.
// On error, x = nil.
public static (ptr<big.Int>, ptr<big.Int>) UnmarshalCompressed(Curve curve, slice<byte> data) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;

    var byteLen = (curve.Params().BitSize + 7) / 8;
    if (len(data) != 1 + byteLen) {
        return (_addr_null!, _addr_null!);
    }
    if (data[0] != 2 && data[0] != 3) { // compressed form
        return (_addr_null!, _addr_null!);
    }
    var p = curve.Params().P;
    x = @new<big.Int>().SetBytes(data[(int)1..]);
    if (x.Cmp(p) >= 0) {
        return (_addr_null!, _addr_null!);
    }
    y = curve.Params().polynomial(x);
    y = y.ModSqrt(y, p);
    if (y == null) {
        return (_addr_null!, _addr_null!);
    }
    if (byte(y.Bit(0)) != data[0] & 1) {
        y.Neg(y).Mod(y, p);
    }
    if (!curve.IsOnCurve(x, y)) {
        return (_addr_null!, _addr_null!);
    }
    return ;
}

private static sync.Once initonce = default;
private static ptr<CurveParams> p384;

private static void initAll() {
    initP224();
    initP256();
    initP384();
    initP521();
}

private static void initP384() { 
    // See FIPS 186-3, section D.2.4
    p384 = addr(new CurveParams(Name:"P-384"));
    p384.P, _ = @new<big.Int>().SetString("39402006196394479212279040100143613805079739270465446667948293404245721771496870329047266088258938001861606973112319", 10);
    p384.N, _ = @new<big.Int>().SetString("39402006196394479212279040100143613805079739270465446667946905279627659399113263569398956308152294913554433653942643", 10);
    p384.B, _ = @new<big.Int>().SetString("b3312fa7e23ee7e4988e056be3f82d19181d9c6efe8141120314088f5013875ac656398d8a2ed19d2a85c8edd3ec2aef", 16);
    p384.Gx, _ = @new<big.Int>().SetString("aa87ca22be8b05378eb1c71ef320ad746e1d3b628ba79b9859f741e082542a385502f25dbf55296c3a545e3872760ab7", 16);
    p384.Gy, _ = @new<big.Int>().SetString("3617de4a96262c6f5d9e98bf9292dc29f8f41dbd289a147ce9da3113b5f0b8c00a60b1ce1d7e819d7a431d7c90ea0e5f", 16);
    p384.BitSize = 384;
}

// P256 returns a Curve which implements NIST P-256 (FIPS 186-3, section D.2.3),
// also known as secp256r1 or prime256v1. The CurveParams.Name of this Curve is
// "P-256".
//
// Multiple invocations of this function will return the same value, so it can
// be used for equality checks and switch statements.
//
// ScalarMult and ScalarBaseMult are implemented using constant-time algorithms.
public static Curve P256() {
    initonce.Do(initAll);
    return p256;
}

// P384 returns a Curve which implements NIST P-384 (FIPS 186-3, section D.2.4),
// also known as secp384r1. The CurveParams.Name of this Curve is "P-384".
//
// Multiple invocations of this function will return the same value, so it can
// be used for equality checks and switch statements.
//
// The cryptographic operations do not use constant-time algorithms.
public static Curve P384() {
    initonce.Do(initAll);
    return p384;
}

// P521 returns a Curve which implements NIST P-521 (FIPS 186-3, section D.2.5),
// also known as secp521r1. The CurveParams.Name of this Curve is "P-521".
//
// Multiple invocations of this function will return the same value, so it can
// be used for equality checks and switch statements.
//
// The cryptographic operations are implemented using constant-time algorithms.
public static Curve P521() {
    initonce.Do(initAll);
    return p521;
}

} // end elliptic_package
