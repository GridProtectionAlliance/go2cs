// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

internal static readonly UntypedInt uvnan = /* 0x7FF8000000000001 */ 9221120237041090561;
internal static readonly UntypedInt uvinf = /* 0x7FF0000000000000 */ 9218868437227405312;
internal static readonly UntypedInt uvneginf = /* 0xFFF0000000000000 */ 18442240474082181120;
internal static readonly UntypedInt uvone = /* 0x3FF0000000000000 */ 4607182418800017408;
internal static readonly UntypedInt mask = /* 0x7FF */ 2047;
internal static readonly UntypedInt shift = /* 64 - 11 - 1 */ 52;
internal static readonly UntypedInt bias = 1023;
internal static readonly UntypedInt signMask = /* 1 << 63 */ 9223372036854775808;
internal static readonly UntypedInt fracMask = /* 1<<shift - 1 */ 4503599627370495;

// Inf returns positive infinity if sign >= 0, negative infinity if sign < 0.
public static float64 Inf(nint sign) {
    uint64 v = default!;
    if (sign >= 0){
        v = uvinf;
    } else {
        v = uvneginf;
    }
    return Float64frombits(v);
}

// NaN returns an IEEE 754 “not-a-number” value.
public static float64 NaN() {
    return Float64frombits(uvnan);
}

// IsNaN reports whether f is an IEEE 754 “not-a-number” value.
public static bool /*is*/ IsNaN(float64 f) {
    bool @is = default!;

    // IEEE 754 says that only NaNs satisfy f != f.
    // To avoid the floating-point hardware, could use:
    //	x := Float64bits(f);
    //	return uint32(x>>shift)&mask == mask && x != uvinf && x != uvneginf
    return f != f;
}

// IsInf reports whether f is an infinity, according to sign.
// If sign > 0, IsInf reports whether f is positive infinity.
// If sign < 0, IsInf reports whether f is negative infinity.
// If sign == 0, IsInf reports whether f is either infinity.
public static bool IsInf(float64 f, nint sign) {
    // Test for infinity by comparing against maximum float.
    // To avoid the floating-point hardware, could use:
    //	x := Float64bits(f);
    //	return sign >= 0 && x == uvinf || sign <= 0 && x == uvneginf;
    return sign >= 0 && f > MaxFloat64 || sign <= 0 && f < -MaxFloat64;
}

// normalize returns a normal number y and exponent exp
// satisfying x == y × 2**exp. It assumes x is finite and non-zero.
internal static (float64 y, nint exp) normalize(float64 x) {
    float64 y = default!;
    nint exp = default!;

    static readonly UntypedFloat SmallestNormal = /* 2.2250738585072014e-308 */ 2.22507e-308; // 2**-1022
    if (Abs(x) < SmallestNormal) {
        return (x * (1 << (int)(52)), -52);
    }
    return (x, 0);
}

} // end math_package
