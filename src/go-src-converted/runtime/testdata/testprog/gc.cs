// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:26:01 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\gc.go
using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using debug = go.runtime.debug_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;
using System;
using System.Threading;


namespace go;

public static partial class main_package {

private static void init() {
    register("GCFairness", GCFairness);
    register("GCFairness2", GCFairness2);
    register("GCSys", GCSys);
    register("GCPhys", GCPhys);
    register("DeferLiveness", DeferLiveness);
    register("GCZombie", GCZombie);
}

public static void GCSys() {
    runtime.GOMAXPROCS(1);
    ptr<object> memstats = @new<runtime.MemStats>();
    runtime.GC();
    runtime.ReadMemStats(memstats);
    var sys = memstats.Sys;

    runtime.MemProfileRate = 0; // disable profiler

    nint itercount = 100000;
    for (nint i = 0; i < itercount; i++) {
        workthegc();
    } 

    // Should only be using a few MB.
    // We allocated 100 MB or (if not short) 1 GB.
    runtime.ReadMemStats(memstats);
    if (sys > memstats.Sys) {
        sys = 0;
    }
    else
 {
        sys = memstats.Sys - sys;
    }
    if (sys > 16 << 20) {
        fmt.Printf("using too much memory: %d bytes\n", sys);
        return ;
    }
    fmt.Printf("OK\n");

}

private static slice<byte> sink = default;

private static slice<byte> workthegc() {
    sink = make_slice<byte>(1029);
    return sink;
}

public static void GCFairness() {
    runtime.GOMAXPROCS(1);
    var (f, err) = os.Open("/dev/null");
    if (os.IsNotExist(err)) { 
        // This test tests what it is intended to test only if writes are fast.
        // If there is no /dev/null, we just don't execute the test.
        fmt.Println("OK");
        return ;

    }
    if (err != null) {
        fmt.Println(err);
        os.Exit(1);
    }
    for (nint i = 0; i < 2; i++) {
        go_(() => () => {
            while (true) {
                f.Write((slice<byte>)".");
            }
        }());
    }
    time.Sleep(10 * time.Millisecond);
    fmt.Println("OK");

}

public static void GCFairness2() { 
    // Make sure user code can't exploit the GC's high priority
    // scheduling to make scheduling of user code unfair. See
    // issue #15706.
    runtime.GOMAXPROCS(1);
    debug.SetGCPercent(1);
    array<long> count = new array<long>(3);
    var sink = default;
    {
        var i__prev1 = i;

        foreach (var (__i) in count) {
            i = __i;
            go_(() => i => {
                while (true) {
                    sink[i] = make_slice<byte>(1024);
                    atomic.AddInt64(_addr_count[i], 1);
                }
            }(i));
        }
        i = i__prev1;
    }

    time.Sleep(30 * time.Millisecond);
    bool fail = default;
    {
        var i__prev1 = i;

        foreach (var (__i) in count) {
            i = __i;
            if (atomic.LoadInt64(_addr_count[i]) == 0) {
                fail = true;
            }
        }
        i = i__prev1;
    }

    if (fail) {
        time.Sleep(1 * time.Second);
        {
            var i__prev1 = i;

            foreach (var (__i) in count) {
                i = __i;
                if (atomic.LoadInt64(_addr_count[i]) == 0) {
                    fmt.Printf("goroutine %d did not run\n", i);
                    return ;
                }
            }

            i = i__prev1;
        }
    }
    fmt.Println("OK");

}

public static void GCPhys() => func((defer, _, _) => { 
    // This test ensures that heap-growth scavenging is working as intended.
    //
    // It sets up a specific scenario: it allocates two pairs of objects whose
    // sizes sum to size. One object in each pair is "small" (though must be
    // large enough to be considered a large object by the runtime) and one is
    // large. The small objects are kept while the large objects are freed,
    // creating two large unscavenged holes in the heap. The heap goal should
    // also be small as a result (so size must be at least as large as the
    // minimum heap size). We then allocate one large object, bigger than both
    // pairs of objects combined. This allocation, because it will tip
    // HeapSys-HeapReleased well above the heap goal, should trigger heap-growth
    // scavenging and scavenge most, if not all, of the large holes we created
    // earlier.
 
    // Size must be also large enough to be considered a large
    // object (not in any size-segregated span).
    const nint size = 4 << 20;
    const nint split = 64 << 10;
    const nint objects = 2; 

    // The page cache could hide 64 8-KiB pages from the scavenger today.
    const nint maxPageCache = (8 << 10) * 64; 

    // Reduce GOMAXPROCS down to 4 if it's greater. We need to bound the amount
    // of memory held in the page cache because the scavenger can't reach it.
    // The page cache will hold at most maxPageCache of memory per-P, so this
    // bounds the amount of memory hidden from the scavenger to 4*maxPageCache
    // at most.
    const nint maxProcs = 4;
 
    // Set GOGC so that this test operates under consistent assumptions.
    debug.SetGCPercent(100);
    var procs = runtime.GOMAXPROCS(-1);
    if (procs > maxProcs) {
        defer(runtime.GOMAXPROCS(runtime.GOMAXPROCS(maxProcs)));
        procs = runtime.GOMAXPROCS(-1);
    }
    var saved = make_slice<slice<byte>>(0, objects + 1);
    var condemned = make_slice<slice<byte>>(0, objects);
    for (nint i = 0; i < 2 * objects; i++) {
        if (i % 2 == 0) {
            saved = append(saved, make_slice<byte>(split));
        }
        else
 {
            condemned = append(condemned, make_slice<byte>(size - split));
        }
    }
    condemned = null; 
    // Clean up the heap. This will free up every other object created above
    // (i.e. everything in condemned) creating holes in the heap.
    // Also, if the condemned objects are still being swept, its possible that
    // the scavenging that happens as a result of the next allocation won't see
    // the holes at all. We call runtime.GC() twice here so that when we allocate
    // our large object there's no race with sweeping.
    runtime.GC();
    runtime.GC(); 
    // Perform one big allocation which should also scavenge any holes.
    //
    // The heap goal will rise after this object is allocated, so it's very
    // important that we try to do all the scavenging in a single allocation
    // that exceeds the heap goal. Otherwise the rising heap goal could foil our
    // test.
    saved = append(saved, make_slice<byte>(objects * size)); 
    // Clean up the heap again just to put it in a known state.
    runtime.GC(); 
    // heapBacked is an estimate of the amount of physical memory used by
    // this test. HeapSys is an estimate of the size of the mapped virtual
    // address space (which may or may not be backed by physical pages)
    // whereas HeapReleased is an estimate of the amount of bytes returned
    // to the OS. Their difference then roughly corresponds to the amount
    // of virtual address space that is backed by physical pages.
    ref runtime.MemStats stats = ref heap(out ptr<runtime.MemStats> _addr_stats);
    runtime.ReadMemStats(_addr_stats);
    var heapBacked = stats.HeapSys - stats.HeapReleased; 
    // If heapBacked does not exceed the heap goal by more than retainExtraPercent
    // then the scavenger is working as expected; the newly-created holes have been
    // scavenged immediately as part of the allocations which cannot fit in the holes.
    //
    // Since the runtime should scavenge the entirety of the remaining holes,
    // theoretically there should be no more free and unscavenged memory. However due
    // to other allocations that happen during this test we may still see some physical
    // memory over-use.
    var overuse = (float64(heapBacked) - float64(stats.HeapAlloc)) / float64(stats.HeapAlloc); 
    // Compute the threshold.
    //
    // In theory, this threshold should just be zero, but that's not possible in practice.
    // Firstly, the runtime's page cache can hide up to maxPageCache of free memory from the
    // scavenger per P. To account for this, we increase the threshold by the ratio between the
    // total amount the runtime could hide from the scavenger to the amount of memory we expect
    // to be able to scavenge here, which is (size-split)*objects. This computation is the crux
    // GOMAXPROCS above; if GOMAXPROCS is too high the threshold just becomes 100%+ since the
    // amount of memory being allocated is fixed. Then we add 5% to account for noise, such as
    // other allocations this test may have performed that we don't explicitly account for The
    // baseline threshold here is around 11% for GOMAXPROCS=1, capping out at around 30% for
    // GOMAXPROCS=4.
    float threshold = 0.05F + float64(procs) * maxPageCache / float64((size - split) * objects);
    if (overuse <= threshold) {
        fmt.Println("OK");
        return ;
    }
    fmt.Printf("exceeded physical memory overuse threshold of %3.2f%%: %3.2f%%\n" + "(alloc: %d, goal: %d, sys: %d, rel: %d, objs: %d)\n", threshold * 100, overuse * 100, stats.HeapAlloc, stats.NextGC, stats.HeapSys, stats.HeapReleased, len(saved));
    runtime.KeepAlive(saved);

});

// Test that defer closure is correctly scanned when the stack is scanned.
public static void DeferLiveness() => func((defer, panic, _) => {
    ref array<nint> x = ref heap(new array<nint>(10), out ptr<array<nint>> _addr_x);
    escape(_addr_x);
    Action fn = () => {
        if (x[0] != 42) {
            panic("FAIL");
        }
    };
    defer(fn());

    x[0] = 42;
    runtime.GC();
    runtime.GC();
    runtime.GC();

});

//go:noinline
private static void escape(object x) {
    sink2 = x;

    sink2 = null;
}

private static var sink2 = default;

// Test zombie object detection and reporting.
public static void GCZombie() { 
    // Allocate several objects of unusual size (so free slots are
    // unlikely to all be re-allocated by the runtime).
    const nint size = 190;

    const nint count = 8192 / size;

    var keep = make_slice<ptr<byte>>(0, (count + 1) / 2);
    var free = make_slice<System.UIntPtr>(0, (count + 1) / 2);
    var zombies = make_slice<ptr<byte>>(0, len(free));
    for (nint i = 0; i < count; i++) {
        var obj = make_slice<byte>(size);
        var p = _addr_obj[0];
        if (i % 2 == 0) {
            keep = append(keep, p);
        }
        else
 {
            free = append(free, uintptr(@unsafe.Pointer(p)));
        }
    } 

    // Free the unreferenced objects.
    runtime.GC(); 

    // Bring the free objects back to life.
    {
        var p__prev1 = p;

        foreach (var (_, __p) in free) {
            p = __p;
            zombies = append(zombies, (byte.val)(@unsafe.Pointer(p)));
        }
        p = p__prev1;
    }

    runtime.GC();
    println("failed");
    runtime.KeepAlive(keep);
    runtime.KeepAlive(zombies);

}

} // end main_package
