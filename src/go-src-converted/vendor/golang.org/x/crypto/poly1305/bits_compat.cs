// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !go1.13
// +build !go1.13

// package poly1305 -- go2cs converted at 2022 March 13 06:44:59 UTC
// import "vendor/golang.org/x/crypto/poly1305" ==> using poly1305 = go.vendor.golang.org.x.crypto.poly1305_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\poly1305\bits_compat.go
namespace go.vendor.golang.org.x.crypto;

public static partial class poly1305_package {

// Generic fallbacks for the math/bits intrinsics, copied from
// src/math/bits/bits.go. They were added in Go 1.12, but Add64 and Sum64 had
// variable time fallbacks until Go 1.13.

private static (ulong, ulong) bitsAdd64(ulong x, ulong y, ulong carry) {
    ulong sum = default;
    ulong carryOut = default;

    sum = x + y + carry;
    carryOut = ((x & y) | ((x | y) & ~sum)) >> 63;
    return ;
}

private static (ulong, ulong) bitsSub64(ulong x, ulong y, ulong borrow) {
    ulong diff = default;
    ulong borrowOut = default;

    diff = x - y - borrow;
    borrowOut = ((~x & y) | (~(x ^ y) & diff)) >> 63;
    return ;
}

private static (ulong, ulong) bitsMul64(ulong x, ulong y) {
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

} // end poly1305_package
