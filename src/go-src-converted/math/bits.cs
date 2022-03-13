// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:41:54 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\bits.go
namespace go;

public static partial class math_package {

private static readonly nuint uvnan = 0x7FF8000000000001;
private static readonly nuint uvinf = 0x7FF0000000000000;
private static readonly nuint uvneginf = 0xFFF0000000000000;
private static readonly nuint uvone = 0x3FF0000000000000;
private static readonly nuint mask = 0x7FF;
private static readonly nint shift = 64 - 11 - 1;
private static readonly nint bias = 1023;
private static readonly nint signMask = 1 << 63;
private static readonly nint fracMask = 1 << (int)(shift) - 1;

// Inf returns positive infinity if sign >= 0, negative infinity if sign < 0.
public static double Inf(nint sign) {
    ulong v = default;
    if (sign >= 0) {
        v = uvinf;
    }
    else
 {
        v = uvneginf;
    }
    return Float64frombits(v);
}

// NaN returns an IEEE 754 ``not-a-number'' value.
public static double NaN() {
    return Float64frombits(uvnan);
}

// IsNaN reports whether f is an IEEE 754 ``not-a-number'' value.
public static bool IsNaN(double f) {
    bool @is = default;
 
    // IEEE 754 says that only NaNs satisfy f != f.
    // To avoid the floating-point hardware, could use:
    //    x := Float64bits(f);
    //    return uint32(x>>shift)&mask == mask && x != uvinf && x != uvneginf
    return f != f;
}

// IsInf reports whether f is an infinity, according to sign.
// If sign > 0, IsInf reports whether f is positive infinity.
// If sign < 0, IsInf reports whether f is negative infinity.
// If sign == 0, IsInf reports whether f is either infinity.
public static bool IsInf(double f, nint sign) { 
    // Test for infinity by comparing against maximum float.
    // To avoid the floating-point hardware, could use:
    //    x := Float64bits(f);
    //    return sign >= 0 && x == uvinf || sign <= 0 && x == uvneginf;
    return sign >= 0 && f > MaxFloat64 || sign <= 0 && f < -MaxFloat64;
}

// normalize returns a normal number y and exponent exp
// satisfying x == y × 2**exp. It assumes x is finite and non-zero.
private static (double, nint) normalize(double x) {
    double y = default;
    nint exp = default;

    const float SmallestNormal = 2.2250738585072014e-308F; // 2**-1022
 // 2**-1022
    if (Abs(x) < SmallestNormal) {
        return (x * (1 << 52), -52);
    }
    return (x, 0);
}

} // end math_package
