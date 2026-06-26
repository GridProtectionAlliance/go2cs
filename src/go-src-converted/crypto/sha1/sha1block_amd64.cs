// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !purego
namespace go.crypto;

using cpu = @internal.cpu_package;
using @internal;

partial class sha1_package {

//go:noescape
internal static partial void blockAVX2(ж<digest> dig, slice<byte> p);

//go:noescape
internal static partial void blockAMD64(ж<digest> dig, slice<byte> p);

internal static bool useAVX2 = cpu.X86.HasAVX2 && cpu.X86.HasBMI1 && cpu.X86.HasBMI2;

internal static void block(ж<digest> Ꮡdig, slice<byte> p) {
    ref var dig = ref Ꮡdig.val;

    if (useAVX2 && len(p) >= 256){
        // blockAVX2 calculates sha1 for 2 block per iteration
        // it also interleaves precalculation for next block.
        // So it may read up-to 192 bytes past end of p
        // We may add checks inside blockAVX2, but this will
        // just turn it into a copy of blockAMD64,
        // so call it directly, instead.
        nint safeLen = len(p) - 128;
        if (safeLen % 128 != 0) {
            safeLen -= 64;
        }
        blockAVX2(Ꮡdig, p[..(int)(safeLen)]);
        blockAMD64(Ꮡdig, p[(int)(safeLen)..]);
    } else {
        blockAMD64(Ꮡdig, p);
    }
}

} // end sha1_package
