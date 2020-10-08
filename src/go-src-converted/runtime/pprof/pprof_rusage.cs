// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin linux

// package pprof -- go2cs converted at 2020 October 08 03:26:20 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Go\src\runtime\pprof\pprof_rusage.go
using fmt = go.fmt_package;
using io = go.io_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace runtime
{
    public static partial class pprof_package
    {
        // Adds MaxRSS to platforms that are supported.
        private static void addMaxRSS(io.Writer w) => func((_, panic, __) =>
        {
            System.UIntPtr rssToBytes = default;
            switch (runtime.GOOS)
            {
                case "linux": 

                case "android": 
                    rssToBytes = 1024L;
                    break;
                case "darwin": 
                    rssToBytes = 1L;
                    break;
                default: 
                    panic("unsupported OS");
                    break;
            }

            ref syscall.Rusage rusage = ref heap(out ptr<syscall.Rusage> _addr_rusage);
            syscall.Getrusage(0L, _addr_rusage);
            fmt.Fprintf(w, "# MaxRSS = %d\n", uintptr(rusage.Maxrss) * rssToBytes);

        });
    }
}}
