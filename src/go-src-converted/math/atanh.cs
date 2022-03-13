// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:41:54 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\atanh.go
namespace go;

public static partial class math_package {

// The original C code, the long comment, and the constants
// below are from FreeBSD's /usr/src/lib/msun/src/e_atanh.c
// and came with this notice. The go code is a simplified
// version of the original C.
//
// ====================================================
// Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
//
// Developed at SunPro, a Sun Microsystems, Inc. business.
// Permission to use, copy, modify, and distribute this
// software is freely granted, provided that this notice
// is preserved.
// ====================================================
//
//
// __ieee754_atanh(x)
// Method :
//    1. Reduce x to positive by atanh(-x) = -atanh(x)
//    2. For x>=0.5
//                1              2x                          x
//    atanh(x) = --- * log(1 + -------) = 0.5 * log1p(2 * --------)
//                2             1 - x                      1 - x
//
//    For x<0.5
//    atanh(x) = 0.5*log1p(2x+2x*x/(1-x))
//
// Special cases:
//    atanh(x) is NaN if |x| > 1 with signal;
//    atanh(NaN) is that NaN with no signal;
//    atanh(+-1) is +-INF with signal.
//

// Atanh returns the inverse hyperbolic tangent of x.
//
// Special cases are:
//    Atanh(1) = +Inf
//    Atanh(±0) = ±0
//    Atanh(-1) = -Inf
//    Atanh(x) = NaN if x < -1 or x > 1
//    Atanh(NaN) = NaN
public static double Atanh(double x) {
    if (haveArchAtanh) {
        return archAtanh(x);
    }
    return atanh(x);
}

private static double atanh(double x) {
    const float NearZero = 1.0F / (1 << 28); // 2**-28
    // special cases
 // 2**-28
    // special cases

    if (x < -1 || x > 1 || IsNaN(x)) 
        return NaN();
    else if (x == 1) 
        return Inf(1);
    else if (x == -1) 
        return Inf(-1);
        var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    double temp = default;

    if (x < NearZero) 
        temp = x;
    else if (x < 0.5F) 
        temp = x + x;
        temp = 0.5F * Log1p(temp + temp * x / (1 - x));
    else 
        temp = 0.5F * Log1p((x + x) / (1 - x));
        if (sign) {
        temp = -temp;
    }
    return temp;
}

} // end math_package
