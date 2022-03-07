// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:31:06 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\hypot.go


namespace go;

public static partial class math_package {

    /*
        Hypot -- sqrt(p*p + q*q), but overflows only if the result does.
    */

    // Hypot returns Sqrt(p*p + q*q), taking care to avoid
    // unnecessary overflow and underflow.
    //
    // Special cases are:
    //    Hypot(±Inf, q) = +Inf
    //    Hypot(p, ±Inf) = +Inf
    //    Hypot(NaN, q) = NaN
    //    Hypot(p, NaN) = NaN
public static double Hypot(double p, double q) {
    if (haveArchHypot) {
        return archHypot(p, q);
    }
    return hypot(p, q);

}

private static double hypot(double p, double q) { 
    // special cases

    if (IsInf(p, 0) || IsInf(q, 0)) 
        return Inf(1);
    else if (IsNaN(p) || IsNaN(q)) 
        return NaN();
        (p, q) = (Abs(p), Abs(q));    if (p < q) {
        (p, q) = (q, p);
    }
    if (p == 0) {
        return 0;
    }
    q = q / p;
    return p * Sqrt(1 + q * q);

}

} // end math_package
