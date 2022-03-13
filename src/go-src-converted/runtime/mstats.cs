// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Memory statistics

// package runtime -- go2cs converted at 2022 March 13 05:26:00 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mstats.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = @unsafe_package;


// Statistics.
//
// For detailed descriptions see the documentation for MemStats.
// Fields that differ from MemStats are further documented here.
//
// Many of these fields are updated on the fly, while others are only
// updated when updatememstats is called.

using System;
public static partial class runtime_package {

private partial struct mstats {
    public ulong alloc; // bytes allocated and not yet freed
    public ulong total_alloc; // bytes allocated (even if freed)
    public ulong sys; // bytes obtained from system (should be sum of xxx_sys below, no locking, approximate)
    public ulong nlookup; // number of pointer lookups (unused)
    public ulong nmalloc; // number of mallocs
    public ulong nfree; // number of frees

// Statistics about malloc heap.
// Updated atomically, or with the world stopped.
//
// Like MemStats, heap_sys and heap_inuse do not count memory
// in manually-managed spans.
    public sysMemStat heap_sys; // virtual address space obtained from system for GC'd heap
    public ulong heap_inuse; // bytes in mSpanInUse spans
    public ulong heap_released; // bytes released to the os

// heap_objects is not used by the runtime directly and instead
// computed on the fly by updatememstats.
    public ulong heap_objects; // total number of allocated objects

// Statistics about stacks.
    public ulong stacks_inuse; // bytes in manually-managed stack spans; computed by updatememstats
    public sysMemStat stacks_sys; // only counts newosproc0 stack in mstats; differs from MemStats.StackSys

// Statistics about allocation of low-level fixed-size structures.
// Protected by FixAlloc locks.
    public ulong mspan_inuse; // mspan structures
    public sysMemStat mspan_sys;
    public ulong mcache_inuse; // mcache structures
    public sysMemStat mcache_sys;
    public sysMemStat buckhash_sys; // profiling bucket hash table

// Statistics about GC overhead.
    public ulong gcWorkBufInUse; // computed by updatememstats
    public ulong gcProgPtrScalarBitsInUse; // computed by updatememstats
    public sysMemStat gcMiscSys; // updated atomically or during STW

// Miscellaneous statistics.
    public sysMemStat other_sys; // updated atomically or during STW

// Statistics about the garbage collector.

// Protected by mheap or stopping the world during GC.
    public ulong last_gc_unix; // last gc (in unix time)
    public ulong pause_total_ns;
    public array<ulong> pause_ns; // circular buffer of recent gc pause lengths
    public array<ulong> pause_end; // circular buffer of recent gc end times (nanoseconds since 1970)
    public uint numgc;
    public uint numforcedgc; // number of user-forced GCs
    public double gc_cpu_fraction; // fraction of CPU time used by GC
    public bool enablegc;
    public bool debuggc; // Statistics about allocation size classes.

    public array<uint> _;
    public ulong last_gc_nanotime; // last gc (monotonic time)
    public ulong last_heap_inuse; // heap_inuse at mark termination of the previous GC

// heapStats is a set of statistics
    public consistentHeapStats heapStats; // _ uint32 // ensure gcPauseDist is aligned

// gcPauseDist represents the distribution of all GC-related
// application pauses in the runtime.
//
// Each individual pause is counted separately, unlike pause_ns.
    public timeHistogram gcPauseDist;
}

private static mstats memstats = default;

// A MemStats records statistics about the memory allocator.
public partial struct MemStats {
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

private static void init() {
    {
        var offset__prev1 = offset;

        var offset = @unsafe.Offsetof(memstats.heapStats);

        if (offset % 8 != 0) {
            println(offset);
            throw("memstats.heapStats not aligned to 8 bytes");
        }
        offset = offset__prev1;

    }
    {
        var offset__prev1 = offset;

        offset = @unsafe.Offsetof(memstats.gcPauseDist);

        if (offset % 8 != 0) {
            println(offset);
            throw("memstats.gcPauseDist not aligned to 8 bytes");
        }
        offset = offset__prev1;

    } 
    // Ensure the size of heapStatsDelta causes adjacent fields/slots (e.g.
    // [3]heapStatsDelta) to be 8-byte aligned.
    {
        var size = @unsafe.Sizeof(new heapStatsDelta());

        if (size % 8 != 0) {
            println(size);
            throw("heapStatsDelta not a multiple of 8 bytes in size");
        }
    }
}

// ReadMemStats populates m with memory allocator statistics.
//
// The returned memory allocator statistics are up to date as of the
// call to ReadMemStats. This is in contrast with a heap profile,
// which is a snapshot as of the most recently completed garbage
// collection cycle.
public static void ReadMemStats(ptr<MemStats> _addr_m) {
    ref MemStats m = ref _addr_m.val;

    stopTheWorld("read mem stats");

    systemstack(() => {
        readmemstats_m(_addr_m);
    });

    startTheWorld();
}

private static void readmemstats_m(ptr<MemStats> _addr_stats) {
    ref MemStats stats = ref _addr_stats.val;

    updatememstats();

    stats.Alloc = memstats.alloc;
    stats.TotalAlloc = memstats.total_alloc;
    stats.Sys = memstats.sys;
    stats.Mallocs = memstats.nmalloc;
    stats.Frees = memstats.nfree;
    stats.HeapAlloc = memstats.alloc;
    stats.HeapSys = memstats.heap_sys.load(); 
    // By definition, HeapIdle is memory that was mapped
    // for the heap but is not currently used to hold heap
    // objects. It also specifically is memory that can be
    // used for other purposes, like stacks, but this memory
    // is subtracted out of HeapSys before it makes that
    // transition. Put another way:
    //
    // heap_sys = bytes allocated from the OS for the heap - bytes ultimately used for non-heap purposes
    // heap_idle = bytes allocated from the OS for the heap - bytes ultimately used for any purpose
    //
    // or
    //
    // heap_sys = sys - stacks_inuse - gcWorkBufInUse - gcProgPtrScalarBitsInUse
    // heap_idle = sys - stacks_inuse - gcWorkBufInUse - gcProgPtrScalarBitsInUse - heap_inuse
    //
    // => heap_idle = heap_sys - heap_inuse
    stats.HeapIdle = memstats.heap_sys.load() - memstats.heap_inuse;
    stats.HeapInuse = memstats.heap_inuse;
    stats.HeapReleased = memstats.heap_released;
    stats.HeapObjects = memstats.heap_objects;
    stats.StackInuse = memstats.stacks_inuse; 
    // memstats.stacks_sys is only memory mapped directly for OS stacks.
    // Add in heap-allocated stack memory for user consumption.
    stats.StackSys = memstats.stacks_inuse + memstats.stacks_sys.load();
    stats.MSpanInuse = memstats.mspan_inuse;
    stats.MSpanSys = memstats.mspan_sys.load();
    stats.MCacheInuse = memstats.mcache_inuse;
    stats.MCacheSys = memstats.mcache_sys.load();
    stats.BuckHashSys = memstats.buckhash_sys.load(); 
    // MemStats defines GCSys as an aggregate of all memory related
    // to the memory management system, but we track this memory
    // at a more granular level in the runtime.
    stats.GCSys = memstats.gcMiscSys.load() + memstats.gcWorkBufInUse + memstats.gcProgPtrScalarBitsInUse;
    stats.OtherSys = memstats.other_sys.load();
    stats.NextGC = gcController.heapGoal;
    stats.LastGC = memstats.last_gc_unix;
    stats.PauseTotalNs = memstats.pause_total_ns;
    stats.PauseNs = memstats.pause_ns;
    stats.PauseEnd = memstats.pause_end;
    stats.NumGC = memstats.numgc;
    stats.NumForcedGC = memstats.numforcedgc;
    stats.GCCPUFraction = memstats.gc_cpu_fraction;
    stats.EnableGC = true; 

    // Handle BySize. Copy N values, where N is
    // the minimum of the lengths of the two arrays.
    // Unfortunately copy() won't work here because
    // the arrays have different structs.
    //
    // TODO(mknyszek): Consider renaming the fields
    // of by_size's elements to align so we can use
    // the copy built-in.
    var bySizeLen = len(stats.BySize);
    {
        var l = len(memstats.by_size);

        if (l < bySizeLen) {
            bySizeLen = l;
        }
    }
    for (nint i = 0; i < bySizeLen; i++) {
        stats.BySize[i].Size = memstats.by_size[i].size;
        stats.BySize[i].Mallocs = memstats.by_size[i].nmalloc;
        stats.BySize[i].Frees = memstats.by_size[i].nfree;
    }
}

//go:linkname readGCStats runtime/debug.readGCStats
private static void readGCStats(ptr<slice<ulong>> _addr_pauses) {
    ref slice<ulong> pauses = ref _addr_pauses.val;

    systemstack(() => {
        readGCStats_m(_addr_pauses);
    });
}

// readGCStats_m must be called on the system stack because it acquires the heap
// lock. See mheap for details.
//go:systemstack
private static void readGCStats_m(ptr<slice<ulong>> _addr_pauses) {
    ref slice<ulong> pauses = ref _addr_pauses.val;

    slice<ulong> p = pauses; 
    // Calling code in runtime/debug should make the slice large enough.
    if (cap(p) < len(memstats.pause_ns) + 3) {
        throw("short slice passed to readGCStats");
    }
    lock(_addr_mheap_.@lock);

    var n = memstats.numgc;
    if (n > uint32(len(memstats.pause_ns))) {
        n = uint32(len(memstats.pause_ns));
    }
    p = p[..(int)cap(p)];
    for (var i = uint32(0); i < n; i++) {
        var j = (memstats.numgc - 1 - i) % uint32(len(memstats.pause_ns));
        p[i] = memstats.pause_ns[j];
        p[n + i] = memstats.pause_end[j];
    }

    p[n + n] = memstats.last_gc_unix;
    p[n + n + 1] = uint64(memstats.numgc);
    p[n + n + 2] = memstats.pause_total_ns;
    unlock(_addr_mheap_.@lock);
    pauses = p[..(int)n + n + 3];
}

// Updates the memstats structure.
//
// The world must be stopped.
//
//go:nowritebarrier
private static void updatememstats() {
    assertWorldStopped(); 

    // Flush mcaches to mcentral before doing anything else.
    //
    // Flushing to the mcentral may in general cause stats to
    // change as mcentral data structures are manipulated.
    systemstack(flushallmcaches);

    memstats.mcache_inuse = uint64(mheap_.cachealloc.inuse);
    memstats.mspan_inuse = uint64(mheap_.spanalloc.inuse);
    memstats.sys = memstats.heap_sys.load() + memstats.stacks_sys.load() + memstats.mspan_sys.load() + memstats.mcache_sys.load() + memstats.buckhash_sys.load() + memstats.gcMiscSys.load() + memstats.other_sys.load(); 

    // Calculate memory allocator stats.
    // During program execution we only count number of frees and amount of freed memory.
    // Current number of alive objects in the heap and amount of alive heap memory
    // are calculated by scanning all spans.
    // Total number of mallocs is calculated as number of frees plus number of alive objects.
    // Similarly, total amount of allocated memory is calculated as amount of freed memory
    // plus amount of alive heap memory.
    memstats.alloc = 0;
    memstats.total_alloc = 0;
    memstats.nmalloc = 0;
    memstats.nfree = 0;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(memstats.by_size); i++) {
            memstats.by_size[i].nmalloc = 0;
            memstats.by_size[i].nfree = 0;
        }

        i = i__prev1;
    } 
    // Collect consistent stats, which are the source-of-truth in the some cases.
    ref heapStatsDelta consStats = ref heap(out ptr<heapStatsDelta> _addr_consStats);
    memstats.heapStats.unsafeRead(_addr_consStats); 

    // Collect large allocation stats.
    var totalAlloc = uint64(consStats.largeAlloc);
    memstats.nmalloc += uint64(consStats.largeAllocCount);
    var totalFree = uint64(consStats.largeFree);
    memstats.nfree += uint64(consStats.largeFreeCount); 

    // Collect per-sizeclass stats.
    {
        nint i__prev1 = i;

        for (i = 0; i < _NumSizeClasses; i++) { 
            // Malloc stats.
            var a = uint64(consStats.smallAllocCount[i]);
            totalAlloc += a * uint64(class_to_size[i]);
            memstats.nmalloc += a;
            memstats.by_size[i].nmalloc = a; 

            // Free stats.
            var f = uint64(consStats.smallFreeCount[i]);
            totalFree += f * uint64(class_to_size[i]);
            memstats.nfree += f;
            memstats.by_size[i].nfree = f;
        }

        i = i__prev1;
    } 

    // Account for tiny allocations.
    memstats.nfree += uint64(consStats.tinyAllocCount);
    memstats.nmalloc += uint64(consStats.tinyAllocCount); 

    // Calculate derived stats.
    memstats.total_alloc = totalAlloc;
    memstats.alloc = totalAlloc - totalFree;
    memstats.heap_objects = memstats.nmalloc - memstats.nfree;

    memstats.stacks_inuse = uint64(consStats.inStacks);
    memstats.gcWorkBufInUse = uint64(consStats.inWorkBufs);
    memstats.gcProgPtrScalarBitsInUse = uint64(consStats.inPtrScalarBits); 

    // We also count stacks_inuse, gcWorkBufInUse, and gcProgPtrScalarBitsInUse as sys memory.
    memstats.sys += memstats.stacks_inuse + memstats.gcWorkBufInUse + memstats.gcProgPtrScalarBitsInUse; 

    // The world is stopped, so the consistent stats (after aggregation)
    // should be identical to some combination of memstats. In particular:
    //
    // * heap_inuse == inHeap
    // * heap_released == released
    // * heap_sys - heap_released == committed - inStacks - inWorkBufs - inPtrScalarBits
    //
    // Check if that's actually true.
    //
    // TODO(mknyszek): Maybe don't throw here. It would be bad if a
    // bug in otherwise benign accounting caused the whole application
    // to crash.
    if (memstats.heap_inuse != uint64(consStats.inHeap)) {
        print("runtime: heap_inuse=", memstats.heap_inuse, "\n");
        print("runtime: consistent value=", consStats.inHeap, "\n");
        throw("heap_inuse and consistent stats are not equal");
    }
    if (memstats.heap_released != uint64(consStats.released)) {
        print("runtime: heap_released=", memstats.heap_released, "\n");
        print("runtime: consistent value=", consStats.released, "\n");
        throw("heap_released and consistent stats are not equal");
    }
    var globalRetained = memstats.heap_sys.load() - memstats.heap_released;
    var consRetained = uint64(consStats.committed - consStats.inStacks - consStats.inWorkBufs - consStats.inPtrScalarBits);
    if (globalRetained != consRetained) {
        print("runtime: global value=", globalRetained, "\n");
        print("runtime: consistent value=", consRetained, "\n");
        throw("measures of the retained heap are not equal");
    }
}

// flushmcache flushes the mcache of allp[i].
//
// The world must be stopped.
//
//go:nowritebarrier
private static void flushmcache(nint i) {
    assertWorldStopped();

    var p = allp[i];
    var c = p.mcache;
    if (c == null) {
        return ;
    }
    c.releaseAll();
    stackcache_clear(c);
}

// flushallmcaches flushes the mcaches of all Ps.
//
// The world must be stopped.
//
//go:nowritebarrier
private static void flushallmcaches() {
    assertWorldStopped();

    for (nint i = 0; i < int(gomaxprocs); i++) {
        flushmcache(i);
    }
}

// sysMemStat represents a global system statistic that is managed atomically.
//
// This type must structurally be a uint64 so that mstats aligns with MemStats.
private partial struct sysMemStat { // : ulong
}

// load atomically reads the value of the stat.
//
// Must be nosplit as it is called in runtime initialization, e.g. newosproc0.
//go:nosplit
private static ulong load(this ptr<sysMemStat> _addr_s) {
    ref sysMemStat s = ref _addr_s.val;

    return atomic.Load64((uint64.val)(s));
}

// add atomically adds the sysMemStat by n.
//
// Must be nosplit as it is called in runtime initialization, e.g. newosproc0.
//go:nosplit
private static void add(this ptr<sysMemStat> _addr_s, long n) {
    ref sysMemStat s = ref _addr_s.val;

    if (s == null) {
        return ;
    }
    var val = atomic.Xadd64((uint64.val)(s), n);
    if ((n > 0 && int64(val) < n) || (n < 0 && int64(val) + n < n)) {
        print("runtime: val=", val, " n=", n, "\n");
        throw("sysMemStat overflow");
    }
}

// heapStatsDelta contains deltas of various runtime memory statistics
// that need to be updated together in order for them to be kept
// consistent with one another.
private partial struct heapStatsDelta {
    public long committed; // byte delta of memory committed
    public long released; // byte delta of released memory generated
    public long inHeap; // byte delta of memory placed in the heap
    public long inStacks; // byte delta of memory reserved for stacks
    public long inWorkBufs; // byte delta of memory reserved for work bufs
    public long inPtrScalarBits; // byte delta of memory reserved for unrolled GC prog bits

// Allocator stats.
    public System.UIntPtr tinyAllocCount; // number of tiny allocations
    public System.UIntPtr largeAlloc; // bytes allocated for large objects
    public System.UIntPtr largeAllocCount; // number of large object allocations
    public array<System.UIntPtr> smallAllocCount; // number of allocs for small objects
    public System.UIntPtr largeFree; // bytes freed for large objects (>maxSmallSize)
    public System.UIntPtr largeFreeCount; // number of frees for large objects (>maxSmallSize)
    public array<System.UIntPtr> smallFreeCount; // number of frees for small objects (<=maxSmallSize)

// Add a uint32 to ensure this struct is a multiple of 8 bytes in size.
// Only necessary on 32-bit platforms.
    public array<uint> _;
}

// merge adds in the deltas from b into a.
private static void merge(this ptr<heapStatsDelta> _addr_a, ptr<heapStatsDelta> _addr_b) {
    ref heapStatsDelta a = ref _addr_a.val;
    ref heapStatsDelta b = ref _addr_b.val;

    a.committed += b.committed;
    a.released += b.released;
    a.inHeap += b.inHeap;
    a.inStacks += b.inStacks;
    a.inWorkBufs += b.inWorkBufs;
    a.inPtrScalarBits += b.inPtrScalarBits;

    a.tinyAllocCount += b.tinyAllocCount;
    a.largeAlloc += b.largeAlloc;
    a.largeAllocCount += b.largeAllocCount;
    {
        var i__prev1 = i;

        foreach (var (__i) in b.smallAllocCount) {
            i = __i;
            a.smallAllocCount[i] += b.smallAllocCount[i];
        }
        i = i__prev1;
    }

    a.largeFree += b.largeFree;
    a.largeFreeCount += b.largeFreeCount;
    {
        var i__prev1 = i;

        foreach (var (__i) in b.smallFreeCount) {
            i = __i;
            a.smallFreeCount[i] += b.smallFreeCount[i];
        }
        i = i__prev1;
    }
}

// consistentHeapStats represents a set of various memory statistics
// whose updates must be viewed completely to get a consistent
// state of the world.
//
// To write updates to memory stats use the acquire and release
// methods. To obtain a consistent global snapshot of these statistics,
// use read.
private partial struct consistentHeapStats {
    public array<heapStatsDelta> stats; // gen represents the current index into which writers
// are writing, and can take on the value of 0, 1, or 2.
// This value is updated atomically.
    public uint gen; // noPLock is intended to provide mutual exclusion for updating
// stats when no P is available. It does not block other writers
// with a P, only other writers without a P and the reader. Because
// stats are usually updated when a P is available, contention on
// this lock should be minimal.
    public mutex noPLock;
}

// acquire returns a heapStatsDelta to be updated. In effect,
// it acquires the shard for writing. release must be called
// as soon as the relevant deltas are updated.
//
// The returned heapStatsDelta must be updated atomically.
//
// The caller's P must not change between acquire and
// release. This also means that the caller should not
// acquire a P or release its P in between.
private static ptr<heapStatsDelta> acquire(this ptr<consistentHeapStats> _addr_m) {
    ref consistentHeapStats m = ref _addr_m.val;

    {
        var pp = getg().m.p.ptr();

        if (pp != null) {
            var seq = atomic.Xadd(_addr_pp.statsSeq, 1);
            if (seq % 2 == 0) { 
                // Should have been incremented to odd.
                print("runtime: seq=", seq, "\n");
                throw("bad sequence number");
            }
        }
        else
 {
            lock(_addr_m.noPLock);
        }
    }
    var gen = atomic.Load(_addr_m.gen) % 3;
    return _addr__addr_m.stats[gen]!;
}

// release indicates that the writer is done modifying
// the delta. The value returned by the corresponding
// acquire must no longer be accessed or modified after
// release is called.
//
// The caller's P must not change between acquire and
// release. This also means that the caller should not
// acquire a P or release its P in between.
private static void release(this ptr<consistentHeapStats> _addr_m) {
    ref consistentHeapStats m = ref _addr_m.val;

    {
        var pp = getg().m.p.ptr();

        if (pp != null) {
            var seq = atomic.Xadd(_addr_pp.statsSeq, 1);
            if (seq % 2 != 0) { 
                // Should have been incremented to even.
                print("runtime: seq=", seq, "\n");
                throw("bad sequence number");
            }
        }
        else
 {
            unlock(_addr_m.noPLock);
        }
    }
}

// unsafeRead aggregates the delta for this shard into out.
//
// Unsafe because it does so without any synchronization. The
// world must be stopped.
private static void unsafeRead(this ptr<consistentHeapStats> _addr_m, ptr<heapStatsDelta> _addr_@out) {
    ref consistentHeapStats m = ref _addr_m.val;
    ref heapStatsDelta @out = ref _addr_@out.val;

    assertWorldStopped();

    foreach (var (i) in m.stats) {
        @out.merge(_addr_m.stats[i]);
    }
}

// unsafeClear clears the shard.
//
// Unsafe because the world must be stopped and values should
// be donated elsewhere before clearing.
private static void unsafeClear(this ptr<consistentHeapStats> _addr_m) {
    ref consistentHeapStats m = ref _addr_m.val;

    assertWorldStopped();

    foreach (var (i) in m.stats) {
        m.stats[i] = new heapStatsDelta();
    }
}

// read takes a globally consistent snapshot of m
// and puts the aggregated value in out. Even though out is a
// heapStatsDelta, the resulting values should be complete and
// valid statistic values.
//
// Not safe to call concurrently. The world must be stopped
// or metricsSema must be held.
private static void read(this ptr<consistentHeapStats> _addr_m, ptr<heapStatsDelta> _addr_@out) {
    ref consistentHeapStats m = ref _addr_m.val;
    ref heapStatsDelta @out = ref _addr_@out.val;
 
    // Getting preempted after this point is not safe because
    // we read allp. We need to make sure a STW can't happen
    // so it doesn't change out from under us.
    var mp = acquirem(); 

    // Get the current generation. We can be confident that this
    // will not change since read is serialized and is the only
    // one that modifies currGen.
    var currGen = atomic.Load(_addr_m.gen);
    var prevGen = currGen - 1;
    if (currGen == 0) {
        prevGen = 2;
    }
    lock(_addr_m.noPLock); 

    // Rotate gen, effectively taking a snapshot of the state of
    // these statistics at the point of the exchange by moving
    // writers to the next set of deltas.
    //
    // This exchange is safe to do because we won't race
    // with anyone else trying to update this value.
    atomic.Xchg(_addr_m.gen, (currGen + 1) % 3); 

    // Allow P-less writers to continue. They'll be writing to the
    // next generation now.
    unlock(_addr_m.noPLock);

    foreach (var (_, p) in allp) { 
        // Spin until there are no more writers.
        while (atomic.Load(_addr_p.statsSeq) % 2 != 0) {
        }
    }    m.stats[currGen].merge(_addr_m.stats[prevGen]);
    m.stats[prevGen] = new heapStatsDelta(); 

    // Finally, copy out the complete delta.
    out.val = m.stats[currGen];

    releasem(mp);
}

} // end runtime_package
