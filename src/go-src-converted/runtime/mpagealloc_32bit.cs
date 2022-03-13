// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build 386 || arm || mips || mipsle || wasm || (ios && arm64)
// +build 386 arm mips mipsle wasm ios,arm64

// wasm is a treated as a 32-bit architecture for the purposes of the page
// allocator, even though it has 64-bit pointers. This is because any wasm
// pointer always has its top 32 bits as zero, so the effective heap address
// space is only 2^32 bytes in size (see heapAddrBits).

// ios/arm64 is treated as a 32-bit architecture for the purposes of the
// page allocator, even though it has 64-bit pointers and a 33-bit address
// space (see heapAddrBits). The 33 bit address space cannot be rounded up
// to 64 bits because there are too many summary levels to fit in just 33
// bits.

// package runtime -- go2cs converted at 2022 March 13 05:25:48 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mpagealloc_32bit.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class runtime_package {

 
// The number of levels in the radix tree.
private static readonly nint summaryLevels = 4; 

// Constants for testing.
private static readonly nint pageAlloc32Bit = 1;
private static readonly nint pageAlloc64Bit = 0; 

// Number of bits needed to represent all indices into the L1 of the
// chunks map.
//
// See (*pageAlloc).chunks for more details. Update the documentation
// there should this number change.
private static readonly nint pallocChunksL1Bits = 0;

// See comment in mpagealloc_64bit.go.
private static array<nuint> levelBits = new array<nuint>(new nuint[] { summaryL0Bits, summaryLevelBits, summaryLevelBits, summaryLevelBits });

// See comment in mpagealloc_64bit.go.
private static array<nuint> levelShift = new array<nuint>(new nuint[] { heapAddrBits-summaryL0Bits, heapAddrBits-summaryL0Bits-1*summaryLevelBits, heapAddrBits-summaryL0Bits-2*summaryLevelBits, heapAddrBits-summaryL0Bits-3*summaryLevelBits });

// See comment in mpagealloc_64bit.go.
private static array<nuint> levelLogPages = new array<nuint>(new nuint[] { logPallocChunkPages+3*summaryLevelBits, logPallocChunkPages+2*summaryLevelBits, logPallocChunkPages+1*summaryLevelBits, logPallocChunkPages });

// See mpagealloc_64bit.go for details.
private static void sysInit(this ptr<pageAlloc> _addr_p) {
    ref pageAlloc p = ref _addr_p.val;
 
    // Calculate how much memory all our entries will take up.
    //
    // This should be around 12 KiB or less.
    var totalSize = uintptr(0);
    {
        nint l__prev1 = l;

        for (nint l = 0; l < summaryLevels; l++) {
            totalSize += (uintptr(1) << (int)((heapAddrBits - levelShift[l]))) * pallocSumBytes;
        }

        l = l__prev1;
    }
    totalSize = alignUp(totalSize, physPageSize); 

    // Reserve memory for all levels in one go. There shouldn't be much for 32-bit.
    var reservation = sysReserve(null, totalSize);
    if (reservation == null) {
        throw("failed to reserve page summary memory");
    }
    sysMap(reservation, totalSize, p.sysStat);
    sysUsed(reservation, totalSize); 

    // Iterate over the reservation and cut it up into slices.
    //
    // Maintain i as the byte offset from reservation where
    // the new slice should start.
    {
        nint l__prev1 = l;

        foreach (var (__l, __shift) in levelShift) {
            l = __l;
            shift = __shift;
            nint entries = 1 << (int)((heapAddrBits - shift)); 

            // Put this reservation into a slice.
            ref notInHeapSlice sl = ref heap(new notInHeapSlice((*notInHeap)(reservation),0,entries), out ptr<notInHeapSlice> _addr_sl);
            p.summary[l] = new ptr<ptr<ptr<slice<pallocSum>>>>(@unsafe.Pointer(_addr_sl));

            reservation = add(reservation, uintptr(entries) * pallocSumBytes);
        }
        l = l__prev1;
    }
}

// See mpagealloc_64bit.go for details.
private static void sysGrow(this ptr<pageAlloc> _addr_p, System.UIntPtr @base, System.UIntPtr limit) {
    ref pageAlloc p = ref _addr_p.val;

    if (base % pallocChunkBytes != 0 || limit % pallocChunkBytes != 0) {
        print("runtime: base = ", hex(base), ", limit = ", hex(limit), "\n");
        throw("sysGrow bounds not aligned to pallocChunkBytes");
    }
    for (var l = len(p.summary) - 1; l >= 0; l--) { 
        // Figure out what part of the summary array this new address space needs.
        // Note that we need to align the ranges to the block width (1<<levelBits[l])
        // at this level because the full block is needed to compute the summary for
        // the next level.
        var (lo, hi) = addrsToSummaryRange(l, base, limit);
        _, hi = blockAlignSummaryRange(l, lo, hi);
        if (hi > len(p.summary[l])) {
            p.summary[l] = p.summary[l][..(int)hi];
        }
    }
}

} // end runtime_package
