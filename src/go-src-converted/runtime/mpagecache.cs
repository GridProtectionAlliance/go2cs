// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:21:20 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mpagecache.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long pageCachePages = (long)8L * @unsafe.Sizeof(new pageCache().cache);

        // pageCache represents a per-p cache of pages the allocator can
        // allocate from without a lock. More specifically, it represents
        // a pageCachePages*pageSize chunk of memory with 0 or more free
        // pages in it.


        // pageCache represents a per-p cache of pages the allocator can
        // allocate from without a lock. More specifically, it represents
        // a pageCachePages*pageSize chunk of memory with 0 or more free
        // pages in it.
        private partial struct pageCache
        {
            public System.UIntPtr @base; // base address of the chunk
            public ulong cache; // 64-bit bitmap representing free pages (1 means free)
            public ulong scav; // 64-bit bitmap representing scavenged pages (1 means scavenged)
        }

        // empty returns true if the pageCache has any free pages, and false
        // otherwise.
        private static bool empty(this ptr<pageCache> _addr_c)
        {
            ref pageCache c = ref _addr_c.val;

            return c.cache == 0L;
        }

        // alloc allocates npages from the page cache and is the main entry
        // point for allocation.
        //
        // Returns a base address and the amount of scavenged memory in the
        // allocated region in bytes.
        //
        // Returns a base address of zero on failure, in which case the
        // amount of scavenged memory should be ignored.
        private static (System.UIntPtr, System.UIntPtr) alloc(this ptr<pageCache> _addr_c, System.UIntPtr npages)
        {
            System.UIntPtr _p0 = default;
            System.UIntPtr _p0 = default;
            ref pageCache c = ref _addr_c.val;

            if (c.cache == 0L)
            {
                return (0L, 0L);
            }

            if (npages == 1L)
            {
                var i = uintptr(sys.TrailingZeros64(c.cache));
                var scav = (c.scav >> (int)(i)) & 1L;
                c.cache &= 1L << (int)(i); // set bit to mark in-use
                c.scav &= 1L << (int)(i); // clear bit to mark unscavenged
                return (c.@base + i * pageSize, uintptr(scav) * pageSize);

            }

            return c.allocN(npages);

        }

        // allocN is a helper which attempts to allocate npages worth of pages
        // from the cache. It represents the general case for allocating from
        // the page cache.
        //
        // Returns a base address and the amount of scavenged memory in the
        // allocated region in bytes.
        private static (System.UIntPtr, System.UIntPtr) allocN(this ptr<pageCache> _addr_c, System.UIntPtr npages)
        {
            System.UIntPtr _p0 = default;
            System.UIntPtr _p0 = default;
            ref pageCache c = ref _addr_c.val;

            var i = findBitRange64(c.cache, uint(npages));
            if (i >= 64L)
            {
                return (0L, 0L);
            }

            var mask = ((uint64(1L) << (int)(npages)) - 1L) << (int)(i);
            var scav = sys.OnesCount64(c.scav & mask);
            c.cache &= mask; // mark in-use bits
            c.scav &= mask; // clear scavenged bits
            return (c.@base + uintptr(i * pageSize), uintptr(scav) * pageSize);

        }

        // flush empties out unallocated free pages in the given cache
        // into s. Then, it clears the cache, such that empty returns
        // true.
        //
        // s.mheapLock must be held or the world must be stopped.
        private static void flush(this ptr<pageCache> _addr_c, ptr<pageAlloc> _addr_s)
        {
            ref pageCache c = ref _addr_c.val;
            ref pageAlloc s = ref _addr_s.val;

            if (c.empty())
            {
                return ;
            }

            var ci = chunkIndex(c.@base);
            var pi = chunkPageIndex(c.@base); 

            // This method is called very infrequently, so just do the
            // slower, safer thing by iterating over each bit individually.
            for (var i = uint(0L); i < 64L; i++)
            {
                if (c.cache & (1L << (int)(i)) != 0L)
                {
                    s.chunkOf(ci).free1(pi + i);
                }

                if (c.scav & (1L << (int)(i)) != 0L)
                {
                    s.chunkOf(ci).scavenged.setRange(pi + i, 1L);
                }

            } 
            // Since this is a lot like a free, we need to make sure
            // we update the searchAddr just like free does.
 
            // Since this is a lot like a free, we need to make sure
            // we update the searchAddr just like free does.
            {
                offAddr b = (new offAddr(c.base));

                if (b.lessThan(s.searchAddr))
                {
                    s.searchAddr = b;
                }

            }

            s.update(c.@base, pageCachePages, false, false);
            c.val = new pageCache();

        }

        // allocToCache acquires a pageCachePages-aligned chunk of free pages which
        // may not be contiguous, and returns a pageCache structure which owns the
        // chunk.
        //
        // s.mheapLock must be held.
        private static pageCache allocToCache(this ptr<pageAlloc> _addr_s)
        {
            ref pageAlloc s = ref _addr_s.val;
 
            // If the searchAddr refers to a region which has a higher address than
            // any known chunk, then we know we're out of memory.
            if (chunkIndex(s.searchAddr.addr()) >= s.end)
            {
                return new pageCache();
            }

            pageCache c = new pageCache();
            var ci = chunkIndex(s.searchAddr.addr()); // chunk index
            if (s.summary[len(s.summary) - 1L][ci] != 0L)
            { 
                // Fast path: there's free pages at or near the searchAddr address.
                var chunk = s.chunkOf(ci);
                var (j, _) = chunk.find(1L, chunkPageIndex(s.searchAddr.addr()));
                if (j == ~uint(0L))
                {
                    throw("bad summary data");
                }

                c = new pageCache(base:chunkBase(ci)+alignDown(uintptr(j),64)*pageSize,cache:^chunk.pages64(j),scav:chunk.scavenged.block64(j),);

            }
            else
            { 
                // Slow path: the searchAddr address had nothing there, so go find
                // the first free page the slow way.
                var (addr, _) = s.find(1L);
                if (addr == 0L)
                { 
                    // We failed to find adequate free space, so mark the searchAddr as OoM
                    // and return an empty pageCache.
                    s.searchAddr = maxSearchAddr;
                    return new pageCache();

                }

                ci = chunkIndex(addr);
                chunk = s.chunkOf(ci);
                c = new pageCache(base:alignDown(addr,64*pageSize),cache:^chunk.pages64(chunkPageIndex(addr)),scav:chunk.scavenged.block64(chunkPageIndex(addr)),);

            } 

            // Set the bits as allocated and clear the scavenged bits.
            s.allocRange(c.@base, pageCachePages); 

            // Update as an allocation, but note that it's not contiguous.
            s.update(c.@base, pageCachePages, false, true); 

            // Set the search address to the last page represented by the cache.
            // Since all of the pages in this block are going to the cache, and we
            // searched for the first free page, we can confidently start at the
            // next page.
            //
            // However, s.searchAddr is not allowed to point into unmapped heap memory
            // unless it is maxSearchAddr, so make it the last page as opposed to
            // the page after.
            s.searchAddr = new offAddr(c.base+pageSize*(pageCachePages-1));
            return c;

        }
    }
}
