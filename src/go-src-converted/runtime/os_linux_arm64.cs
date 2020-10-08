// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build arm64

// package runtime -- go2cs converted at 2020 October 08 03:21:57 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_arm64.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {

            if (tag == _AT_HWCAP) 
                // arm64 doesn't have a 'cpuid' instruction equivalent and relies on
                // HWCAP/HWCAP2 bits for hardware capabilities.
                var hwcap = uint(val);
                if (GOOS == "android")
                { 
                    // The Samsung S9+ kernel reports support for atomics, but not all cores
                    // actually support them, resulting in SIGILL. See issue #28431.
                    // TODO(elias.naur): Only disable the optimization on bad chipsets.
                    const long hwcap_ATOMICS = (long)1L << (int)(8L);

                    hwcap &= ~uint(hwcap_ATOMICS);

                }
                cpu.HWCap = hwcap;
            else if (tag == _AT_HWCAP2) 
                cpu.HWCap2 = uint(val);
            
        }

        private static void osArchInit()
        {
        }

        //go:nosplit
        private static long cputicks()
        { 
            // Currently cputicks() is used in blocking profiler and to seed fastrand().
            // nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
            return nanotime();

        }
    }
}
