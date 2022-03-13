// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:41:54 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\asin.go
namespace go;

public static partial class math_package {

/*
    Floating-point arcsine and arccosine.

    They are implemented by computing the arctangent
    after appropriate range reduction.
*/

// Asin returns the arcsine, in radians, of x.
//
// Special cases are:
//    Asin(±0) = ±0
//    Asin(x) = NaN if x < -1 or x > 1
public static double Asin(double x) {
    if (haveArchAsin) {
        return archAsin(x);
    }
    return asin(x);
}

private static double asin(double x) {
    if (x == 0) {
        return x; // special case
    }
    var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    if (x > 1) {
        return NaN(); // special case
    }
    var temp = Sqrt(1 - x * x);
    if (x > 0.7F) {
        temp = Pi / 2 - satan(temp / x);
    }
    else
 {
        temp = satan(x / temp);
    }
    if (sign) {
        temp = -temp;
    }
    return temp;
}

// Acos returns the arccosine, in radians, of x.
//
// Special case is:
//    Acos(x) = NaN if x < -1 or x > 1
public static double Acos(double x) {
    if (haveArchAcos) {
        return archAcos(x);
    }
    return acos(x);
}

private static double acos(double x) {
    return Pi / 2 - Asin(x);
}

} // end math_package
