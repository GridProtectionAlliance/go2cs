// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sha1 -- go2cs converted at 2020 August 29 08:31:01 UTC
// import "crypto/sha1" ==> using sha1 = go.crypto.sha1_package
// Original source: C:\Go\src\crypto\sha1\sha1block_amd64.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha1_package
    {
        //go:noescape
        private static void blockAVX2(ref digest dig, slice<byte> p)
;

        //go:noescape
        private static void blockAMD64(ref digest dig, slice<byte> p)
;

        private static var useAVX2 = cpu.X86.HasAVX2 && cpu.X86.HasBMI1 && cpu.X86.HasBMI2;

        private static void block(ref digest dig, slice<byte> p)
        {
            if (useAVX2 && len(p) >= 256L)
            {>>MARKER:FUNCTION_blockAMD64_BLOCK_PREFIX<< 
                // blockAVX2 calculates sha1 for 2 block per iteration
                // it also interleaves precalculation for next block.
                // So it may read up-to 192 bytes past end of p
                // We may add checks inside blockAVX2, but this will
                // just turn it into a copy of blockAMD64,
                // so call it directly, instead.
                var safeLen = len(p) - 128L;
                if (safeLen % 128L != 0L)
                {>>MARKER:FUNCTION_blockAVX2_BLOCK_PREFIX<<
                    safeLen -= 64L;
                }
                blockAVX2(dig, p[..safeLen]);
                blockAMD64(dig, p[safeLen..]);
            }
            else
            {
                blockAMD64(dig, p);
            }
        }
    }
}}
