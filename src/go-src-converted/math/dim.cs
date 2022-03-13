// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:41:55 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\dim.go
namespace go;

public static partial class math_package {

// Dim returns the maximum of x-y or 0.
//
// Special cases are:
//    Dim(+Inf, +Inf) = NaN
//    Dim(-Inf, -Inf) = NaN
//    Dim(x, NaN) = Dim(NaN, x) = NaN
public static double Dim(double x, double y) { 
    // The special cases result in NaN after the subtraction:
    //      +Inf - +Inf = NaN
    //      -Inf - -Inf = NaN
    //       NaN - y    = NaN
    //         x - NaN  = NaN
    var v = x - y;
    if (v <= 0) { 
        // v is negative or 0
        return 0;
    }
    return v;
}

// Max returns the larger of x or y.
//
// Special cases are:
//    Max(x, +Inf) = Max(+Inf, x) = +Inf
//    Max(x, NaN) = Max(NaN, x) = NaN
//    Max(+0, ±0) = Max(±0, +0) = +0
//    Max(-0, -0) = -0
public static double Max(double x, double y) {
    if (haveArchMax) {
        return archMax(x, y);
    }
    return max(x, y);
}

private static double max(double x, double y) { 
    // special cases

    if (IsInf(x, 1) || IsInf(y, 1)) 
        return Inf(1);
    else if (IsNaN(x) || IsNaN(y)) 
        return NaN();
    else if (x == 0 && x == y) 
        if (Signbit(x)) {
            return y;
        }
        return x;
        if (x > y) {
        return x;
    }
    return y;
}

// Min returns the smaller of x or y.
//
// Special cases are:
//    Min(x, -Inf) = Min(-Inf, x) = -Inf
//    Min(x, NaN) = Min(NaN, x) = NaN
//    Min(-0, ±0) = Min(±0, -0) = -0
public static double Min(double x, double y) {
    if (haveArchMin) {
        return archMin(x, y);
    }
    return min(x, y);
}

private static double min(double x, double y) { 
    // special cases

    if (IsInf(x, -1) || IsInf(y, -1)) 
        return Inf(-1);
    else if (IsNaN(x) || IsNaN(y)) 
        return NaN();
    else if (x == 0 && x == y) 
        if (Signbit(x)) {
            return x;
        }
        return y;
        if (x < y) {
        return x;
    }
    return y;
}

} // end math_package
