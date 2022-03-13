// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || linux
// +build darwin linux

// package pprof -- go2cs converted at 2022 March 13 05:28:49 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Program Files\Go\src\runtime\pprof\pprof_rusage.go
namespace go.runtime;

using fmt = fmt_package;
using io = io_package;
using runtime = runtime_package;
using syscall = syscall_package;


// Adds MaxRSS to platforms that are supported.

public static partial class pprof_package {

private static void addMaxRSS(io.Writer w) => func((_, panic, _) => {
    System.UIntPtr rssToBytes = default;
    switch (runtime.GOOS) {
        case "linux": 

        case "android": 
            rssToBytes = 1024;
            break;
        case "darwin": 

        case "ios": 
            rssToBytes = 1;
            break;
        default: 
            panic("unsupported OS");
            break;
    }

    ref syscall.Rusage rusage = ref heap(out ptr<syscall.Rusage> _addr_rusage);
    syscall.Getrusage(0, _addr_rusage);
    fmt.Fprintf(w, "# MaxRSS = %d\n", uintptr(rusage.Maxrss) * rssToBytes);
});

} // end pprof_package
