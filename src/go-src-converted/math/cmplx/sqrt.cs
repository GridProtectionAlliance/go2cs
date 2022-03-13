// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cmplx -- go2cs converted at 2022 March 13 05:42:08 UTC
// import "math/cmplx" ==> using cmplx = go.math.cmplx_package
// Original source: C:\Program Files\Go\src\math\cmplx\sqrt.go
namespace go.math;

using math = math_package;

public static partial class cmplx_package {

// The original C code, the long comment, and the constants
// below are from http://netlib.sandia.gov/cephes/c9x-complex/clog.c.
// The go code is a simplified version of the original C.
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

// Complex square root
//
// DESCRIPTION:
//
// If z = x + iy,  r = |z|, then
//
//                       1/2
// Re w  =  [ (r + x)/2 ]   ,
//
//                       1/2
// Im w  =  [ (r - x)/2 ]   .
//
// Cancellation error in r-x or r+x is avoided by using the
// identity  2 Re w Im w  =  y.
//
// Note that -w is also a square root of z. The root chosen
// is always in the right half plane and Im w has the same sign as y.
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain     # trials      peak         rms
//    DEC       -10,+10     25000       3.2e-17     9.6e-18
//    IEEE      -10,+10   1,000,000     2.9e-16     6.1e-17

// Sqrt returns the square root of x.
// The result r is chosen so that real(r) ≥ 0 and imag(r) has the same sign as imag(x).
public static System.Numerics.Complex128 Sqrt(System.Numerics.Complex128 x) {
    if (imag(x) == 0) { 
        // Ensure that imag(r) has the same sign as imag(x) for imag(x) == signed zero.
        if (real(x) == 0) {
            return complex(0, imag(x));
        }
        if (real(x) < 0) {
            return complex(0, math.Copysign(math.Sqrt(-real(x)), imag(x)));
        }
        return complex(math.Sqrt(real(x)), imag(x));
    }
    else if (math.IsInf(imag(x), 0)) {
        return complex(math.Inf(1.0F), imag(x));
    }
    if (real(x) == 0) {
        if (imag(x) < 0) {
            var r = math.Sqrt(-0.5F * imag(x));
            return complex(r, -r);
        }
        r = math.Sqrt(0.5F * imag(x));
        return complex(r, r);
    }
    var a = real(x);
    var b = imag(x);
    double scale = default; 
    // Rescale to avoid internal overflow or underflow.
    if (math.Abs(a) > 4 || math.Abs(b) > 4) {
        a *= 0.25F;
        b *= 0.25F;
        scale = 2;
    }
    else
 {
        a *= 1.8014398509481984e16F; // 2**54
        b *= 1.8014398509481984e16F;
        scale = 7.450580596923828125e-9F; // 2**-27
    }
    r = math.Hypot(a, b);
    double t = default;
    if (a > 0) {
        t = math.Sqrt(0.5F * r + 0.5F * a);
        r = scale * math.Abs((0.5F * b) / t);
        t *= scale;
    }
    else
 {
        r = math.Sqrt(0.5F * r - 0.5F * a);
        t = scale * math.Abs((0.5F * b) / r);
        r *= scale;
    }
    if (b < 0) {
        return complex(t, -r);
    }
    return complex(t, r);
}

} // end cmplx_package
