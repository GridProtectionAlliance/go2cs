// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Memory statistics

// package runtime -- go2cs converted at 2020 August 29 08:18:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mstats.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // Statistics.
        // If you edit this structure, also edit type MemStats below.
        // Their layouts must match exactly.
        //
        // For detailed descriptions see the documentation for MemStats.
        // Fields that differ from MemStats are further documented here.
        //
        // Many of these fields are updated on the fly, while others are only
        // updated when updatememstats is called.
        private partial struct mstats
        {
            public ulong alloc; // bytes allocated and not yet freed
            public ulong total_alloc; // bytes allocated (even if freed)
            public ulong sys; // bytes obtained from system (should be sum of xxx_sys below, no locking, approximate)
            public ulong nlookup; // number of pointer lookups
            public ulong nmalloc; // number of mallocs
            public ulong nfree; // number of frees

// Statistics about malloc heap.
// Protected by mheap.lock
//
// Like MemStats, heap_sys and heap_inuse do not count memory
// in manually-managed spans.
            public ulong heap_alloc; // bytes allocated and not yet freed (same as alloc above)
            public ulong heap_sys; // virtual address space obtained from system for GC'd heap
            public ulong heap_idle; // bytes in idle spans
            public ulong heap_inuse; // bytes in _MSpanInUse spans
            public ulong heap_released; // bytes released to the os
            public ulong heap_objects; // total number of allocated objects

// TODO(austin): heap_released is both useless and inaccurate
// in its current form. It's useless because, from the user's
// and OS's perspectives, there's no difference between a page
// that has not yet been faulted in and a page that has been
// released back to the OS. We could fix this by considering
// newly mapped spans to be "released". It's inaccurate
// because when we split a large span for allocation, we
// "unrelease" all pages in the large span and not just the
// ones we split off for use. This is trickier to fix because
// we currently don't know which pages of a span we've
// released. We could fix it by separating "free" and
// "released" spans, but then we have to allocate from runs of
// free and released spans.

// Statistics about allocation of low-level fixed-size structures.
// Protected by FixAlloc locks.
            public ulong stacks_inuse; // bytes in manually-managed stack spans
            public ulong stacks_sys; // only counts newosproc0 stack in mstats; differs from MemStats.StackSys
            public ulong mspan_inuse; // mspan structures
            public ulong mspan_sys;
            public ulong mcache_inuse; // mcache structures
            public ulong mcache_sys;
            public ulong buckhash_sys; // profiling bucket hash table
            public ulong gc_sys;
            public ulong other_sys; // Statistics about garbage collector.
// Protected by mheap or stopping the world during GC.
            public ulong next_gc; // goal heap_live for when next GC ends; ^0 if disabled
            public ulong last_gc_unix; // last gc (in unix time)
            public ulong pause_total_ns;
            public array<ulong> pause_ns; // circular buffer of recent gc pause lengths
            public array<ulong> pause_end; // circular buffer of recent gc end times (nanoseconds since 1970)
            public uint numgc;
            public uint numforcedgc; // number of user-forced GCs
            public double gc_cpu_fraction; // fraction of CPU time used by GC
            public bool enablegc;
            public bool debuggc; // Statistics about allocation size classes.

            public ulong last_gc_nanotime; // last gc (monotonic time)
            public ulong tinyallocs; // number of tiny allocations that didn't cause actual allocation; not exported to go directly

// triggerRatio is the heap growth ratio that triggers marking.
//
// E.g., if this is 0.6, then GC should start when the live
// heap has reached 1.6 times the heap size marked by the
// previous cycle. This should be ≤ GOGC/100 so the trigger
// heap size is less than the goal heap size. This is set
// during mark termination for the next cycle's trigger.
            public double triggerRatio; // gc_trigger is the heap size that triggers marking.
//
// When heap_live ≥ gc_trigger, the mark phase will start.
// This is also the heap size by which proportional sweeping
// must be complete.
//
// This is computed from triggerRatio during mark termination
// for the next cycle's trigger.
            public ulong gc_trigger; // heap_live is the number of bytes considered live by the GC.
// That is: retained by the most recent GC plus allocated
// since then. heap_live <= heap_alloc, since heap_alloc
// includes unmarked objects that have not yet been swept (and
// hence goes up as we allocate and down as we sweep) while
// heap_live excludes these objects (and hence only goes up
// between GCs).
//
// This is updated atomically without locking. To reduce
// contention, this is updated only when obtaining a span from
// an mcentral and at this point it counts all of the
// unallocated slots in that span (which will be allocated
// before that mcache obtains another span from that
// mcentral). Hence, it slightly overestimates the "true" live
// heap size. It's better to overestimate than to
// underestimate because 1) this triggers the GC earlier than
// necessary rather than potentially too late and 2) this
// leads to a conservative GC rate rather than a GC rate that
// is potentially too low.
//
// Reads should likewise be atomic (or during STW).
//
// Whenever this is updated, call traceHeapAlloc() and
// gcController.revise().
            public ulong heap_live; // heap_scan is the number of bytes of "scannable" heap. This
// is the live heap (as counted by heap_live), but omitting
// no-scan objects and no-scan tails of objects.
//
// Whenever this is updated, call gcController.revise().
            public ulong heap_scan; // heap_marked is the number of bytes marked by the previous
// GC. After mark termination, heap_live == heap_marked, but
// unlike heap_live, heap_marked does not change until the
// next mark termination.
            public ulong heap_marked;
        }

        private static mstats memstats = default;

        // A MemStats records statistics about the memory allocator.
        public partial struct MemStats
        {
            public ulong Alloc; // TotalAlloc is cumulative bytes allocated for heap objects.
//
// TotalAlloc increases as heap objects are allocated, but
// unlike Alloc and HeapAlloc, it does not decrease when
// objects are freed.
            public ulong TotalAlloc; // Sys is the total bytes of memory obtained from the OS.
//
// Sys is the sum of the XSys fields below. Sys measures the
// virtual address space reserved by the Go runtime for the
// heap, stacks, and other internal data structures. It's
// likely that not all of the virtual address space is backed
// by physical memory at any given moment, though in general
// it all was at some point.
            public ulong Sys; // Lookups is the number of pointer lookups performed by the
// runtime.
//
// This is primarily useful for debugging runtime internals.
            public ulong Lookups; // Mallocs is the cumulative count of heap objects allocated.
// The number of live objects is Mallocs - Frees.
            public ulong Mallocs; // Frees is the cumulative count of heap objects freed.
            public ulong Frees; // Heap memory statistics.
//
// Interpreting the heap statistics requires some knowledge of
// how Go organizes memory. Go divides the virtual address
// space of the heap into "spans", which are contiguous
// regions of memory 8K or larger. A span may be in one of
// three states:
//
// An "idle" span contains no objects or other data. The
// physical memory backing an idle span can be released back
// to the OS (but the virtual address space never is), or it
// can be converted into an "in use" or "stack" span.
//
// An "in use" span contains at least one heap object and may
// have free space available to allocate more heap objects.
//
// A "stack" span is used for goroutine stacks. Stack spans
// are not considered part of the heap. A span can change
// between heap and stack memory; it is never used for both
// simultaneously.

// HeapAlloc is bytes of allocated heap objects.
//
// "Allocated" heap objects include all reachable objects, as
// well as unreachable objects that the garbage collector has
// not yet freed. Specifically, HeapAlloc increases as heap
// objects are allocated and decreases as the heap is swept
// and unreachable objects are freed. Sweeping occurs
// incrementally between GC cycles, so these two processes
// occur simultaneously, and as a result HeapAlloc tends to
// change smoothly (in contrast with the sawtooth that is
// typical of stop-the-world garbage collectors).
            public ulong HeapAlloc; // HeapSys is bytes of heap memory obtained from the OS.
//
// HeapSys measures the amount of virtual address space
// reserved for the heap. This includes virtual address space
// that has been reserved but not yet used, which consumes no
// physical memory, but tends to be small, as well as virtual
// address space for which the physical memory has been
// returned to the OS after it became unused (see HeapReleased
// for a measure of the latter).
//
// HeapSys estimates the largest size the heap has had.
            public ulong HeapSys; // HeapIdle is bytes in idle (unused) spans.
//
// Idle spans have no objects in them. These spans could be
// (and may already have been) returned to the OS, or they can
// be reused for heap allocations, or they can be reused as
// stack memory.
//
// HeapIdle minus HeapReleased estimates the amount of memory
// that could be returned to the OS, but is being retained by
// the runtime so it can grow the heap without requesting more
// memory from the OS. If this difference is significantly
// larger than the heap size, it indicates there was a recent
// transient spike in live heap size.
            public ulong HeapIdle; // HeapInuse is bytes in in-use spans.
//
// In-use spans have at least one object in them. These spans
// can only be used for other objects of roughly the same
// size.
//
// HeapInuse minus HeapAlloc estimates the amount of memory
// that has been dedicated to particular size classes, but is
// not currently being used. This is an upper bound on
// fragmentation, but in general this memory can be reused
// efficiently.
            public ulong HeapInuse; // HeapReleased is bytes of physical memory returned to the OS.
//
// This counts heap memory from idle spans that was returned
// to the OS and has not yet been reacquired for the heap.
            public ulong HeapReleased; // HeapObjects is the number of allocated heap objects.
//
// Like HeapAlloc, this increases as objects are allocated and
// decreases as the heap is swept and unreachable objects are
// freed.
            public ulong HeapObjects; // Stack memory statistics.
//
// Stacks are not considered part of the heap, but the runtime
// can reuse a span of heap memory for stack memory, and
// vice-versa.

// StackInuse is bytes in stack spans.
//
// In-use stack spans have at least one stack in them. These
// spans can only be used for other stacks of the same size.
//
// There is no StackIdle because unused stack spans are
// returned to the heap (and hence counted toward HeapIdle).
            public ulong StackInuse; // StackSys is bytes of stack memory obtained from the OS.
//
// StackSys is StackInuse, plus any memory obtained directly
// from the OS for OS thread stacks (which should be minimal).
            public ulong StackSys; // Off-heap memory statistics.
//
// The following statistics measure runtime-internal
// structures that are not allocated from heap memory (usually
// because they are part of implementing the heap). Unlike
// heap or stack memory, any memory allocated to these
// structures is dedicated to these structures.
//
// These are primarily useful for debugging runtime memory
// overheads.

// MSpanInuse is bytes of allocated mspan structures.
            public ulong MSpanInuse; // MSpanSys is bytes of memory obtained from the OS for mspan
// structures.
            public ulong MSpanSys; // MCacheInuse is bytes of allocated mcache structures.
            public ulong MCacheInuse; // MCacheSys is bytes of memory obtained from the OS for
// mcache structures.
            public ulong MCacheSys; // BuckHashSys is bytes of memory in profiling bucket hash tables.
            public ulong BuckHashSys; // GCSys is bytes of memory in garbage collection metadata.
            public ulong GCSys; // OtherSys is bytes of memory in miscellaneous off-heap
// runtime allocations.
            public ulong OtherSys; // Garbage collector statistics.

// NextGC is the target heap size of the next GC cycle.
//
// The garbage collector's goal is to keep HeapAlloc ≤ NextGC.
// At the end of each GC cycle, the target for the next cycle
// is computed based on the amount of reachable data and the
// value of GOGC.
            public ulong NextGC; // LastGC is the time the last garbage collection finished, as
// nanoseconds since 1970 (the UNIX epoch).
            public ulong LastGC; // PauseTotalNs is the cumulative nanoseconds in GC
// stop-the-world pauses since the program started.
//
// During a stop-the-world pause, all goroutines are paused
// and only the garbage collector can run.
            public ulong PauseTotalNs; // PauseNs is a circular buffer of recent GC stop-the-world
// pause times in nanoseconds.
//
// The most recent pause is at PauseNs[(NumGC+255)%256]. In
// general, PauseNs[N%256] records the time paused in the most
// recent N%256th GC cycle. There may be multiple pauses per
// GC cycle; this is the sum of all pauses during a cycle.
            public array<ulong> PauseNs; // PauseEnd is a circular buffer of recent GC pause end times,
// as nanoseconds since 1970 (the UNIX epoch).
//
// This buffer is filled the same way as PauseNs. There may be
// multiple pauses per GC cycle; this records the end of the
// last pause in a cycle.
            public array<ulong> PauseEnd; // NumGC is the number of completed GC cycles.
            public uint NumGC; // NumForcedGC is the number of GC cycles that were forced by
// the application calling the GC function.
            public uint NumForcedGC; // GCCPUFraction is the fraction of this program's available
// CPU time used by the GC since the program started.
//
// GCCPUFraction is expressed as a number between 0 and 1,
// where 0 means GC has consumed none of this program's CPU. A
// program's available CPU time is defined as the integral of
// GOMAXPROCS since the program started. That is, if
// GOMAXPROCS is 2 and a program has been running for 10
// seconds, its "available CPU" is 20 seconds. GCCPUFraction
// does not include CPU time used for write barrier activity.
//
// This is the same as the fraction of CPU reported by
// GODEBUG=gctrace=1.
            public double GCCPUFraction; // EnableGC indicates that GC is enabled. It is always true,
// even if GOGC=off.
            public bool EnableGC; // DebugGC is currently unused.
            public bool DebugGC; // BySize reports per-size class allocation statistics.
//
// BySize[N] gives statistics for allocations of size S where
// BySize[N-1].Size < S ≤ BySize[N].Size.
//
// This does not report allocations larger than BySize[60].Size.
        }

        // Size of the trailing by_size array differs between mstats and MemStats,
        // and all data after by_size is local to runtime, not exported.
        // NumSizeClasses was changed, but we cannot change MemStats because of backward compatibility.
        // sizeof_C_MStats is the size of the prefix of mstats that
        // corresponds to MemStats. It should match Sizeof(MemStats{}).
        private static var sizeof_C_MStats = @unsafe.Offsetof(memstats.by_size) + 61L * @unsafe.Sizeof(memstats.by_size[0L]);

        private static void init()
        {
            MemStats memStats = default;
            if (sizeof_C_MStats != @unsafe.Sizeof(memStats))
            {
                println(sizeof_C_MStats, @unsafe.Sizeof(memStats));
                throw("MStats vs MemStatsType size mismatch");
            }
            if (@unsafe.Offsetof(memstats.heap_live) % 8L != 0L)
            {
                println(@unsafe.Offsetof(memstats.heap_live));
                throw("memstats.heap_live not aligned to 8 bytes");
            }
        }

        // ReadMemStats populates m with memory allocator statistics.
        //
        // The returned memory allocator statistics are up to date as of the
        // call to ReadMemStats. This is in contrast with a heap profile,
        // which is a snapshot as of the most recently completed garbage
        // collection cycle.
        public static void ReadMemStats(ref MemStats m)
        {
            stopTheWorld("read mem stats");

            systemstack(() =>
            {
                readmemstats_m(m);
            });

            startTheWorld();
        }

        private static void readmemstats_m(ref MemStats stats)
        {
            updatememstats(); 

            // The size of the trailing by_size array differs between
            // mstats and MemStats. NumSizeClasses was changed, but we
            // cannot change MemStats because of backward compatibility.
            memmove(@unsafe.Pointer(stats), @unsafe.Pointer(ref memstats), sizeof_C_MStats); 

            // memstats.stacks_sys is only memory mapped directly for OS stacks.
            // Add in heap-allocated stack memory for user consumption.
            stats.StackSys += stats.StackInuse;
        }

        //go:linkname readGCStats runtime/debug.readGCStats
        private static void readGCStats(ref slice<ulong> pauses)
        {
            systemstack(() =>
            {
                readGCStats_m(pauses);
            });
        }

        private static void readGCStats_m(ref slice<ulong> pauses)
        {
            var p = pauses.Value; 
            // Calling code in runtime/debug should make the slice large enough.
            if (cap(p) < len(memstats.pause_ns) + 3L)
            {
                throw("short slice passed to readGCStats");
            } 

            // Pass back: pauses, pause ends, last gc (absolute time), number of gc, total pause ns.
            lock(ref mheap_.@lock);

            var n = memstats.numgc;
            if (n > uint32(len(memstats.pause_ns)))
            {
                n = uint32(len(memstats.pause_ns));
            } 

            // The pause buffer is circular. The most recent pause is at
            // pause_ns[(numgc-1)%len(pause_ns)], and then backward
            // from there to go back farther in time. We deliver the times
            // most recent first (in p[0]).
            p = p[..cap(p)];
            for (var i = uint32(0L); i < n; i++)
            {
                var j = (memstats.numgc - 1L - i) % uint32(len(memstats.pause_ns));
                p[i] = memstats.pause_ns[j];
                p[n + i] = memstats.pause_end[j];
            }


            p[n + n] = memstats.last_gc_unix;
            p[n + n + 1L] = uint64(memstats.numgc);
            p[n + n + 2L] = memstats.pause_total_ns;
            unlock(ref mheap_.@lock);
            pauses.Value = p[..n + n + 3L];
        }

        //go:nowritebarrier
        private static void updatememstats()
        {
            memstats.mcache_inuse = uint64(mheap_.cachealloc.inuse);
            memstats.mspan_inuse = uint64(mheap_.spanalloc.inuse);
            memstats.sys = memstats.heap_sys + memstats.stacks_sys + memstats.mspan_sys + memstats.mcache_sys + memstats.buckhash_sys + memstats.gc_sys + memstats.other_sys; 

            // We also count stacks_inuse as sys memory.
            memstats.sys += memstats.stacks_inuse; 

            // Calculate memory allocator stats.
            // During program execution we only count number of frees and amount of freed memory.
            // Current number of alive object in the heap and amount of alive heap memory
            // are calculated by scanning all spans.
            // Total number of mallocs is calculated as number of frees plus number of alive objects.
            // Similarly, total amount of allocated memory is calculated as amount of freed memory
            // plus amount of alive heap memory.
            memstats.alloc = 0L;
            memstats.total_alloc = 0L;
            memstats.nmalloc = 0L;
            memstats.nfree = 0L;
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(memstats.by_size); i++)
                {
                    memstats.by_size[i].nmalloc = 0L;
                    memstats.by_size[i].nfree = 0L;
                } 

                // Flush MCache's to MCentral.


                i = i__prev1;
            } 

            // Flush MCache's to MCentral.
            systemstack(flushallmcaches); 

            // Aggregate local stats.
            cachestats(); 

            // Collect allocation stats. This is safe and consistent
            // because the world is stopped.
            ulong smallFree = default;            ulong totalAlloc = default;            ulong totalFree = default; 
            // Collect per-spanclass stats.
 
            // Collect per-spanclass stats.
            foreach (var (spc) in mheap_.central)
            { 
                // The mcaches are now empty, so mcentral stats are
                // up-to-date.
                var c = ref mheap_.central[spc].mcentral;
                memstats.nmalloc += c.nmalloc;
                i = spanClass(spc).sizeclass();
                memstats.by_size[i].nmalloc += c.nmalloc;
                totalAlloc += c.nmalloc * uint64(class_to_size[i]);
            } 
            // Collect per-sizeclass stats.
            {
                long i__prev1 = i;

                for (i = 0L; i < _NumSizeClasses; i++)
                {
                    if (i == 0L)
                    {
                        memstats.nmalloc += mheap_.nlargealloc;
                        totalAlloc += mheap_.largealloc;
                        totalFree += mheap_.largefree;
                        memstats.nfree += mheap_.nlargefree;
                        continue;
                    } 

                    // The mcache stats have been flushed to mheap_.
                    memstats.nfree += mheap_.nsmallfree[i];
                    memstats.by_size[i].nfree = mheap_.nsmallfree[i];
                    smallFree += mheap_.nsmallfree[i] * uint64(class_to_size[i]);
                }


                i = i__prev1;
            }
            totalFree += smallFree;

            memstats.nfree += memstats.tinyallocs;
            memstats.nmalloc += memstats.tinyallocs; 

            // Calculate derived stats.
            memstats.total_alloc = totalAlloc;
            memstats.alloc = totalAlloc - totalFree;
            memstats.heap_alloc = memstats.alloc;
            memstats.heap_objects = memstats.nmalloc - memstats.nfree;
        }

        // cachestats flushes all mcache stats.
        //
        // The world must be stopped.
        //
        //go:nowritebarrier
        private static void cachestats()
        {
            foreach (var (_, p) in allp)
            {
                var c = p.mcache;
                if (c == null)
                {
                    continue;
                }
                purgecachedstats(c);
            }
        }

        // flushmcache flushes the mcache of allp[i].
        //
        // The world must be stopped.
        //
        //go:nowritebarrier
        private static void flushmcache(long i)
        {
            var p = allp[i];
            var c = p.mcache;
            if (c == null)
            {
                return;
            }
            c.releaseAll();
            stackcache_clear(c);
        }

        // flushallmcaches flushes the mcaches of all Ps.
        //
        // The world must be stopped.
        //
        //go:nowritebarrier
        private static void flushallmcaches()
        {
            for (long i = 0L; i < int(gomaxprocs); i++)
            {
                flushmcache(i);
            }

        }

        //go:nosplit
        private static void purgecachedstats(ref mcache c)
        { 
            // Protected by either heap or GC lock.
            var h = ref mheap_;
            memstats.heap_scan += uint64(c.local_scan);
            c.local_scan = 0L;
            memstats.tinyallocs += uint64(c.local_tinyallocs);
            c.local_tinyallocs = 0L;
            memstats.nlookup += uint64(c.local_nlookup);
            c.local_nlookup = 0L;
            h.largefree += uint64(c.local_largefree);
            c.local_largefree = 0L;
            h.nlargefree += uint64(c.local_nlargefree);
            c.local_nlargefree = 0L;
            for (long i = 0L; i < len(c.local_nsmallfree); i++)
            {
                h.nsmallfree[i] += uint64(c.local_nsmallfree[i]);
                c.local_nsmallfree[i] = 0L;
            }

        }

        // Atomically increases a given *system* memory stat. We are counting on this
        // stat never overflowing a uintptr, so this function must only be used for
        // system memory stats.
        //
        // The current implementation for little endian architectures is based on
        // xadduintptr(), which is less than ideal: xadd64() should really be used.
        // Using xadduintptr() is a stop-gap solution until arm supports xadd64() that
        // doesn't use locks.  (Locks are a problem as they require a valid G, which
        // restricts their useability.)
        //
        // A side-effect of using xadduintptr() is that we need to check for
        // overflow errors.
        //go:nosplit
        private static void mSysStatInc(ref ulong sysStat, System.UIntPtr n)
        {
            if (sys.BigEndian)
            {
                atomic.Xadd64(sysStat, int64(n));
                return;
            }
            {
                var val = atomic.Xadduintptr((uintptr.Value)(@unsafe.Pointer(sysStat)), n);

                if (val < n)
                {
                    print("runtime: stat overflow: val ", val, ", n ", n, "\n");
                    exit(2L);
                }

            }
        }

        // Atomically decreases a given *system* memory stat. Same comments as
        // mSysStatInc apply.
        //go:nosplit
        private static void mSysStatDec(ref ulong sysStat, System.UIntPtr n)
        {
            if (sys.BigEndian)
            {
                atomic.Xadd64(sysStat, -int64(n));
                return;
            }
            {
                var val = atomic.Xadduintptr((uintptr.Value)(@unsafe.Pointer(sysStat)), uintptr(-int64(n)));

                if (val + n < n)
                {
                    print("runtime: stat underflow: val ", val, ", n ", n, "\n");
                    exit(2L);
                }

            }
        }
    }
}
