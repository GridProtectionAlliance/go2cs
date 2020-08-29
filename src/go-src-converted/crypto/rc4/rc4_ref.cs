// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!amd64p32,!arm,!386 arm,nacl

// package rc4 -- go2cs converted at 2020 August 29 08:30:54 UTC
// import "crypto/rc4" ==> using rc4 = go.crypto.rc4_package
// Original source: C:\Go\src\crypto\rc4\rc4_ref.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class rc4_package
    {
        // XORKeyStream sets dst to the result of XORing src with the key stream.
        // Dst and src must overlap entirely or not at all.
        private static void XORKeyStream(this ref Cipher c, slice<byte> dst, slice<byte> src)
        {
            c.xorKeyStreamGeneric(dst, src);
        }
    }
}}
