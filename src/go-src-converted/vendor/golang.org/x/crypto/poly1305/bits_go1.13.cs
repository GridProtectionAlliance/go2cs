// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build go1.13

// package poly1305 -- go2cs converted at 2020 October 09 06:06:33 UTC
// import "vendor/golang.org/x/crypto/poly1305" ==> using poly1305 = go.vendor.golang.org.x.crypto.poly1305_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\poly1305\bits_go1.13.go
using bits = go.math.bits_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class poly1305_package
    {
        private static (ulong, ulong) bitsAdd64(ulong x, ulong y, ulong carry)
        {
            ulong sum = default;
            ulong carryOut = default;

            return bits.Add64(x, y, carry);
        }

        private static (ulong, ulong) bitsSub64(ulong x, ulong y, ulong borrow)
        {
            ulong diff = default;
            ulong borrowOut = default;

            return bits.Sub64(x, y, borrow);
        }

        private static (ulong, ulong) bitsMul64(ulong x, ulong y)
        {
            ulong hi = default;
            ulong lo = default;

            return bits.Mul64(x, y);
        }
    }
}}}}}
