// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:42:03 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\logb.go
namespace go;

public static partial class math_package {

// Logb returns the binary exponent of x.
//
// Special cases are:
//    Logb(±Inf) = +Inf
//    Logb(0) = -Inf
//    Logb(NaN) = NaN
public static double Logb(double x) { 
    // special cases

    if (x == 0) 
        return Inf(-1);
    else if (IsInf(x, 0)) 
        return Inf(1);
    else if (IsNaN(x)) 
        return x;
        return float64(ilogb(x));
}

// Ilogb returns the binary exponent of x as an integer.
//
// Special cases are:
//    Ilogb(±Inf) = MaxInt32
//    Ilogb(0) = MinInt32
//    Ilogb(NaN) = MaxInt32
public static nint Ilogb(double x) { 
    // special cases

    if (x == 0) 
        return MinInt32;
    else if (IsNaN(x)) 
        return MaxInt32;
    else if (IsInf(x, 0)) 
        return MaxInt32;
        return ilogb(x);
}

// logb returns the binary exponent of x. It assumes x is finite and
// non-zero.
private static nint ilogb(double x) {
    var (x, exp) = normalize(x);
    return int((Float64bits(x) >> (int)(shift)) & mask) - bias + exp;
}

} // end math_package
