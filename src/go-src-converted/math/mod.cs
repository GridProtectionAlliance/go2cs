// Copyright 2009-2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:42:03 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\mod.go
namespace go;

public static partial class math_package {

/*
    Floating-point mod function.
*/

// Mod returns the floating-point remainder of x/y.
// The magnitude of the result is less than y and its
// sign agrees with that of x.
//
// Special cases are:
//    Mod(±Inf, y) = NaN
//    Mod(NaN, y) = NaN
//    Mod(x, 0) = NaN
//    Mod(x, ±Inf) = x
//    Mod(x, NaN) = NaN
public static double Mod(double x, double y) {
    if (haveArchMod) {
        return archMod(x, y);
    }
    return mod(x, y);
}

private static double mod(double x, double y) {
    if (y == 0 || IsInf(x, 0) || IsNaN(x) || IsNaN(y)) {
        return NaN();
    }
    y = Abs(y);

    var (yfr, yexp) = Frexp(y);
    var r = x;
    if (x < 0) {
        r = -x;
    }
    while (r >= y) {
        var (rfr, rexp) = Frexp(r);
        if (rfr < yfr) {
            rexp = rexp - 1;
        }
        r = r - Ldexp(y, rexp - yexp);
    }
    if (x < 0) {
        r = -r;
    }
    return r;
}

} // end math_package
