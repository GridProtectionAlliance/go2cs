// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:42:04 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\pow.go
namespace go;

public static partial class math_package {

private static bool isOddInt(double x) {
    var (xi, xf) = Modf(x);
    return xf == 0 && int64(xi) & 1 == 1;
}

// Special cases taken from FreeBSD's /usr/src/lib/msun/src/e_pow.c
// updated by IEEE Std. 754-2008 "Section 9.2.1 Special values".

// Pow returns x**y, the base-x exponential of y.
//
// Special cases are (in order):
//    Pow(x, ±0) = 1 for any x
//    Pow(1, y) = 1 for any y
//    Pow(x, 1) = x for any x
//    Pow(NaN, y) = NaN
//    Pow(x, NaN) = NaN
//    Pow(±0, y) = ±Inf for y an odd integer < 0
//    Pow(±0, -Inf) = +Inf
//    Pow(±0, +Inf) = +0
//    Pow(±0, y) = +Inf for finite y < 0 and not an odd integer
//    Pow(±0, y) = ±0 for y an odd integer > 0
//    Pow(±0, y) = +0 for finite y > 0 and not an odd integer
//    Pow(-1, ±Inf) = 1
//    Pow(x, +Inf) = +Inf for |x| > 1
//    Pow(x, -Inf) = +0 for |x| > 1
//    Pow(x, +Inf) = +0 for |x| < 1
//    Pow(x, -Inf) = +Inf for |x| < 1
//    Pow(+Inf, y) = +Inf for y > 0
//    Pow(+Inf, y) = +0 for y < 0
//    Pow(-Inf, y) = Pow(-0, -y)
//    Pow(x, y) = NaN for finite x < 0 and finite non-integer y
public static double Pow(double x, double y) {
    if (haveArchPow) {
        return archPow(x, y);
    }
    return pow(x, y);
}

private static double pow(double x, double y) {

    if (y == 0 || x == 1) 
        return 1;
    else if (y == 1) 
        return x;
    else if (IsNaN(x) || IsNaN(y)) 
        return NaN();
    else if (x == 0) 

        if (y < 0) 
            if (isOddInt(y)) {
                return Copysign(Inf(1), x);
            }
            return Inf(1);
        else if (y > 0) 
            if (isOddInt(y)) {
                return x;
            }
            return 0;
            else if (IsInf(y, 0)) 

        if (x == -1) 
            return 1;
        else if ((Abs(x) < 1) == IsInf(y, 1)) 
            return 0;
        else 
            return Inf(1);
            else if (IsInf(x, 0)) 
        if (IsInf(x, -1)) {
            return Pow(1 / x, -y); // Pow(-0, -y)
        }

        if (y < 0) 
            return 0;
        else if (y > 0) 
            return Inf(1);
            else if (y == 0.5F) 
        return Sqrt(x);
    else if (y == -0.5F) 
        return 1 / Sqrt(x);
        var (yi, yf) = Modf(Abs(y));
    if (yf != 0 && x < 0) {
        return NaN();
    }
    if (yi >= 1 << 63) { 
        // yi is a large even int that will lead to overflow (or underflow to 0)
        // for all x except -1 (x == 1 was handled earlier)

        if (x == -1) 
            return 1;
        else if ((Abs(x) < 1) == (y > 0)) 
            return 0;
        else 
            return Inf(1);
            }
    float a1 = 1.0F;
    nint ae = 0; 

    // ans *= x**yf
    if (yf != 0) {
        if (yf > 0.5F) {
            yf--;
            yi++;
        }
        a1 = Exp(yf * Log(x));
    }
    var (x1, xe) = Frexp(x);
    {
        var i = int64(yi);

        while (i != 0) {
            if (xe < -1 << 12 || 1 << 12 < xe) { 
                // catch xe before it overflows the left shift below
                // Since i !=0 it has at least one bit still set, so ae will accumulate xe
                // on at least one more iteration, ae += xe is a lower bound on ae
                // the lower bound on ae exceeds the size of a float64 exp
                // so the final call to Ldexp will produce under/overflow (0/Inf)
                ae += xe;
                break;
            i>>=1;
            }
            if (i & 1 == 1) {
                a1 *= x1;
                ae += xe;
            }
            x1 *= x1;
            xe<<=1;
            if (x1 < .5F) {
                x1 += x1;
                xe--;
            }
        }
    } 

    // ans = a1*2**ae
    // if y < 0 { ans = 1 / ans }
    // but in the opposite order
    if (y < 0) {
        a1 = 1 / a1;
        ae = -ae;
    }
    return Ldexp(a1, ae);
}

} // end math_package
