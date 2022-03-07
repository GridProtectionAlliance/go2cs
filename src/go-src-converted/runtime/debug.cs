// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:27 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\debug.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // GOMAXPROCS sets the maximum number of CPUs that can be executing
    // simultaneously and returns the previous setting. It defaults to
    // the value of runtime.NumCPU. If n < 1, it does not change the current setting.
    // This call will go away when the scheduler improves.
public static nint GOMAXPROCS(nint n) {
    if (GOARCH == "wasm" && n > 1) {
        n = 1; // WebAssembly has no threads yet, so only one CPU is possible.
    }
    lock(_addr_sched.@lock);
    var ret = int(gomaxprocs);
    unlock(_addr_sched.@lock);
    if (n <= 0 || n == ret) {
        return ret;
    }
    stopTheWorldGC("GOMAXPROCS"); 

    // newprocs will be processed by startTheWorld
    newprocs = int32(n);

    startTheWorldGC();
    return ret;

}

// NumCPU returns the number of logical CPUs usable by the current process.
//
// The set of available CPUs is checked by querying the operating system
// at process startup. Changes to operating system CPU allocation after
// process startup are not reflected.
public static nint NumCPU() {
    return int(ncpu);
}

// NumCgoCall returns the number of cgo calls made by the current process.
public static long NumCgoCall() {
    var n = int64(atomic.Load64(_addr_ncgocall));
    {
        var mp = (m.val)(atomic.Loadp(@unsafe.Pointer(_addr_allm)));

        while (mp != null) {
            n += int64(mp.ncgocall);
            mp = mp.alllink;
        }
    }
    return n;

}

// NumGoroutine returns the number of goroutines that currently exist.
public static nint NumGoroutine() {
    return int(gcount());
}

//go:linkname debug_modinfo runtime/debug.modinfo
private static @string debug_modinfo() {
    return modinfo;
}

} // end runtime_package
