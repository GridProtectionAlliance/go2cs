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

// package runtime -- go2cs converted at 2020 October 08 03:20:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mcentral.go
using atomic = go.runtime.@internal.atomic_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Central list of free objects of a given size.
        //
        //go:notinheap
        private partial struct mcentral
        {
            public mutex @lock;
            public spanClass spanclass; // For !go115NewMCentralImpl.
            public mSpanList nonempty; // list of spans with a free object, ie a nonempty free list
            public mSpanList empty; // list of spans with no free objects (or cached in an mcache)

// partial and full contain two mspan sets: one of swept in-use
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

// nmalloc is the cumulative count of objects allocated from
// this mcentral, assuming all spans in mcaches are
// fully-allocated. Written atomically, read under STW.
            public ulong nmalloc;
        }

        // Initialize a single central free list.
        private static void init(this ptr<mcentral> _addr_c, spanClass spc)
        {
            ref mcentral c = ref _addr_c.val;

            c.spanclass = spc;
            if (go115NewMCentralImpl)
            {
                lockInit(_addr_c.partial[0L].spineLock, lockRankSpanSetSpine);
                lockInit(_addr_c.partial[1L].spineLock, lockRankSpanSetSpine);
                lockInit(_addr_c.full[0L].spineLock, lockRankSpanSetSpine);
                lockInit(_addr_c.full[1L].spineLock, lockRankSpanSetSpine);
            }
            else
            {
                c.nonempty.init();
                c.empty.init();
                lockInit(_addr_c.@lock, lockRankMcentral);
            }

        }

        // partialUnswept returns the spanSet which holds partially-filled
        // unswept spans for this sweepgen.
        private static ptr<spanSet> partialUnswept(this ptr<mcentral> _addr_c, uint sweepgen)
        {
            ref mcentral c = ref _addr_c.val;

            return _addr__addr_c.partial[1L - sweepgen / 2L % 2L]!;
        }

        // partialSwept returns the spanSet which holds partially-filled
        // swept spans for this sweepgen.
        private static ptr<spanSet> partialSwept(this ptr<mcentral> _addr_c, uint sweepgen)
        {
            ref mcentral c = ref _addr_c.val;

            return _addr__addr_c.partial[sweepgen / 2L % 2L]!;
        }

        // fullUnswept returns the spanSet which holds unswept spans without any
        // free slots for this sweepgen.
        private static ptr<spanSet> fullUnswept(this ptr<mcentral> _addr_c, uint sweepgen)
        {
            ref mcentral c = ref _addr_c.val;

            return _addr__addr_c.full[1L - sweepgen / 2L % 2L]!;
        }

        // fullSwept returns the spanSet which holds swept spans without any
        // free slots for this sweepgen.
        private static ptr<spanSet> fullSwept(this ptr<mcentral> _addr_c, uint sweepgen)
        {
            ref mcentral c = ref _addr_c.val;

            return _addr__addr_c.full[sweepgen / 2L % 2L]!;
        }

        // Allocate a span to use in an mcache.
        private static ptr<mspan> cacheSpan(this ptr<mcentral> _addr_c)
        {
            ref mcentral c = ref _addr_c.val;

            if (!go115NewMCentralImpl)
            {
                return _addr_c.oldCacheSpan()!;
            } 
            // Deduct credit for this span allocation and sweep if necessary.
            var spanBytes = uintptr(class_to_allocnpages[c.spanclass.sizeclass()]) * _PageSize;
            deductSweepCredit(spanBytes, 0L);

            var sg = mheap_.sweepgen;

            var traceDone = false;
            if (trace.enabled)
            {
                traceGCSweepStart();
            } 

            // If we sweep spanBudget spans without finding any free
            // space, just allocate a fresh span. This limits the amount
            // of time we can spend trying to find free space and
            // amortizes the cost of small object sweeping over the
            // benefit of having a full free span to allocate from. By
            // setting this to 100, we limit the space overhead to 1%.
            //
            // TODO(austin,mknyszek): This still has bad worst-case
            // throughput. For example, this could find just one free slot
            // on the 100th swept span. That limits allocation latency, but
            // still has very poor throughput. We could instead keep a
            // running free-to-used budget and switch to fresh span
            // allocation if the budget runs low.
            long spanBudget = 100L;

            ptr<mspan> s; 

            // Try partial swept spans first.
            s = c.partialSwept(sg).pop();

            if (s != null)
            {
                goto havespan;
            } 

            // Now try partial unswept spans.
            while (spanBudget >= 0L)
            {
                s = c.partialUnswept(sg).pop();
                if (s == null)
                {
                    break;
                spanBudget--;
                }

                if (atomic.Load(_addr_s.sweepgen) == sg - 2L && atomic.Cas(_addr_s.sweepgen, sg - 2L, sg - 1L))
                { 
                    // We got ownership of the span, so let's sweep it and use it.
                    s.sweep(true);
                    goto havespan;

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
 
            // Now try full unswept spans, sweeping them and putting them into the
            // right list if we fail to get a span.
            while (spanBudget >= 0L)
            {
                s = c.fullUnswept(sg).pop();
                if (s == null)
                {
                    break;
                spanBudget--;
                }

                if (atomic.Load(_addr_s.sweepgen) == sg - 2L && atomic.Cas(_addr_s.sweepgen, sg - 2L, sg - 1L))
                { 
                    // We got ownership of the span, so let's sweep it.
                    s.sweep(true); 
                    // Check if there's any free space.
                    var freeIndex = s.nextFreeIndex();
                    if (freeIndex != s.nelems)
                    {
                        s.freeindex = freeIndex;
                        goto havespan;
                    } 
                    // Add it to the swept list, because sweeping didn't give us any free space.
                    c.fullSwept(sg).push(s);

                } 
                // See comment for partial unswept spans.
            }

            if (trace.enabled)
            {
                traceGCSweepDone();
                traceDone = true;
            } 

            // We failed to get a span from the mcentral so get one from mheap.
            s = c.grow();
            if (s == null)
            {
                return _addr_null!;
            } 

            // At this point s is a span that should have free slots.
havespan:
            if (trace.enabled && !traceDone)
            {
                traceGCSweepDone();
            }

            var n = int(s.nelems) - int(s.allocCount);
            if (n == 0L || s.freeindex == s.nelems || uintptr(s.allocCount) == s.nelems)
            {
                throw("span has no free objects");
            } 
            // Assume all objects from this span will be allocated in the
            // mcache. If it gets uncached, we'll adjust this.
            atomic.Xadd64(_addr_c.nmalloc, int64(n));
            var usedBytes = uintptr(s.allocCount) * s.elemsize;
            atomic.Xadd64(_addr_memstats.heap_live, int64(spanBytes) - int64(usedBytes));
            if (trace.enabled)
            { 
                // heap_live changed.
                traceHeapAlloc();

            }

            if (gcBlackenEnabled != 0L)
            { 
                // heap_live changed.
                gcController.revise();

            }

            var freeByteBase = s.freeindex & ~(64L - 1L);
            var whichByte = freeByteBase / 8L; 
            // Init alloc bits cache.
            s.refillAllocCache(whichByte); 

            // Adjust the allocCache so that s.freeindex corresponds to the low bit in
            // s.allocCache.
            s.allocCache >>= s.freeindex % 64L;

            return _addr_s!;

        }

        // Allocate a span to use in an mcache.
        //
        // For !go115NewMCentralImpl.
        private static ptr<mspan> oldCacheSpan(this ptr<mcentral> _addr_c)
        {
            ref mcentral c = ref _addr_c.val;
 
            // Deduct credit for this span allocation and sweep if necessary.
            var spanBytes = uintptr(class_to_allocnpages[c.spanclass.sizeclass()]) * _PageSize;
            deductSweepCredit(spanBytes, 0L);

            lock(_addr_c.@lock);
            var traceDone = false;
            if (trace.enabled)
            {
                traceGCSweepStart();
            }

            var sg = mheap_.sweepgen;
retry:
            ptr<mspan> s;
            s = c.nonempty.first;

            while (s != null)
            {
                if (s.sweepgen == sg - 2L && atomic.Cas(_addr_s.sweepgen, sg - 2L, sg - 1L))
                {
                    c.nonempty.remove(s);
                    c.empty.insertBack(s);
                    unlock(_addr_c.@lock);
                    s.sweep(true);
                    goto havespan;
                s = s.next;
                }

                if (s.sweepgen == sg - 1L)
                { 
                    // the span is being swept by background sweeper, skip
                    continue;

                } 
                // we have a nonempty span that does not require sweeping, allocate from it
                c.nonempty.remove(s);
                c.empty.insertBack(s);
                unlock(_addr_c.@lock);
                goto havespan;

            }


            s = c.empty.first;

            while (s != null)
            {
                if (s.sweepgen == sg - 2L && atomic.Cas(_addr_s.sweepgen, sg - 2L, sg - 1L))
                { 
                    // we have an empty span that requires sweeping,
                    // sweep it and see if we can free some space in it
                    c.empty.remove(s); 
                    // swept spans are at the end of the list
                    c.empty.insertBack(s);
                    unlock(_addr_c.@lock);
                    s.sweep(true);
                    var freeIndex = s.nextFreeIndex();
                    if (freeIndex != s.nelems)
                    {
                        s.freeindex = freeIndex;
                        goto havespan;
                s = s.next;
                    }

                    lock(_addr_c.@lock); 
                    // the span is still empty after sweep
                    // it is already in the empty list, so just retry
                    goto retry;

                }

                if (s.sweepgen == sg - 1L)
                { 
                    // the span is being swept by background sweeper, skip
                    continue;

                } 
                // already swept empty span,
                // all subsequent ones must also be either swept or in process of sweeping
                break;

            }

            if (trace.enabled)
            {
                traceGCSweepDone();
                traceDone = true;
            }

            unlock(_addr_c.@lock); 

            // Replenish central list if empty.
            s = c.grow();
            if (s == null)
            {
                return _addr_null!;
            }

            lock(_addr_c.@lock);
            c.empty.insertBack(s);
            unlock(_addr_c.@lock); 

            // At this point s is a non-empty span, queued at the end of the empty list,
            // c is unlocked.
havespan:
            if (trace.enabled && !traceDone)
            {
                traceGCSweepDone();
            }

            var n = int(s.nelems) - int(s.allocCount);
            if (n == 0L || s.freeindex == s.nelems || uintptr(s.allocCount) == s.nelems)
            {
                throw("span has no free objects");
            } 
            // Assume all objects from this span will be allocated in the
            // mcache. If it gets uncached, we'll adjust this.
            atomic.Xadd64(_addr_c.nmalloc, int64(n));
            var usedBytes = uintptr(s.allocCount) * s.elemsize;
            atomic.Xadd64(_addr_memstats.heap_live, int64(spanBytes) - int64(usedBytes));
            if (trace.enabled)
            { 
                // heap_live changed.
                traceHeapAlloc();

            }

            if (gcBlackenEnabled != 0L)
            { 
                // heap_live changed.
                gcController.revise();

            }

            var freeByteBase = s.freeindex & ~(64L - 1L);
            var whichByte = freeByteBase / 8L; 
            // Init alloc bits cache.
            s.refillAllocCache(whichByte); 

            // Adjust the allocCache so that s.freeindex corresponds to the low bit in
            // s.allocCache.
            s.allocCache >>= s.freeindex % 64L;

            return _addr_s!;

        }

        // Return span from an mcache.
        //
        // s must have a span class corresponding to this
        // mcentral and it must not be empty.
        private static void uncacheSpan(this ptr<mcentral> _addr_c, ptr<mspan> _addr_s)
        {
            ref mcentral c = ref _addr_c.val;
            ref mspan s = ref _addr_s.val;

            if (!go115NewMCentralImpl)
            {
                c.oldUncacheSpan(s);
                return ;
            }

            if (s.allocCount == 0L)
            {
                throw("uncaching span but s.allocCount == 0");
            }

            var sg = mheap_.sweepgen;
            var stale = s.sweepgen == sg + 1L; 

            // Fix up sweepgen.
            if (stale)
            { 
                // Span was cached before sweep began. It's our
                // responsibility to sweep it.
                //
                // Set sweepgen to indicate it's not cached but needs
                // sweeping and can't be allocated from. sweep will
                // set s.sweepgen to indicate s is swept.
                atomic.Store(_addr_s.sweepgen, sg - 1L);

            }
            else
            { 
                // Indicate that s is no longer cached.
                atomic.Store(_addr_s.sweepgen, sg);

            }

            var n = int(s.nelems) - int(s.allocCount); 

            // Fix up statistics.
            if (n > 0L)
            { 
                // cacheSpan updated alloc assuming all objects on s
                // were going to be allocated. Adjust for any that
                // weren't. We must do this before potentially
                // sweeping the span.
                atomic.Xadd64(_addr_c.nmalloc, -int64(n));

                if (!stale)
                { 
                    // (*mcentral).cacheSpan conservatively counted
                    // unallocated slots in heap_live. Undo this.
                    //
                    // If this span was cached before sweep, then
                    // heap_live was totally recomputed since
                    // caching this span, so we don't do this for
                    // stale spans.
                    atomic.Xadd64(_addr_memstats.heap_live, -int64(n) * int64(s.elemsize));

                }

            } 

            // Put the span in the appropriate place.
            if (stale)
            { 
                // It's stale, so just sweep it. Sweeping will put it on
                // the right list.
                s.sweep(false);

            }
            else
            {
                if (n > 0L)
                { 
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

        // Return span from an mcache.
        //
        // For !go115NewMCentralImpl.
        private static void oldUncacheSpan(this ptr<mcentral> _addr_c, ptr<mspan> _addr_s)
        {
            ref mcentral c = ref _addr_c.val;
            ref mspan s = ref _addr_s.val;

            if (s.allocCount == 0L)
            {
                throw("uncaching span but s.allocCount == 0");
            }

            var sg = mheap_.sweepgen;
            var stale = s.sweepgen == sg + 1L;
            if (stale)
            { 
                // Span was cached before sweep began. It's our
                // responsibility to sweep it.
                //
                // Set sweepgen to indicate it's not cached but needs
                // sweeping and can't be allocated from. sweep will
                // set s.sweepgen to indicate s is swept.
                atomic.Store(_addr_s.sweepgen, sg - 1L);

            }
            else
            { 
                // Indicate that s is no longer cached.
                atomic.Store(_addr_s.sweepgen, sg);

            }

            var n = int(s.nelems) - int(s.allocCount);
            if (n > 0L)
            { 
                // cacheSpan updated alloc assuming all objects on s
                // were going to be allocated. Adjust for any that
                // weren't. We must do this before potentially
                // sweeping the span.
                atomic.Xadd64(_addr_c.nmalloc, -int64(n));

                lock(_addr_c.@lock);
                c.empty.remove(s);
                c.nonempty.insert(s);
                if (!stale)
                { 
                    // mCentral_CacheSpan conservatively counted
                    // unallocated slots in heap_live. Undo this.
                    //
                    // If this span was cached before sweep, then
                    // heap_live was totally recomputed since
                    // caching this span, so we don't do this for
                    // stale spans.
                    atomic.Xadd64(_addr_memstats.heap_live, -int64(n) * int64(s.elemsize));

                }

                unlock(_addr_c.@lock);

            }

            if (stale)
            { 
                // Now that s is in the right mcentral list, we can
                // sweep it.
                s.sweep(false);

            }

        }

        // freeSpan updates c and s after sweeping s.
        // It sets s's sweepgen to the latest generation,
        // and, based on the number of free objects in s,
        // moves s to the appropriate list of c or returns it
        // to the heap.
        // freeSpan reports whether s was returned to the heap.
        // If preserve=true, it does not move s (the caller
        // must take care of it).
        //
        // For !go115NewMCentralImpl.
        private static bool freeSpan(this ptr<mcentral> _addr_c, ptr<mspan> _addr_s, bool preserve, bool wasempty)
        {
            ref mcentral c = ref _addr_c.val;
            ref mspan s = ref _addr_s.val;

            {
                var sg = mheap_.sweepgen;

                if (s.sweepgen == sg + 1L || s.sweepgen == sg + 3L)
                {
                    throw("freeSpan given cached span");
                }

            }

            s.needzero = 1L;

            if (preserve)
            { 
                // preserve is set only when called from (un)cacheSpan above,
                // the span must be in the empty list.
                if (!s.inList())
                {
                    throw("can't preserve unlinked span");
                }

                atomic.Store(_addr_s.sweepgen, mheap_.sweepgen);
                return false;

            }

            lock(_addr_c.@lock); 

            // Move to nonempty if necessary.
            if (wasempty)
            {
                c.empty.remove(s);
                c.nonempty.insert(s);
            } 

            // delay updating sweepgen until here. This is the signal that
            // the span may be used in an mcache, so it must come after the
            // linked list operations above (actually, just after the
            // lock of c above.)
            atomic.Store(_addr_s.sweepgen, mheap_.sweepgen);

            if (s.allocCount != 0L)
            {
                unlock(_addr_c.@lock);
                return false;
            }

            c.nonempty.remove(s);
            unlock(_addr_c.@lock);
            mheap_.freeSpan(s);
            return true;

        }

        // grow allocates a new empty span from the heap and initializes it for c's size class.
        private static ptr<mspan> grow(this ptr<mcentral> _addr_c)
        {
            ref mcentral c = ref _addr_c.val;

            var npages = uintptr(class_to_allocnpages[c.spanclass.sizeclass()]);
            var size = uintptr(class_to_size[c.spanclass.sizeclass()]);

            var s = mheap_.alloc(npages, c.spanclass, true);
            if (s == null)
            {
                return _addr_null!;
            } 

            // Use division by multiplication and shifts to quickly compute:
            // n := (npages << _PageShift) / size
            var n = (npages << (int)(_PageShift)) >> (int)(s.divShift) * uintptr(s.divMul) >> (int)(s.divShift2);
            s.limit = s.@base() + size * n;
            heapBitsForAddr(s.@base()).initSpan(s);
            return _addr_s!;

        }
    }
}
