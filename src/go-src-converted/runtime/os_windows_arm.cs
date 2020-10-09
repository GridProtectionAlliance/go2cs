// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:47:38 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_windows_arm.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        //go:nosplit
        private static long cputicks()
        {
            ref long counter = ref heap(out ptr<long> _addr_counter);
            stdcall1(_QueryPerformanceCounter, uintptr(@unsafe.Pointer(_addr_counter)));
            return counter;
        }

        private static void checkgoarm()
        {
            if (goarm < 7L)
            {
                print("Need atomic synchronization instructions, coprocessor ", "access instructions. Recompile using GOARM=7.\n");
                exit(1L);
            }

        }
    }
}
