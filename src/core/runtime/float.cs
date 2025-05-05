// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

internal static float64 inf = float64frombits((nint)9218868437227405312L);

// isNaN reports whether f is an IEEE 754 “not-a-number” value.
internal static bool /*is*/ isNaN(float64 f) {
    bool @is = default!;

    // IEEE 754 says that only NaNs satisfy f != f.
    return f != f;
}

// isFinite reports whether f is neither NaN nor an infinity.
internal static bool isFinite(float64 f) {
    return !isNaN(f - f);
}

// isInf reports whether f is an infinity.
internal static bool isInf(float64 f) {
    return !isNaN(f) && !isFinite(f);
}

// abs returns the absolute value of x.
//
// Special cases are:
//
//	abs(±Inf) = +Inf
//	abs(NaN) = NaN
internal static float64 abs(float64 x) {
    GoUntyped sign = /* 1 << 63 */
            GoUntyped.Parse("9223372036854775808");
    return float64frombits((uint64)(float64bits(x) & ~sign));
}

// copysign returns a value with the magnitude
// of x and the sign of y.
internal static float64 copysign(float64 x, float64 y) {
    GoUntyped sign = /* 1 << 63 */
            GoUntyped.Parse("9223372036854775808");
    return float64frombits((uint64)((uint64)(float64bits(x) & ~sign) | (uint64)(float64bits(y) & sign)));
}

// float64bits returns the IEEE 754 binary representation of f.
internal static uint64 float64bits(float64 f) {
    return ~(ж<uint64>)(uintptr)(new @unsafe.Pointer(Ꮡ(f)));
}

// float64frombits returns the floating point number corresponding
// the IEEE 754 binary representation b.
internal static float64 float64frombits(uint64 b) {
    return ~(ж<float64>)(uintptr)(new @unsafe.Pointer(Ꮡ(b)));
}

} // end runtime_package
