// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:58 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_nacl_arm.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static bool hardDiv = default; // TODO: set if a hardware divider is available

        private static void checkgoarm()
        { 
            // TODO(minux): FP checks like in os_linux_arm.go.

            // NaCl/ARM only supports ARMv7
            if (goarm != 7L)
            {
                print("runtime: NaCl requires ARMv7. Recompile using GOARM=7.\n");
                exit(1L);
            }
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
