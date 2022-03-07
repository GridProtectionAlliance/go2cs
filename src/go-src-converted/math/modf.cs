// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:31:10 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\modf.go


namespace go;

public static partial class math_package {

    // Modf returns integer and fractional floating-point numbers
    // that sum to f. Both values have the same sign as f.
    //
    // Special cases are:
    //    Modf(±Inf) = ±Inf, NaN
    //    Modf(NaN) = NaN, NaN
public static (double, double) Modf(double f) {
    double @int = default;
    double frac = default;

    if (haveArchModf) {
        return archModf(f);
    }
    return modf(f);

}

private static (double, double) modf(double f) {
    double @int = default;
    double frac = default;

    if (f < 1) {

        if (f < 0) 
            int, frac = Modf(-f);
            return (-int, -frac);
        else if (f == 0) 
            return (f, f); // Return -0, -0 when f == -0
                return (0, f);

    }
    var x = Float64bits(f);
    var e = uint(x >> (int)(shift)) & mask - bias; 

    // Keep the top 12+e bits, the integer part; clear the rest.
    if (e < 64 - 12) {
        x &= 1 << (int)((64 - 12 - e)) - 1;
    }
    int = Float64frombits(x);
    frac = f - int;
    return ;

}

} // end math_package
