// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:20:31 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mcache.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // Per-thread (in Go, per-P) cache for small objects.
        // No locking needed because it is per-thread (per-P).
        //
        // mcaches are allocated from non-GC'd memory, so any heap pointers
        // must be specially handled.
        //
        //go:notinheap
        private partial struct mcache
        {
            public System.UIntPtr next_sample; // trigger heap sample after allocating this many bytes
            public System.UIntPtr local_scan; // bytes of scannable heap allocated

// Allocator cache for tiny objects w/o pointers.
// See "Tiny allocator" comment in malloc.go.

// tiny points to the beginning of the current tiny block, or
// nil if there is no current tiny block.
//
// tiny is a heap pointer. Since mcache is in non-GC'd memory,
// we handle it by clearing it in releaseAll during mark
// termination.
            public System.UIntPtr tiny;
            public System.UIntPtr tinyoffset;
            public System.UIntPtr local_tinyallocs; // number of tiny allocs not counted in other stats

// The rest is not accessed on every malloc.

            public array<ptr<mspan>> alloc; // spans to allocate from, indexed by spanClass

            public array<stackfreelist> stackcache; // Local allocator stats, flushed during GC.
            public System.UIntPtr local_largefree; // bytes freed for large objects (>maxsmallsize)
            public System.UIntPtr local_nlargefree; // number of frees for large objects (>maxsmallsize)
            public array<System.UIntPtr> local_nsmallfree; // number of frees for small objects (<=maxsmallsize)

// flushGen indicates the sweepgen during which this mcache
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
        private partial struct gclink
        {
            public gclinkptr next;
        }

        // A gclinkptr is a pointer to a gclink, but it is opaque
        // to the garbage collector.
        private partial struct gclinkptr // : System.UIntPtr
        {
        }

        // ptr returns the *gclink form of p.
        // The result should be used for accessing fields, not stored
        // in other data structures.
        private static ptr<gclink> ptr(this gclinkptr p)
        {
            return _addr_(gclink.val)(@unsafe.Pointer(p))!;
        }

        private partial struct stackfreelist
        {
            public gclinkptr list; // linked list of free stacks
            public System.UIntPtr size; // total size of stacks in list
        }

        // dummy mspan that contains no free objects.
        private static mspan emptymspan = default;

        private static ptr<mcache> allocmcache()
        {
            ptr<mcache> c;
            systemstack(() =>
            {
                lock(_addr_mheap_.@lock);
                c = (mcache.val)(mheap_.cachealloc.alloc());
                c.flushGen = mheap_.sweepgen;
                unlock(_addr_mheap_.@lock);
            });
            foreach (var (i) in c.alloc)
            {
                c.alloc[i] = _addr_emptymspan;
            }
            c.next_sample = nextSample();
            return _addr_c!;

        }

        private static void freemcache(ptr<mcache> _addr_c)
        {
            ref mcache c = ref _addr_c.val;

            systemstack(() =>
            {
                c.releaseAll();
                stackcache_clear(c); 

                // NOTE(rsc,rlh): If gcworkbuffree comes back, we need to coordinate
                // with the stealing of gcworkbufs during garbage collection to avoid
                // a race where the workbuf is double-freed.
                // gcworkbuffree(c.gcworkbuf)

                lock(_addr_mheap_.@lock);
                purgecachedstats(c);
                mheap_.cachealloc.free(@unsafe.Pointer(c));
                unlock(_addr_mheap_.@lock);

            });

        }

        // refill acquires a new span of span class spc for c. This span will
        // have at least one free object. The current span in c must be full.
        //
        // Must run in a non-preemptible context since otherwise the owner of
        // c could change.
        private static void refill(this ptr<mcache> _addr_c, spanClass spc)
        {
            ref mcache c = ref _addr_c.val;
 
            // Return the current cached span to the central lists.
            var s = c.alloc[spc];

            if (uintptr(s.allocCount) != s.nelems)
            {
                throw("refill of span with free space remaining");
            }

            if (s != _addr_emptymspan)
            { 
                // Mark this span as no longer cached.
                if (s.sweepgen != mheap_.sweepgen + 3L)
                {
                    throw("bad sweepgen in refill");
                }

                if (go115NewMCentralImpl)
                {
                    mheap_.central[spc].mcentral.uncacheSpan(s);
                }
                else
                {
                    atomic.Store(_addr_s.sweepgen, mheap_.sweepgen);
                }

            } 

            // Get a new cached span from the central lists.
            s = mheap_.central[spc].mcentral.cacheSpan();
            if (s == null)
            {
                throw("out of memory");
            }

            if (uintptr(s.allocCount) == s.nelems)
            {
                throw("span has no free space");
            } 

            // Indicate that this span is cached and prevent asynchronous
            // sweeping in the next sweep phase.
            s.sweepgen = mheap_.sweepgen + 3L;

            c.alloc[spc] = s;

        }

        private static void releaseAll(this ptr<mcache> _addr_c)
        {
            ref mcache c = ref _addr_c.val;

            foreach (var (i) in c.alloc)
            {
                var s = c.alloc[i];
                if (s != _addr_emptymspan)
                {
                    mheap_.central[i].mcentral.uncacheSpan(s);
                    c.alloc[i] = _addr_emptymspan;
                }

            } 
            // Clear tinyalloc pool.
            c.tiny = 0L;
            c.tinyoffset = 0L;

        }

        // prepareForSweep flushes c if the system has entered a new sweep phase
        // since c was populated. This must happen between the sweep phase
        // starting and the first allocation from c.
        private static void prepareForSweep(this ptr<mcache> _addr_c)
        {
            ref mcache c = ref _addr_c.val;
 
            // Alternatively, instead of making sure we do this on every P
            // between starting the world and allocating on that P, we
            // could leave allocate-black on, allow allocation to continue
            // as usual, use a ragged barrier at the beginning of sweep to
            // ensure all cached spans are swept, and then disable
            // allocate-black. However, with this approach it's difficult
            // to avoid spilling mark bits into the *next* GC cycle.
            var sg = mheap_.sweepgen;
            if (c.flushGen == sg)
            {
                return ;
            }
            else if (c.flushGen != sg - 2L)
            {
                println("bad flushGen", c.flushGen, "in prepareForSweep; sweepgen", sg);
                throw("bad flushGen");
            }

            c.releaseAll();
            stackcache_clear(c);
            atomic.Store(_addr_c.flushGen, mheap_.sweepgen); // Synchronizes with gcStart
        }
    }
}
