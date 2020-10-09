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

// package runtime -- go2cs converted at 2020 October 09 04:47:02 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mpagealloc.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
 
        // The size of a bitmap chunk, i.e. the amount of bits (that is, pages) to consider
        // in the bitmap at once.
        private static readonly long pallocChunkPages = (long)1L << (int)(logPallocChunkPages);
        private static readonly var pallocChunkBytes = pallocChunkPages * pageSize;
        private static readonly long logPallocChunkPages = (long)9L;
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
        private static readonly long summaryLevelBits = (long)3L;
        private static readonly var summaryL0Bits = heapAddrBits - logPallocChunkBytes - (summaryLevels - 1L) * summaryLevelBits; 

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
        private partial struct chunkIdx // : ulong
        {
        }

        // chunkIndex returns the global index of the palloc chunk containing the
        // pointer p.
        private static chunkIdx chunkIndex(System.UIntPtr p)
        {
            return chunkIdx((p - arenaBaseOffset) / pallocChunkBytes);
        }

        // chunkIndex returns the base address of the palloc chunk at index ci.
        private static System.UIntPtr chunkBase(chunkIdx ci)
        {
            return uintptr(ci) * pallocChunkBytes + arenaBaseOffset;
        }

        // chunkPageIndex computes the index of the page that contains p,
        // relative to the chunk which contains p.
        private static ulong chunkPageIndex(System.UIntPtr p)
        {
            return uint(p % pallocChunkBytes / pageSize);
        }

        // l1 returns the index into the first level of (*pageAlloc).chunks.
        private static ulong l1(this chunkIdx i)
        {
            if (pallocChunksL1Bits == 0L)
            { 
                // Let the compiler optimize this away if there's no
                // L1 map.
                return 0L;

            }
            else
            {
                return uint(i) >> (int)(pallocChunksL1Shift);
            }

        }

        // l2 returns the index into the second level of (*pageAlloc).chunks.
        private static ulong l2(this chunkIdx i)
        {
            if (pallocChunksL1Bits == 0L)
            {
                return uint(i);
            }
            else
            {
                return uint(i) & (1L << (int)(pallocChunksL2Bits) - 1L);
            }

        }

        // offAddrToLevelIndex converts an address in the offset address space
        // to the index into summary[level] containing addr.
        private static long offAddrToLevelIndex(long level, offAddr addr)
        {
            return int((addr.a - arenaBaseOffset) >> (int)(levelShift[level]));
        }

        // levelIndexToOffAddr converts an index into summary[level] into
        // the corresponding address in the offset address space.
        private static offAddr levelIndexToOffAddr(long level, long idx)
        {
            return new offAddr((uintptr(idx)<<levelShift[level])+arenaBaseOffset);
        }

        // addrsToSummaryRange converts base and limit pointers into a range
        // of entries for the given summary level.
        //
        // The returned range is inclusive on the lower bound and exclusive on
        // the upper bound.
        private static (long, long) addrsToSummaryRange(long level, System.UIntPtr @base, System.UIntPtr limit)
        {
            long lo = default;
            long hi = default;
 
            // This is slightly more nuanced than just a shift for the exclusive
            // upper-bound. Note that the exclusive upper bound may be within a
            // summary at this level, meaning if we just do the obvious computation
            // hi will end up being an inclusive upper bound. Unfortunately, just
            // adding 1 to that is too broad since we might be on the very edge of
            // of a summary's max page count boundary for this level
            // (1 << levelLogPages[level]). So, make limit an inclusive upper bound
            // then shift, then add 1, so we get an exclusive upper bound at the end.
            lo = int((base - arenaBaseOffset) >> (int)(levelShift[level]));
            hi = int(((limit - 1L) - arenaBaseOffset) >> (int)(levelShift[level])) + 1L;
            return ;

        }

        // blockAlignSummaryRange aligns indices into the given level to that
        // level's block width (1 << levelBits[level]). It assumes lo is inclusive
        // and hi is exclusive, and so aligns them down and up respectively.
        private static (long, long) blockAlignSummaryRange(long level, long lo, long hi)
        {
            long _p0 = default;
            long _p0 = default;

            var e = uintptr(1L) << (int)(levelBits[level]);
            return (int(alignDown(uintptr(lo), e)), int(alignUp(uintptr(hi), e)));
        }

        private partial struct pageAlloc
        {
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
            public ptr<ulong> sysStat; // Whether or not this struct is being used in tests.
            public bool test;
        }

        private static void init(this ptr<pageAlloc> _addr_s, ptr<mutex> _addr_mheapLock, ptr<ulong> _addr_sysStat)
        {
            ref pageAlloc s = ref _addr_s.val;
            ref mutex mheapLock = ref _addr_mheapLock.val;
            ref ulong sysStat = ref _addr_sysStat.val;

            if (levelLogPages[0L] > logMaxPackedValue)
            { 
                // We can't represent 1<<levelLogPages[0] pages, the maximum number
                // of pages we need to represent at the root level, in a summary, which
                // is a big problem. Throw.
                print("runtime: root level max pages = ", 1L << (int)(levelLogPages[0L]), "\n");
                print("runtime: summary max pages = ", maxPackedValue, "\n");
                throw("root level max pages doesn't fit in summary");

            }

            s.sysStat = sysStat; 

            // Initialize s.inUse.
            s.inUse.init(sysStat); 

            // System-dependent initialization.
            s.sysInit(); 

            // Start with the searchAddr in a state indicating there's no free memory.
            s.searchAddr = maxSearchAddr; 

            // Set the mheapLock.
            s.mheapLock = mheapLock; 

            // Initialize scavenge tracking state.
            s.scav.scavLWM = maxSearchAddr;

        }

        // chunkOf returns the chunk at the given chunk index.
        private static ptr<pallocData> chunkOf(this ptr<pageAlloc> _addr_s, chunkIdx ci)
        {
            ref pageAlloc s = ref _addr_s.val;

            return _addr__addr_s.chunks[ci.l1()][ci.l2()]!;
        }

        // grow sets up the metadata for the address range [base, base+size).
        // It may allocate metadata, in which case *s.sysStat will be updated.
        //
        // s.mheapLock must be held.
        private static void grow(this ptr<pageAlloc> _addr_s, System.UIntPtr @base, System.UIntPtr size)
        {
            ref pageAlloc s = ref _addr_s.val;
 
            // Round up to chunks, since we can't deal with increments smaller
            // than chunks. Also, sysGrow expects aligned values.
            var limit = alignUp(base + size, pallocChunkBytes);
            base = alignDown(base, pallocChunkBytes); 

            // Grow the summary levels in a system-dependent manner.
            // We just update a bunch of additional metadata here.
            s.sysGrow(base, limit); 

            // Update s.start and s.end.
            // If no growth happened yet, start == 0. This is generally
            // safe since the zero page is unmapped.
            var firstGrowth = s.start == 0L;
            var start = chunkIndex(base);
            var end = chunkIndex(limit);
            if (firstGrowth || start < s.start)
            {
                s.start = start;
            }

            if (end > s.end)
            {
                s.end = end;
            } 
            // Note that [base, limit) will never overlap with any existing
            // range inUse because grow only ever adds never-used memory
            // regions to the page allocator.
            s.inUse.add(makeAddrRange(base, limit)); 

            // A grow operation is a lot like a free operation, so if our
            // chunk ends up below s.searchAddr, update s.searchAddr to the
            // new address, just like in free.
            {
                offAddr b = (new offAddr(base));

                if (b.lessThan(s.searchAddr))
                {
                    s.searchAddr = b;
                } 

                // Add entries into chunks, which is sparse, if needed. Then,
                // initialize the bitmap.
                //
                // Newly-grown memory is always considered scavenged.
                // Set all the bits in the scavenged bitmaps high.

            } 

            // Add entries into chunks, which is sparse, if needed. Then,
            // initialize the bitmap.
            //
            // Newly-grown memory is always considered scavenged.
            // Set all the bits in the scavenged bitmaps high.
            for (var c = chunkIndex(base); c < chunkIndex(limit); c++)
            {
                if (s.chunks[c.l1()] == null)
                { 
                    // Create the necessary l2 entry.
                    //
                    // Store it atomically to avoid races with readers which
                    // don't acquire the heap lock.
                    var r = sysAlloc(@unsafe.Sizeof(s.chunks[0L].val), s.sysStat);
                    atomic.StorepNoWB(@unsafe.Pointer(_addr_s.chunks[c.l1()]), r);

                }

                s.chunkOf(c).scavenged.setRange(0L, pallocChunkPages);

            } 

            // Update summaries accordingly. The grow acts like a free, so
            // we need to ensure this newly-free memory is visible in the
            // summaries.
 

            // Update summaries accordingly. The grow acts like a free, so
            // we need to ensure this newly-free memory is visible in the
            // summaries.
            s.update(base, size / pageSize, true, false);

        }

        // update updates heap metadata. It must be called each time the bitmap
        // is updated.
        //
        // If contig is true, update does some optimizations assuming that there was
        // a contiguous allocation or free between addr and addr+npages. alloc indicates
        // whether the operation performed was an allocation or a free.
        //
        // s.mheapLock must be held.
        private static void update(this ptr<pageAlloc> _addr_s, System.UIntPtr @base, System.UIntPtr npages, bool contig, bool alloc)
        {
            ref pageAlloc s = ref _addr_s.val;
 
            // base, limit, start, and end are inclusive.
            var limit = base + npages * pageSize - 1L;
            var sc = chunkIndex(base);
            var ec = chunkIndex(limit); 

            // Handle updating the lowest level first.
            if (sc == ec)
            { 
                // Fast path: the allocation doesn't span more than one chunk,
                // so update this one and if the summary didn't change, return.
                var x = s.summary[len(s.summary) - 1L][sc];
                var y = s.chunkOf(sc).summarize();
                if (x == y)
                {
                    return ;
                }

                s.summary[len(s.summary) - 1L][sc] = y;

            }
            else if (contig)
            { 
                // Slow contiguous path: the allocation spans more than one chunk
                // and at least one summary is guaranteed to change.
                var summary = s.summary[len(s.summary) - 1L]; 

                // Update the summary for chunk sc.
                summary[sc] = s.chunkOf(sc).summarize(); 

                // Update the summaries for chunks in between, which are
                // either totally allocated or freed.
                var whole = s.summary[len(s.summary) - 1L][sc + 1L..ec];
                if (alloc)
                { 
                    // Should optimize into a memclr.
                    {
                        var i__prev1 = i;

                        foreach (var (__i) in whole)
                        {
                            i = __i;
                            whole[i] = 0L;
                        }
                else

                        i = i__prev1;
                    }
                }                {
                    {
                        var i__prev1 = i;

                        foreach (var (__i) in whole)
                        {
                            i = __i;
                            whole[i] = freeChunkSum;
                        }
            else

                        i = i__prev1;
                    }
                } 

                // Update the summary for chunk ec.
                summary[ec] = s.chunkOf(ec).summarize();

            }            { 
                // Slow general path: the allocation spans more than one chunk
                // and at least one summary is guaranteed to change.
                //
                // We can't assume a contiguous allocation happened, so walk over
                // every chunk in the range and manually recompute the summary.
                summary = s.summary[len(s.summary) - 1L];
                for (var c = sc; c <= ec; c++)
                {
                    summary[c] = s.chunkOf(c).summarize();
                }


            } 

            // Walk up the radix tree and update the summaries appropriately.
            var changed = true;
            for (var l = len(s.summary) - 2L; l >= 0L && changed; l--)
            { 
                // Update summaries at level l from summaries at level l+1.
                changed = false; 

                // "Constants" for the previous level which we
                // need to compute the summary from that level.
                var logEntriesPerBlock = levelBits[l + 1L];
                var logMaxPages = levelLogPages[l + 1L]; 

                // lo and hi describe all the parts of the level we need to look at.
                var (lo, hi) = addrsToSummaryRange(l, base, limit + 1L); 

                // Iterate over each block, updating the corresponding summary in the less-granular level.
                {
                    var i__prev2 = i;

                    for (var i = lo; i < hi; i++)
                    {
                        var children = s.summary[l + 1L][i << (int)(logEntriesPerBlock)..(i + 1L) << (int)(logEntriesPerBlock)];
                        var sum = mergeSummaries(children, logMaxPages);
                        var old = s.summary[l][i];
                        if (old != sum)
                        {
                            changed = true;
                            s.summary[l][i] = sum;
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
        // s.mheapLock must be held.
        private static System.UIntPtr allocRange(this ptr<pageAlloc> _addr_s, System.UIntPtr @base, System.UIntPtr npages)
        {
            ref pageAlloc s = ref _addr_s.val;

            var limit = base + npages * pageSize - 1L;
            var sc = chunkIndex(base);
            var ec = chunkIndex(limit);
            var si = chunkPageIndex(base);
            var ei = chunkPageIndex(limit);

            var scav = uint(0L);
            if (sc == ec)
            { 
                // The range doesn't cross any chunk boundaries.
                var chunk = s.chunkOf(sc);
                scav += chunk.scavenged.popcntRange(si, ei + 1L - si);
                chunk.allocRange(si, ei + 1L - si);

            }
            else
            { 
                // The range crosses at least one chunk boundary.
                chunk = s.chunkOf(sc);
                scav += chunk.scavenged.popcntRange(si, pallocChunkPages - si);
                chunk.allocRange(si, pallocChunkPages - si);
                for (var c = sc + 1L; c < ec; c++)
                {
                    chunk = s.chunkOf(c);
                    scav += chunk.scavenged.popcntRange(0L, pallocChunkPages);
                    chunk.allocAll();
                }

                chunk = s.chunkOf(ec);
                scav += chunk.scavenged.popcntRange(0L, ei + 1L);
                chunk.allocRange(0L, ei + 1L);

            }

            s.update(base, npages, true, true);
            return uintptr(scav) * pageSize;

        }

        // findMappedAddr returns the smallest mapped offAddr that is
        // >= addr. That is, if addr refers to mapped memory, then it is
        // returned. If addr is higher than any mapped region, then
        // it returns maxOffAddr.
        //
        // s.mheapLock must be held.
        private static offAddr findMappedAddr(this ptr<pageAlloc> _addr_s, offAddr addr)
        {
            ref pageAlloc s = ref _addr_s.val;
 
            // If we're not in a test, validate first by checking mheap_.arenas.
            // This is a fast path which is only safe to use outside of testing.
            var ai = arenaIndex(addr.addr());
            if (s.test || mheap_.arenas[ai.l1()] == null || mheap_.arenas[ai.l1()][ai.l2()] == null)
            {
                var (vAddr, ok) = s.inUse.findAddrGreaterEqual(addr.addr());
                if (ok)
                {
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
        // It uses s.searchAddr to prune its search and assumes that no palloc chunks
        // below chunkIndex(s.searchAddr) contain any free memory at all.
        //
        // find also computes and returns a candidate s.searchAddr, which may or
        // may not prune more of the address space than s.searchAddr already does.
        // This candidate is always a valid s.searchAddr.
        //
        // find represents the slow path and the full radix tree search.
        //
        // Returns a base address of 0 on failure, in which case the candidate
        // searchAddr returned is invalid and must be ignored.
        //
        // s.mheapLock must be held.
        private static (System.UIntPtr, offAddr) find(this ptr<pageAlloc> _addr_s, System.UIntPtr npages)
        {
            System.UIntPtr _p0 = default;
            offAddr _p0 = default;
            ref pageAlloc s = ref _addr_s.val;
 
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
            long i = 0L; 

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
            Action<offAddr, System.UIntPtr> foundFree = (addr, size) =>
            {
                if (firstFree.@base.lessEqual(addr) && addr.add(size - 1L).lessEqual(firstFree.bound))
                { 
                    // This range fits within the current firstFree window, so narrow
                    // down the firstFree window to the base and bound of this range.
                    firstFree.@base = addr;
                    firstFree.bound = addr.add(size - 1L);

                }
                else if (!(addr.add(size - 1L).lessThan(firstFree.@base) || firstFree.bound.lessThan(addr)))
                { 
                    // This range only partially overlaps with the firstFree range,
                    // so throw.
                    print("runtime: addr = ", hex(addr.addr()), ", size = ", size, "\n");
                    print("runtime: base = ", hex(firstFree.@base.addr()), ", bound = ", hex(firstFree.bound.addr()), "\n");
                    throw("range partially overlaps");

                }

            } 

            // lastSum is the summary which we saw on the previous level that made us
            // move on to the next level. Used to print additional information in the
            // case of a catastrophic failure.
            // lastSumIdx is that summary's index in the previous level.
; 

            // lastSum is the summary which we saw on the previous level that made us
            // move on to the next level. Used to print additional information in the
            // case of a catastrophic failure.
            // lastSumIdx is that summary's index in the previous level.
            var lastSum = packPallocSum(0L, 0L, 0L);
            long lastSumIdx = -1L;

nextLevel: 

            // Since we've gotten to this point, that means we haven't found a
            // sufficiently-sized free region straddling some boundary (chunk or larger).
            // This means the last summary we inspected must have had a large enough "max"
            // value, so look inside the chunk to find a suitable run.
            //
            // After iterating over all levels, i must contain a chunk index which
            // is what the final level represents.
            for (long l = 0L; l < len(s.summary); l++)
            { 
                // For the root level, entriesPerBlock is the whole level.
                long entriesPerBlock = 1L << (int)(levelBits[l]);
                var logMaxPages = levelLogPages[l]; 

                // We've moved into a new level, so let's update i to our new
                // starting index. This is a no-op for level 0.
                i <<= levelBits[l]; 

                // Slice out the block of entries we care about.
                var entries = s.summary[l][i..i + entriesPerBlock]; 

                // Determine j0, the first index we should start iterating from.
                // The searchAddr may help us eliminate iterations if we followed the
                // searchAddr on the previous level or we're on the root leve, in which
                // case the searchAddr should be the same as i after levelShift.
                long j0 = 0L;
                {
                    var searchIdx = offAddrToLevelIndex(l, s.searchAddr);

                    if (searchIdx & ~(entriesPerBlock - 1L) == i)
                    {
                        j0 = searchIdx & (entriesPerBlock - 1L);
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
                ulong @base = default;                ulong size = default;

                {
                    var j__prev2 = j;

                    for (var j = j0; j < len(entries); j++)
                    {
                        var sum = entries[j];
                        if (sum == 0L)
                        { 
                            // A full entry means we broke any streak and
                            // that we should skip it altogether.
                            size = 0L;
                            continue;

                        } 

                        // We've encountered a non-zero summary which means
                        // free memory, so update firstFree.
                        foundFree(levelIndexToOffAddr(l, i + j), (uintptr(1L) << (int)(logMaxPages)) * pageSize);

                        var s = sum.start();
                        if (size + s >= uint(npages))
                        { 
                            // If size == 0 we don't have a run yet,
                            // which means base isn't valid. So, set
                            // base to the first page in this block.
                            if (size == 0L)
                            {
                                base = uint(j) << (int)(logMaxPages);
                            } 
                            // We hit npages; we're done!
                            size += s;
                            break;

                        }

                        if (sum.max() >= uint(npages))
                        { 
                            // The entry itself contains npages contiguous
                            // free pages, so continue on the next level
                            // to find that run.
                            i += j;
                            lastSumIdx = i;
                            lastSum = sum;
                            _continuenextLevel = true;
                            break;
                        }

                        if (size == 0L || s < 1L << (int)(logMaxPages))
                        { 
                            // We either don't have a current run started, or this entry
                            // isn't totally free (meaning we can't continue the current
                            // one), so try to begin a new run by setting size and base
                            // based on sum.end.
                            size = sum.end();
                            base = uint(j + 1L) << (int)(logMaxPages) - size;
                            continue;

                        } 
                        // The entry is completely free, so continue the run.
                        size += 1L << (int)(logMaxPages);

                    }


                    j = j__prev2;
                }
                if (size >= uint(npages))
                { 
                    // We found a sufficiently large run of free pages straddling
                    // some boundary, so compute the address and return it.
                    var addr = levelIndexToOffAddr(l, i).add(uintptr(base) * pageSize).addr();
                    return (addr, s.findMappedAddr(firstFree.@base));

                }

                if (l == 0L)
                { 
                    // We're at level zero, so that means we've exhausted our search.
                    return (0L, maxSearchAddr);

                } 

                // We're not at level zero, and we exhausted the level we were looking in.
                // This means that either our calculations were wrong or the level above
                // lied to us. In either case, dump some useful state and throw.
                print("runtime: summary[", l - 1L, "][", lastSumIdx, "] = ", lastSum.start(), ", ", lastSum.max(), ", ", lastSum.end(), "\n");
                print("runtime: level = ", l, ", npages = ", npages, ", j0 = ", j0, "\n");
                print("runtime: s.searchAddr = ", hex(s.searchAddr.addr()), ", i = ", i, "\n");
                print("runtime: levelShift[level] = ", levelShift[l], ", levelBits[level] = ", levelBits[l], "\n");
                {
                    var j__prev2 = j;

                    for (j = 0L; j < len(entries); j++)
                    {
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
 

            // Since we've gotten to this point, that means we haven't found a
            // sufficiently-sized free region straddling some boundary (chunk or larger).
            // This means the last summary we inspected must have had a large enough "max"
            // value, so look inside the chunk to find a suitable run.
            //
            // After iterating over all levels, i must contain a chunk index which
            // is what the final level represents.
            var ci = chunkIdx(i);
            var (j, searchIdx) = s.chunkOf(ci).find(npages, 0L);
            if (j == ~uint(0L))
            { 
                // We couldn't find any space in this chunk despite the summaries telling
                // us it should be there. There's likely a bug, so dump some state and throw.
                sum = s.summary[len(s.summary) - 1L][i];
                print("runtime: summary[", len(s.summary) - 1L, "][", i, "] = (", sum.start(), ", ", sum.max(), ", ", sum.end(), ")\n");
                print("runtime: npages = ", npages, "\n");
                throw("bad summary data");

            } 

            // Compute the address at which the free space starts.
            addr = chunkBase(ci) + uintptr(j) * pageSize; 

            // Since we actually searched the chunk, we may have
            // found an even narrower free window.
            var searchAddr = chunkBase(ci) + uintptr(searchIdx) * pageSize;
            foundFree(new offAddr(searchAddr), chunkBase(ci + 1L) - searchAddr);
            return (addr, s.findMappedAddr(firstFree.@base));

        }

        // alloc allocates npages worth of memory from the page heap, returning the base
        // address for the allocation and the amount of scavenged memory in bytes
        // contained in the region [base address, base address + npages*pageSize).
        //
        // Returns a 0 base address on failure, in which case other returned values
        // should be ignored.
        //
        // s.mheapLock must be held.
        private static (System.UIntPtr, System.UIntPtr) alloc(this ptr<pageAlloc> _addr_s, System.UIntPtr npages)
        {
            System.UIntPtr addr = default;
            System.UIntPtr scav = default;
            ref pageAlloc s = ref _addr_s.val;
 
            // If the searchAddr refers to a region which has a higher address than
            // any known chunk, then we know we're out of memory.
            if (chunkIndex(s.searchAddr.addr()) >= s.end)
            {
                return (0L, 0L);
            } 

            // If npages has a chance of fitting in the chunk where the searchAddr is,
            // search it directly.
            var searchAddr = minOffAddr;
            if (pallocChunkPages - chunkPageIndex(s.searchAddr.addr()) >= uint(npages))
            { 
                // npages is guaranteed to be no greater than pallocChunkPages here.
                var i = chunkIndex(s.searchAddr.addr());
                {
                    var max = s.summary[len(s.summary) - 1L][i].max();

                    if (max >= uint(npages))
                    {
                        var (j, searchIdx) = s.chunkOf(i).find(npages, chunkPageIndex(s.searchAddr.addr()));
                        if (j == ~uint(0L))
                        {
                            print("runtime: max = ", max, ", npages = ", npages, "\n");
                            print("runtime: searchIdx = ", chunkPageIndex(s.searchAddr.addr()), ", s.searchAddr = ", hex(s.searchAddr.addr()), "\n");
                            throw("bad summary data");
                        }

                        addr = chunkBase(i) + uintptr(j) * pageSize;
                        searchAddr = new offAddr(chunkBase(i)+uintptr(searchIdx)*pageSize);
                        goto Found;

                    }

                }

            } 
            // We failed to use a searchAddr for one reason or another, so try
            // the slow path.
            addr, searchAddr = s.find(npages);
            if (addr == 0L)
            {
                if (npages == 1L)
                { 
                    // We failed to find a single free page, the smallest unit
                    // of allocation. This means we know the heap is completely
                    // exhausted. Otherwise, the heap still might have free
                    // space in it, just not enough contiguous space to
                    // accommodate npages.
                    s.searchAddr = maxSearchAddr;

                }

                return (0L, 0L);

            }

Found: 

            // If we found a higher searchAddr, we know that all the
            // heap memory before that searchAddr in an offset address space is
            // allocated, so bump s.searchAddr up to the new one.
            scav = s.allocRange(addr, npages); 

            // If we found a higher searchAddr, we know that all the
            // heap memory before that searchAddr in an offset address space is
            // allocated, so bump s.searchAddr up to the new one.
            if (s.searchAddr.lessThan(searchAddr))
            {
                s.searchAddr = searchAddr;
            }

            return (addr, scav);

        }

        // free returns npages worth of memory starting at base back to the page heap.
        //
        // s.mheapLock must be held.
        private static void free(this ptr<pageAlloc> _addr_s, System.UIntPtr @base, System.UIntPtr npages)
        {
            ref pageAlloc s = ref _addr_s.val;
 
            // If we're freeing pages below the s.searchAddr, update searchAddr.
            {
                offAddr b = (new offAddr(base));

                if (b.lessThan(s.searchAddr))
                {
                    s.searchAddr = b;
                } 
                // Update the free high watermark for the scavenger.

            } 
            // Update the free high watermark for the scavenger.
            var limit = base + npages * pageSize - 1L;
            {
                offAddr offLimit = (new offAddr(limit));

                if (s.scav.freeHWM.lessThan(offLimit))
                {
                    s.scav.freeHWM = offLimit;
                }

            }

            if (npages == 1L)
            { 
                // Fast path: we're clearing a single bit, and we know exactly
                // where it is, so mark it directly.
                var i = chunkIndex(base);
                s.chunkOf(i).free1(chunkPageIndex(base));

            }
            else
            { 
                // Slow path: we're clearing more bits so we may need to iterate.
                var sc = chunkIndex(base);
                var ec = chunkIndex(limit);
                var si = chunkPageIndex(base);
                var ei = chunkPageIndex(limit);

                if (sc == ec)
                { 
                    // The range doesn't cross any chunk boundaries.
                    s.chunkOf(sc).free(si, ei + 1L - si);

                }
                else
                { 
                    // The range crosses at least one chunk boundary.
                    s.chunkOf(sc).free(si, pallocChunkPages - si);
                    for (var c = sc + 1L; c < ec; c++)
                    {
                        s.chunkOf(c).freeAll();
                    }

                    s.chunkOf(ec).free(0L, ei + 1L);

                }

            }

            s.update(base, npages, true, false);

        }

        private static readonly var pallocSumBytes = @unsafe.Sizeof(pallocSum(0L)); 

        // maxPackedValue is the maximum value that any of the three fields in
        // the pallocSum may take on.
        private static readonly long maxPackedValue = (long)1L << (int)(logMaxPackedValue);
        private static readonly var logMaxPackedValue = logPallocChunkPages + (summaryLevels - 1L) * summaryLevelBits;

        private static readonly var freeChunkSum = pallocSum(uint64(pallocChunkPages) | uint64(pallocChunkPages << (int)(logMaxPackedValue)) | uint64(pallocChunkPages << (int)((2L * logMaxPackedValue))));


        // pallocSum is a packed summary type which packs three numbers: start, max,
        // and end into a single 8-byte value. Each of these values are a summary of
        // a bitmap and are thus counts, each of which may have a maximum value of
        // 2^21 - 1, or all three may be equal to 2^21. The latter case is represented
        // by just setting the 64th bit.
        private partial struct pallocSum // : ulong
        {
        }

        // packPallocSum takes a start, max, and end value and produces a pallocSum.
        private static pallocSum packPallocSum(ulong start, ulong max, ulong end)
        {
            if (max == maxPackedValue)
            {
                return pallocSum(uint64(1L << (int)(63L)));
            }

            return pallocSum((uint64(start) & (maxPackedValue - 1L)) | ((uint64(max) & (maxPackedValue - 1L)) << (int)(logMaxPackedValue)) | ((uint64(end) & (maxPackedValue - 1L)) << (int)((2L * logMaxPackedValue))));

        }

        // start extracts the start value from a packed sum.
        private static ulong start(this pallocSum p)
        {
            if (uint64(p) & uint64(1L << (int)(63L)) != 0L)
            {
                return maxPackedValue;
            }

            return uint(uint64(p) & (maxPackedValue - 1L));

        }

        // max extracts the max value from a packed sum.
        private static ulong max(this pallocSum p)
        {
            if (uint64(p) & uint64(1L << (int)(63L)) != 0L)
            {
                return maxPackedValue;
            }

            return uint((uint64(p) >> (int)(logMaxPackedValue)) & (maxPackedValue - 1L));

        }

        // end extracts the end value from a packed sum.
        private static ulong end(this pallocSum p)
        {
            if (uint64(p) & uint64(1L << (int)(63L)) != 0L)
            {
                return maxPackedValue;
            }

            return uint((uint64(p) >> (int)((2L * logMaxPackedValue))) & (maxPackedValue - 1L));

        }

        // unpack unpacks all three values from the summary.
        private static (ulong, ulong, ulong) unpack(this pallocSum p)
        {
            ulong _p0 = default;
            ulong _p0 = default;
            ulong _p0 = default;

            if (uint64(p) & uint64(1L << (int)(63L)) != 0L)
            {
                return (maxPackedValue, maxPackedValue, maxPackedValue);
            }

            return (uint(uint64(p) & (maxPackedValue - 1L)), uint((uint64(p) >> (int)(logMaxPackedValue)) & (maxPackedValue - 1L)), uint((uint64(p) >> (int)((2L * logMaxPackedValue))) & (maxPackedValue - 1L)));

        }

        // mergeSummaries merges consecutive summaries which may each represent at
        // most 1 << logMaxPagesPerSum pages each together into one.
        private static pallocSum mergeSummaries(slice<pallocSum> sums, ulong logMaxPagesPerSum)
        { 
            // Merge the summaries in sums into one.
            //
            // We do this by keeping a running summary representing the merged
            // summaries of sums[:i] in start, max, and end.
            var (start, max, end) = sums[0L].unpack();
            for (long i = 1L; i < len(sums); i++)
            { 
                // Merge in sums[i].
                var (si, mi, ei) = sums[i].unpack(); 

                // Merge in sums[i].start only if the running summary is
                // completely free, otherwise this summary's start
                // plays no role in the combined sum.
                if (start == uint(i) << (int)(logMaxPagesPerSum))
                {
                    start += si;
                } 

                // Recompute the max value of the running sum by looking
                // across the boundary between the running sum and sums[i]
                // and at the max sums[i], taking the greatest of those two
                // and the max of the running sum.
                if (end + si > max)
                {
                    max = end + si;
                }

                if (mi > max)
                {
                    max = mi;
                } 

                // Merge in end by checking if this new summary is totally
                // free. If it is, then we want to extend the running sum's
                // end by the new summary. If not, then we have some alloc'd
                // pages in there and we just want to take the end value in
                // sums[i].
                if (ei == 1L << (int)(logMaxPagesPerSum))
                {
                    end += 1L << (int)(logMaxPagesPerSum);
                }
                else
                {
                    end = ei;
                }

            }

            return packPallocSum(start, max, end);

        }
    }
}
