// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:47:26 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_freebsd_arm.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _HWCAP_VFP = (long)1L << (int)(6L);
        private static readonly long _HWCAP_VFPv3 = (long)1L << (int)(13L);


        private static void checkgoarm()
        {
            if (goarm > 5L && cpu.HWCap & _HWCAP_VFP == 0L)
            {
                print("runtime: this CPU has no floating point hardware, so it cannot run\n");
                print("this GOARM=", goarm, " binary. Recompile using GOARM=5.\n");
                exit(1L);
            }

            if (goarm > 6L && cpu.HWCap & _HWCAP_VFPv3 == 0L)
            {
                print("runtime: this CPU has no VFPv3 floating point hardware, so it cannot run\n");
                print("this GOARM=", goarm, " binary. Recompile using GOARM=5 or GOARM=6.\n");
                exit(1L);
            } 

            // osinit not called yet, so ncpu not set: must use getncpu directly.
            if (getncpu() > 1L && goarm < 7L)
            {
                print("runtime: this system has multiple CPUs and must use\n");
                print("atomic synchronization instructions. Recompile using GOARM=7.\n");
                exit(1L);
            }

        }

        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {

            if (tag == _AT_HWCAP) 
                cpu.HWCap = uint(val);
            else if (tag == _AT_HWCAP2) 
                cpu.HWCap2 = uint(val);
            
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
