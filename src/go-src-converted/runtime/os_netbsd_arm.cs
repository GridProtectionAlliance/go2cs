// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:01 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_netbsd_arm.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static bool hardDiv = default; // TODO: set if a hardware divider is available

        private static void lwp_mcontext_init(ref mcontextt mc, unsafe.Pointer stk, ref m mp, ref g gp, System.UIntPtr fn)
        { 
            // Machine dependent mcontext initialisation for LWP.
            mc.__gregs[_REG_R15] = uint32(funcPC(lwp_tramp));
            mc.__gregs[_REG_R13] = uint32(uintptr(stk));
            mc.__gregs[_REG_R0] = uint32(uintptr(@unsafe.Pointer(mp)));
            mc.__gregs[_REG_R1] = uint32(uintptr(@unsafe.Pointer(gp)));
            mc.__gregs[_REG_R2] = uint32(fn);
        }

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
            // TODO: need more entropy to better seed fastrand.
            return nanotime();
        }
    }
}
