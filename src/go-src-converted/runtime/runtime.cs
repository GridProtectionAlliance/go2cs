// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:11:15 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\runtime.go
using atomic = go.runtime.@internal.atomic_package;
using _@unsafe_ = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    //go:generate go run wincallback.go
    //go:generate go run mkduff.go
    //go:generate go run mkfastlog2table.go
private static var ticks = default;

// Note: Called by runtime/pprof in addition to runtime code.
private static long tickspersecond() {
    var r = int64(atomic.Load64(_addr_ticks.val));
    if (r != 0) {
        return r;
    }
    lock(_addr_ticks.@lock);
    r = int64(ticks.val);
    if (r == 0) {
        var t0 = nanotime();
        var c0 = cputicks();
        usleep(100 * 1000);
        var t1 = nanotime();
        var c1 = cputicks();
        if (t1 == t0) {
            t1++;
        }
        r = (c1 - c0) * 1000 * 1000 * 1000 / (t1 - t0);
        if (r == 0) {
            r++;
        }
        atomic.Store64(_addr_ticks.val, uint64(r));

    }
    unlock(_addr_ticks.@lock);
    return r;

}

private static slice<@string> envs = default;
private static slice<@string> argslice = default;

//go:linkname syscall_runtime_envs syscall.runtime_envs
private static slice<@string> syscall_runtime_envs() {
    return append(new slice<@string>(new @string[] {  }), envs);
}

//go:linkname syscall_Getpagesize syscall.Getpagesize
private static nint syscall_Getpagesize() {
    return int(physPageSize);
}

//go:linkname os_runtime_args os.runtime_args
private static slice<@string> os_runtime_args() {
    return append(new slice<@string>(new @string[] {  }), argslice);
}

//go:linkname syscall_Exit syscall.Exit
//go:nosplit
private static void syscall_Exit(nint code) {
    exit(int32(code));
}

} // end runtime_package
