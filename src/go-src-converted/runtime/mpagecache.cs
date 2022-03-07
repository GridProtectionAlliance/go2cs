// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:02 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mpagecache.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static readonly nint pageCachePages = 8 * @unsafe.Sizeof(new pageCache().cache);

// pageCache represents a per-p cache of pages the allocator can
// allocate from without a lock. More specifically, it represents
// a pageCachePages*pageSize chunk of memory with 0 or more free
// pages in it.


// pageCache represents a per-p cache of pages the allocator can
// allocate from without a lock. More specifically, it represents
// a pageCachePages*pageSize chunk of memory with 0 or more free
// pages in it.
private partial struct pageCache {
    public System.UIntPtr @base; // base address of the chunk
    public ulong cache; // 64-bit bitmap representing free pages (1 means free)
    public ulong scav; // 64-bit bitmap representing scavenged pages (1 means scavenged)
}

// empty returns true if the pageCache has any free pages, and false
// otherwise.
private static bool empty(this ptr<pageCache> _addr_c) {
    ref pageCache c = ref _addr_c.val;

    return c.cache == 0;
}

// alloc allocates npages from the page cache and is the main entry
// point for allocation.
//
// Returns a base address and the amount of scavenged memory in the
// allocated region in bytes.
//
// Returns a base address of zero on failure, in which case the
// amount of scavenged memory should be ignored.
private static (System.UIntPtr, System.UIntPtr) alloc(this ptr<pageCache> _addr_c, System.UIntPtr npages) {
    System.UIntPtr _p0 = default;
    System.UIntPtr _p0 = default;
    ref pageCache c = ref _addr_c.val;

    if (c.cache == 0) {
        return (0, 0);
    }
    if (npages == 1) {
        var i = uintptr(sys.TrailingZeros64(c.cache));
        var scav = (c.scav >> (int)(i)) & 1;
        c.cache &= 1 << (int)(i); // set bit to mark in-use
        c.scav &= 1 << (int)(i); // clear bit to mark unscavenged
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
private static (System.UIntPtr, System.UIntPtr) allocN(this ptr<pageCache> _addr_c, System.UIntPtr npages) {
    System.UIntPtr _p0 = default;
    System.UIntPtr _p0 = default;
    ref pageCache c = ref _addr_c.val;

    var i = findBitRange64(c.cache, uint(npages));
    if (i >= 64) {
        return (0, 0);
    }
    var mask = ((uint64(1) << (int)(npages)) - 1) << (int)(i);
    var scav = sys.OnesCount64(c.scav & mask);
    c.cache &= mask; // mark in-use bits
    c.scav &= mask; // clear scavenged bits
    return (c.@base + uintptr(i * pageSize), uintptr(scav) * pageSize);

}

// flush empties out unallocated free pages in the given cache
// into s. Then, it clears the cache, such that empty returns
// true.
//
// p.mheapLock must be held.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
private static void flush(this ptr<pageCache> _addr_c, ptr<pageAlloc> _addr_p) {
    ref pageCache c = ref _addr_c.val;
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock);

    if (c.empty()) {
        return ;
    }
    var ci = chunkIndex(c.@base);
    var pi = chunkPageIndex(c.@base); 

    // This method is called very infrequently, so just do the
    // slower, safer thing by iterating over each bit individually.
    for (var i = uint(0); i < 64; i++) {
        if (c.cache & (1 << (int)(i)) != 0) {
            p.chunkOf(ci).free1(pi + i);
        }
        if (c.scav & (1 << (int)(i)) != 0) {
            p.chunkOf(ci).scavenged.setRange(pi + i, 1);
        }
    } 
    // Since this is a lot like a free, we need to make sure
    // we update the searchAddr just like free does.
    {
        offAddr b = (new offAddr(c.base));

        if (b.lessThan(p.searchAddr)) {
            p.searchAddr = b;
        }
    }

    p.update(c.@base, pageCachePages, false, false);
    c.val = new pageCache();

}

// allocToCache acquires a pageCachePages-aligned chunk of free pages which
// may not be contiguous, and returns a pageCache structure which owns the
// chunk.
//
// p.mheapLock must be held.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
private static pageCache allocToCache(this ptr<pageAlloc> _addr_p) {
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock); 

    // If the searchAddr refers to a region which has a higher address than
    // any known chunk, then we know we're out of memory.
    if (chunkIndex(p.searchAddr.addr()) >= p.end) {
        return new pageCache();
    }
    pageCache c = new pageCache();
    var ci = chunkIndex(p.searchAddr.addr()); // chunk index
    if (p.summary[len(p.summary) - 1][ci] != 0) { 
        // Fast path: there's free pages at or near the searchAddr address.
        var chunk = p.chunkOf(ci);
        var (j, _) = chunk.find(1, chunkPageIndex(p.searchAddr.addr()));
        if (j == ~uint(0)) {
            throw("bad summary data");
        }
        c = new pageCache(base:chunkBase(ci)+alignDown(uintptr(j),64)*pageSize,cache:^chunk.pages64(j),scav:chunk.scavenged.block64(j),);

    }
    else
 { 
        // Slow path: the searchAddr address had nothing there, so go find
        // the first free page the slow way.
        var (addr, _) = p.find(1);
        if (addr == 0) { 
            // We failed to find adequate free space, so mark the searchAddr as OoM
            // and return an empty pageCache.
            p.searchAddr = maxSearchAddr;
            return new pageCache();

        }
        ci = chunkIndex(addr);
        chunk = p.chunkOf(ci);
        c = new pageCache(base:alignDown(addr,64*pageSize),cache:^chunk.pages64(chunkPageIndex(addr)),scav:chunk.scavenged.block64(chunkPageIndex(addr)),);

    }
    p.allocRange(c.@base, pageCachePages); 

    // Update as an allocation, but note that it's not contiguous.
    p.update(c.@base, pageCachePages, false, true); 

    // Set the search address to the last page represented by the cache.
    // Since all of the pages in this block are going to the cache, and we
    // searched for the first free page, we can confidently start at the
    // next page.
    //
    // However, p.searchAddr is not allowed to point into unmapped heap memory
    // unless it is maxSearchAddr, so make it the last page as opposed to
    // the page after.
    p.searchAddr = new offAddr(c.base+pageSize*(pageCachePages-1));
    return c;

}

} // end runtime_package
