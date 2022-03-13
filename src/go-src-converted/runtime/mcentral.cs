// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Central free lists.
//
// See malloc.go for an overview.
//
// The mcentral doesn't actually contain the list of free objects; the mspan does.
// Each mcentral is two lists of mspans: those with free objects (c->nonempty)
// and those that are completely allocated (c->empty).

// package runtime -- go2cs converted at 2022 March 13 05:25:05 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mcentral.go
namespace go;

using atomic = runtime.@internal.atomic_package;

public static partial class runtime_package {

// Central list of free objects of a given size.
//
//go:notinheap
private partial struct mcentral {
    public spanClass spanclass; // partial and full contain two mspan sets: one of swept in-use
// spans, and one of unswept in-use spans. These two trade
// roles on each GC cycle. The unswept set is drained either by
// allocation or by the background sweeper in every GC cycle,
// so only two roles are necessary.
//
// sweepgen is increased by 2 on each GC cycle, so the swept
// spans are in partial[sweepgen/2%2] and the unswept spans are in
// partial[1-sweepgen/2%2]. Sweeping pops spans from the
// unswept set and pushes spans that are still in-use on the
// swept set. Likewise, allocating an in-use span pushes it
// on the swept set.
//
// Some parts of the sweeper can sweep arbitrary spans, and hence
// can't remove them from the unswept set, but will add the span
// to the appropriate swept list. As a result, the parts of the
// sweeper and mcentral that do consume from the unswept list may
// encounter swept spans, and these should be ignored.
    public array<spanSet> partial; // list of spans with a free object
    public array<spanSet> full; // list of spans with no free objects
}

// Initialize a single central free list.
private static void init(this ptr<mcentral> _addr_c, spanClass spc) {
    ref mcentral c = ref _addr_c.val;

    c.spanclass = spc;
    lockInit(_addr_c.partial[0].spineLock, lockRankSpanSetSpine);
    lockInit(_addr_c.partial[1].spineLock, lockRankSpanSetSpine);
    lockInit(_addr_c.full[0].spineLock, lockRankSpanSetSpine);
    lockInit(_addr_c.full[1].spineLock, lockRankSpanSetSpine);
}

// partialUnswept returns the spanSet which holds partially-filled
// unswept spans for this sweepgen.
private static ptr<spanSet> partialUnswept(this ptr<mcentral> _addr_c, uint sweepgen) {
    ref mcentral c = ref _addr_c.val;

    return _addr__addr_c.partial[1 - sweepgen / 2 % 2]!;
}

// partialSwept returns the spanSet which holds partially-filled
// swept spans for this sweepgen.
private static ptr<spanSet> partialSwept(this ptr<mcentral> _addr_c, uint sweepgen) {
    ref mcentral c = ref _addr_c.val;

    return _addr__addr_c.partial[sweepgen / 2 % 2]!;
}

// fullUnswept returns the spanSet which holds unswept spans without any
// free slots for this sweepgen.
private static ptr<spanSet> fullUnswept(this ptr<mcentral> _addr_c, uint sweepgen) {
    ref mcentral c = ref _addr_c.val;

    return _addr__addr_c.full[1 - sweepgen / 2 % 2]!;
}

// fullSwept returns the spanSet which holds swept spans without any
// free slots for this sweepgen.
private static ptr<spanSet> fullSwept(this ptr<mcentral> _addr_c, uint sweepgen) {
    ref mcentral c = ref _addr_c.val;

    return _addr__addr_c.full[sweepgen / 2 % 2]!;
}

// Allocate a span to use in an mcache.
private static ptr<mspan> cacheSpan(this ptr<mcentral> _addr_c) {
    ref mcentral c = ref _addr_c.val;
 
    // Deduct credit for this span allocation and sweep if necessary.
    var spanBytes = uintptr(class_to_allocnpages[c.spanclass.sizeclass()]) * _PageSize;
    deductSweepCredit(spanBytes, 0);

    var traceDone = false;
    if (trace.enabled) {
        traceGCSweepStart();
    }
    nint spanBudget = 100;

    ptr<mspan> s;
    var sl = newSweepLocker();
    var sg = sl.sweepGen; 

    // Try partial swept spans first.
    s = c.partialSwept(sg).pop();

    if (s != null) {
        goto havespan;
    }
    while (spanBudget >= 0) {
        s = c.partialUnswept(sg).pop();
        if (s == null) {
            break;
        spanBudget--;
        }
        {
            ptr<mspan> s__prev1 = s;

            var (s, ok) = sl.tryAcquire(s);

            if (ok) { 
                // We got ownership of the span, so let's sweep it and use it.
                s.sweep(true);
                sl.dispose();
                goto havespan;
            } 
            // We failed to get ownership of the span, which means it's being or
            // has been swept by an asynchronous sweeper that just couldn't remove it
            // from the unswept list. That sweeper took ownership of the span and
            // responsibility for either freeing it to the heap or putting it on the
            // right swept list. Either way, we should just ignore it (and it's unsafe
            // for us to do anything else).

            s = s__prev1;

        } 
        // We failed to get ownership of the span, which means it's being or
        // has been swept by an asynchronous sweeper that just couldn't remove it
        // from the unswept list. That sweeper took ownership of the span and
        // responsibility for either freeing it to the heap or putting it on the
        // right swept list. Either way, we should just ignore it (and it's unsafe
        // for us to do anything else).
    } 
    // Now try full unswept spans, sweeping them and putting them into the
    // right list if we fail to get a span.
    while (spanBudget >= 0) {
        s = c.fullUnswept(sg).pop();
        if (s == null) {
            break;
        spanBudget--;
        }
        {
            ptr<mspan> s__prev1 = s;

            (s, ok) = sl.tryAcquire(s);

            if (ok) { 
                // We got ownership of the span, so let's sweep it.
                s.sweep(true); 
                // Check if there's any free space.
                var freeIndex = s.nextFreeIndex();
                if (freeIndex != s.nelems) {
                    s.freeindex = freeIndex;
                    sl.dispose();
                    goto havespan;
                } 
                // Add it to the swept list, because sweeping didn't give us any free space.
                c.fullSwept(sg).push(s.mspan);
            } 
            // See comment for partial unswept spans.

            s = s__prev1;

        } 
        // See comment for partial unswept spans.
    }
    sl.dispose();
    if (trace.enabled) {
        traceGCSweepDone();
        traceDone = true;
    }
    s = c.grow();
    if (s == null) {
        return _addr_null!;
    }
havespan:
    if (trace.enabled && !traceDone) {
        traceGCSweepDone();
    }
    var n = int(s.nelems) - int(s.allocCount);
    if (n == 0 || s.freeindex == s.nelems || uintptr(s.allocCount) == s.nelems) {
        throw("span has no free objects");
    }
    var freeByteBase = s.freeindex & ~(64 - 1);
    var whichByte = freeByteBase / 8; 
    // Init alloc bits cache.
    s.refillAllocCache(whichByte); 

    // Adjust the allocCache so that s.freeindex corresponds to the low bit in
    // s.allocCache.
    s.allocCache>>=s.freeindex % 64;

    return _addr_s!;
}

// Return span from an mcache.
//
// s must have a span class corresponding to this
// mcentral and it must not be empty.
private static void uncacheSpan(this ptr<mcentral> _addr_c, ptr<mspan> _addr_s) {
    ref mcentral c = ref _addr_c.val;
    ref mspan s = ref _addr_s.val;

    if (s.allocCount == 0) {
        throw("uncaching span but s.allocCount == 0");
    }
    var sg = mheap_.sweepgen;
    var stale = s.sweepgen == sg + 1; 

    // Fix up sweepgen.
    if (stale) { 
        // Span was cached before sweep began. It's our
        // responsibility to sweep it.
        //
        // Set sweepgen to indicate it's not cached but needs
        // sweeping and can't be allocated from. sweep will
        // set s.sweepgen to indicate s is swept.
        atomic.Store(_addr_s.sweepgen, sg - 1);
    }
    else
 { 
        // Indicate that s is no longer cached.
        atomic.Store(_addr_s.sweepgen, sg);
    }
    if (stale) { 
        // It's stale, so just sweep it. Sweeping will put it on
        // the right list.
        //
        // We don't use a sweepLocker here. Stale cached spans
        // aren't in the global sweep lists, so mark termination
        // itself holds up sweep completion until all mcaches
        // have been swept.
        sweepLocked ss = new sweepLocked(s);
        ss.sweep(false);
    }
    else
 {
        if (int(s.nelems) - int(s.allocCount) > 0) { 
            // Put it back on the partial swept list.
            c.partialSwept(sg).push(s);
        }
        else
 { 
            // There's no free space and it's not stale, so put it on the
            // full swept list.
            c.fullSwept(sg).push(s);
        }
    }
}

// grow allocates a new empty span from the heap and initializes it for c's size class.
private static ptr<mspan> grow(this ptr<mcentral> _addr_c) {
    ref mcentral c = ref _addr_c.val;

    var npages = uintptr(class_to_allocnpages[c.spanclass.sizeclass()]);
    var size = uintptr(class_to_size[c.spanclass.sizeclass()]);

    var (s, _) = mheap_.alloc(npages, c.spanclass, true);
    if (s == null) {
        return _addr_null!;
    }
    var n = s.divideByElemSize(npages << (int)(_PageShift));
    s.limit = s.@base() + size * n;
    heapBitsForAddr(s.@base()).initSpan(s);
    return _addr_s!;
}

} // end runtime_package
