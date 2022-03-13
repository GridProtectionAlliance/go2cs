// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:41:55 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\cbrt.go
namespace go;

public static partial class math_package {

// The go code is a modified version of the original C code from
// http://www.netlib.org/fdlibm/s_cbrt.c and came with this notice.
//
// ====================================================
// Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
//
// Developed at SunSoft, a Sun Microsystems, Inc. business.
// Permission to use, copy, modify, and distribute this
// software is freely granted, provided that this notice
// is preserved.
// ====================================================

// Cbrt returns the cube root of x.
//
// Special cases are:
//    Cbrt(±0) = ±0
//    Cbrt(±Inf) = ±Inf
//    Cbrt(NaN) = NaN
public static double Cbrt(double x) {
    if (haveArchCbrt) {
        return archCbrt(x);
    }
    return cbrt(x);
}

private static double cbrt(double x) {
    const nint B1 = 715094163; // (682-0.03306235651)*2**20
    const nint B2 = 696219795; // (664-0.03306235651)*2**20
    const float C = 5.42857142857142815906e-01F; // 19/35     = 0x3FE15F15F15F15F1
    const float D = -7.05306122448979611050e-01F; // -864/1225 = 0xBFE691DE2532C834
    const float E = 1.41428571428571436819e+00F; // 99/70     = 0x3FF6A0EA0EA0EA0F
    const float F = 1.60714285714285720630e+00F; // 45/28     = 0x3FF9B6DB6DB6DB6E
    const float G = 3.57142857142857150787e-01F; // 5/14      = 0x3FD6DB6DB6DB6DB7
    const float SmallestNormal = 2.22507385850720138309e-308F; // 2**-1022  = 0x0010000000000000 
    // special cases

    if (x == 0 || IsNaN(x) || IsInf(x, 0)) 
        return x;
        var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    var t = Float64frombits(Float64bits(x) / 3 + B1 << 32);
    if (x < SmallestNormal) { 
        // subnormal number
        t = float64(1 << 54); // set t= 2**54
        t *= x;
        t = Float64frombits(Float64bits(t) / 3 + B2 << 32);
    }
    var r = t * t / x;
    var s = C + r * t;
    t *= G + F / (s + E + D / s); 

    // chop to 22 bits, make larger than cbrt(x)
    t = Float64frombits(Float64bits(t) & (0xFFFFFFFFC << 28) + 1 << 30); 

    // one step newton iteration to 53 bits with error less than 0.667ulps
    s = t * t; // t*t is exact
    r = x / s;
    var w = t + t;
    r = (r - t) / (w + r); // r-s is exact
    t = t + t * r; 

    // restore the sign bit
    if (sign) {
        t = -t;
    }
    return t;
}

} // end math_package
