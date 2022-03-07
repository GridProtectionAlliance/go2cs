// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sha1 -- go2cs converted at 2022 March 06 22:19:26 UTC
// import "crypto/sha1" ==> using sha1 = go.crypto.sha1_package
// Original source: C:\Program Files\Go\src\crypto\sha1\sha1block_amd64.go
using cpu = go.@internal.cpu_package;

namespace go.crypto;

public static partial class sha1_package {

    //go:noescape
private static void blockAVX2(ptr<digest> dig, slice<byte> p);

//go:noescape
private static void blockAMD64(ptr<digest> dig, slice<byte> p);

private static var useAVX2 = cpu.X86.HasAVX2 && cpu.X86.HasBMI1 && cpu.X86.HasBMI2;

private static void block(ptr<digest> _addr_dig, slice<byte> p) {
    ref digest dig = ref _addr_dig.val;

    if (useAVX2 && len(p) >= 256) {>>MARKER:FUNCTION_blockAMD64_BLOCK_PREFIX<< 
        // blockAVX2 calculates sha1 for 2 block per iteration
        // it also interleaves precalculation for next block.
        // So it may read up-to 192 bytes past end of p
        // We may add checks inside blockAVX2, but this will
        // just turn it into a copy of blockAMD64,
        // so call it directly, instead.
        var safeLen = len(p) - 128;
        if (safeLen % 128 != 0) {>>MARKER:FUNCTION_blockAVX2_BLOCK_PREFIX<<
            safeLen -= 64;
        }
        blockAVX2(_addr_dig, p[..(int)safeLen]);
        blockAMD64(_addr_dig, p[(int)safeLen..]);

    }
    else
 {
        blockAMD64(_addr_dig, p);
    }
}

} // end sha1_package
