// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// Per-thread (in Go, per-P) cache for small objects.
// This includes a small object cache and local allocation stats.
// No locking needed because it is per-thread (per-P).
//
// mcaches are allocated from non-GC'd memory, so any heap pointers
// must be specially handled.
[GoType] partial struct mcache {
    internal runtime.@internal.sys_package.NotInHeap _;
    // The following members are accessed on every malloc,
    // so they are grouped here for better caching.
    internal uintptr nextSample; // trigger heap sample after allocating this many bytes
    internal uintptr scanAlloc; // bytes of scannable heap allocated
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
    internal uintptr tiny;
    internal uintptr tinyoffset;
    internal uintptr tinyAllocs;
// The rest is not accessed on every malloc.
    internal array<ж<mspan>> alloc = new(numSpanClasses); // spans to allocate from, indexed by spanClass
    internal array<stackfreelist> stackcache = new(_NumStackOrders);
    // flushGen indicates the sweepgen during which this mcache
    // was last flushed. If flushGen != mheap_.sweepgen, the spans
    // in this mcache are stale and need to the flushed so they
    // can be swept. This is done in acquirep.
    internal @internal.runtime.atomic_package.Uint32 flushGen;
}

// A gclink is a node in a linked list of blocks, like mlink,
// but it is opaque to the garbage collector.
// The GC does not trace the pointers during collection,
// and the compiler does not emit write barriers for assignments
// of gclinkptr values. Code should store references to gclinks
// as gclinkptr, not as *gclink.
[GoType] partial struct gclink {
    internal gclinkptr next;
}

[GoType("num:uintptr")] partial struct gclinkptr;

// ptr returns the *gclink form of p.
// The result should be used for accessing fields, not stored
// in other data structures.
internal static ж<gclink> ptr(this gclinkptr Δp) {
    return (ж<gclink>)(uintptr)(((@unsafe.Pointer)Δp));
}

[GoType] partial struct stackfreelist {
    internal gclinkptr list; // linked list of free stacks
    internal uintptr size;   // total size of stacks in list
}

// dummy mspan that contains no free objects.
internal static mspan emptymspan;

internal static ж<mcache> allocmcache() {
    ж<mcache> c = default!;
    systemstack(
    var cʗ2 = c;
    var mheap_ʗ2 = mheap_;
    () => {
        @lock(Ꮡmheap_ʗ2.of(mheap.Ꮡlock));
        cʗ2 = (ж<mcache>)(uintptr)(mheap_ʗ2.cachealloc.alloc());
        (~cʗ2).flushGen.Store(mheap_ʗ2.sweepgen);
        unlock(Ꮡmheap_ʗ2.of(mheap.Ꮡlock));
    });
    foreach (var (i, _) in (~c).alloc) {
        (~c).alloc[i] = Ꮡ(emptymspan);
    }
    c.val.nextSample = nextSample();
    return c;
}

// freemcache releases resources associated with this
// mcache and puts the object onto a free list.
//
// In some cases there is no way to simply release
// resources, such as statistics, so donate them to
// a different mcache (the recipient).
internal static void freemcache(ж<mcache> Ꮡc) {
    ref var c = ref Ꮡc.val;

    systemstack(
    var mheap_ʗ2 = mheap_;
    () => {
        c.releaseAll();
        stackcache_clear(Ꮡc);
        @lock(Ꮡmheap_ʗ2.of(mheap.Ꮡlock));
        mheap_ʗ2.cachealloc.free(new @unsafe.Pointer(Ꮡc));
        unlock(Ꮡmheap_ʗ2.of(mheap.Ꮡlock));
    });
}

// getMCache is a convenience function which tries to obtain an mcache.
//
// Returns nil if we're not bootstrapping or we don't have a P. The caller's
// P must not change, so we must be in a non-preemptible state.
internal static ж<mcache> getMCache(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.val;

    // Grab the mcache, since that's where stats live.
    var pp = mp.p.ptr();
    ж<mcache> c = default!;
    if (pp == nil){
        // We will be called without a P while bootstrapping,
        // in which case we use mcache0, which is set in mallocinit.
        // mcache0 is cleared when bootstrapping is complete,
        // by procresize.
        c = mcache0;
    } else {
        c = pp.val.mcache;
    }
    return c;
}

// refill acquires a new span of span class spc for c. This span will
// have at least one free object. The current span in c must be full.
//
// Must run in a non-preemptible context since otherwise the owner of
// c could change.
[GoRecv] internal static void refill(this ref mcache c, spanClass spc) {
    // Return the current cached span to the central lists.
    var s = c.alloc[spc];
    if ((~s).allocCount != (~s).nelems) {
        @throw("refill of span with free space remaining"u8);
    }
    if (s != Ꮡ(emptymspan)) {
        // Mark this span as no longer cached.
        if ((~s).sweepgen != mheap_.sweepgen + 3) {
            @throw("bad sweepgen in refill"u8);
        }
        mheap_.central[spc].mcentral.uncacheSpan(s);
        // Count up how many slots were used and record it.
        var stats = memstats.heapStats.acquire();
        var slotsUsed = ((int64)(~s).allocCount) - ((int64)(~s).allocCountBeforeCache);
        atomic.Xadd64(Ꮡ(~stats).smallAllocCount.at<uint64>(spc.sizeclass()), slotsUsed);
        // Flush tinyAllocs.
        if (spc == tinySpanClass) {
            atomic.Xadd64(Ꮡ((~stats).tinyAllocCount), ((int64)c.tinyAllocs));
            c.tinyAllocs = 0;
        }
        memstats.heapStats.release();
        // Count the allocs in inconsistent, internal stats.
        var bytesAllocated = slotsUsed * ((int64)(~s).elemsize);
        gcController.totalAlloc.Add(bytesAllocated);
        // Clear the second allocCount just to be safe.
        s.val.allocCountBeforeCache = 0;
    }
    // Get a new cached span from the central lists.
    s = mheap_.central[spc].mcentral.cacheSpan();
    if (s == nil) {
        @throw("out of memory"u8);
    }
    if ((~s).allocCount == (~s).nelems) {
        @throw("span has no free space"u8);
    }
    // Indicate that this span is cached and prevent asynchronous
    // sweeping in the next sweep phase.
    s.val.sweepgen = mheap_.sweepgen + 3;
    // Store the current alloc count for accounting later.
    s.val.allocCountBeforeCache = s.val.allocCount;
    // Update heapLive and flush scanAlloc.
    //
    // We have not yet allocated anything new into the span, but we
    // assume that all of its slots will get used, so this makes
    // heapLive an overestimate.
    //
    // When the span gets uncached, we'll fix up this overestimate
    // if necessary (see releaseAll).
    //
    // We pick an overestimate here because an underestimate leads
    // the pacer to believe that it's in better shape than it is,
    // which appears to lead to more memory used. See #53738 for
    // more details.
    var usedBytes = ((uintptr)(~s).allocCount) * (~s).elemsize;
    gcController.update(((int64)((~s).npages * pageSize)) - ((int64)usedBytes), ((int64)c.scanAlloc));
    c.scanAlloc = 0;
    c.alloc[spc] = s;
}

// allocLarge allocates a span for a large object.
[GoRecv] internal static ж<mspan> allocLarge(this ref mcache c, uintptr size, bool noscan) {
    if (size + _PageSize < size) {
        @throw("out of memory"u8);
    }
    var npages = size >> (int)(_PageShift);
    if ((uintptr)(size & _PageMask) != 0) {
        npages++;
    }
    // Deduct credit for this span allocation and sweep if
    // necessary. mHeap_Alloc will also sweep npages, so this only
    // pays the debt down to npage pages.
    deductSweepCredit(npages * _PageSize, npages);
    var spc = makeSpanClass(0, noscan);
    var s = mheap_.alloc(npages, spc);
    if (s == nil) {
        @throw("out of memory"u8);
    }
    // Count the alloc in consistent, external stats.
    var stats = memstats.heapStats.acquire();
    atomic.Xadd64(Ꮡ((~stats).largeAlloc), ((int64)(npages * pageSize)));
    atomic.Xadd64(Ꮡ((~stats).largeAllocCount), 1);
    memstats.heapStats.release();
    // Count the alloc in inconsistent, internal stats.
    gcController.totalAlloc.Add(((int64)(npages * pageSize)));
    // Update heapLive.
    gcController.update(((int64)((~s).npages * pageSize)), 0);
    // Put the large span in the mcentral swept list so that it's
    // visible to the background sweeper.
    mheap_.central[spc].mcentral.fullSwept(mheap_.sweepgen).push(s);
    s.val.limit = s.@base() + size;
    s.initHeapBits(false);
    return s;
}

[GoRecv] internal static void releaseAll(this ref mcache c) {
    // Take this opportunity to flush scanAlloc.
    var scanAlloc = ((int64)c.scanAlloc);
    c.scanAlloc = 0;
    var sg = mheap_.sweepgen;
    var dHeapLive = ((int64)0);
    ref var i = ref heap(new nint(), out var Ꮡi);

    foreach (var (i, _) in c.alloc) {
        var s = c.alloc[i];
        if (s != Ꮡ(emptymspan)) {
            var slotsUsed = ((int64)(~s).allocCount) - ((int64)(~s).allocCountBeforeCache);
            s.val.allocCountBeforeCache = 0;
            // Adjust smallAllocCount for whatever was allocated.
            var statsΔ1 = memstats.heapStats.acquire();
            atomic.Xadd64(Ꮡ(~statsΔ1).smallAllocCount.at<uint64>(((spanClass)i).sizeclass()), slotsUsed);
            memstats.heapStats.release();
            // Adjust the actual allocs in inconsistent, internal stats.
            // We assumed earlier that the full span gets allocated.
            gcController.totalAlloc.Add(slotsUsed * ((int64)(~s).elemsize));
            if ((~s).sweepgen != sg + 1) {
                // refill conservatively counted unallocated slots in gcController.heapLive.
                // Undo this.
                //
                // If this span was cached before sweep, then gcController.heapLive was totally
                // recomputed since caching this span, so we don't do this for stale spans.
                dHeapLive -= ((int64)((~s).nelems - (~s).allocCount)) * ((int64)(~s).elemsize);
            }
            // Release the span to the mcentral.
            mheap_.central[i].mcentral.uncacheSpan(s);
            c.alloc[i] = Ꮡ(emptymspan);
        }
    }
    // Clear tinyalloc pool.
    c.tiny = 0;
    c.tinyoffset = 0;
    // Flush tinyAllocs.
    var stats = memstats.heapStats.acquire();
    atomic.Xadd64(Ꮡ((~stats).tinyAllocCount), ((int64)c.tinyAllocs));
    c.tinyAllocs = 0;
    memstats.heapStats.release();
    // Update heapLive and heapScan.
    gcController.update(dHeapLive, scanAlloc);
}

// prepareForSweep flushes c if the system has entered a new sweep phase
// since c was populated. This must happen between the sweep phase
// starting and the first allocation from c.
[GoRecv] internal static void prepareForSweep(this ref mcache c) {
    // Alternatively, instead of making sure we do this on every P
    // between starting the world and allocating on that P, we
    // could leave allocate-black on, allow allocation to continue
    // as usual, use a ragged barrier at the beginning of sweep to
    // ensure all cached spans are swept, and then disable
    // allocate-black. However, with this approach it's difficult
    // to avoid spilling mark bits into the *next* GC cycle.
    var sg = mheap_.sweepgen;
    var flushGen = c.flushGen.Load();
    if (flushGen == sg){
        return;
    } else 
    if (flushGen != sg - 2) {
        println("bad flushGen", flushGen, "in prepareForSweep; sweepgen", sg);
        @throw("bad flushGen"u8);
    }
    c.releaseAll();
    stackcache_clear(c);
    c.flushGen.Store(mheap_.sweepgen);
}

// Synchronizes with gcStart

} // end runtime_package
