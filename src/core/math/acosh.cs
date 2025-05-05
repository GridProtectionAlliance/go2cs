// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// The original C code, the long comment, and the constants
// below are from FreeBSD's /usr/src/lib/msun/src/e_acosh.c
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
// __ieee754_acosh(x)
// Method :
//	Based on
//	        acosh(x) = log [ x + sqrt(x*x-1) ]
//	we have
//	        acosh(x) := log(x)+ln2,	if x is large; else
//	        acosh(x) := log(2x-1/(sqrt(x*x-1)+x)) if x>2; else
//	        acosh(x) := log1p(t+sqrt(2.0*t+t*t)); where t=x-1.
//
// Special cases:
//	acosh(x) is NaN with signal if x<1.
//	acosh(NaN) is NaN without signal.
//

// Acosh returns the inverse hyperbolic cosine of x.
//
// Special cases are:
//
//	Acosh(+Inf) = +Inf
//	Acosh(x) = NaN if x < 1
//	Acosh(NaN) = NaN
public static float64 Acosh(float64 x) {
    if (haveArchAcosh) {
        return archAcosh(x);
    }
    return acosh(x);
}

internal static float64 acosh(float64 x) {
    static readonly UntypedInt Large = /* 1 << 28 */ 268435456; // 2**28
    // first case is special case
    switch (ᐧ) {
    case {} when x < 1 || IsNaN(x): {
        return NaN();
    }
    case {} when x is 1: {
        return 0;
    }
    case {} when x is >= Large: {
        return Log(x) + Ln2;
    }
    case {} when x is > 2: {
        return Log(2 * x - 1 / (x + Sqrt(x * x - 1)));
    }}

    // x > 2**28
    // 2**28 > x > 2
    var t = x - 1;
    return Log1p(t + Sqrt(2 * t + t * t));
}

// 2 >= x > 1

} // end math_package
