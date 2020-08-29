// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package curve25519 provides an implementation of scalar multiplication on
// the elliptic curve known as curve25519. See http://cr.yp.to/ecdh.html
// package curve25519 -- go2cs converted at 2020 August 29 10:11:34 UTC
// import "vendor/golang_org/x/crypto/curve25519" ==> using curve25519 = go.vendor.golang_org.x.crypto.curve25519_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\curve25519\doc.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto
{
    public static partial class curve25519_package
    { // import "golang.org/x/crypto/curve25519"

        // basePoint is the x coordinate of the generator of the curve.
        private static array<byte> basePoint = new array<byte>(new byte[] { 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

        // ScalarMult sets dst to the product in*base where dst and base are the x
        // coordinates of group points and all values are in little-endian form.
        public static void ScalarMult(ref array<byte> dst, ref array<byte> @in, ref array<byte> @base)
        {
            scalarMult(dst, in, base);
        }

        // ScalarBaseMult sets dst to the product in*base where dst and base are the x
        // coordinates of group points, base is the standard generator and all values
        // are in little-endian form.
        public static void ScalarBaseMult(ref array<byte> dst, ref array<byte> @in)
        {
            ScalarMult(dst, in, ref basePoint);
        }
    }
}}}}}
