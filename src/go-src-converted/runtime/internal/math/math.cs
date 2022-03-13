// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:24:14 UTC
// import "runtime/internal/math" ==> using math = go.runtime.@internal.math_package
// Original source: C:\Program Files\Go\src\runtime\internal\math\math.go
namespace go.runtime.@internal;

using sys = runtime.@internal.sys_package;

public static partial class math_package {

public static readonly var MaxUintptr = ~uintptr(0);

// MulUintptr returns a * b and whether the multiplication overflowed.
// On supported platforms this is an intrinsic lowered by the compiler.


// MulUintptr returns a * b and whether the multiplication overflowed.
// On supported platforms this is an intrinsic lowered by the compiler.
public static (System.UIntPtr, bool) MulUintptr(System.UIntPtr a, System.UIntPtr b) {
    System.UIntPtr _p0 = default;
    bool _p0 = default;

    if (a | b < 1 << (int)((4 * sys.PtrSize)) || a == 0) {
        return (a * b, false);
    }
    var overflow = b > MaxUintptr / a;
    return (a * b, overflow);
}

// Mul64 returns the 128-bit product of x and y: (hi, lo) = x * y
// with the product bits' upper half returned in hi and the lower
// half returned in lo.
// This is a copy from math/bits.Mul64
// On supported platforms this is an intrinsic lowered by the compiler.
public static (ulong, ulong) Mul64(ulong x, ulong y) {
    ulong hi = default;
    ulong lo = default;

    const nint mask32 = 1 << 32 - 1;

    var x0 = x & mask32;
    var x1 = x >> 32;
    var y0 = y & mask32;
    var y1 = y >> 32;
    var w0 = x0 * y0;
    var t = x1 * y0 + w0 >> 32;
    var w1 = t & mask32;
    var w2 = t >> 32;
    w1 += x0 * y1;
    hi = x1 * y1 + w2 + w1 >> 32;
    lo = x * y;
    return ;
}

} // end math_package
