// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:06 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_plan9_arm.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static bool hardDiv = default; // TODO: set if a hardware divider is available

        private static void checkgoarm()
        {
            return; // TODO(minux)
        }

        //go:nosplit
        private static long cputicks()
        { 
            // Currently cputicks() is used in blocking profiler and to seed runtime·fastrand().
            // runtime·nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
            // TODO: need more entropy to better seed fastrand.
            return nanotime();
        }
    }
}
