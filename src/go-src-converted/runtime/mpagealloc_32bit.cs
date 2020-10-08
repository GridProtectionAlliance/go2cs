// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build 386 arm mips mipsle wasm darwin,arm64

// wasm is a treated as a 32-bit architecture for the purposes of the page
// allocator, even though it has 64-bit pointers. This is because any wasm
// pointer always has its top 32 bits as zero, so the effective heap address
// space is only 2^32 bytes in size (see heapAddrBits).

// darwin/arm64 is treated as a 32-bit architecture for the purposes of the
// page allocator, even though it has 64-bit pointers and a 33-bit address
// space (see heapAddrBits). The 33 bit address space cannot be rounded up
// to 64 bits because there are too many summary levels to fit in just 33
// bits.

// package runtime -- go2cs converted at 2020 October 08 03:21:18 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mpagealloc_32bit.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
 
        // The number of levels in the radix tree.
        private static readonly long summaryLevels = (long)4L; 

        // Constants for testing.
        private static readonly long pageAlloc32Bit = (long)1L;
        private static readonly long pageAlloc64Bit = (long)0L; 

        // Number of bits needed to represent all indices into the L1 of the
        // chunks map.
        //
        // See (*pageAlloc).chunks for more details. Update the documentation
        // there should this number change.
        private static readonly long pallocChunksL1Bits = (long)0L;


        // See comment in mpagealloc_64bit.go.
        private static array<ulong> levelBits = new array<ulong>(new ulong[] { summaryL0Bits, summaryLevelBits, summaryLevelBits, summaryLevelBits });

        // See comment in mpagealloc_64bit.go.
        private static array<ulong> levelShift = new array<ulong>(new ulong[] { heapAddrBits-summaryL0Bits, heapAddrBits-summaryL0Bits-1*summaryLevelBits, heapAddrBits-summaryL0Bits-2*summaryLevelBits, heapAddrBits-summaryL0Bits-3*summaryLevelBits });

        // See comment in mpagealloc_64bit.go.
        private static array<ulong> levelLogPages = new array<ulong>(new ulong[] { logPallocChunkPages+3*summaryLevelBits, logPallocChunkPages+2*summaryLevelBits, logPallocChunkPages+1*summaryLevelBits, logPallocChunkPages });

        // See mpagealloc_64bit.go for details.
        private static void sysInit(this ptr<pageAlloc> _addr_s)
        {
            ref pageAlloc s = ref _addr_s.val;
 
            // Calculate how much memory all our entries will take up.
            //
            // This should be around 12 KiB or less.
            var totalSize = uintptr(0L);
            {
                long l__prev1 = l;

                for (long l = 0L; l < summaryLevels; l++)
                {
                    totalSize += (uintptr(1L) << (int)((heapAddrBits - levelShift[l]))) * pallocSumBytes;
                }


                l = l__prev1;
            }
            totalSize = alignUp(totalSize, physPageSize); 

            // Reserve memory for all levels in one go. There shouldn't be much for 32-bit.
            var reservation = sysReserve(null, totalSize);
            if (reservation == null)
            {
                throw("failed to reserve page summary memory");
            } 
            // There isn't much. Just map it and mark it as used immediately.
            sysMap(reservation, totalSize, s.sysStat);
            sysUsed(reservation, totalSize); 

            // Iterate over the reservation and cut it up into slices.
            //
            // Maintain i as the byte offset from reservation where
            // the new slice should start.
            {
                long l__prev1 = l;

                foreach (var (__l, __shift) in levelShift)
                {
                    l = __l;
                    shift = __shift;
                    long entries = 1L << (int)((heapAddrBits - shift)); 

                    // Put this reservation into a slice.
                    ref notInHeapSlice sl = ref heap(new notInHeapSlice((*notInHeap)(reservation),0,entries), out ptr<notInHeapSlice> _addr_sl);
                    s.summary[l] = new ptr<ptr<ptr<slice<pallocSum>>>>(@unsafe.Pointer(_addr_sl));

                    reservation = add(reservation, uintptr(entries) * pallocSumBytes);

                }

                l = l__prev1;
            }
        }

        // See mpagealloc_64bit.go for details.
        private static void sysGrow(this ptr<pageAlloc> _addr_s, System.UIntPtr @base, System.UIntPtr limit)
        {
            ref pageAlloc s = ref _addr_s.val;

            if (base % pallocChunkBytes != 0L || limit % pallocChunkBytes != 0L)
            {
                print("runtime: base = ", hex(base), ", limit = ", hex(limit), "\n");
                throw("sysGrow bounds not aligned to pallocChunkBytes");
            } 

            // Walk up the tree and update the summary slices.
            for (var l = len(s.summary) - 1L; l >= 0L; l--)
            { 
                // Figure out what part of the summary array this new address space needs.
                // Note that we need to align the ranges to the block width (1<<levelBits[l])
                // at this level because the full block is needed to compute the summary for
                // the next level.
                var (lo, hi) = addrsToSummaryRange(l, base, limit);
                _, hi = blockAlignSummaryRange(l, lo, hi);
                if (hi > len(s.summary[l]))
                {
                    s.summary[l] = s.summary[l][..hi];
                }

            }


        }
    }
}
