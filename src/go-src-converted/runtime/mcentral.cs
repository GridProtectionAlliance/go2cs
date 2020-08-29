// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Central free lists.
//
// See malloc.go for an overview.
//
// The MCentral doesn't actually contain the list of free objects; the MSpan does.
// Each MCentral is two lists of MSpans: those with free objects (c->nonempty)
// and those that are completely allocated (c->empty).

// package runtime -- go2cs converted at 2020 August 29 08:17:44 UTC
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
            public spanClass spanclass;
            public mSpanList nonempty; // list of spans with a free object, ie a nonempty free list
            public mSpanList empty; // list of spans with no free objects (or cached in an mcache)

// nmalloc is the cumulative count of objects allocated from
// this mcentral, assuming all spans in mcaches are
// fully-allocated. Written atomically, read under STW.
            public ulong nmalloc;
        }

        // Initialize a single central free list.
        private static void init(this ref mcentral c, spanClass spc)
        {
            c.spanclass = spc;
            c.nonempty.init();
            c.empty.init();
        }

        // Allocate a span to use in an MCache.
        private static ref mspan cacheSpan(this ref mcentral c)
        { 
            // Deduct credit for this span allocation and sweep if necessary.
            var spanBytes = uintptr(class_to_allocnpages[c.spanclass.sizeclass()]) * _PageSize;
            deductSweepCredit(spanBytes, 0L);

            lock(ref c.@lock);
            var traceDone = false;
            if (trace.enabled)
            {
                traceGCSweepStart();
            }
            var sg = mheap_.sweepgen;
retry:
            ref mspan s = default;
            s = c.nonempty.first;

            while (s != null)
            {
                if (s.sweepgen == sg - 2L && atomic.Cas(ref s.sweepgen, sg - 2L, sg - 1L))
                {
                    c.nonempty.remove(s);
                    c.empty.insertBack(s);
                    unlock(ref c.@lock);
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
                unlock(ref c.@lock);
                goto havespan;
            }


            s = c.empty.first;

            while (s != null)
            {
                if (s.sweepgen == sg - 2L && atomic.Cas(ref s.sweepgen, sg - 2L, sg - 1L))
                { 
                    // we have an empty span that requires sweeping,
                    // sweep it and see if we can free some space in it
                    c.empty.remove(s); 
                    // swept spans are at the end of the list
                    c.empty.insertBack(s);
                    unlock(ref c.@lock);
                    s.sweep(true);
                    var freeIndex = s.nextFreeIndex();
                    if (freeIndex != s.nelems)
                    {
                        s.freeindex = freeIndex;
                        goto havespan;
                s = s.next;
                    }
                    lock(ref c.@lock); 
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
            unlock(ref c.@lock); 

            // Replenish central list if empty.
            s = c.grow();
            if (s == null)
            {
                return null;
            }
            lock(ref c.@lock);
            c.empty.insertBack(s);
            unlock(ref c.@lock); 

            // At this point s is a non-empty span, queued at the end of the empty list,
            // c is unlocked.
havespan:
            if (trace.enabled && !traceDone)
            {
                traceGCSweepDone();
            }
            var cap = int32((s.npages << (int)(_PageShift)) / s.elemsize);
            var n = cap - int32(s.allocCount);
            if (n == 0L || s.freeindex == s.nelems || uintptr(s.allocCount) == s.nelems)
            {
                throw("span has no free objects");
            } 
            // Assume all objects from this span will be allocated in the
            // mcache. If it gets uncached, we'll adjust this.
            atomic.Xadd64(ref c.nmalloc, int64(n));
            var usedBytes = uintptr(s.allocCount) * s.elemsize;
            atomic.Xadd64(ref memstats.heap_live, int64(spanBytes) - int64(usedBytes));
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
            s.incache = true;
            var freeByteBase = s.freeindex & ~(64L - 1L);
            var whichByte = freeByteBase / 8L; 
            // Init alloc bits cache.
            s.refillAllocCache(whichByte); 

            // Adjust the allocCache so that s.freeindex corresponds to the low bit in
            // s.allocCache.
            s.allocCache >>= s.freeindex % 64L;

            return s;
        }

        // Return span from an MCache.
        private static void uncacheSpan(this ref mcentral c, ref mspan s)
        {
            lock(ref c.@lock);

            s.incache = false;

            if (s.allocCount == 0L)
            {
                throw("uncaching span but s.allocCount == 0");
            }
            var cap = int32((s.npages << (int)(_PageShift)) / s.elemsize);
            var n = cap - int32(s.allocCount);
            if (n > 0L)
            {
                c.empty.remove(s);
                c.nonempty.insert(s); 
                // mCentral_CacheSpan conservatively counted
                // unallocated slots in heap_live. Undo this.
                atomic.Xadd64(ref memstats.heap_live, -int64(n) * int64(s.elemsize)); 
                // cacheSpan updated alloc assuming all objects on s
                // were going to be allocated. Adjust for any that
                // weren't.
                atomic.Xadd64(ref c.nmalloc, -int64(n));
            }
            unlock(ref c.@lock);
        }

        // freeSpan updates c and s after sweeping s.
        // It sets s's sweepgen to the latest generation,
        // and, based on the number of free objects in s,
        // moves s to the appropriate list of c or returns it
        // to the heap.
        // freeSpan returns true if s was returned to the heap.
        // If preserve=true, it does not move s (the caller
        // must take care of it).
        private static bool freeSpan(this ref mcentral c, ref mspan s, bool preserve, bool wasempty)
        {
            if (s.incache)
            {
                throw("freeSpan given cached span");
            }
            s.needzero = 1L;

            if (preserve)
            { 
                // preserve is set only when called from MCentral_CacheSpan above,
                // the span must be in the empty list.
                if (!s.inList())
                {
                    throw("can't preserve unlinked span");
                }
                atomic.Store(ref s.sweepgen, mheap_.sweepgen);
                return false;
            }
            lock(ref c.@lock); 

            // Move to nonempty if necessary.
            if (wasempty)
            {
                c.empty.remove(s);
                c.nonempty.insert(s);
            } 

            // delay updating sweepgen until here. This is the signal that
            // the span may be used in an MCache, so it must come after the
            // linked list operations above (actually, just after the
            // lock of c above.)
            atomic.Store(ref s.sweepgen, mheap_.sweepgen);

            if (s.allocCount != 0L)
            {
                unlock(ref c.@lock);
                return false;
            }
            c.nonempty.remove(s);
            unlock(ref c.@lock);
            mheap_.freeSpan(s, 0L);
            return true;
        }

        // grow allocates a new empty span from the heap and initializes it for c's size class.
        private static ref mspan grow(this ref mcentral c)
        {
            var npages = uintptr(class_to_allocnpages[c.spanclass.sizeclass()]);
            var size = uintptr(class_to_size[c.spanclass.sizeclass()]);
            var n = (npages << (int)(_PageShift)) / size;

            var s = mheap_.alloc(npages, c.spanclass, false, true);
            if (s == null)
            {
                return null;
            }
            var p = s.@base();
            s.limit = p + size * n;

            heapBitsForSpan(s.@base()).initSpan(s);
            return s;
        }
    }
}
