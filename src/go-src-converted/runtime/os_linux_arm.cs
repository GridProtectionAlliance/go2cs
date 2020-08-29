// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:53 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_arm.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private static readonly long _AT_PLATFORM = 15L; //  introduced in at least 2.6.11

        private static readonly long _HWCAP_VFP = 1L << (int)(6L); // introduced in at least 2.6.11
        private static readonly long _HWCAP_VFPv3 = 1L << (int)(13L); // introduced in 2.6.30
        private static readonly long _HWCAP_IDIVA = 1L << (int)(17L);

        private static uint randomNumber = default;
        private static byte armArch = 6L; // we default to ARMv6
        private static uint hwcap = default; // set by setup_auxv
        private static bool hardDiv = default; // set if a hardware divider is available

        private static void checkgoarm()
        { 
            // On Android, /proc/self/auxv might be unreadable and hwcap won't
            // reflect the CPU capabilities. Assume that every Android arm device
            // has the necessary floating point hardware available.
            if (GOOS == "android")
            {
                return;
            }
            if (goarm > 5L && hwcap & _HWCAP_VFP == 0L)
            {
                print("runtime: this CPU has no floating point hardware, so it cannot run\n");
                print("this GOARM=", goarm, " binary. Recompile using GOARM=5.\n");
                exit(1L);
            }
            if (goarm > 6L && hwcap & _HWCAP_VFPv3 == 0L)
            {
                print("runtime: this CPU has no VFPv3 floating point hardware, so it cannot run\n");
                print("this GOARM=", goarm, " binary. Recompile using GOARM=5.\n");
                exit(1L);
            }
        }

        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {

            if (tag == _AT_RANDOM) 
                // sysargs filled in startupRandomData, but that
                // pointer may not be word aligned, so we must treat
                // it as a byte array.
                randomNumber = uint32(startupRandomData[4L]) | uint32(startupRandomData[5L]) << (int)(8L) | uint32(startupRandomData[6L]) << (int)(16L) | uint32(startupRandomData[7L]) << (int)(24L);
            else if (tag == _AT_PLATFORM) // v5l, v6l, v7l
                *(*byte) t = @unsafe.Pointer(val + 1L).Value;
                if ('5' <= t && t <= '7')
                {
                    armArch = t - '0';
                }
            else if (tag == _AT_HWCAP) // CPU capability bit flags
                hwcap = uint32(val);
                hardDiv = (hwcap & _HWCAP_IDIVA) != 0L;
                    }

        //go:nosplit
        private static long cputicks()
        { 
            // Currently cputicks() is used in blocking profiler and to seed fastrand().
            // nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
            // randomNumber provides better seeding of fastrand.
            return nanotime() + int64(randomNumber);
        }
    }
}
