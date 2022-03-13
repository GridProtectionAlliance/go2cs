// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:25:04 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mcache.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using @unsafe = @unsafe_package;


// Per-thread (in Go, per-P) cache for small objects.
// This includes a small object cache and local allocation stats.
// No locking needed because it is per-thread (per-P).
//
// mcaches are allocated from non-GC'd memory, so any heap pointers
// must be specially handled.
//
//go:notinheap

using System;
public static partial class runtime_package {

private partial struct mcache {
    public System.UIntPtr nextSample; // trigger heap sample after allocating this many bytes
    public System.UIntPtr scanAlloc; // bytes of scannable heap allocated

// Allocator cache for tiny objects w/o pointers.
// See "Tiny allocator" comment in malloc.go.

// tiny points to the beginning of the current tiny block, or
// nil if there is no current tiny block.
//
// tiny is a heap pointer. Since mcache is in non-GC'd memory,
// we handle it by clearing it in releaseAll during mark
// termination.
//
// tinyAllocs is the number of tiny allocations performed
// by the P that owns this mcache.
    public System.UIntPtr tiny;
    public System.UIntPtr tinyoffset;
    public System.UIntPtr tinyAllocs; // The rest is not accessed on every malloc.

    public array<ptr<mspan>> alloc; // spans to allocate from, indexed by spanClass

    public array<stackfreelist> stackcache; // flushGen indicates the sweepgen during which this mcache
// was last flushed. If flushGen != mheap_.sweepgen, the spans
// in this mcache are stale and need to the flushed so they
// can be swept. This is done in acquirep.
    public uint flushGen;
}

// A gclink is a node in a linked list of blocks, like mlink,
// but it is opaque to the garbage collector.
// The GC does not trace the pointers during collection,
// and the compiler does not emit write barriers for assignments
// of gclinkptr values. Code should store references to gclinks
// as gclinkptr, not as *gclink.
private partial struct gclink {
    public gclinkptr next;
}

// A gclinkptr is a pointer to a gclink, but it is opaque
// to the garbage collector.
private partial struct gclinkptr { // : System.UIntPtr
}

// ptr returns the *gclink form of p.
// The result should be used for accessing fields, not stored
// in other data structures.
private static ptr<gclink> ptr(this gclinkptr p) {
    return _addr_(gclink.val)(@unsafe.Pointer(p))!;
}

private partial struct stackfreelist {
    public gclinkptr list; // linked list of free stacks
    public System.UIntPtr size; // total size of stacks in list
}

// dummy mspan that contains no free objects.
private static mspan emptymspan = default;

private static ptr<mcache> allocmcache() {
    ptr<mcache> c;
    systemstack(() => {
        lock(_addr_mheap_.@lock);
        c = (mcache.val)(mheap_.cachealloc.alloc());
        c.flushGen = mheap_.sweepgen;
        unlock(_addr_mheap_.@lock);
    });
    foreach (var (i) in c.alloc) {
        c.alloc[i] = _addr_emptymspan;
    }    c.nextSample = nextSample();
    return _addr_c!;
}

// freemcache releases resources associated with this
// mcache and puts the object onto a free list.
//
// In some cases there is no way to simply release
// resources, such as statistics, so donate them to
// a different mcache (the recipient).
private static void freemcache(ptr<mcache> _addr_c) {
    ref mcache c = ref _addr_c.val;

    systemstack(() => {
        c.releaseAll();
        stackcache_clear(c); 

        // NOTE(rsc,rlh): If gcworkbuffree comes back, we need to coordinate
        // with the stealing of gcworkbufs during garbage collection to avoid
        // a race where the workbuf is double-freed.
        // gcworkbuffree(c.gcworkbuf)

        lock(_addr_mheap_.@lock);
        mheap_.cachealloc.free(@unsafe.Pointer(c));
        unlock(_addr_mheap_.@lock);
    });
}

// getMCache is a convenience function which tries to obtain an mcache.
//
// Returns nil if we're not bootstrapping or we don't have a P. The caller's
// P must not change, so we must be in a non-preemptible state.
private static ptr<mcache> getMCache() { 
    // Grab the mcache, since that's where stats live.
    var pp = getg().m.p.ptr();
    ptr<mcache> c;
    if (pp == null) { 
        // We will be called without a P while bootstrapping,
        // in which case we use mcache0, which is set in mallocinit.
        // mcache0 is cleared when bootstrapping is complete,
        // by procresize.
        c = mcache0;
    }
    else
 {
        c = pp.mcache;
    }
    return _addr_c!;
}

// refill acquires a new span of span class spc for c. This span will
// have at least one free object. The current span in c must be full.
//
// Must run in a non-preemptible context since otherwise the owner of
// c could change.
private static void refill(this ptr<mcache> _addr_c, spanClass spc) {
    ref mcache c = ref _addr_c.val;
 
    // Return the current cached span to the central lists.
    var s = c.alloc[spc];

    if (uintptr(s.allocCount) != s.nelems) {
        throw("refill of span with free space remaining");
    }
    if (s != _addr_emptymspan) { 
        // Mark this span as no longer cached.
        if (s.sweepgen != mheap_.sweepgen + 3) {
            throw("bad sweepgen in refill");
        }
        mheap_.central[spc].mcentral.uncacheSpan(s);
    }
    s = mheap_.central[spc].mcentral.cacheSpan();
    if (s == null) {
        throw("out of memory");
    }
    if (uintptr(s.allocCount) == s.nelems) {
        throw("span has no free space");
    }
    s.sweepgen = mheap_.sweepgen + 3; 

    // Assume all objects from this span will be allocated in the
    // mcache. If it gets uncached, we'll adjust this.
    var stats = memstats.heapStats.acquire();
    atomic.Xadduintptr(_addr_stats.smallAllocCount[spc.sizeclass()], uintptr(s.nelems) - uintptr(s.allocCount)); 

    // Flush tinyAllocs.
    if (spc == tinySpanClass) {
        atomic.Xadduintptr(_addr_stats.tinyAllocCount, c.tinyAllocs);
        c.tinyAllocs = 0;
    }
    memstats.heapStats.release(); 

    // Update gcController.heapLive with the same assumption.
    var usedBytes = uintptr(s.allocCount) * s.elemsize;
    atomic.Xadd64(_addr_gcController.heapLive, int64(s.npages * pageSize) - int64(usedBytes)); 

    // While we're here, flush scanAlloc, since we have to call
    // revise anyway.
    atomic.Xadd64(_addr_gcController.heapScan, int64(c.scanAlloc));
    c.scanAlloc = 0;

    if (trace.enabled) { 
        // gcController.heapLive changed.
        traceHeapAlloc();
    }
    if (gcBlackenEnabled != 0) { 
        // gcController.heapLive and heapScan changed.
        gcController.revise();
    }
    c.alloc[spc] = s;
}

// allocLarge allocates a span for a large object.
// The boolean result indicates whether the span is known-zeroed.
// If it did not need to be zeroed, it may not have been zeroed;
// but if it came directly from the OS, it is already zeroed.
private static (ptr<mspan>, bool) allocLarge(this ptr<mcache> _addr_c, System.UIntPtr size, bool needzero, bool noscan) {
    ptr<mspan> _p0 = default!;
    bool _p0 = default;
    ref mcache c = ref _addr_c.val;

    if (size + _PageSize < size) {
        throw("out of memory");
    }
    var npages = size >> (int)(_PageShift);
    if (size & _PageMask != 0) {
        npages++;
    }
    deductSweepCredit(npages * _PageSize, npages);

    var spc = makeSpanClass(0, noscan);
    var (s, isZeroed) = mheap_.alloc(npages, spc, needzero);
    if (s == null) {
        throw("out of memory");
    }
    var stats = memstats.heapStats.acquire();
    atomic.Xadduintptr(_addr_stats.largeAlloc, npages * pageSize);
    atomic.Xadduintptr(_addr_stats.largeAllocCount, 1);
    memstats.heapStats.release(); 

    // Update gcController.heapLive and revise pacing if needed.
    atomic.Xadd64(_addr_gcController.heapLive, int64(npages * pageSize));
    if (trace.enabled) { 
        // Trace that a heap alloc occurred because gcController.heapLive changed.
        traceHeapAlloc();
    }
    if (gcBlackenEnabled != 0) {
        gcController.revise();
    }
    mheap_.central[spc].mcentral.fullSwept(mheap_.sweepgen).push(s);
    s.limit = s.@base() + size;
    heapBitsForAddr(s.@base()).initSpan(s);
    return (_addr_s!, isZeroed);
}

private static void releaseAll(this ptr<mcache> _addr_c) {
    ref mcache c = ref _addr_c.val;
 
    // Take this opportunity to flush scanAlloc.
    atomic.Xadd64(_addr_gcController.heapScan, int64(c.scanAlloc));
    c.scanAlloc = 0;

    var sg = mheap_.sweepgen;
    foreach (var (i) in c.alloc) {
        var s = c.alloc[i];
        if (s != _addr_emptymspan) { 
            // Adjust nsmallalloc in case the span wasn't fully allocated.
            var n = uintptr(s.nelems) - uintptr(s.allocCount);
            var stats = memstats.heapStats.acquire();
            atomic.Xadduintptr(_addr_stats.smallAllocCount[spanClass(i).sizeclass()], -n);
            memstats.heapStats.release();
            if (s.sweepgen != sg + 1) { 
                // refill conservatively counted unallocated slots in gcController.heapLive.
                // Undo this.
                //
                // If this span was cached before sweep, then
                // gcController.heapLive was totally recomputed since
                // caching this span, so we don't do this for
                // stale spans.
                atomic.Xadd64(_addr_gcController.heapLive, -int64(n) * int64(s.elemsize));
            } 
            // Release the span to the mcentral.
            mheap_.central[i].mcentral.uncacheSpan(s);
            c.alloc[i] = _addr_emptymspan;
        }
    }    c.tiny = 0;
    c.tinyoffset = 0; 

    // Flush tinyAllocs.
    stats = memstats.heapStats.acquire();
    atomic.Xadduintptr(_addr_stats.tinyAllocCount, c.tinyAllocs);
    c.tinyAllocs = 0;
    memstats.heapStats.release(); 

    // Updated heapScan and possible gcController.heapLive.
    if (gcBlackenEnabled != 0) {
        gcController.revise();
    }
}

// prepareForSweep flushes c if the system has entered a new sweep phase
// since c was populated. This must happen between the sweep phase
// starting and the first allocation from c.
private static void prepareForSweep(this ptr<mcache> _addr_c) {
    ref mcache c = ref _addr_c.val;
 
    // Alternatively, instead of making sure we do this on every P
    // between starting the world and allocating on that P, we
    // could leave allocate-black on, allow allocation to continue
    // as usual, use a ragged barrier at the beginning of sweep to
    // ensure all cached spans are swept, and then disable
    // allocate-black. However, with this approach it's difficult
    // to avoid spilling mark bits into the *next* GC cycle.
    var sg = mheap_.sweepgen;
    if (c.flushGen == sg) {
        return ;
    }
    else if (c.flushGen != sg - 2) {
        println("bad flushGen", c.flushGen, "in prepareForSweep; sweepgen", sg);
        throw("bad flushGen");
    }
    c.releaseAll();
    stackcache_clear(c);
    atomic.Store(_addr_c.flushGen, mheap_.sweepgen); // Synchronizes with gcStart
}

} // end runtime_package
