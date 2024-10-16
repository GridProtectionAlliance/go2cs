// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:42:05 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\sinh.go
namespace go;

public static partial class math_package {

/*
    Floating-point hyperbolic sine and cosine.

    The exponential func is called for arguments
    greater in magnitude than 0.5.

    A series is used for arguments smaller in magnitude than 0.5.

    Cosh(x) is computed from the exponential func for
    all arguments.
*/

// Sinh returns the hyperbolic sine of x.
//
// Special cases are:
//    Sinh(±0) = ±0
//    Sinh(±Inf) = ±Inf
//    Sinh(NaN) = NaN
public static double Sinh(double x) {
    if (haveArchSinh) {
        return archSinh(x);
    }
    return sinh(x);
}

private static double sinh(double x) { 
    // The coefficients are #2029 from Hart & Cheney. (20.36D)
    const float P0 = -0.6307673640497716991184787251e+6F;
    const float P1 = -0.8991272022039509355398013511e+5F;
    const float P2 = -0.2894211355989563807284660366e+4F;
    const float P3 = -0.2630563213397497062819489e+2F;
    const float Q0 = -0.6307673640497716991212077277e+6F;
    const float Q1 = 0.1521517378790019070696485176e+5F;
    const float Q2 = -0.173678953558233699533450911e+3F;

    var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    double temp = default;

    if (x > 21) 
        temp = Exp(x) * 0.5F;
    else if (x > 0.5F) 
        var ex = Exp(x);
        temp = (ex - 1 / ex) * 0.5F;
    else 
        var sq = x * x;
        temp = (((P3 * sq + P2) * sq + P1) * sq + P0) * x;
        temp = temp / (((sq + Q2) * sq + Q1) * sq + Q0);
        if (sign) {
        temp = -temp;
    }
    return temp;
}

// Cosh returns the hyperbolic cosine of x.
//
// Special cases are:
//    Cosh(±0) = 1
//    Cosh(±Inf) = +Inf
//    Cosh(NaN) = NaN
public static double Cosh(double x) {
    if (haveArchCosh) {
        return archCosh(x);
    }
    return cosh(x);
}

private static double cosh(double x) {
    x = Abs(x);
    if (x > 21) {
        return Exp(x) * 0.5F;
    }
    var ex = Exp(x);
    return (ex + 1 / ex) * 0.5F;
}

} // end math_package
