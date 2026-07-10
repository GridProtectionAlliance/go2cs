// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64 || arm64 || loong64 || mips64 || mips64le || ppc64 || ppc64le || riscv64 || s390x || wasm
namespace go;

using goarch = @internal.goarch_package;
using goos = @internal.goos_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

internal static readonly UntypedInt addrBits = 48;
internal static readonly UntypedInt tagBits = /* 64 - addrBits + 3 */ 19;
internal static readonly UntypedInt aixAddrBits = 57;
internal static readonly UntypedInt aixTagBits = /* 64 - aixAddrBits + 3 */ 10;
internal static readonly UntypedInt riscv64AddrBits = 56;
internal static readonly UntypedInt riscv64TagBits = /* 64 - riscv64AddrBits + 3 */ 11;

// The number of bits stored in the numeric tag of a taggedPointer
internal static readonly UntypedInt taggedPointerBits = /* (goos.IsAix * aixTagBits) + (goarch.IsRiscv64 * riscv64TagBits) + ((1 - goos.IsAix) * (1 - goarch.IsRiscv64) * tagBits) */ 19;

// taggedPointerPack created a taggedPointer from a pointer and a tag.
// Tag bits that don't fit in the result are discarded.
internal static taggedPointer taggedPointerPack(@unsafe.Pointer ptr, uintptr tag) {
    if (GOOS == "aix"u8) {
        if (GOARCH != "ppc64"u8) {
            @throw("check this code for aix on non-ppc64"u8);
        }
        return ((taggedPointer)((uint64)(((uint64)(uintptr)ptr << (int)((64 - aixAddrBits))) | (uint64)((uintptr)(tag & (uintptr)((1 << (int)(aixTagBits)) - 1))))));
    }
    if (GOARCH == "riscv64"u8) {
        return ((taggedPointer)((uint64)(((uint64)(uintptr)ptr << (int)((64 - riscv64AddrBits))) | (uint64)((uintptr)(tag & (uintptr)((1 << (int)(riscv64TagBits)) - 1))))));
    }
    return ((taggedPointer)((uint64)(((uint64)(uintptr)ptr << (int)((64 - addrBits))) | (uint64)((uintptr)(tag & (uintptr)((1 << (int)(tagBits)) - 1))))));
}

// Pointer returns the pointer from a taggedPointer.
internal static @unsafe.Pointer pointer(this taggedPointer tp) {
    if (GOARCH == "amd64"u8) {
        // amd64 systems can place the stack above the VA hole, so we need to sign extend
        // val before unpacking.
        return (@unsafe.Pointer)(uintptr)((((int64)(uint64)tp >> (int)(tagBits)) << (int)(3)));
    }
    if (GOOS == "aix"u8) {
        return (@unsafe.Pointer)(uintptr)(uint64)((taggedPointer)((((tp >> (int)(aixTagBits)) << (int)(3))) | (taggedPointer)((uint64)0xa << (int)(56))));
    }
    if (GOARCH == "riscv64"u8) {
        return (@unsafe.Pointer)(uintptr)(uint64)(((tp >> (int)(riscv64TagBits)) << (int)(3)));
    }
    return (@unsafe.Pointer)(uintptr)(uint64)(((tp >> (int)(tagBits)) << (int)(3)));
}

// Tag returns the tag from a taggedPointer.
internal static uintptr tag(this taggedPointer tp) {
    return (uintptr)(uint64)((taggedPointer)(tp & (uint64)(((1 << (int)(taggedPointerBits)) - 1))));
}

} // end runtime_package
