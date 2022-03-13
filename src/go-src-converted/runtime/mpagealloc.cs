// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Page allocator.
//
// The page allocator manages mapped pages (defined by pageSize, NOT
// physPageSize) for allocation and re-use. It is embedded into mheap.
//
// Pages are managed using a bitmap that is sharded into chunks.
// In the bitmap, 1 means in-use, and 0 means free. The bitmap spans the
// process's address space. Chunks are managed in a sparse-array-style structure
// similar to mheap.arenas, since the bitmap may be large on some systems.
//
// The bitmap is efficiently searched by using a radix tree in combination
// with fast bit-wise intrinsics. Allocation is performed using an address-ordered
// first-fit approach.
//
// Each entry in the radix tree is a summary that describes three properties of
// a particular region of the address space: the number of contiguous free pages
// at the start and end of the region it represents, and the maximum number of
// contiguous free pages found anywhere in that region.
//
// Each level of the radix tree is stored as one contiguous array, which represents
// a different granularity of subdivision of the processes' address space. Thus, this
// radix tree is actually implicit in these large arrays, as opposed to having explicit
// dynamically-allocated pointer-based node structures. Naturally, these arrays may be
// quite large for system with large address spaces, so in these cases they are mapped
// into memory as needed. The leaf summaries of the tree correspond to a bitmap chunk.
//
// The root level (referred to as L0 and index 0 in pageAlloc.summary) has each
// summary represent the largest section of address space (16 GiB on 64-bit systems),
// with each subsequent level representing successively smaller subsections until we
// reach the finest granularity at the leaves, a chunk.
//
// More specifically, each summary in each level (except for leaf summaries)
// represents some number of entries in the following level. For example, each
// summary in the root level may represent a 16 GiB region of address space,
// and in the next level there could be 8 corresponding entries which represent 2
// GiB subsections of that 16 GiB region, each of which could correspond to 8
// entries in the next level which each represent 256 MiB regions, and so on.
//
// Thus, this design only scales to heaps so large, but can always be extended to
// larger heaps by simply adding levels to the radix tree, which mostly costs
// additional virtual address space. The choice of managing large arrays also means
// that a large amount of virtual address space may be reserved by the runtime.

// package runtime -- go2cs converted at 2022 March 13 05:25:48 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mpagealloc.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using @unsafe = @unsafe_package;
using System;

public static partial class runtime_package {

 
// The size of a bitmap chunk, i.e. the amount of bits (that is, pages) to consider
// in the bitmap at once.
private static readonly nint pallocChunkPages = 1 << (int)(logPallocChunkPages);
private static readonly var pallocChunkBytes = pallocChunkPages * pageSize;
private static readonly nint logPallocChunkPages = 9;
private static readonly var logPallocChunkBytes = logPallocChunkPages + pageShift; 

// The number of radix bits for each level.
//
// The value of 3 is chosen such that the block of summaries we need to scan at
// each level fits in 64 bytes (2^3 summaries * 8 bytes per summary), which is
// close to the L1 cache line width on many systems. Also, a value of 3 fits 4 tree
// levels perfectly into the 21-bit pallocBits summary field at the root level.
//
// The following equation explains how each of the constants relate:
// summaryL0Bits + (summaryLevels-1)*summaryLevelBits + logPallocChunkBytes = heapAddrBits
//
// summaryLevels is an architecture-dependent value defined in mpagealloc_*.go.
private static readonly nint summaryLevelBits = 3;
private static readonly var summaryL0Bits = heapAddrBits - logPallocChunkBytes - (summaryLevels - 1) * summaryLevelBits; 

// pallocChunksL2Bits is the number of bits of the chunk index number
// covered by the second level of the chunks map.
//
// See (*pageAlloc).chunks for more details. Update the documentation
// there should this change.
private static readonly var pallocChunksL2Bits = heapAddrBits - logPallocChunkBytes - pallocChunksL1Bits;
private static readonly var pallocChunksL1Shift = pallocChunksL2Bits;

// Maximum searchAddr value, which indicates that the heap has no free space.
//
// We alias maxOffAddr just to make it clear that this is the maximum address
// for the page allocator's search space. See maxOffAddr for details.
private static var maxSearchAddr = maxOffAddr;

// Global chunk index.
//
// Represents an index into the leaf level of the radix tree.
// Similar to arenaIndex, except instead of arenas, it divides the address
// space into chunks.
private partial struct chunkIdx { // : nuint
}

// chunkIndex returns the global index of the palloc chunk containing the
// pointer p.
private static chunkIdx chunkIndex(System.UIntPtr p) {
    return chunkIdx((p - arenaBaseOffset) / pallocChunkBytes);
}

// chunkIndex returns the base address of the palloc chunk at index ci.
private static System.UIntPtr chunkBase(chunkIdx ci) {
    return uintptr(ci) * pallocChunkBytes + arenaBaseOffset;
}

// chunkPageIndex computes the index of the page that contains p,
// relative to the chunk which contains p.
private static nuint chunkPageIndex(System.UIntPtr p) {
    return uint(p % pallocChunkBytes / pageSize);
}

// l1 returns the index into the first level of (*pageAlloc).chunks.
private static nuint l1(this chunkIdx i) {
    if (pallocChunksL1Bits == 0) { 
        // Let the compiler optimize this away if there's no
        // L1 map.
        return 0;
    }
    else
 {
        return uint(i) >> (int)(pallocChunksL1Shift);
    }
}

// l2 returns the index into the second level of (*pageAlloc).chunks.
private static nuint l2(this chunkIdx i) {
    if (pallocChunksL1Bits == 0) {
        return uint(i);
    }
    else
 {
        return uint(i) & (1 << (int)(pallocChunksL2Bits) - 1);
    }
}

// offAddrToLevelIndex converts an address in the offset address space
// to the index into summary[level] containing addr.
private static nint offAddrToLevelIndex(nint level, offAddr addr) {
    return int((addr.a - arenaBaseOffset) >> (int)(levelShift[level]));
}

// levelIndexToOffAddr converts an index into summary[level] into
// the corresponding address in the offset address space.
private static offAddr levelIndexToOffAddr(nint level, nint idx) {
    return new offAddr((uintptr(idx)<<levelShift[level])+arenaBaseOffset);
}

// addrsToSummaryRange converts base and limit pointers into a range
// of entries for the given summary level.
//
// The returned range is inclusive on the lower bound and exclusive on
// the upper bound.
private static (nint, nint) addrsToSummaryRange(nint level, System.UIntPtr @base, System.UIntPtr limit) {
    nint lo = default;
    nint hi = default;
 
    // This is slightly more nuanced than just a shift for the exclusive
    // upper-bound. Note that the exclusive upper bound may be within a
    // summary at this level, meaning if we just do the obvious computation
    // hi will end up being an inclusive upper bound. Unfortunately, just
    // adding 1 to that is too broad since we might be on the very edge of
    // of a summary's max page count boundary for this level
    // (1 << levelLogPages[level]). So, make limit an inclusive upper bound
    // then shift, then add 1, so we get an exclusive upper bound at the end.
    lo = int((base - arenaBaseOffset) >> (int)(levelShift[level]));
    hi = int(((limit - 1) - arenaBaseOffset) >> (int)(levelShift[level])) + 1;
    return ;
}

// blockAlignSummaryRange aligns indices into the given level to that
// level's block width (1 << levelBits[level]). It assumes lo is inclusive
// and hi is exclusive, and so aligns them down and up respectively.
private static (nint, nint) blockAlignSummaryRange(nint level, nint lo, nint hi) {
    nint _p0 = default;
    nint _p0 = default;

    var e = uintptr(1) << (int)(levelBits[level]);
    return (int(alignDown(uintptr(lo), e)), int(alignUp(uintptr(hi), e)));
}

private partial struct pageAlloc {
    public array<slice<pallocSum>> summary; // chunks is a slice of bitmap chunks.
//
// The total size of chunks is quite large on most 64-bit platforms
// (O(GiB) or more) if flattened, so rather than making one large mapping
// (which has problems on some platforms, even when PROT_NONE) we use a
// two-level sparse array approach similar to the arena index in mheap.
//
// To find the chunk containing a memory address `a`, do:
//   chunkOf(chunkIndex(a))
//
// Below is a table describing the configuration for chunks for various
// heapAddrBits supported by the runtime.
//
// heapAddrBits | L1 Bits | L2 Bits | L2 Entry Size
// ------------------------------------------------
// 32           | 0       | 10      | 128 KiB
// 33 (iOS)     | 0       | 11      | 256 KiB
// 48           | 13      | 13      | 1 MiB
//
// There's no reason to use the L1 part of chunks on 32-bit, the
// address space is small so the L2 is small. For platforms with a
// 48-bit address space, we pick the L1 such that the L2 is 1 MiB
// in size, which is a good balance between low granularity without
// making the impact on BSS too high (note the L1 is stored directly
// in pageAlloc).
//
// To iterate over the bitmap, use inUse to determine which ranges
// are currently available. Otherwise one might iterate over unused
// ranges.
//
// TODO(mknyszek): Consider changing the definition of the bitmap
// such that 1 means free and 0 means in-use so that summaries and
// the bitmaps align better on zero-values.
    public array<ptr<array<pallocData>>> chunks; // The address to start an allocation search with. It must never
// point to any memory that is not contained in inUse, i.e.
// inUse.contains(searchAddr.addr()) must always be true. The one
// exception to this rule is that it may take on the value of
// maxOffAddr to indicate that the heap is exhausted.
//
// We guarantee that all valid heap addresses below this value
// are allocated and not worth searching.
    public offAddr searchAddr; // start and end represent the chunk indices
// which pageAlloc knows about. It assumes
// chunks in the range [start, end) are
// currently ready to use.
    public chunkIdx start; // inUse is a slice of ranges of address space which are
// known by the page allocator to be currently in-use (passed
// to grow).
//
// This field is currently unused on 32-bit architectures but
// is harmless to track. We care much more about having a
// contiguous heap in these cases and take additional measures
// to ensure that, so in nearly all cases this should have just
// 1 element.
//
// All access is protected by the mheapLock.
    public chunkIdx end; // inUse is a slice of ranges of address space which are
// known by the page allocator to be currently in-use (passed
// to grow).
//
// This field is currently unused on 32-bit architectures but
// is harmless to track. We care much more about having a
// contiguous heap in these cases and take additional measures
// to ensure that, so in nearly all cases this should have just
// 1 element.
//
// All access is protected by the mheapLock.
    public addrRanges inUse; // scav stores the scavenger state.
//
// All fields are protected by mheapLock.
    public ptr<mutex> mheapLock; // sysStat is the runtime memstat to update when new system
// memory is committed by the pageAlloc for allocation metadata.
    public ptr<sysMemStat> sysStat; // Whether or not this struct is being used in tests.
    public bool test;
}

private static void init(this ptr<pageAlloc> _addr_p, ptr<mutex> _addr_mheapLock, ptr<sysMemStat> _addr_sysStat) {
    ref pageAlloc p = ref _addr_p.val;
    ref mutex mheapLock = ref _addr_mheapLock.val;
    ref sysMemStat sysStat = ref _addr_sysStat.val;

    if (levelLogPages[0] > logMaxPackedValue) { 
        // We can't represent 1<<levelLogPages[0] pages, the maximum number
        // of pages we need to represent at the root level, in a summary, which
        // is a big problem. Throw.
        print("runtime: root level max pages = ", 1 << (int)(levelLogPages[0]), "\n");
        print("runtime: summary max pages = ", maxPackedValue, "\n");
        throw("root level max pages doesn't fit in summary");
    }
    p.sysStat = sysStat; 

    // Initialize p.inUse.
    p.inUse.init(sysStat); 

    // System-dependent initialization.
    p.sysInit(); 

    // Start with the searchAddr in a state indicating there's no free memory.
    p.searchAddr = maxSearchAddr; 

    // Set the mheapLock.
    p.mheapLock = mheapLock; 

    // Initialize scavenge tracking state.
    p.scav.scavLWM = maxSearchAddr;
}

// tryChunkOf returns the bitmap data for the given chunk.
//
// Returns nil if the chunk data has not been mapped.
private static ptr<pallocData> tryChunkOf(this ptr<pageAlloc> _addr_p, chunkIdx ci) {
    ref pageAlloc p = ref _addr_p.val;

    var l2 = p.chunks[ci.l1()];
    if (l2 == null) {
        return _addr_null!;
    }
    return _addr__addr_l2[ci.l2()]!;
}

// chunkOf returns the chunk at the given chunk index.
//
// The chunk index must be valid or this method may throw.
private static ptr<pallocData> chunkOf(this ptr<pageAlloc> _addr_p, chunkIdx ci) {
    ref pageAlloc p = ref _addr_p.val;

    return _addr__addr_p.chunks[ci.l1()][ci.l2()]!;
}

// grow sets up the metadata for the address range [base, base+size).
// It may allocate metadata, in which case *p.sysStat will be updated.
//
// p.mheapLock must be held.
private static void grow(this ptr<pageAlloc> _addr_p, System.UIntPtr @base, System.UIntPtr size) {
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock); 

    // Round up to chunks, since we can't deal with increments smaller
    // than chunks. Also, sysGrow expects aligned values.
    var limit = alignUp(base + size, pallocChunkBytes);
    base = alignDown(base, pallocChunkBytes); 

    // Grow the summary levels in a system-dependent manner.
    // We just update a bunch of additional metadata here.
    p.sysGrow(base, limit); 

    // Update p.start and p.end.
    // If no growth happened yet, start == 0. This is generally
    // safe since the zero page is unmapped.
    var firstGrowth = p.start == 0;
    var start = chunkIndex(base);
    var end = chunkIndex(limit);
    if (firstGrowth || start < p.start) {
        p.start = start;
    }
    if (end > p.end) {
        p.end = end;
    }
    p.inUse.add(makeAddrRange(base, limit)); 

    // A grow operation is a lot like a free operation, so if our
    // chunk ends up below p.searchAddr, update p.searchAddr to the
    // new address, just like in free.
    {
        offAddr b = (new offAddr(base));

        if (b.lessThan(p.searchAddr)) {
            p.searchAddr = b;
        }
    } 

    // Add entries into chunks, which is sparse, if needed. Then,
    // initialize the bitmap.
    //
    // Newly-grown memory is always considered scavenged.
    // Set all the bits in the scavenged bitmaps high.
    for (var c = chunkIndex(base); c < chunkIndex(limit); c++) {
        if (p.chunks[c.l1()] == null) { 
            // Create the necessary l2 entry.
            //
            // Store it atomically to avoid races with readers which
            // don't acquire the heap lock.
            var r = sysAlloc(@unsafe.Sizeof(p.chunks[0].val), p.sysStat);
            if (r == null) {
                throw("pageAlloc: out of memory");
            }
            atomic.StorepNoWB(@unsafe.Pointer(_addr_p.chunks[c.l1()]), r);
        }
        p.chunkOf(c).scavenged.setRange(0, pallocChunkPages);
    } 

    // Update summaries accordingly. The grow acts like a free, so
    // we need to ensure this newly-free memory is visible in the
    // summaries.
    p.update(base, size / pageSize, true, false);
}

// update updates heap metadata. It must be called each time the bitmap
// is updated.
//
// If contig is true, update does some optimizations assuming that there was
// a contiguous allocation or free between addr and addr+npages. alloc indicates
// whether the operation performed was an allocation or a free.
//
// p.mheapLock must be held.
private static void update(this ptr<pageAlloc> _addr_p, System.UIntPtr @base, System.UIntPtr npages, bool contig, bool alloc) {
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock); 

    // base, limit, start, and end are inclusive.
    var limit = base + npages * pageSize - 1;
    var sc = chunkIndex(base);
    var ec = chunkIndex(limit); 

    // Handle updating the lowest level first.
    if (sc == ec) { 
        // Fast path: the allocation doesn't span more than one chunk,
        // so update this one and if the summary didn't change, return.
        var x = p.summary[len(p.summary) - 1][sc];
        var y = p.chunkOf(sc).summarize();
        if (x == y) {
            return ;
        }
        p.summary[len(p.summary) - 1][sc] = y;
    }
    else if (contig) { 
        // Slow contiguous path: the allocation spans more than one chunk
        // and at least one summary is guaranteed to change.
        var summary = p.summary[len(p.summary) - 1]; 

        // Update the summary for chunk sc.
        summary[sc] = p.chunkOf(sc).summarize(); 

        // Update the summaries for chunks in between, which are
        // either totally allocated or freed.
        var whole = p.summary[len(p.summary) - 1][(int)sc + 1..(int)ec];
        if (alloc) { 
            // Should optimize into a memclr.
            {
                var i__prev1 = i;

                foreach (var (__i) in whole) {
                    i = __i;
                    whole[i] = 0;
                }
        else

                i = i__prev1;
            }
        } {
            {
                var i__prev1 = i;

                foreach (var (__i) in whole) {
                    i = __i;
                    whole[i] = freeChunkSum;
                }
    else

                i = i__prev1;
            }
        }
        summary[ec] = p.chunkOf(ec).summarize();
    } { 
        // Slow general path: the allocation spans more than one chunk
        // and at least one summary is guaranteed to change.
        //
        // We can't assume a contiguous allocation happened, so walk over
        // every chunk in the range and manually recompute the summary.
        summary = p.summary[len(p.summary) - 1];
        for (var c = sc; c <= ec; c++) {
            summary[c] = p.chunkOf(c).summarize();
        }
    }
    var changed = true;
    for (var l = len(p.summary) - 2; l >= 0 && changed; l--) { 
        // Update summaries at level l from summaries at level l+1.
        changed = false; 

        // "Constants" for the previous level which we
        // need to compute the summary from that level.
        var logEntriesPerBlock = levelBits[l + 1];
        var logMaxPages = levelLogPages[l + 1]; 

        // lo and hi describe all the parts of the level we need to look at.
        var (lo, hi) = addrsToSummaryRange(l, base, limit + 1); 

        // Iterate over each block, updating the corresponding summary in the less-granular level.
        {
            var i__prev2 = i;

            for (var i = lo; i < hi; i++) {
                var children = p.summary[l + 1][(int)i << (int)(logEntriesPerBlock)..(int)(i + 1) << (int)(logEntriesPerBlock)];
                var sum = mergeSummaries(children, logMaxPages);
                var old = p.summary[l][i];
                if (old != sum) {
                    changed = true;
                    p.summary[l][i] = sum;
                }
            }


            i = i__prev2;
        }
    }
}

// allocRange marks the range of memory [base, base+npages*pageSize) as
// allocated. It also updates the summaries to reflect the newly-updated
// bitmap.
//
// Returns the amount of scavenged memory in bytes present in the
// allocated range.
//
// p.mheapLock must be held.
private static System.UIntPtr allocRange(this ptr<pageAlloc> _addr_p, System.UIntPtr @base, System.UIntPtr npages) {
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock);

    var limit = base + npages * pageSize - 1;
    var sc = chunkIndex(base);
    var ec = chunkIndex(limit);
    var si = chunkPageIndex(base);
    var ei = chunkPageIndex(limit);

    var scav = uint(0);
    if (sc == ec) { 
        // The range doesn't cross any chunk boundaries.
        var chunk = p.chunkOf(sc);
        scav += chunk.scavenged.popcntRange(si, ei + 1 - si);
        chunk.allocRange(si, ei + 1 - si);
    }
    else
 { 
        // The range crosses at least one chunk boundary.
        chunk = p.chunkOf(sc);
        scav += chunk.scavenged.popcntRange(si, pallocChunkPages - si);
        chunk.allocRange(si, pallocChunkPages - si);
        for (var c = sc + 1; c < ec; c++) {
            chunk = p.chunkOf(c);
            scav += chunk.scavenged.popcntRange(0, pallocChunkPages);
            chunk.allocAll();
        }
        chunk = p.chunkOf(ec);
        scav += chunk.scavenged.popcntRange(0, ei + 1);
        chunk.allocRange(0, ei + 1);
    }
    p.update(base, npages, true, true);
    return uintptr(scav) * pageSize;
}

// findMappedAddr returns the smallest mapped offAddr that is
// >= addr. That is, if addr refers to mapped memory, then it is
// returned. If addr is higher than any mapped region, then
// it returns maxOffAddr.
//
// p.mheapLock must be held.
private static offAddr findMappedAddr(this ptr<pageAlloc> _addr_p, offAddr addr) {
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock); 

    // If we're not in a test, validate first by checking mheap_.arenas.
    // This is a fast path which is only safe to use outside of testing.
    var ai = arenaIndex(addr.addr());
    if (p.test || mheap_.arenas[ai.l1()] == null || mheap_.arenas[ai.l1()][ai.l2()] == null) {
        var (vAddr, ok) = p.inUse.findAddrGreaterEqual(addr.addr());
        if (ok) {
            return new offAddr(vAddr);
        }
        else
 { 
            // The candidate search address is greater than any
            // known address, which means we definitely have no
            // free memory left.
            return maxOffAddr;
        }
    }
    return addr;
}

// find searches for the first (address-ordered) contiguous free region of
// npages in size and returns a base address for that region.
//
// It uses p.searchAddr to prune its search and assumes that no palloc chunks
// below chunkIndex(p.searchAddr) contain any free memory at all.
//
// find also computes and returns a candidate p.searchAddr, which may or
// may not prune more of the address space than p.searchAddr already does.
// This candidate is always a valid p.searchAddr.
//
// find represents the slow path and the full radix tree search.
//
// Returns a base address of 0 on failure, in which case the candidate
// searchAddr returned is invalid and must be ignored.
//
// p.mheapLock must be held.
private static (System.UIntPtr, offAddr) find(this ptr<pageAlloc> _addr_p, System.UIntPtr npages) {
    System.UIntPtr _p0 = default;
    offAddr _p0 = default;
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock); 

    // Search algorithm.
    //
    // This algorithm walks each level l of the radix tree from the root level
    // to the leaf level. It iterates over at most 1 << levelBits[l] of entries
    // in a given level in the radix tree, and uses the summary information to
    // find either:
    //  1) That a given subtree contains a large enough contiguous region, at
    //     which point it continues iterating on the next level, or
    //  2) That there are enough contiguous boundary-crossing bits to satisfy
    //     the allocation, at which point it knows exactly where to start
    //     allocating from.
    //
    // i tracks the index into the current level l's structure for the
    // contiguous 1 << levelBits[l] entries we're actually interested in.
    //
    // NOTE: Technically this search could allocate a region which crosses
    // the arenaBaseOffset boundary, which when arenaBaseOffset != 0, is
    // a discontinuity. However, the only way this could happen is if the
    // page at the zero address is mapped, and this is impossible on
    // every system we support where arenaBaseOffset != 0. So, the
    // discontinuity is already encoded in the fact that the OS will never
    // map the zero page for us, and this function doesn't try to handle
    // this case in any way.

    // i is the beginning of the block of entries we're searching at the
    // current level.
    nint i = 0; 

    // firstFree is the region of address space that we are certain to
    // find the first free page in the heap. base and bound are the inclusive
    // bounds of this window, and both are addresses in the linearized, contiguous
    // view of the address space (with arenaBaseOffset pre-added). At each level,
    // this window is narrowed as we find the memory region containing the
    // first free page of memory. To begin with, the range reflects the
    // full process address space.
    //
    // firstFree is updated by calling foundFree each time free space in the
    // heap is discovered.
    //
    // At the end of the search, base.addr() is the best new
    // searchAddr we could deduce in this search.
    struct{base,boundoffAddr} firstFree = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{base,boundoffAddr}{base:minOffAddr,bound:maxOffAddr,}; 
    // foundFree takes the given address range [addr, addr+size) and
    // updates firstFree if it is a narrower range. The input range must
    // either be fully contained within firstFree or not overlap with it
    // at all.
    //
    // This way, we'll record the first summary we find with any free
    // pages on the root level and narrow that down if we descend into
    // that summary. But as soon as we need to iterate beyond that summary
    // in a level to find a large enough range, we'll stop narrowing.
    Action<offAddr, System.UIntPtr> foundFree = (addr, size) => {
        if (firstFree.@base.lessEqual(addr) && addr.add(size - 1).lessEqual(firstFree.bound)) { 
            // This range fits within the current firstFree window, so narrow
            // down the firstFree window to the base and bound of this range.
            firstFree.@base = addr;
            firstFree.bound = addr.add(size - 1);
        }
        else if (!(addr.add(size - 1).lessThan(firstFree.@base) || firstFree.bound.lessThan(addr))) { 
            // This range only partially overlaps with the firstFree range,
            // so throw.
            print("runtime: addr = ", hex(addr.addr()), ", size = ", size, "\n");
            print("runtime: base = ", hex(firstFree.@base.addr()), ", bound = ", hex(firstFree.bound.addr()), "\n");
            throw("range partially overlaps");
        }
    }; 

    // lastSum is the summary which we saw on the previous level that made us
    // move on to the next level. Used to print additional information in the
    // case of a catastrophic failure.
    // lastSumIdx is that summary's index in the previous level.
    var lastSum = packPallocSum(0, 0, 0);
    nint lastSumIdx = -1;

nextLevel: 

    // Since we've gotten to this point, that means we haven't found a
    // sufficiently-sized free region straddling some boundary (chunk or larger).
    // This means the last summary we inspected must have had a large enough "max"
    // value, so look inside the chunk to find a suitable run.
    //
    // After iterating over all levels, i must contain a chunk index which
    // is what the final level represents.
    for (nint l = 0; l < len(p.summary); l++) { 
        // For the root level, entriesPerBlock is the whole level.
        nint entriesPerBlock = 1 << (int)(levelBits[l]);
        var logMaxPages = levelLogPages[l]; 

        // We've moved into a new level, so let's update i to our new
        // starting index. This is a no-op for level 0.
        i<<=levelBits[l]; 

        // Slice out the block of entries we care about.
        var entries = p.summary[l][(int)i..(int)i + entriesPerBlock]; 

        // Determine j0, the first index we should start iterating from.
        // The searchAddr may help us eliminate iterations if we followed the
        // searchAddr on the previous level or we're on the root leve, in which
        // case the searchAddr should be the same as i after levelShift.
        nint j0 = 0;
        {
            var searchIdx = offAddrToLevelIndex(l, p.searchAddr);

            if (searchIdx & ~(entriesPerBlock - 1) == i) {
                j0 = searchIdx & (entriesPerBlock - 1);
            } 

            // Run over the level entries looking for
            // a contiguous run of at least npages either
            // within an entry or across entries.
            //
            // base contains the page index (relative to
            // the first entry's first page) of the currently
            // considered run of consecutive pages.
            //
            // size contains the size of the currently considered
            // run of consecutive pages.

        } 

        // Run over the level entries looking for
        // a contiguous run of at least npages either
        // within an entry or across entries.
        //
        // base contains the page index (relative to
        // the first entry's first page) of the currently
        // considered run of consecutive pages.
        //
        // size contains the size of the currently considered
        // run of consecutive pages.
        nuint @base = default;        nuint size = default;

        {
            var j__prev2 = j;

            for (var j = j0; j < len(entries); j++) {
                var sum = entries[j];
                if (sum == 0) { 
                    // A full entry means we broke any streak and
                    // that we should skip it altogether.
                    size = 0;
                    continue;
                } 

                // We've encountered a non-zero summary which means
                // free memory, so update firstFree.
                foundFree(levelIndexToOffAddr(l, i + j), (uintptr(1) << (int)(logMaxPages)) * pageSize);

                var s = sum.start();
                if (size + s >= uint(npages)) { 
                    // If size == 0 we don't have a run yet,
                    // which means base isn't valid. So, set
                    // base to the first page in this block.
                    if (size == 0) {
                        base = uint(j) << (int)(logMaxPages);
                    } 
                    // We hit npages; we're done!
                    size += s;
                    break;
                }
                if (sum.max() >= uint(npages)) { 
                    // The entry itself contains npages contiguous
                    // free pages, so continue on the next level
                    // to find that run.
                    i += j;
                    lastSumIdx = i;
                    lastSum = sum;
                    _continuenextLevel = true;
                    break;
                }
                if (size == 0 || s < 1 << (int)(logMaxPages)) { 
                    // We either don't have a current run started, or this entry
                    // isn't totally free (meaning we can't continue the current
                    // one), so try to begin a new run by setting size and base
                    // based on sum.end.
                    size = sum.end();
                    base = uint(j + 1) << (int)(logMaxPages) - size;
                    continue;
                } 
                // The entry is completely free, so continue the run.
                size += 1 << (int)(logMaxPages);
            }


            j = j__prev2;
        }
        if (size >= uint(npages)) { 
            // We found a sufficiently large run of free pages straddling
            // some boundary, so compute the address and return it.
            var addr = levelIndexToOffAddr(l, i).add(uintptr(base) * pageSize).addr();
            return (addr, p.findMappedAddr(firstFree.@base));
        }
        if (l == 0) { 
            // We're at level zero, so that means we've exhausted our search.
            return (0, maxSearchAddr);
        }
        print("runtime: summary[", l - 1, "][", lastSumIdx, "] = ", lastSum.start(), ", ", lastSum.max(), ", ", lastSum.end(), "\n");
        print("runtime: level = ", l, ", npages = ", npages, ", j0 = ", j0, "\n");
        print("runtime: p.searchAddr = ", hex(p.searchAddr.addr()), ", i = ", i, "\n");
        print("runtime: levelShift[level] = ", levelShift[l], ", levelBits[level] = ", levelBits[l], "\n");
        {
            var j__prev2 = j;

            for (j = 0; j < len(entries); j++) {
                sum = entries[j];
                print("runtime: summary[", l, "][", i + j, "] = (", sum.start(), ", ", sum.max(), ", ", sum.end(), ")\n");
            }


            j = j__prev2;
        }
        throw("bad summary data");
    } 

    // Since we've gotten to this point, that means we haven't found a
    // sufficiently-sized free region straddling some boundary (chunk or larger).
    // This means the last summary we inspected must have had a large enough "max"
    // value, so look inside the chunk to find a suitable run.
    //
    // After iterating over all levels, i must contain a chunk index which
    // is what the final level represents.
    var ci = chunkIdx(i);
    var (j, searchIdx) = p.chunkOf(ci).find(npages, 0);
    if (j == ~uint(0)) { 
        // We couldn't find any space in this chunk despite the summaries telling
        // us it should be there. There's likely a bug, so dump some state and throw.
        sum = p.summary[len(p.summary) - 1][i];
        print("runtime: summary[", len(p.summary) - 1, "][", i, "] = (", sum.start(), ", ", sum.max(), ", ", sum.end(), ")\n");
        print("runtime: npages = ", npages, "\n");
        throw("bad summary data");
    }
    addr = chunkBase(ci) + uintptr(j) * pageSize; 

    // Since we actually searched the chunk, we may have
    // found an even narrower free window.
    var searchAddr = chunkBase(ci) + uintptr(searchIdx) * pageSize;
    foundFree(new offAddr(searchAddr), chunkBase(ci + 1) - searchAddr);
    return (addr, p.findMappedAddr(firstFree.@base));
}

// alloc allocates npages worth of memory from the page heap, returning the base
// address for the allocation and the amount of scavenged memory in bytes
// contained in the region [base address, base address + npages*pageSize).
//
// Returns a 0 base address on failure, in which case other returned values
// should be ignored.
//
// p.mheapLock must be held.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
private static (System.UIntPtr, System.UIntPtr) alloc(this ptr<pageAlloc> _addr_p, System.UIntPtr npages) {
    System.UIntPtr addr = default;
    System.UIntPtr scav = default;
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock); 

    // If the searchAddr refers to a region which has a higher address than
    // any known chunk, then we know we're out of memory.
    if (chunkIndex(p.searchAddr.addr()) >= p.end) {
        return (0, 0);
    }
    var searchAddr = minOffAddr;
    if (pallocChunkPages - chunkPageIndex(p.searchAddr.addr()) >= uint(npages)) { 
        // npages is guaranteed to be no greater than pallocChunkPages here.
        var i = chunkIndex(p.searchAddr.addr());
        {
            var max = p.summary[len(p.summary) - 1][i].max();

            if (max >= uint(npages)) {
                var (j, searchIdx) = p.chunkOf(i).find(npages, chunkPageIndex(p.searchAddr.addr()));
                if (j == ~uint(0)) {
                    print("runtime: max = ", max, ", npages = ", npages, "\n");
                    print("runtime: searchIdx = ", chunkPageIndex(p.searchAddr.addr()), ", p.searchAddr = ", hex(p.searchAddr.addr()), "\n");
                    throw("bad summary data");
                }
                addr = chunkBase(i) + uintptr(j) * pageSize;
                searchAddr = new offAddr(chunkBase(i)+uintptr(searchIdx)*pageSize);
                goto Found;
            }

        }
    }
    addr, searchAddr = p.find(npages);
    if (addr == 0) {
        if (npages == 1) { 
            // We failed to find a single free page, the smallest unit
            // of allocation. This means we know the heap is completely
            // exhausted. Otherwise, the heap still might have free
            // space in it, just not enough contiguous space to
            // accommodate npages.
            p.searchAddr = maxSearchAddr;
        }
        return (0, 0);
    }
Found: 

    // If we found a higher searchAddr, we know that all the
    // heap memory before that searchAddr in an offset address space is
    // allocated, so bump p.searchAddr up to the new one.
    scav = p.allocRange(addr, npages); 

    // If we found a higher searchAddr, we know that all the
    // heap memory before that searchAddr in an offset address space is
    // allocated, so bump p.searchAddr up to the new one.
    if (p.searchAddr.lessThan(searchAddr)) {
        p.searchAddr = searchAddr;
    }
    return (addr, scav);
}

// free returns npages worth of memory starting at base back to the page heap.
//
// p.mheapLock must be held.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
private static void free(this ptr<pageAlloc> _addr_p, System.UIntPtr @base, System.UIntPtr npages) {
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock); 

    // If we're freeing pages below the p.searchAddr, update searchAddr.
    {
        offAddr b = (new offAddr(base));

        if (b.lessThan(p.searchAddr)) {
            p.searchAddr = b;
        }
    } 
    // Update the free high watermark for the scavenger.
    var limit = base + npages * pageSize - 1;
    {
        offAddr offLimit = (new offAddr(limit));

        if (p.scav.freeHWM.lessThan(offLimit)) {
            p.scav.freeHWM = offLimit;
        }
    }
    if (npages == 1) { 
        // Fast path: we're clearing a single bit, and we know exactly
        // where it is, so mark it directly.
        var i = chunkIndex(base);
        p.chunkOf(i).free1(chunkPageIndex(base));
    }
    else
 { 
        // Slow path: we're clearing more bits so we may need to iterate.
        var sc = chunkIndex(base);
        var ec = chunkIndex(limit);
        var si = chunkPageIndex(base);
        var ei = chunkPageIndex(limit);

        if (sc == ec) { 
            // The range doesn't cross any chunk boundaries.
            p.chunkOf(sc).free(si, ei + 1 - si);
        }
        else
 { 
            // The range crosses at least one chunk boundary.
            p.chunkOf(sc).free(si, pallocChunkPages - si);
            for (var c = sc + 1; c < ec; c++) {
                p.chunkOf(c).freeAll();
            }

            p.chunkOf(ec).free(0, ei + 1);
        }
    }
    p.update(base, npages, true, false);
}

private static readonly var pallocSumBytes = @unsafe.Sizeof(pallocSum(0)); 

// maxPackedValue is the maximum value that any of the three fields in
// the pallocSum may take on.
private static readonly nint maxPackedValue = 1 << (int)(logMaxPackedValue);
private static readonly var logMaxPackedValue = logPallocChunkPages + (summaryLevels - 1) * summaryLevelBits;

private static readonly var freeChunkSum = pallocSum(uint64(pallocChunkPages) | uint64(pallocChunkPages << (int)(logMaxPackedValue)) | uint64(pallocChunkPages << (int)((2 * logMaxPackedValue))));

// pallocSum is a packed summary type which packs three numbers: start, max,
// and end into a single 8-byte value. Each of these values are a summary of
// a bitmap and are thus counts, each of which may have a maximum value of
// 2^21 - 1, or all three may be equal to 2^21. The latter case is represented
// by just setting the 64th bit.
private partial struct pallocSum { // : ulong
}

// packPallocSum takes a start, max, and end value and produces a pallocSum.
private static pallocSum packPallocSum(nuint start, nuint max, nuint end) {
    if (max == maxPackedValue) {
        return pallocSum(uint64(1 << 63));
    }
    return pallocSum((uint64(start) & (maxPackedValue - 1)) | ((uint64(max) & (maxPackedValue - 1)) << (int)(logMaxPackedValue)) | ((uint64(end) & (maxPackedValue - 1)) << (int)((2 * logMaxPackedValue))));
}

// start extracts the start value from a packed sum.
private static nuint start(this pallocSum p) {
    if (uint64(p) & uint64(1 << 63) != 0) {
        return maxPackedValue;
    }
    return uint(uint64(p) & (maxPackedValue - 1));
}

// max extracts the max value from a packed sum.
private static nuint max(this pallocSum p) {
    if (uint64(p) & uint64(1 << 63) != 0) {
        return maxPackedValue;
    }
    return uint((uint64(p) >> (int)(logMaxPackedValue)) & (maxPackedValue - 1));
}

// end extracts the end value from a packed sum.
private static nuint end(this pallocSum p) {
    if (uint64(p) & uint64(1 << 63) != 0) {
        return maxPackedValue;
    }
    return uint((uint64(p) >> (int)((2 * logMaxPackedValue))) & (maxPackedValue - 1));
}

// unpack unpacks all three values from the summary.
private static (nuint, nuint, nuint) unpack(this pallocSum p) {
    nuint _p0 = default;
    nuint _p0 = default;
    nuint _p0 = default;

    if (uint64(p) & uint64(1 << 63) != 0) {
        return (maxPackedValue, maxPackedValue, maxPackedValue);
    }
    return (uint(uint64(p) & (maxPackedValue - 1)), uint((uint64(p) >> (int)(logMaxPackedValue)) & (maxPackedValue - 1)), uint((uint64(p) >> (int)((2 * logMaxPackedValue))) & (maxPackedValue - 1)));
}

// mergeSummaries merges consecutive summaries which may each represent at
// most 1 << logMaxPagesPerSum pages each together into one.
private static pallocSum mergeSummaries(slice<pallocSum> sums, nuint logMaxPagesPerSum) { 
    // Merge the summaries in sums into one.
    //
    // We do this by keeping a running summary representing the merged
    // summaries of sums[:i] in start, max, and end.
    var (start, max, end) = sums[0].unpack();
    for (nint i = 1; i < len(sums); i++) { 
        // Merge in sums[i].
        var (si, mi, ei) = sums[i].unpack(); 

        // Merge in sums[i].start only if the running summary is
        // completely free, otherwise this summary's start
        // plays no role in the combined sum.
        if (start == uint(i) << (int)(logMaxPagesPerSum)) {
            start += si;
        }
        if (end + si > max) {
            max = end + si;
        }
        if (mi > max) {
            max = mi;
        }
        if (ei == 1 << (int)(logMaxPagesPerSum)) {
            end += 1 << (int)(logMaxPagesPerSum);
        }
        else
 {
            end = ei;
        }
    }
    return packPallocSum(start, max, end);
}

} // end runtime_package
