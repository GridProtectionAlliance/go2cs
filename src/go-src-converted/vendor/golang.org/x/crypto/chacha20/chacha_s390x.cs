// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !gccgo,!purego

// package chacha20 -- go2cs converted at 2020 October 09 06:06:15 UTC
// import "vendor/golang.org/x/crypto/chacha20" ==> using chacha20 = go.vendor.golang.org.x.crypto.chacha20_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\chacha20\chacha_s390x.go
using cpu = go.golang.org.x.sys.cpu_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class chacha20_package
    {
        private static var haveAsm = cpu.S390X.HasVX;

        private static readonly long bufSize = (long)256L;

        // xorKeyStreamVX is an assembly implementation of XORKeyStream. It must only
        // be called when the vector facility is available. Implementation in asm_s390x.s.
        //go:noescape


        // xorKeyStreamVX is an assembly implementation of XORKeyStream. It must only
        // be called when the vector facility is available. Implementation in asm_s390x.s.
        //go:noescape
        private static void xorKeyStreamVX(slice<byte> dst, slice<byte> src, ptr<array<uint>> key, ptr<array<uint>> nonce, ptr<uint> counter)
;

        private static void xorKeyStreamBlocks(this ptr<Cipher> _addr_c, slice<byte> dst, slice<byte> src)
        {
            ref Cipher c = ref _addr_c.val;

            if (cpu.S390X.HasVX)
            {>>MARKER:FUNCTION_xorKeyStreamVX_BLOCK_PREFIX<<
                xorKeyStreamVX(dst, src, _addr_c.key, _addr_c.nonce, _addr_c.counter);
            }
            else
            {
                c.xorKeyStreamBlocksGeneric(dst, src);
            }

        }
    }
}}}}}
