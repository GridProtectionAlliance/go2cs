// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

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
//
//	Cbrt(±0) = ±0
//	Cbrt(±Inf) = ±Inf
//	Cbrt(NaN) = NaN
public static float64 Cbrt(float64 x) {
    if (haveArchCbrt) {
        return archCbrt(x);
    }
    return cbrt(x);
}

internal static float64 cbrt(float64 x) {
    static readonly UntypedInt B1 = 715094163;      // (682-0.03306235651)*2**20
    static readonly UntypedInt B2 = 696219795;      // (664-0.03306235651)*2**20
    static readonly UntypedFloat C = /* 5.42857142857142815906e-01 */ 0.542857;     // 19/35     = 0x3FE15F15F15F15F1
    static readonly UntypedFloat D = /* -7.05306122448979611050e-01 */ -0.705306; // -864/1225 = 0xBFE691DE2532C834
    static readonly UntypedFloat E = /* 1.41428571428571436819e+00 */ 1.41429;      // 99/70     = 0x3FF6A0EA0EA0EA0F
    static readonly UntypedFloat F = /* 1.60714285714285720630e+00 */ 1.60714;      // 45/28     = 0x3FF9B6DB6DB6DB6E
    static readonly UntypedFloat G = /* 3.57142857142857150787e-01 */ 0.357143;     // 5/14      = 0x3FD6DB6DB6DB6DB7
    static readonly UntypedFloat SmallestNormal = /* 2.22507385850720138309e-308 */ 2.22507e-308; // 2**-1022  = 0x0010000000000000
    // special cases
    switch (ᐧ) {
    case {} when x == 0 || IsNaN(x) || IsInf(x, 0): {
        return x;
    }}

    var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    // rough cbrt to 5 bits
    var t = Float64frombits(Float64bits(x) / 3 + B1 << (int)(32));
    if (x < SmallestNormal) {
        // subnormal number
        t = ((float64)(1 << (int)(54)));
        // set t= 2**54
        t *= x;
        t = Float64frombits(Float64bits(t) / 3 + B2 << (int)(32));
    }
    // new cbrt to 23 bits
    var r = t * t / x;
    var s = C + r * t;
    t *= G + F / (s + E + D / s);
    // chop to 22 bits, make larger than cbrt(x)
    t = Float64frombits((uint64)(Float64bits(t) & ((nint)68719476732L << (int)(28))) + 1 << (int)(30));
    // one step newton iteration to 53 bits with error less than 0.667ulps
    s = t * t;
    // t*t is exact
    r = x / s;
    var w = t + t;
    r = (r - t) / (w + r);
    // r-s is exact
    t = t + t * r;
    // restore the sign bit
    if (sign) {
        t = -t;
    }
    return t;
}

} // end math_package
