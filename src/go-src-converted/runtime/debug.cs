// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:45:42 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\debug.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // GOMAXPROCS sets the maximum number of CPUs that can be executing
        // simultaneously and returns the previous setting. If n < 1, it does not
        // change the current setting.
        // The number of logical CPUs on the local machine can be queried with NumCPU.
        // This call will go away when the scheduler improves.
        public static long GOMAXPROCS(long n)
        {
            if (GOARCH == "wasm" && n > 1L)
            {
                n = 1L; // WebAssembly has no threads yet, so only one CPU is possible.
            }
            lock(_addr_sched.@lock);
            var ret = int(gomaxprocs);
            unlock(_addr_sched.@lock);
            if (n <= 0L || n == ret)
            {
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
        public static long NumCPU()
        {
            return int(ncpu);
        }

        // NumCgoCall returns the number of cgo calls made by the current process.
        public static long NumCgoCall()
        {
            long n = default;
            {
                var mp = (m.val)(atomic.Loadp(@unsafe.Pointer(_addr_allm)));

                while (mp != null)
                {
                    n += int64(mp.ncgocall);
                    mp = mp.alllink;
                }

            }
            return n;

        }

        // NumGoroutine returns the number of goroutines that currently exist.
        public static long NumGoroutine()
        {
            return int(gcount());
        }

        //go:linkname debug_modinfo runtime/debug.modinfo
        private static @string debug_modinfo()
        {
            return modinfo;
        }
    }
}
