// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:31:10 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\remainder.go


namespace go;

public static partial class math_package {

    // The original C code and the comment below are from
    // FreeBSD's /usr/src/lib/msun/src/e_remainder.c and came
    // with this notice. The go code is a simplified version of
    // the original C.
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
    // __ieee754_remainder(x,y)
    // Return :
    //      returns  x REM y  =  x - [x/y]*y  as if in infinite
    //      precision arithmetic, where [x/y] is the (infinite bit)
    //      integer nearest x/y (in half way cases, choose the even one).
    // Method :
    //      Based on Mod() returning  x - [x/y]chopped * y  exactly.

    // Remainder returns the IEEE 754 floating-point remainder of x/y.
    //
    // Special cases are:
    //    Remainder(±Inf, y) = NaN
    //    Remainder(NaN, y) = NaN
    //    Remainder(x, 0) = NaN
    //    Remainder(x, ±Inf) = x
    //    Remainder(x, NaN) = NaN
public static double Remainder(double x, double y) {
    if (haveArchRemainder) {
        return archRemainder(x, y);
    }
    return remainder(x, y);

}

private static double remainder(double x, double y) {
    const float Tiny = 4.45014771701440276618e-308F; // 0x0020000000000000
    const var HalfMax = MaxFloat64 / 2;
 
    // special cases

    if (IsNaN(x) || IsNaN(y) || IsInf(x, 0) || y == 0) 
        return NaN();
    else if (IsInf(y, 0)) 
        return x;
        var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    if (y < 0) {
        y = -y;
    }
    if (x == y) {
        if (sign) {
            float zero = 0.0F;
            return -zero;
        }
        return 0;

    }
    if (y <= HalfMax) {
        x = Mod(x, y + y); // now x < 2y
    }
    if (y < Tiny) {
        if (x + x > y) {
            x -= y;
            if (x + x >= y) {
                x -= y;
            }
        }
    }
    else
 {
        float yHalf = 0.5F * y;
        if (x > yHalf) {
            x -= y;
            if (x >= yHalf) {
                x -= y;
            }
        }
    }
    if (sign) {
        x = -x;
    }
    return x;

}

} // end math_package
