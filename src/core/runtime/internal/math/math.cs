// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime.@internal;

using goarch = @internal.goarch_package;
using @internal;

partial class math_package {

public static readonly GoUntyped MaxUintptr = /* ^uintptr(0) */
    GoUntyped.Parse("18446744073709551615");

// MulUintptr returns a * b and whether the multiplication overflowed.
// On supported platforms this is an intrinsic lowered by the compiler.
public static (uintptr, bool) MulUintptr(uintptr a, uintptr b) {
    if ((uintptr)(a | b) < 1 << (int)((4 * goarch.PtrSize)) || a == 0) {
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
public static (uint64 hi, uint64 lo) Mul64(uint64 x, uint64 y) {
    uint64 hi = default!;
    uint64 lo = default!;

    static readonly UntypedInt mask32 = /* 1<<32 - 1 */ 4294967295;
    var x0 = (uint64)(x & mask32);
    var x1 = x >> (int)(32);
    var y0 = (uint64)(y & mask32);
    var y1 = y >> (int)(32);
    var w0 = x0 * y0;
    var t = x1 * y0 + w0 >> (int)(32);
    var w1 = (uint64)(t & mask32);
    var w2 = t >> (int)(32);
    w1 += x0 * y1;
    hi = x1 * y1 + w2 + w1 >> (int)(32);
    lo = x * y;
    return (hi, lo);
}

// Add64 returns the sum with carry of x, y and carry: sum = x + y + carry.
// The carry input must be 0 or 1; otherwise the behavior is undefined.
// The carryOut output is guaranteed to be 0 or 1.
//
// This function's execution time does not depend on the inputs.
// On supported platforms this is an intrinsic lowered by the compiler.
public static (uint64 sum, uint64 carryOut) Add64(uint64 x, uint64 y, uint64 carry) {
    uint64 sum = default!;
    uint64 carryOut = default!;

    sum = x + y + carry;
    // The sum will overflow if both top bits are set (x & y) or if one of them
    // is (x | y), and a carry from the lower place happened. If such a carry
    // happens, the top bit will be 1 + 0 + 1 = 0 (&^ sum).
    carryOut = ((uint64)(((uint64)(x & y)) | ((uint64)(((uint64)(x | y)) & ~sum)))) >> (int)(63);
    return (sum, carryOut);
}

} // end math_package
