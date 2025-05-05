// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// pow10tab stores the pre-computed values 10**i for i < 32.
internal static array<float64> pow10tab = new float64[]{
    1e00F, 1e01F, 1e02F, 1e03F, 1e04F, 1e05F, 1e06F, 1e07F, 1e08F, 1e09F,
    1e10F, 1e11F, 1e12F, 1e13F, 1e14F, 1e15F, 1e16F, 1e17F, 1e18F, 1e19F,
    1e20F, 1e21F, 1e22F, 1e23F, 1e24F, 1e25F, 1e26F, 1e27F, 1e28F, 1e29F,
    1e30F, 1e31F
}.array();

// pow10postab32 stores the pre-computed value for 10**(i*32) at index i.
internal static array<float64> pow10postab32 = new float64[]{
    1e00F, 1e32F, 1e64D, 1e96D, 1e128D, 1e160D, 1e192D, 1e224D, 1e256D, 1e288D
}.array();

// pow10negtab32 stores the pre-computed value for 10**(-i*32) at index i.
internal static array<float64> pow10negtab32 = new float64[]{
    1e-00F, 1e-32F, 1e-64F, 1e-96F, 1e-128F, 1e-160F, 1e-192F, 1e-224F, 1e-256F, 1e-288F, 1e-320F
}.array();

// Pow10 returns 10**n, the base-10 exponential of n.
//
// Special cases are:
//
//	Pow10(n) =    0 for n < -323
//	Pow10(n) = +Inf for n > 308
public static float64 Pow10(nint n) {
    if (0 <= n && n <= 308) {
        return pow10postab32[((nuint)n) / 32] * pow10tab[((nuint)n) % 32];
    }
    if (-323 <= n && n <= 0) {
        return pow10negtab32[((nuint)(-n)) / 32] / pow10tab[((nuint)(-n)) % 32];
    }
    // n < -323 || 308 < n
    if (n > 0) {
        return Inf(1);
    }
    // n < -323
    return 0;
}

} // end math_package
