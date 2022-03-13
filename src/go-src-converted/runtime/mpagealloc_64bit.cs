// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64 || (!ios && arm64) || mips64 || mips64le || ppc64 || ppc64le || riscv64 || s390x
// +build amd64 !ios,arm64 mips64 mips64le ppc64 ppc64le riscv64 s390x

// See mpagealloc_32bit.go for why ios/arm64 is excluded here.

// package runtime -- go2cs converted at 2022 March 13 05:25:49 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mpagealloc_64bit.go
namespace go;

using @unsafe = @unsafe_package;
using System;

public static partial class runtime_package {

 
// The number of levels in the radix tree.
private static readonly nint summaryLevels = 5; 

// Constants for testing.
private static readonly nint pageAlloc32Bit = 0;
private static readonly nint pageAlloc64Bit = 1; 

// Number of bits needed to represent all indices into the L1 of the
// chunks map.
//
// See (*pageAlloc).chunks for more details. Update the documentation
// there should this number change.
private static readonly nint pallocChunksL1Bits = 13;

// levelBits is the number of bits in the radix for a given level in the super summary
// structure.
//
// The sum of all the entries of levelBits should equal heapAddrBits.
private static array<nuint> levelBits = new array<nuint>(new nuint[] { summaryL0Bits, summaryLevelBits, summaryLevelBits, summaryLevelBits, summaryLevelBits });

// levelShift is the number of bits to shift to acquire the radix for a given level
// in the super summary structure.
//
// With levelShift, one can compute the index of the summary at level l related to a
// pointer p by doing:
//   p >> levelShift[l]
private static array<nuint> levelShift = new array<nuint>(new nuint[] { heapAddrBits-summaryL0Bits, heapAddrBits-summaryL0Bits-1*summaryLevelBits, heapAddrBits-summaryL0Bits-2*summaryLevelBits, heapAddrBits-summaryL0Bits-3*summaryLevelBits, heapAddrBits-summaryL0Bits-4*summaryLevelBits });

// levelLogPages is log2 the maximum number of runtime pages in the address space
// a summary in the given level represents.
//
// The leaf level always represents exactly log2 of 1 chunk's worth of pages.
private static array<nuint> levelLogPages = new array<nuint>(new nuint[] { logPallocChunkPages+4*summaryLevelBits, logPallocChunkPages+3*summaryLevelBits, logPallocChunkPages+2*summaryLevelBits, logPallocChunkPages+1*summaryLevelBits, logPallocChunkPages });

// sysInit performs architecture-dependent initialization of fields
// in pageAlloc. pageAlloc should be uninitialized except for sysStat
// if any runtime statistic should be updated.
private static void sysInit(this ptr<pageAlloc> _addr_p) {
    ref pageAlloc p = ref _addr_p.val;
 
    // Reserve memory for each level. This will get mapped in
    // as R/W by setArenas.
    foreach (var (l, shift) in levelShift) {
        nint entries = 1 << (int)((heapAddrBits - shift)); 

        // Reserve b bytes of memory anywhere in the address space.
        var b = alignUp(uintptr(entries) * pallocSumBytes, physPageSize);
        var r = sysReserve(null, b);
        if (r == null) {
            throw("failed to reserve page summary memory");
        }
        ref notInHeapSlice sl = ref heap(new notInHeapSlice((*notInHeap)(r),0,entries), out ptr<notInHeapSlice> _addr_sl);
        p.summary[l] = new ptr<ptr<ptr<slice<pallocSum>>>>(@unsafe.Pointer(_addr_sl));
    }
}

// sysGrow performs architecture-dependent operations on heap
// growth for the page allocator, such as mapping in new memory
// for summaries. It also updates the length of the slices in
// [.summary.
//
// base is the base of the newly-added heap memory and limit is
// the first address past the end of the newly-added heap memory.
// Both must be aligned to pallocChunkBytes.
//
// The caller must update p.start and p.end after calling sysGrow.
private static void sysGrow(this ptr<pageAlloc> _addr_p, System.UIntPtr @base, System.UIntPtr limit) {
    ref pageAlloc p = ref _addr_p.val;

    if (base % pallocChunkBytes != 0 || limit % pallocChunkBytes != 0) {
        print("runtime: base = ", hex(base), ", limit = ", hex(limit), "\n");
        throw("sysGrow bounds not aligned to pallocChunkBytes");
    }
    Func<nint, addrRange, (nint, nint)> addrRangeToSummaryRange = (level, r) => {
        var (sumIdxBase, sumIdxLimit) = addrsToSummaryRange(level, r.@base.addr(), r.limit.addr());
        return blockAlignSummaryRange(level, sumIdxBase, sumIdxLimit);
    }; 

    // summaryRangeToSumAddrRange converts a range of indices in any
    // level of p.summary into page-aligned addresses which cover that
    // range of indices.
    Func<nint, nint, nint, addrRange> summaryRangeToSumAddrRange = (level, sumIdxBase, sumIdxLimit) => {
        var baseOffset = alignDown(uintptr(sumIdxBase) * pallocSumBytes, physPageSize);
        var limitOffset = alignUp(uintptr(sumIdxLimit) * pallocSumBytes, physPageSize);
        var @base = @unsafe.Pointer(_addr_p.summary[level][0]);
        return new addrRange(offAddr{uintptr(add(base,baseOffset))},offAddr{uintptr(add(base,limitOffset))},);
    }; 

    // addrRangeToSumAddrRange is a convienience function that converts
    // an address range r to the address range of the given summary level
    // that stores the summaries for r.
    Func<nint, addrRange, addrRange> addrRangeToSumAddrRange = (level, r) => {
        (sumIdxBase, sumIdxLimit) = addrRangeToSummaryRange(level, r);
        return summaryRangeToSumAddrRange(level, sumIdxBase, sumIdxLimit);
    }; 

    // Find the first inUse index which is strictly greater than base.
    //
    // Because this function will never be asked remap the same memory
    // twice, this index is effectively the index at which we would insert
    // this new growth, and base will never overlap/be contained within
    // any existing range.
    //
    // This will be used to look at what memory in the summary array is already
    // mapped before and after this new range.
    var inUseIndex = p.inUse.findSucc(base); 

    // Walk up the radix tree and map summaries in as needed.
    foreach (var (l) in p.summary) { 
        // Figure out what part of the summary array this new address space needs.
        var (needIdxBase, needIdxLimit) = addrRangeToSummaryRange(l, makeAddrRange(base, limit)); 

        // Update the summary slices with a new upper-bound. This ensures
        // we get tight bounds checks on at least the top bound.
        //
        // We must do this regardless of whether we map new memory.
        if (needIdxLimit > len(p.summary[l])) {
            p.summary[l] = p.summary[l][..(int)needIdxLimit];
        }
        var need = summaryRangeToSumAddrRange(l, needIdxBase, needIdxLimit); 

        // Prune need down to what needs to be newly mapped. Some parts of it may
        // already be mapped by what inUse describes due to page alignment requirements
        // for mapping. prune's invariants are guaranteed by the fact that this
        // function will never be asked to remap the same memory twice.
        if (inUseIndex > 0) {
            need = need.subtract(addrRangeToSumAddrRange(l, p.inUse.ranges[inUseIndex - 1]));
        }
        if (inUseIndex < len(p.inUse.ranges)) {
            need = need.subtract(addrRangeToSumAddrRange(l, p.inUse.ranges[inUseIndex]));
        }
        if (need.size() == 0) {
            continue;
        }
        sysMap(@unsafe.Pointer(need.@base.addr()), need.size(), p.sysStat);
        sysUsed(@unsafe.Pointer(need.@base.addr()), need.size());
    }
}

} // end runtime_package
