// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:46 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\runtime.go
using atomic = go.runtime.@internal.atomic_package;
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        //go:generate go run wincallback.go
        //go:generate go run mkduff.go
        //go:generate go run mkfastlog2table.go
        private static var ticks = default;

        // Note: Called by runtime/pprof in addition to runtime code.
        private static long tickspersecond()
        {
            var r = int64(atomic.Load64(ref ticks.val));
            if (r != 0L)
            {
                return r;
            }
            lock(ref ticks.@lock);
            r = int64(ticks.val);
            if (r == 0L)
            {
                var t0 = nanotime();
                var c0 = cputicks();
                usleep(100L * 1000L);
                var t1 = nanotime();
                var c1 = cputicks();
                if (t1 == t0)
                {
                    t1++;
                }
                r = (c1 - c0) * 1000L * 1000L * 1000L / (t1 - t0);
                if (r == 0L)
                {
                    r++;
                }
                atomic.Store64(ref ticks.val, uint64(r));
            }
            unlock(ref ticks.@lock);
            return r;
        }

        private static slice<@string> envs = default;
        private static slice<@string> argslice = default;

        //go:linkname syscall_runtime_envs syscall.runtime_envs
        private static slice<@string> syscall_runtime_envs()
        {
            return append(new slice<@string>(new @string[] {  }), envs);
        }

        //go:linkname syscall_Getpagesize syscall.Getpagesize
        private static long syscall_Getpagesize()
        {
            return int(physPageSize);
        }

        //go:linkname os_runtime_args os.runtime_args
        private static slice<@string> os_runtime_args()
        {
            return append(new slice<@string>(new @string[] {  }), argslice);
        }

        //go:linkname syscall_Exit syscall.Exit
        //go:nosplit
        private static void syscall_Exit(long code)
        {
            exit(int32(code));
        }
    }
}
