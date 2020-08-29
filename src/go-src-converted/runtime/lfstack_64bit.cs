// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64 arm64 mips64 mips64le ppc64 ppc64le s390x

// package runtime -- go2cs converted at 2020 August 29 08:17:25 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\lfstack_64bit.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
 
        // addrBits is the number of bits needed to represent a virtual address.
        //
        // In Linux the user address space for each architecture is limited as
        // follows (taken from the processor.h file for the architecture):
        //
        // Architecture  Name              Maximum Value (exclusive)
        // ---------------------------------------------------------------------
        // arm64         TASK_SIZE_64      Depends on configuration.
        // ppc64{,le}    TASK_SIZE_USER64  0x400000000000UL (46 bit addresses)
        // mips64{,le}   TASK_SIZE64       0x010000000000UL (40 bit addresses)
        // s390x         TASK_SIZE         0x020000000000UL (41 bit addresses)
        //
        // These values may increase over time.
        //
        // On AMD64, virtual addresses are 48-bit numbers sign extended to 64.
        // We shift the address left 16 to eliminate the sign extended part and make
        // room in the bottom for the count.
        private static readonly long addrBits = 48L; 

        // In addition to the 16 bits taken from the top, we can take 3 from the
        // bottom, because node must be pointer-aligned, giving a total of 19 bits
        // of count.
        private static readonly long cntBits = 64L - addrBits + 3L;

        private static ulong lfstackPack(ref lfnode node, System.UIntPtr cnt)
        {
            return uint64(uintptr(@unsafe.Pointer(node))) << (int)((64L - addrBits)) | uint64(cnt & (1L << (int)(cntBits) - 1L));
        }

        private static ref lfnode lfstackUnpack(ulong val)
        {
            if (GOARCH == "amd64")
            { 
                // amd64 systems can place the stack above the VA hole, so we need to sign extend
                // val before unpacking.
                return (lfnode.Value)(@unsafe.Pointer(uintptr(int64(val) >> (int)(cntBits) << (int)(3L))));
            }
            return (lfnode.Value)(@unsafe.Pointer(uintptr(val >> (int)(cntBits) << (int)(3L))));
        }
    }
}
