// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

/*
	Floating-point arctangent.
*/
// The original C code, the long comment, and the constants below were
// from http://netlib.sandia.gov/cephes/cmath/atan.c, available from
// http://www.netlib.org/cephes/cmath.tgz.
// The go code is a version of the original C.
//
// atan.c
// Inverse circular tangent (arctangent)
//
// SYNOPSIS:
// double x, y, atan();
// y = atan( x );
//
// DESCRIPTION:
// Returns radian angle between -pi/2 and +pi/2 whose tangent is x.
//
// Range reduction is from three intervals into the interval from zero to 0.66.
// The approximant uses a rational function of degree 4/5 of the form
// x + x**3 P(x)/Q(x).
//
// ACCURACY:
//                      Relative error:
// arithmetic   domain    # trials  peak     rms
//    DEC       -10, 10   50000     2.4e-17  8.3e-18
//    IEEE      -10, 10   10^6      1.8e-16  5.0e-17
//
// Cephes Math Library Release 2.8:  June, 2000
// Copyright 1984, 1987, 1989, 1992, 2000 by Stephen L. Moshier
//
// The readme file at http://netlib.sandia.gov/cephes/ says:
//    Some software in this archive may be from the book _Methods and
// Programs for Mathematical Functions_ (Prentice-Hall or Simon & Schuster
// International, 1989) or from the Cephes Mathematical Library, a
// commercial product. In either event, it is copyrighted by the author.
// What you see here may be used freely but it comes with no support or
// guarantee.
//
//   The two known misprints in the book are repaired here in the
// source listings for the gamma function and the incomplete beta
// integral.
//
//   Stephen L. Moshier
//   moshier@na-net.ornl.gov

// xatan evaluates a series valid in the range [0, 0.66].
internal static float64 xatan(float64 x) {
    static readonly UntypedFloat P0 = /* -8.750608600031904122785e-01 */ -0.875061;
    static readonly UntypedFloat P1 = /* -1.615753718733365076637e+01 */ -16.1575;
    static readonly UntypedFloat P2 = /* -7.500855792314704667340e+01 */ -75.0086;
    static readonly UntypedFloat P3 = /* -1.228866684490136173410e+02 */ -122.887;
    static readonly UntypedFloat P4 = /* -6.485021904942025371773e+01 */ -64.8502;
    static readonly UntypedFloat Q0 = /* +2.485846490142306297962e+01 */ 24.8585;
    static readonly UntypedFloat Q1 = /* +1.650270098316988542046e+02 */ 165.027;
    static readonly UntypedFloat Q2 = /* +4.328810604912902668951e+02 */ 432.881;
    static readonly UntypedFloat Q3 = /* +4.853903996359136964868e+02 */ 485.39;
    static readonly UntypedFloat Q4 = /* +1.945506571482613964425e+02 */ 194.551;
    var z = x * x;
    z = z * ((((P0 * z + P1) * z + P2) * z + P3) * z + P4) / (((((z + Q0) * z + Q1) * z + Q2) * z + Q3) * z + Q4);
    z = x * z + x;
    return z;
}

// satan reduces its argument (known to be positive)
// to the range [0, 0.66] and calls xatan.
internal static float64 satan(float64 x) {
    static readonly UntypedFloat Morebits = /* 6.123233995736765886130e-17 */ 6.12323e-17;  // pi/2 = PIO2 + Morebits
    static readonly UntypedFloat Tan3pio8 = /* 2.41421356237309504880 */ 2.41421;      // tan(3*pi/8)
    if (x <= 0.66F) {
        return xatan(x);
    }
    if (x > Tan3pio8) {
        return Pi / 2 - xatan(1 / x) + Morebits;
    }
    return Pi / 4 + xatan((x - 1) / (x + 1)) + 0.5F * Morebits;
}

// Atan returns the arctangent, in radians, of x.
//
// Special cases are:
//
//	Atan(±0) = ±0
//	Atan(±Inf) = ±Pi/2
public static float64 Atan(float64 x) {
    if (haveArchAtan) {
        return archAtan(x);
    }
    return atan(x);
}

internal static float64 atan(float64 x) {
    if (x == 0) {
        return x;
    }
    if (x > 0) {
        return satan(x);
    }
    return -satan(-x);
}

} // end math_package
