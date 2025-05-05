// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// The original C code, the long comment, and the constants
// below were from http://netlib.sandia.gov/cephes/cmath/sin.c,
// available from http://www.netlib.org/cephes/cmath.tgz.
// The go code is a simplified version of the original C.
//      tanh.c
//
//      Hyperbolic tangent
//
// SYNOPSIS:
//
// double x, y, tanh();
//
// y = tanh( x );
//
// DESCRIPTION:
//
// Returns hyperbolic tangent of argument in the range MINLOG to MAXLOG.
//      MAXLOG = 8.8029691931113054295988e+01 = log(2**127)
//      MINLOG = -8.872283911167299960540e+01 = log(2**-128)
//
// A rational function is used for |x| < 0.625.  The form
// x + x**3 P(x)/Q(x) of Cody & Waite is employed.
// Otherwise,
//      tanh(x) = sinh(x)/cosh(x) = 1  -  2/(exp(2x) + 1).
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain     # trials      peak         rms
//    IEEE      -2,2        30000       2.5e-16     5.8e-17
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
//
internal static array<float64> tanhP = new float64[]{
    -9.64399179425052238628e-1F,
    -9.92877231001918586564e1F,
    -1.61468768441708447952e3F
}.array();

internal static array<float64> tanhQ = new float64[]{
    1.12811678491632931402e2F,
    2.23548839060100448583e3F,
    4.84406305325125486048e3F
}.array();

// Tanh returns the hyperbolic tangent of x.
//
// Special cases are:
//
//	Tanh(±0) = ±0
//	Tanh(±Inf) = ±1
//	Tanh(NaN) = NaN
public static float64 Tanh(float64 x) {
    if (haveArchTanh) {
        return archTanh(x);
    }
    return tanh(x);
}

internal static float64 tanh(float64 x) {
    static readonly UntypedFloat MAXLOG = /* 8.8029691931113054295988e+01 */ 88.0297;       // log(2**127)
    var z = Abs(x);
    switch (ᐧ) {
    case {} when z is > 0.5F * MAXLOG: {
        if (x < 0) {
            return -1;
        }
        return 1;
    }
    case {} when z is >= 0.625F: {
        var s = Exp(2 * z);
        z = 1 - 2 / (s + 1);
        if (x < 0) {
            z = -z;
        }
        break;
    }
    default: {
        if (x == 0) {
            return x;
        }
        var s = x * x;
        z = x + x * s * ((tanhP[0] * s + tanhP[1]) * s + tanhP[2]) / (((s + tanhQ[0]) * s + tanhQ[1]) * s + tanhQ[2]);
        break;
    }}

    return z;
}

} // end math_package
