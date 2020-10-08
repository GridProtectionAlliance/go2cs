// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !arm64,!s390x,!ppc64le arm64,!go1.11 gccgo purego

// package chacha20 -- go2cs converted at 2020 October 08 04:59:53 UTC
// import "vendor/golang.org/x/crypto/chacha20" ==> using chacha20 = go.vendor.golang.org.x.crypto.chacha20_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\chacha20\chacha_noasm.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class chacha20_package
    {
        private static readonly var bufSize = (var)blockSize;



        private static void xorKeyStreamBlocks(this ptr<Cipher> _addr_s, slice<byte> dst, slice<byte> src)
        {
            ref Cipher s = ref _addr_s.val;

            s.xorKeyStreamBlocksGeneric(dst, src);
        }
    }
}}}}}
