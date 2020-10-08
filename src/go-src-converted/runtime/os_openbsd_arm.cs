// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:22:05 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_openbsd_arm.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void checkgoarm()
        { 
            // TODO(minux): FP checks like in os_linux_arm.go.

            // osinit not called yet, so ncpu not set: must use getncpu directly.
            if (getncpu() > 1L && goarm < 7L)
            {
                print("runtime: this system has multiple CPUs and must use\n");
                print("atomic synchronization instructions. Recompile using GOARM=7.\n");
                exit(1L);
            }
        }

        //go:nosplit
        private static long cputicks()
        { 
            // Currently cputicks() is used in blocking profiler and to seed runtime·fastrand().
            // runtime·nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
            return nanotime();

        }
    }
}
