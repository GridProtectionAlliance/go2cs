// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Garbage collector: sweeping

// package runtime -- go2cs converted at 2020 August 29 08:18:11 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mgcsweep.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private static sweepdata sweep = default;

        // State of background sweep.
        private partial struct sweepdata
        {
            public mutex @lock;
            public ptr<g> g;
            public bool parked;
            public bool started;
            public uint nbgsweep;
            public uint npausesweep;
        }

        // finishsweep_m ensures that all spans are swept.
        //
        // The world must be stopped. This ensures there are no sweeps in
        // progress.
        //
        //go:nowritebarrier
        private static void finishsweep_m()
        { 
            // Sweeping must be complete before marking commences, so
            // sweep any unswept spans. If this is a concurrent GC, there
            // shouldn't be any spans left to sweep, so this should finish
            // instantly. If GC was forced before the concurrent sweep
            // finished, there may be spans to sweep.
            while (sweepone() != ~uintptr(0L))
            {
                sweep.npausesweep++;
            }


            nextMarkBitArenaEpoch();
        }

        private static void bgsweep(channel<long> c)
        {
            sweep.g = getg();

            lock(ref sweep.@lock);
            sweep.parked = true;
            c.Send(1L);
            goparkunlock(ref sweep.@lock, "GC sweep wait", traceEvGoBlock, 1L);

            while (true)
            {
                while (gosweepone() != ~uintptr(0L))
                {
                    sweep.nbgsweep++;
                    Gosched();
                }

                while (freeSomeWbufs(true))
                {
                    Gosched();
                }

                lock(ref sweep.@lock);
                if (!gosweepdone())
                { 
                    // This can happen if a GC runs between
                    // gosweepone returning ^0 above
                    // and the lock being acquired.
                    unlock(ref sweep.@lock);
                    continue;
                }
                sweep.parked = true;
                goparkunlock(ref sweep.@lock, "GC sweep wait", traceEvGoBlock, 1L);
            }

        }

        // sweeps one span
        // returns number of pages returned to heap, or ^uintptr(0) if there is nothing to sweep
        //go:nowritebarrier
        private static System.UIntPtr sweepone()
        {
            var _g_ = getg();
            var sweepRatio = mheap_.sweepPagesPerByte; // For debugging

            // increment locks to ensure that the goroutine is not preempted
            // in the middle of sweep thus leaving the span in an inconsistent state for next GC
            _g_.m.locks++;
            if (atomic.Load(ref mheap_.sweepdone) != 0L)
            {
                _g_.m.locks--;
                return ~uintptr(0L);
            }
            atomic.Xadd(ref mheap_.sweepers, +1L);

            var npages = ~uintptr(0L);
            var sg = mheap_.sweepgen;
            while (true)
            {
                var s = mheap_.sweepSpans[1L - sg / 2L % 2L].pop();
                if (s == null)
                {
                    atomic.Store(ref mheap_.sweepdone, 1L);
                    break;
                }
                if (s.state != mSpanInUse)
                { 
                    // This can happen if direct sweeping already
                    // swept this span, but in that case the sweep
                    // generation should always be up-to-date.
                    if (s.sweepgen != sg)
                    {
                        print("runtime: bad span s.state=", s.state, " s.sweepgen=", s.sweepgen, " sweepgen=", sg, "\n");
                        throw("non in-use span in unswept list");
                    }
                    continue;
                }
                if (s.sweepgen != sg - 2L || !atomic.Cas(ref s.sweepgen, sg - 2L, sg - 1L))
                {
                    continue;
                }
                npages = s.npages;
                if (!s.sweep(false))
                { 
                    // Span is still in-use, so this returned no
                    // pages to the heap and the span needs to
                    // move to the swept in-use list.
                    npages = 0L;
                }
                break;
            } 

            // Decrement the number of active sweepers and if this is the
            // last one print trace information.
 

            // Decrement the number of active sweepers and if this is the
            // last one print trace information.
            if (atomic.Xadd(ref mheap_.sweepers, -1L) == 0L && atomic.Load(ref mheap_.sweepdone) != 0L)
            {
                if (debug.gcpacertrace > 0L)
                {
                    print("pacer: sweep done at heap size ", memstats.heap_live >> (int)(20L), "MB; allocated ", (memstats.heap_live - mheap_.sweepHeapLiveBasis) >> (int)(20L), "MB during sweep; swept ", mheap_.pagesSwept, " pages at ", sweepRatio, " pages/byte\n");
                }
            }
            _g_.m.locks--;
            return npages;
        }

        //go:nowritebarrier
        private static System.UIntPtr gosweepone()
        {
            System.UIntPtr ret = default;
            systemstack(() =>
            {
                ret = sweepone();
            });
            return ret;
        }

        //go:nowritebarrier
        private static bool gosweepdone()
        {
            return mheap_.sweepdone != 0L;
        }

        // Returns only when span s has been swept.
        //go:nowritebarrier
        private static void ensureSwept(this ref mspan s)
        { 
            // Caller must disable preemption.
            // Otherwise when this function returns the span can become unswept again
            // (if GC is triggered on another goroutine).
            var _g_ = getg();
            if (_g_.m.locks == 0L && _g_.m.mallocing == 0L && _g_ != _g_.m.g0)
            {
                throw("MSpan_EnsureSwept: m is not locked");
            }
            var sg = mheap_.sweepgen;
            if (atomic.Load(ref s.sweepgen) == sg)
            {
                return;
            } 
            // The caller must be sure that the span is a MSpanInUse span.
            if (atomic.Cas(ref s.sweepgen, sg - 2L, sg - 1L))
            {
                s.sweep(false);
                return;
            } 
            // unfortunate condition, and we don't have efficient means to wait
            while (atomic.Load(ref s.sweepgen) != sg)
            {
                osyield();
            }

        }

        // Sweep frees or collects finalizers for blocks not marked in the mark phase.
        // It clears the mark bits in preparation for the next GC round.
        // Returns true if the span was returned to heap.
        // If preserve=true, don't return it to heap nor relink in MCentral lists;
        // caller takes care of it.
        //TODO go:nowritebarrier
        private static bool sweep(this ref mspan s, bool preserve)
        { 
            // It's critical that we enter this function with preemption disabled,
            // GC must not start while we are in the middle of this function.
            var _g_ = getg();
            if (_g_.m.locks == 0L && _g_.m.mallocing == 0L && _g_ != _g_.m.g0)
            {
                throw("MSpan_Sweep: m is not locked");
            }
            var sweepgen = mheap_.sweepgen;
            if (s.state != mSpanInUse || s.sweepgen != sweepgen - 1L)
            {
                print("MSpan_Sweep: state=", s.state, " sweepgen=", s.sweepgen, " mheap.sweepgen=", sweepgen, "\n");
                throw("MSpan_Sweep: bad span state");
            }
            if (trace.enabled)
            {
                traceGCSweepSpan(s.npages * _PageSize);
            }
            atomic.Xadd64(ref mheap_.pagesSwept, int64(s.npages));

            var spc = s.spanclass;
            var size = s.elemsize;
            var res = false;

            var c = _g_.m.mcache;
            var freeToHeap = false; 

            // The allocBits indicate which unmarked objects don't need to be
            // processed since they were free at the end of the last GC cycle
            // and were not allocated since then.
            // If the allocBits index is >= s.freeindex and the bit
            // is not marked then the object remains unallocated
            // since the last GC.
            // This situation is analogous to being on a freelist.

            // Unlink & free special records for any objects we're about to free.
            // Two complications here:
            // 1. An object can have both finalizer and profile special records.
            //    In such case we need to queue finalizer for execution,
            //    mark the object as live and preserve the profile special.
            // 2. A tiny object can have several finalizers setup for different offsets.
            //    If such object is not marked, we need to queue all finalizers at once.
            // Both 1 and 2 are possible at the same time.
            var specialp = ref s.specials;
            var special = specialp.Value;
            while (special != null)
            { 
                // A finalizer can be set for an inner byte of an object, find object beginning.
                var objIndex = uintptr(special.offset) / size;
                var p = s.@base() + objIndex * size;
                var mbits = s.markBitsForIndex(objIndex);
                if (!mbits.isMarked())
                { 
                    // This object is not marked and has at least one special record.
                    // Pass 1: see if it has at least one finalizer.
                    var hasFin = false;
                    var endOffset = p - s.@base() + size;
                    {
                        var tmp = special;

                        while (tmp != null && uintptr(tmp.offset) < endOffset)
                        {
                            if (tmp.kind == _KindSpecialFinalizer)
                            { 
                                // Stop freeing of object if it has a finalizer.
                                mbits.setMarkedNonAtomic();
                                hasFin = true;
                                break;
                            tmp = tmp.next;
                            }
                        }
                else
 
                        // Pass 2: queue all finalizers _or_ handle profile record.

                    } 
                    // Pass 2: queue all finalizers _or_ handle profile record.
                    while (special != null && uintptr(special.offset) < endOffset)
                    { 
                        // Find the exact byte for which the special was setup
                        // (as opposed to object beginning).
                        p = s.@base() + uintptr(special.offset);
                        if (special.kind == _KindSpecialFinalizer || !hasFin)
                        { 
                            // Splice out special record.
                            var y = special;
                            special = special.next;
                            specialp.Value = special;
                            freespecial(y, @unsafe.Pointer(p), size);
                        }
                        else
                        { 
                            // This is profile record, but the object has finalizers (so kept alive).
                            // Keep special record.
                            specialp = ref special.next;
                            special = specialp.Value;
                        }
                    }

                }                { 
                    // object is still live: keep special record
                    specialp = ref special.next;
                    special = specialp.Value;
                }
            }


            if (debug.allocfreetrace != 0L || raceenabled || msanenabled)
            { 
                // Find all newly freed objects. This doesn't have to
                // efficient; allocfreetrace has massive overhead.
                mbits = s.markBitsForBase();
                var abits = s.allocBitsForIndex(0L);
                for (var i = uintptr(0L); i < s.nelems; i++)
                {
                    if (!mbits.isMarked() && (abits.index < s.freeindex || abits.isMarked()))
                    {
                        var x = s.@base() + i * s.elemsize;
                        if (debug.allocfreetrace != 0L)
                        {
                            tracefree(@unsafe.Pointer(x), size);
                        }
                        if (raceenabled)
                        {
                            racefree(@unsafe.Pointer(x), size);
                        }
                        if (msanenabled)
                        {
                            msanfree(@unsafe.Pointer(x), size);
                        }
                    }
                    mbits.advance();
                    abits.advance();
                }

            } 

            // Count the number of free objects in this span.
            var nalloc = uint16(s.countAlloc());
            if (spc.sizeclass() == 0L && nalloc == 0L)
            {
                s.needzero = 1L;
                freeToHeap = true;
            }
            var nfreed = s.allocCount - nalloc;
            if (nalloc > s.allocCount)
            {
                print("runtime: nelems=", s.nelems, " nalloc=", nalloc, " previous allocCount=", s.allocCount, " nfreed=", nfreed, "\n");
                throw("sweep increased allocation count");
            }
            s.allocCount = nalloc;
            var wasempty = s.nextFreeIndex() == s.nelems;
            s.freeindex = 0L; // reset allocation index to start of span.
            if (trace.enabled)
            {
                getg().m.p.ptr().traceReclaimed += uintptr(nfreed) * s.elemsize;
            } 

            // gcmarkBits becomes the allocBits.
            // get a fresh cleared gcmarkBits in preparation for next GC
            s.allocBits = s.gcmarkBits;
            s.gcmarkBits = newMarkBits(s.nelems); 

            // Initialize alloc bits cache.
            s.refillAllocCache(0L); 

            // We need to set s.sweepgen = h.sweepgen only when all blocks are swept,
            // because of the potential for a concurrent free/SetFinalizer.
            // But we need to set it before we make the span available for allocation
            // (return it to heap or mcentral), because allocation code assumes that a
            // span is already swept if available for allocation.
            if (freeToHeap || nfreed == 0L)
            { 
                // The span must be in our exclusive ownership until we update sweepgen,
                // check for potential races.
                if (s.state != mSpanInUse || s.sweepgen != sweepgen - 1L)
                {
                    print("MSpan_Sweep: state=", s.state, " sweepgen=", s.sweepgen, " mheap.sweepgen=", sweepgen, "\n");
                    throw("MSpan_Sweep: bad span state after sweep");
                } 
                // Serialization point.
                // At this point the mark bits are cleared and allocation ready
                // to go so release the span.
                atomic.Store(ref s.sweepgen, sweepgen);
            }
            if (nfreed > 0L && spc.sizeclass() != 0L)
            {
                c.local_nsmallfree[spc.sizeclass()] += uintptr(nfreed);
                res = mheap_.central[spc].mcentral.freeSpan(s, preserve, wasempty); 
                // MCentral_FreeSpan updates sweepgen
            }
            else if (freeToHeap)
            { 
                // Free large span to heap

                // NOTE(rsc,dvyukov): The original implementation of efence
                // in CL 22060046 used SysFree instead of SysFault, so that
                // the operating system would eventually give the memory
                // back to us again, so that an efence program could run
                // longer without running out of memory. Unfortunately,
                // calling SysFree here without any kind of adjustment of the
                // heap data structures means that when the memory does
                // come back to us, we have the wrong metadata for it, either in
                // the MSpan structures or in the garbage collection bitmap.
                // Using SysFault here means that the program will run out of
                // memory fairly quickly in efence mode, but at least it won't
                // have mysterious crashes due to confused memory reuse.
                // It should be possible to switch back to SysFree if we also
                // implement and then call some kind of MHeap_DeleteSpan.
                if (debug.efence > 0L)
                {
                    s.limit = 0L; // prevent mlookup from finding this span
                    sysFault(@unsafe.Pointer(s.@base()), size);
                }
                else
                {
                    mheap_.freeSpan(s, 1L);
                }
                c.local_nlargefree++;
                c.local_largefree += size;
                res = true;
            }
            if (!res)
            { 
                // The span has been swept and is still in-use, so put
                // it on the swept in-use list.
                mheap_.sweepSpans[sweepgen / 2L % 2L].push(s);
            }
            return res;
        }

        // deductSweepCredit deducts sweep credit for allocating a span of
        // size spanBytes. This must be performed *before* the span is
        // allocated to ensure the system has enough credit. If necessary, it
        // performs sweeping to prevent going in to debt. If the caller will
        // also sweep pages (e.g., for a large allocation), it can pass a
        // non-zero callerSweepPages to leave that many pages unswept.
        //
        // deductSweepCredit makes a worst-case assumption that all spanBytes
        // bytes of the ultimately allocated span will be available for object
        // allocation.
        //
        // deductSweepCredit is the core of the "proportional sweep" system.
        // It uses statistics gathered by the garbage collector to perform
        // enough sweeping so that all pages are swept during the concurrent
        // sweep phase between GC cycles.
        //
        // mheap_ must NOT be locked.
        private static void deductSweepCredit(System.UIntPtr spanBytes, System.UIntPtr callerSweepPages)
        {
            if (mheap_.sweepPagesPerByte == 0L)
            { 
                // Proportional sweep is done or disabled.
                return;
            }
            if (trace.enabled)
            {
                traceGCSweepStart();
            }
retry: 

            // Fix debt if necessary.
            var sweptBasis = atomic.Load64(ref mheap_.pagesSweptBasis); 

            // Fix debt if necessary.
            var newHeapLive = uintptr(atomic.Load64(ref memstats.heap_live) - mheap_.sweepHeapLiveBasis) + spanBytes;
            var pagesTarget = int64(mheap_.sweepPagesPerByte * float64(newHeapLive)) - int64(callerSweepPages);
            while (pagesTarget > int64(atomic.Load64(ref mheap_.pagesSwept) - sweptBasis))
            {
                if (gosweepone() == ~uintptr(0L))
                {
                    mheap_.sweepPagesPerByte = 0L;
                    break;
                }
                if (atomic.Load64(ref mheap_.pagesSweptBasis) != sweptBasis)
                { 
                    // Sweep pacing changed. Recompute debt.
                    goto retry;
                }
            }


            if (trace.enabled)
            {
                traceGCSweepDone();
            }
        }
    }
}
