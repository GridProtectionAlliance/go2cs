// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:47:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_openbsd_arm64.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        //go:nosplit
        private static long cputicks()
        { 
            // Currently cputicks() is used in blocking profiler and to seed runtime·fastrand().
            // runtime·nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
            return nanotime();

        }

        private static void sysargs(int argc, ptr<ptr<byte>> _addr_argv)
        {
            ref ptr<byte> argv = ref _addr_argv.val;
 
            // OpenBSD does not have auxv, however we still need to initialise cpu.HWCaps.
            // For now specify the bare minimum until we add some form of capabilities
            // detection. See issue #31746.
            cpu.HWCap = 1L << (int)(1L) | 1L << (int)(0L); // ASIMD, FP
        }
    }
}
