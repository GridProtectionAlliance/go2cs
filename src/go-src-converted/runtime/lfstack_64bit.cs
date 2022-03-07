// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64 || arm64 || mips64 || mips64le || ppc64 || ppc64le || riscv64 || s390x || wasm
// +build amd64 arm64 mips64 mips64le ppc64 ppc64le riscv64 s390x wasm

// package runtime -- go2cs converted at 2022 March 06 22:08:48 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\lfstack_64bit.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

 
// addrBits is the number of bits needed to represent a virtual address.
//
// See heapAddrBits for a table of address space sizes on
// various architectures. 48 bits is enough for all
// architectures except s390x.
//
// On AMD64, virtual addresses are 48-bit (or 57-bit) numbers sign extended to 64.
// We shift the address left 16 to eliminate the sign extended part and make
// room in the bottom for the count.
//
// On s390x, virtual addresses are 64-bit. There's not much we
// can do about this, so we just hope that the kernel doesn't
// get to really high addresses and panic if it does.
private static readonly nint addrBits = 48; 

// In addition to the 16 bits taken from the top, we can take 3 from the
// bottom, because node must be pointer-aligned, giving a total of 19 bits
// of count.
private static readonly nint cntBits = 64 - addrBits + 3; 

// On AIX, 64-bit addresses are split into 36-bit segment number and 28-bit
// offset in segment.  Segment numbers in the range 0x0A0000000-0x0AFFFFFFF(LSA)
// are available for mmap.
// We assume all lfnode addresses are from memory allocated with mmap.
// We use one bit to distinguish between the two ranges.
private static readonly nint aixAddrBits = 57;
private static readonly nint aixCntBits = 64 - aixAddrBits + 3;


private static ulong lfstackPack(ptr<lfnode> _addr_node, System.UIntPtr cnt) {
    ref lfnode node = ref _addr_node.val;

    if (GOARCH == "ppc64" && GOOS == "aix") {
        return uint64(uintptr(@unsafe.Pointer(node))) << (int)((64 - aixAddrBits)) | uint64(cnt & (1 << (int)(aixCntBits) - 1));
    }
    return uint64(uintptr(@unsafe.Pointer(node))) << (int)((64 - addrBits)) | uint64(cnt & (1 << (int)(cntBits) - 1));

}

private static ptr<lfnode> lfstackUnpack(ulong val) {
    if (GOARCH == "amd64") { 
        // amd64 systems can place the stack above the VA hole, so we need to sign extend
        // val before unpacking.
        return _addr_(lfnode.val)(@unsafe.Pointer(uintptr(int64(val) >> (int)(cntBits) << 3)))!;

    }
    if (GOARCH == "ppc64" && GOOS == "aix") {
        return _addr_(lfnode.val)(@unsafe.Pointer(uintptr((val >> (int)(aixCntBits) << 3) | 0xa << 56)))!;
    }
    return _addr_(lfnode.val)(@unsafe.Pointer(uintptr(val >> (int)(cntBits) << 3)))!;

}

} // end runtime_package
