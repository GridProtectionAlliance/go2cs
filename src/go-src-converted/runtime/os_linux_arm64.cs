// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build arm64

// package runtime -- go2cs converted at 2020 August 29 08:18:54 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_arm64.go
// For go:linkname
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static uint randomNumber = default;

        // arm64 doesn't have a 'cpuid' instruction equivalent and relies on
        // HWCAP/HWCAP2 bits for hardware capabilities.

        //go:linkname cpu_hwcap internal/cpu.arm64_hwcap
        //go:linkname cpu_hwcap2 internal/cpu.arm64_hwcap2
        private static ulong cpu_hwcap = default;
        private static ulong cpu_hwcap2 = default;

        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {

            if (tag == _AT_RANDOM) 
                // sysargs filled in startupRandomData, but that
                // pointer may not be word aligned, so we must treat
                // it as a byte array.
                randomNumber = uint32(startupRandomData[4L]) | uint32(startupRandomData[5L]) << (int)(8L) | uint32(startupRandomData[6L]) << (int)(16L) | uint32(startupRandomData[7L]) << (int)(24L);
            else if (tag == _AT_HWCAP) 
                cpu_hwcap = uint(val);
            else if (tag == _AT_HWCAP2) 
                cpu_hwcap2 = uint(val);
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
