// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64

// package sha512 -- go2cs converted at 2020 October 09 04:53:41 UTC
// import "crypto/sha512" ==> using sha512 = go.crypto.sha512_package
// Original source: C:\Go\src\crypto\sha512\sha512block_amd64.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha512_package
    {
        //go:noescape
        private static void blockAVX2(ptr<digest> dig, slice<byte> p)
;

        //go:noescape
        private static void blockAMD64(ptr<digest> dig, slice<byte> p)
;

        private static var useAVX2 = cpu.X86.HasAVX2 && cpu.X86.HasBMI1 && cpu.X86.HasBMI2;

        private static void block(ptr<digest> _addr_dig, slice<byte> p)
        {
            ref digest dig = ref _addr_dig.val;

            if (useAVX2)
            {>>MARKER:FUNCTION_blockAMD64_BLOCK_PREFIX<<
                blockAVX2(_addr_dig, p);
            }
            else
            {>>MARKER:FUNCTION_blockAVX2_BLOCK_PREFIX<<
                blockAMD64(_addr_dig, p);
            }

        }
    }
}}
