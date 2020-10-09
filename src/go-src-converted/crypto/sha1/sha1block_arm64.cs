// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sha1 -- go2cs converted at 2020 October 09 04:54:38 UTC
// import "crypto/sha1" ==> using sha1 = go.crypto.sha1_package
// Original source: C:\Go\src\crypto\sha1\sha1block_arm64.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha1_package
    {
        private static uint k = new slice<uint>(new uint[] { 0x5A827999, 0x6ED9EBA1, 0x8F1BBCDC, 0xCA62C1D6 });

        //go:noescape
        private static void sha1block(slice<uint> h, slice<byte> p, slice<uint> k)
;

        private static void block(ptr<digest> _addr_dig, slice<byte> p)
        {
            ref digest dig = ref _addr_dig.val;

            if (!cpu.ARM64.HasSHA1)
            {>>MARKER:FUNCTION_sha1block_BLOCK_PREFIX<<
                blockGeneric(dig, p);
            }
            else
            {
                var h = dig.h[..];
                sha1block(h, p, k);
            }

        }
    }
}}
