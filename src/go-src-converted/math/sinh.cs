// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

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
//
//	Sinh(±0) = ±0
//	Sinh(±Inf) = ±Inf
//	Sinh(NaN) = NaN
public static float64 Sinh(float64 x) {
    if (haveArchSinh) {
        return archSinh(x);
    }
    return sinh(x);
}

internal static float64 sinh(float64 x) {
    // The coefficients are #2029 from Hart & Cheney. (20.36D)
    UntypedFloat P0 = -630767.3640497717;
    
    UntypedFloat P1 = /* -0.8991272022039509355398013511e+5 */ -89912.7;
    
    UntypedFloat P2 = /* -0.2894211355989563807284660366e+4 */ -2894.21;
    
    UntypedFloat P3 = /* -0.2630563213397497062819489e+2 */ -26.3056;
    
    UntypedFloat Q0 = -630767.3640497717;
    
    UntypedFloat Q1 = /* 0.1521517378790019070696485176e+5 */ 15215.2;
    
    UntypedFloat Q2 = /* -0.173678953558233699533450911e+3 */ -173.679;
    var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    float64 temp = default!;
    switch (ᐧ) {
    case {} when x is > 21: {
        temp = Exp(x) * 0.5D;
        break;
    }
    case {} when x is > 0.5D: {
        var ex = Exp(x);
        temp = (ex - 1 / ex) * 0.5D;
        break;
    }
    default: {
        var sq = x * x;
        temp = ((((float64)P3 * sq + (float64)P2) * sq + (float64)P1) * sq + (float64)P0) * x;
        temp = temp / (((sq + (float64)Q2) * sq + (float64)Q1) * sq + (float64)Q0);
        break;
    }}

    if (sign) {
        temp = -temp;
    }
    return temp;
}

// Cosh returns the hyperbolic cosine of x.
//
// Special cases are:
//
//	Cosh(±0) = 1
//	Cosh(±Inf) = +Inf
//	Cosh(NaN) = NaN
public static float64 Cosh(float64 x) {
    if (haveArchCosh) {
        return archCosh(x);
    }
    return cosh(x);
}

internal static float64 cosh(float64 x) {
    x = Abs(x);
    if (x > 21) {
        return Exp(x) * 0.5D;
    }
    var ex = Exp(x);
    return (ex + 1 / ex) * 0.5D;
}

} // end math_package
