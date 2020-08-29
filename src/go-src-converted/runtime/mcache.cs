// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:17:42 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mcache.go
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
            public int next_sample; // trigger heap sample after allocating this many bytes
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

            public array<ref mspan> alloc; // spans to allocate from, indexed by spanClass

            public array<stackfreelist> stackcache; // Local allocator stats, flushed during GC.
            public System.UIntPtr local_nlookup; // number of pointer lookups
            public System.UIntPtr local_largefree; // bytes freed for large objects (>maxsmallsize)
            public System.UIntPtr local_nlargefree; // number of frees for large objects (>maxsmallsize)
            public array<System.UIntPtr> local_nsmallfree; // number of frees for small objects (<=maxsmallsize)
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
        private static ref gclink ptr(this gclinkptr p)
        {
            return (gclink.Value)(@unsafe.Pointer(p));
        }

        private partial struct stackfreelist
        {
            public gclinkptr list; // linked list of free stacks
            public System.UIntPtr size; // total size of stacks in list
        }

        // dummy MSpan that contains no free objects.
        private static mspan emptymspan = default;

        private static ref mcache allocmcache()
        {
            lock(ref mheap_.@lock);
            var c = (mcache.Value)(mheap_.cachealloc.alloc());
            unlock(ref mheap_.@lock);
            foreach (var (i) in c.alloc)
            {
                c.alloc[i] = ref emptymspan;
            }
            c.next_sample = nextSample();
            return c;
        }

        private static void freemcache(ref mcache c)
        {
            systemstack(() =>
            {
                c.releaseAll();
                stackcache_clear(c); 

                // NOTE(rsc,rlh): If gcworkbuffree comes back, we need to coordinate
                // with the stealing of gcworkbufs during garbage collection to avoid
                // a race where the workbuf is double-freed.
                // gcworkbuffree(c.gcworkbuf)

                lock(ref mheap_.@lock);
                purgecachedstats(c);
                mheap_.cachealloc.free(@unsafe.Pointer(c));
                unlock(ref mheap_.@lock);
            });
        }

        // Gets a span that has a free object in it and assigns it
        // to be the cached span for the given sizeclass. Returns this span.
        private static void refill(this ref mcache c, spanClass spc)
        {
            var _g_ = getg();

            _g_.m.locks++; 
            // Return the current cached span to the central lists.
            var s = c.alloc[spc];

            if (uintptr(s.allocCount) != s.nelems)
            {
                throw("refill of span with free space remaining");
            }
            if (s != ref emptymspan)
            {
                s.incache = false;
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
            c.alloc[spc] = s;
            _g_.m.locks--;
        }

        private static void releaseAll(this ref mcache c)
        {
            foreach (var (i) in c.alloc)
            {
                var s = c.alloc[i];
                if (s != ref emptymspan)
                {
                    mheap_.central[i].mcentral.uncacheSpan(s);
                    c.alloc[i] = ref emptymspan;
                }
            } 
            // Clear tinyalloc pool.
            c.tiny = 0L;
            c.tinyoffset = 0L;
        }
    }
}
