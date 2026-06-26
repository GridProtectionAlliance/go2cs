// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !purego
namespace go.crypto;

using cpu = @internal.cpu_package;
using @internal;

partial class sha512_package {

//go:noescape
internal static partial void blockAVX2(ж<digest> dig, slice<byte> p);

//go:noescape
internal static partial void blockAMD64(ж<digest> dig, slice<byte> p);

internal static bool useAVX2 = cpu.X86.HasAVX2 && cpu.X86.HasBMI1 && cpu.X86.HasBMI2;

internal static void block(ж<digest> Ꮡdig, slice<byte> p) {
    ref var dig = ref Ꮡdig.val;

    if (useAVX2){
        blockAVX2(Ꮡdig, p);
    } else {
        blockAMD64(Ꮡdig, p);
    }
}

} // end sha512_package
