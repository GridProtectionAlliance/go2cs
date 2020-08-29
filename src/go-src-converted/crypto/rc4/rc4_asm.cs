// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64 amd64p32 arm,!nacl 386

// package rc4 -- go2cs converted at 2020 August 29 08:30:54 UTC
// import "crypto/rc4" ==> using rc4 = go.crypto.rc4_package
// Original source: C:\Go\src\crypto\rc4\rc4_asm.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class rc4_package
    {
        private static void xorKeyStream(ref byte dst, ref byte src, long n, ref array<uint> state, ref byte i, ref byte j)
;

        // XORKeyStream sets dst to the result of XORing src with the key stream.
        // Dst and src must overlap entirely or not at all.
        private static void XORKeyStream(this ref Cipher c, slice<byte> dst, slice<byte> src)
        {>>MARKER:FUNCTION_xorKeyStream_BLOCK_PREFIX<<
            if (len(src) == 0L)
            {
                return;
            } 
            // Assert len(dst) >= len(src)
            _ = dst[len(src) - 1L];
            xorKeyStream(ref dst[0L], ref src[0L], len(src), ref c.s, ref c.i, ref c.j);
        }
    }
}}
