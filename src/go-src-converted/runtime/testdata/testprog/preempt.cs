// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:25 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\preempt.go
namespace go;

using runtime = runtime_package;
using debug = runtime.debug_package;
using atomic = sync.atomic_package;
using System;
using System.Threading;

public static partial class main_package {

private static void init() {
    register("AsyncPreempt", AsyncPreempt);
}

public static void AsyncPreempt() { 
    // Run with just 1 GOMAXPROCS so the runtime is required to
    // use scheduler preemption.
    runtime.GOMAXPROCS(1); 
    // Disable GC so we have complete control of what we're testing.
    debug.SetGCPercent(-1); 

    // Start a goroutine with no sync safe-points.
    ref uint ready = ref heap(out ptr<uint> _addr_ready);    ref uint ready2 = ref heap(out ptr<uint> _addr_ready2);

    go_(() => () => {
        while (true) {
            atomic.StoreUint32(_addr_ready, 1);
            dummy();
            dummy();
        }
    }()); 
    // Also start one with a frameless function.
    // This is an especially interesting case for
    // LR machines.
    go_(() => () => {
        atomic.AddUint32(_addr_ready2, 1);
        frameless();
    }()); 
    // Also test empty infinite loop.
    go_(() => () => {
        atomic.AddUint32(_addr_ready2, 1);
        while (true) {
        }
    }()); 

    // Wait for the goroutine to stop passing through sync
    // safe-points.
    while (atomic.LoadUint32(_addr_ready) == 0 || atomic.LoadUint32(_addr_ready2) < 2) {
        runtime.Gosched();
    } 

    // Run a GC, which will have to stop the goroutine for STW and
    // for stack scanning. If this doesn't work, the test will
    // deadlock and timeout.
    runtime.GC();

    println("OK");
}

//go:noinline
private static void frameless() {
    for (var i = int64(0); i < 1 << 62; i++) {
        out += i * i * i * i * i * 12345;
    }
}

private static long @out = default;

//go:noinline
private static void dummy() {
}

} // end main_package
