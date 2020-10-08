// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64 !darwin,arm64 mips64 mips64le ppc64 ppc64le riscv64 s390x

// See mpagealloc_32bit.go for why darwin/arm64 is excluded here.

// package runtime -- go2cs converted at 2020 October 08 03:21:19 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mpagealloc_64bit.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
 
        // The number of levels in the radix tree.
        private static readonly long summaryLevels = (long)5L; 

        // Constants for testing.
        private static readonly long pageAlloc32Bit = (long)0L;
        private static readonly long pageAlloc64Bit = (long)1L; 

        // Number of bits needed to represent all indices into the L1 of the
        // chunks map.
        //
        // See (*pageAlloc).chunks for more details. Update the documentation
        // there should this number change.
        private static readonly long pallocChunksL1Bits = (long)13L;


        // levelBits is the number of bits in the radix for a given level in the super summary
        // structure.
        //
        // The sum of all the entries of levelBits should equal heapAddrBits.
        private static array<ulong> levelBits = new array<ulong>(new ulong[] { summaryL0Bits, summaryLevelBits, summaryLevelBits, summaryLevelBits, summaryLevelBits });

        // levelShift is the number of bits to shift to acquire the radix for a given level
        // in the super summary structure.
        //
        // With levelShift, one can compute the index of the summary at level l related to a
        // pointer p by doing:
        //   p >> levelShift[l]
        private static array<ulong> levelShift = new array<ulong>(new ulong[] { heapAddrBits-summaryL0Bits, heapAddrBits-summaryL0Bits-1*summaryLevelBits, heapAddrBits-summaryL0Bits-2*summaryLevelBits, heapAddrBits-summaryL0Bits-3*summaryLevelBits, heapAddrBits-summaryL0Bits-4*summaryLevelBits });

        // levelLogPages is log2 the maximum number of runtime pages in the address space
        // a summary in the given level represents.
        //
        // The leaf level always represents exactly log2 of 1 chunk's worth of pages.
        private static array<ulong> levelLogPages = new array<ulong>(new ulong[] { logPallocChunkPages+4*summaryLevelBits, logPallocChunkPages+3*summaryLevelBits, logPallocChunkPages+2*summaryLevelBits, logPallocChunkPages+1*summaryLevelBits, logPallocChunkPages });

        // sysInit performs architecture-dependent initialization of fields
        // in pageAlloc. pageAlloc should be uninitialized except for sysStat
        // if any runtime statistic should be updated.
        private static void sysInit(this ptr<pageAlloc> _addr_s)
        {
            ref pageAlloc s = ref _addr_s.val;
 
            // Reserve memory for each level. This will get mapped in
            // as R/W by setArenas.
            foreach (var (l, shift) in levelShift)
            {
                long entries = 1L << (int)((heapAddrBits - shift)); 

                // Reserve b bytes of memory anywhere in the address space.
                var b = alignUp(uintptr(entries) * pallocSumBytes, physPageSize);
                var r = sysReserve(null, b);
                if (r == null)
                {
                    throw("failed to reserve page summary memory");
                } 

                // Put this reservation into a slice.
                ref notInHeapSlice sl = ref heap(new notInHeapSlice((*notInHeap)(r),0,entries), out ptr<notInHeapSlice> _addr_sl);
                s.summary[l] = new ptr<ptr<ptr<slice<pallocSum>>>>(@unsafe.Pointer(_addr_sl));

            }

        }

        // sysGrow performs architecture-dependent operations on heap
        // growth for the page allocator, such as mapping in new memory
        // for summaries. It also updates the length of the slices in
        // s.summary.
        //
        // base is the base of the newly-added heap memory and limit is
        // the first address past the end of the newly-added heap memory.
        // Both must be aligned to pallocChunkBytes.
        //
        // The caller must update s.start and s.end after calling sysGrow.
        private static void sysGrow(this ptr<pageAlloc> _addr_s, System.UIntPtr @base, System.UIntPtr limit)
        {
            ref pageAlloc s = ref _addr_s.val;

            if (base % pallocChunkBytes != 0L || limit % pallocChunkBytes != 0L)
            {
                print("runtime: base = ", hex(base), ", limit = ", hex(limit), "\n");
                throw("sysGrow bounds not aligned to pallocChunkBytes");
            } 

            // addrRangeToSummaryRange converts a range of addresses into a range
            // of summary indices which must be mapped to support those addresses
            // in the summary range.
            Func<long, addrRange, (long, long)> addrRangeToSummaryRange = (level, r) =>
            {
                var (sumIdxBase, sumIdxLimit) = addrsToSummaryRange(level, r.@base.addr(), r.limit.addr());
                return blockAlignSummaryRange(level, sumIdxBase, sumIdxLimit);
            } 

            // summaryRangeToSumAddrRange converts a range of indices in any
            // level of s.summary into page-aligned addresses which cover that
            // range of indices.
; 

            // summaryRangeToSumAddrRange converts a range of indices in any
            // level of s.summary into page-aligned addresses which cover that
            // range of indices.
            Func<long, long, long, addrRange> summaryRangeToSumAddrRange = (level, sumIdxBase, sumIdxLimit) =>
            {
                var baseOffset = alignDown(uintptr(sumIdxBase) * pallocSumBytes, physPageSize);
                var limitOffset = alignUp(uintptr(sumIdxLimit) * pallocSumBytes, physPageSize);
                var @base = @unsafe.Pointer(_addr_s.summary[level][0L]);
                return new addrRange(offAddr{uintptr(add(base,baseOffset))},offAddr{uintptr(add(base,limitOffset))},);
            } 

            // addrRangeToSumAddrRange is a convienience function that converts
            // an address range r to the address range of the given summary level
            // that stores the summaries for r.
; 

            // addrRangeToSumAddrRange is a convienience function that converts
            // an address range r to the address range of the given summary level
            // that stores the summaries for r.
            Func<long, addrRange, addrRange> addrRangeToSumAddrRange = (level, r) =>
            {
                (sumIdxBase, sumIdxLimit) = addrRangeToSummaryRange(level, r);
                return summaryRangeToSumAddrRange(level, sumIdxBase, sumIdxLimit);
            } 

            // Find the first inUse index which is strictly greater than base.
            //
            // Because this function will never be asked remap the same memory
            // twice, this index is effectively the index at which we would insert
            // this new growth, and base will never overlap/be contained within
            // any existing range.
            //
            // This will be used to look at what memory in the summary array is already
            // mapped before and after this new range.
; 

            // Find the first inUse index which is strictly greater than base.
            //
            // Because this function will never be asked remap the same memory
            // twice, this index is effectively the index at which we would insert
            // this new growth, and base will never overlap/be contained within
            // any existing range.
            //
            // This will be used to look at what memory in the summary array is already
            // mapped before and after this new range.
            var inUseIndex = s.inUse.findSucc(base); 

            // Walk up the radix tree and map summaries in as needed.
            foreach (var (l) in s.summary)
            { 
                // Figure out what part of the summary array this new address space needs.
                var (needIdxBase, needIdxLimit) = addrRangeToSummaryRange(l, makeAddrRange(base, limit)); 

                // Update the summary slices with a new upper-bound. This ensures
                // we get tight bounds checks on at least the top bound.
                //
                // We must do this regardless of whether we map new memory.
                if (needIdxLimit > len(s.summary[l]))
                {
                    s.summary[l] = s.summary[l][..needIdxLimit];
                } 

                // Compute the needed address range in the summary array for level l.
                var need = summaryRangeToSumAddrRange(l, needIdxBase, needIdxLimit); 

                // Prune need down to what needs to be newly mapped. Some parts of it may
                // already be mapped by what inUse describes due to page alignment requirements
                // for mapping. prune's invariants are guaranteed by the fact that this
                // function will never be asked to remap the same memory twice.
                if (inUseIndex > 0L)
                {
                    need = need.subtract(addrRangeToSumAddrRange(l, s.inUse.ranges[inUseIndex - 1L]));
                }

                if (inUseIndex < len(s.inUse.ranges))
                {
                    need = need.subtract(addrRangeToSumAddrRange(l, s.inUse.ranges[inUseIndex]));
                } 
                // It's possible that after our pruning above, there's nothing new to map.
                if (need.size() == 0L)
                {
                    continue;
                } 

                // Map and commit need.
                sysMap(@unsafe.Pointer(need.@base.addr()), need.size(), s.sysStat);
                sysUsed(@unsafe.Pointer(need.@base.addr()), need.size());

            }

        }
    }
}
