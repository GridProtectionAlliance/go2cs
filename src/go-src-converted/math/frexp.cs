// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:31:05 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\frexp.go


namespace go;

public static partial class math_package {

    // Frexp breaks f into a normalized fraction
    // and an integral power of two.
    // It returns frac and exp satisfying f == frac × 2**exp,
    // with the absolute value of frac in the interval [½, 1).
    //
    // Special cases are:
    //    Frexp(±0) = ±0, 0
    //    Frexp(±Inf) = ±Inf, 0
    //    Frexp(NaN) = NaN, 0
public static (double, nint) Frexp(double f) {
    double frac = default;
    nint exp = default;

    if (haveArchFrexp) {
        return archFrexp(f);
    }
    return frexp(f);

}

private static (double, nint) frexp(double f) {
    double frac = default;
    nint exp = default;
 
    // special cases

    if (f == 0) 
        return (f, 0); // correctly return -0
    else if (IsInf(f, 0) || IsNaN(f)) 
        return (f, 0);
        f, exp = normalize(f);
    var x = Float64bits(f);
    exp += int((x >> (int)(shift)) & mask) - bias + 1;
    x &= mask << (int)(shift);
    x |= (-1 + bias) << (int)(shift);
    frac = Float64frombits(x);
    return ;

}

} // end math_package
