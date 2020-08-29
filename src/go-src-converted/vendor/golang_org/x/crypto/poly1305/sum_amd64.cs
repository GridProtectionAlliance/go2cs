// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64,!gccgo,!appengine

// package poly1305 -- go2cs converted at 2020 August 29 10:11:36 UTC
// import "vendor/golang_org/x/crypto/poly1305" ==> using poly1305 = go.vendor.golang_org.x.crypto.poly1305_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\poly1305\sum_amd64.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto
{
    public static partial class poly1305_package
    {
        // This function is implemented in sum_amd64.s
        //go:noescape
        private static void poly1305(ref array<byte> @out, ref byte m, ulong mlen, ref array<byte> key)
;

        // Sum generates an authenticator for m using a one-time key and puts the
        // 16-byte result into out. Authenticating two different messages with the same
        // key allows an attacker to forge messages at will.
        public static void Sum(ref array<byte> @out, slice<byte> m, ref array<byte> key)
        {
            ref byte mPtr = default;
            if (len(m) > 0L)
            {>>MARKER:FUNCTION_poly1305_BLOCK_PREFIX<<
                mPtr = ref m[0L];
            }
            poly1305(out, mPtr, uint64(len(m)), key);
        }
    }
}}}}}
