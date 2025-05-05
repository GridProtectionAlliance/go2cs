// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

/*
	Floating-point tangent.
*/
// The original C code, the long comment, and the constants
// below were from http://netlib.sandia.gov/cephes/cmath/sin.c,
// available from http://www.netlib.org/cephes/cmath.tgz.
// The go code is a simplified version of the original C.
//
//      tan.c
//
//      Circular tangent
//
// SYNOPSIS:
//
// double x, y, tan();
// y = tan( x );
//
// DESCRIPTION:
//
// Returns the circular tangent of the radian argument x.
//
// Range reduction is modulo pi/4.  A rational function
//       x + x**3 P(x**2)/Q(x**2)
// is employed in the basic interval [0, pi/4].
//
// ACCURACY:
//                      Relative error:
// arithmetic   domain     # trials      peak         rms
//    DEC      +-1.07e9      44000      4.1e-17     1.0e-17
//    IEEE     +-1.07e9      30000      2.9e-16     8.1e-17
//
// Partial loss of accuracy begins to occur at x = 2**30 = 1.074e9.  The loss
// is not gradual, but jumps suddenly to about 1 part in 10e7.  Results may
// be meaningless for x > 2**49 = 5.6e14.
// [Accuracy loss statement from sin.go comments.]
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
// 0xc0c992d8d24f3f38
// 0x413199eca5fc9ddd
// 0xc1711fead3299176
// tan coefficients
internal static array<float64> _tanP = new float64[]{
    -1.30936939181383777646e4F,
    1.15351664838587416140e6F,
    -1.79565251976484877988e7F
}.array();

// 0x40cab8a5eeb36572
// 0xc13427bc582abc96
// 0x4177d98fc2ead8ef
// 0xc189afe03cbe5a31
internal static array<float64> _tanQ = new float64[]{
    1.00000000000000000000e0F,
    1.36812963470692954678e4F,
    -1.32089234440210967447e6F,
    2.50083801823357915839e7F,
    -5.38695755929454629881e7F
}.array();

// Tan returns the tangent of the radian argument x.
//
// Special cases are:
//
//	Tan(±0) = ±0
//	Tan(±Inf) = NaN
//	Tan(NaN) = NaN
public static float64 Tan(float64 x) {
    if (haveArchTan) {
        return archTan(x);
    }
    return tan(x);
}

internal static float64 tan(float64 x) {
    static readonly UntypedFloat PI4A = /* 7.85398125648498535156e-1 */ 0.785398;    // 0x3fe921fb40000000, Pi/4 split into three parts
    static readonly UntypedFloat PI4B = /* 3.77489470793079817668e-8 */ 3.77489e-08; // 0x3e64442d00000000,
    static readonly UntypedFloat PI4C = /* 2.69515142907905952645e-15 */ 2.69515e-15; // 0x3ce8469898cc5170,
    // special cases
    switch (ᐧ) {
    case {} when x == 0 || IsNaN(x): {
        return x;
    }
    case {} when IsInf(x, // return ±0 || NaN()
 0): {
        return NaN();
    }}

    // make argument positive but save the sign
    var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    uint64 j = default!;
    float64 y = default!;
    float64 z = default!;
    if (x >= reduceThreshold){
        (j, z) = trigReduce(x);
    } else {
        j = ((uint64)(x * (4 / Pi)));
        // integer part of x/(Pi/4), as integer for tests on the phase angle
        y = ((float64)j);
        // integer part of x/(Pi/4), as float
        /* map zeros and singularities to origin */
        if ((uint64)(j & 1) == 1) {
            j++;
            y++;
        }
        z = ((x - y * PI4A) - y * PI4B) - y * PI4C;
    }
    var zz = z * z;
    if (zz > 1e-14F){
        y = z + z * (zz * (((_tanP[0] * zz) + _tanP[1]) * zz + _tanP[2]) / ((((zz + _tanQ[1]) * zz + _tanQ[2]) * zz + _tanQ[3]) * zz + _tanQ[4]));
    } else {
        y = z;
    }
    if ((uint64)(j & 2) == 2) {
        y = -1 / y;
    }
    if (sign) {
        y = -y;
    }
    return y;
}

} // end math_package
