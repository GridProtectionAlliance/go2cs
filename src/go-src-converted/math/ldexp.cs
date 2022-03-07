// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:31:08 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\ldexp.go


namespace go;

public static partial class math_package {

    // Ldexp is the inverse of Frexp.
    // It returns frac × 2**exp.
    //
    // Special cases are:
    //    Ldexp(±0, exp) = ±0
    //    Ldexp(±Inf, exp) = ±Inf
    //    Ldexp(NaN, exp) = NaN
public static double Ldexp(double frac, nint exp) {
    if (haveArchLdexp) {
        return archLdexp(frac, exp);
    }
    return ldexp(frac, exp);

}

private static double ldexp(double frac, nint exp) { 
    // special cases

    if (frac == 0) 
        return frac; // correctly return -0
    else if (IsInf(frac, 0) || IsNaN(frac)) 
        return frac;
        var (frac, e) = normalize(frac);
    exp += e;
    var x = Float64bits(frac);
    exp += int(x >> (int)(shift)) & mask - bias;
    if (exp < -1075) {
        return Copysign(0, frac); // underflow
    }
    if (exp > 1023) { // overflow
        if (frac < 0) {
            return Inf(-1);
        }
        return Inf(1);

    }
    double m = 1;
    if (exp < -1022) { // denormal
        exp += 53;
        m = 1.0F / (1 << 53); // 2**-53
    }
    x &= mask << (int)(shift);
    x |= uint64(exp + bias) << (int)(shift);
    return m * Float64frombits(x);

}

} // end math_package
