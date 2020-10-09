// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !gccgo,!purego

// package chacha20 -- go2cs converted at 2020 October 09 06:06:15 UTC
// import "vendor/golang.org/x/crypto/chacha20" ==> using chacha20 = go.vendor.golang.org.x.crypto.chacha20_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\chacha20\chacha_ppc64le.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class chacha20_package
    {
        private static readonly long bufSize = (long)256L;

        //go:noescape


        //go:noescape
        private static void chaCha20_ctr32_vsx(ptr<byte> @out, ptr<byte> inp, long len, ptr<array<uint>> key, ptr<uint> counter)
;

        private static void xorKeyStreamBlocks(this ptr<Cipher> _addr_c, slice<byte> dst, slice<byte> src)
        {
            ref Cipher c = ref _addr_c.val;

            chaCha20_ctr32_vsx(_addr_dst[0L], _addr_src[0L], len(src), _addr_c.key, _addr_c.counter);
        }
    }
}}}}}
