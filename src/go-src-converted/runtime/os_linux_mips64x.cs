// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build mips64 mips64le

// package runtime -- go2cs converted at 2020 October 09 04:47:30 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_mips64x.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {

            if (tag == _AT_HWCAP) 
                cpu.HWCap = uint(val);
            
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

        private static readonly long _SS_DISABLE = (long)2L;
        private static readonly long _NSIG = (long)129L;
        private static readonly long _SI_USER = (long)0L;
        private static readonly long _SIG_BLOCK = (long)1L;
        private static readonly long _SIG_UNBLOCK = (long)2L;
        private static readonly long _SIG_SETMASK = (long)3L;


        private partial struct sigset // : array<ulong>
        {
        }

        private static sigset sigset_all = new sigset(^uint64(0),^uint64(0));

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaddset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            (mask)[(i - 1L) / 64L] |= 1L << (int)(((uint32(i) - 1L) & 63L));
        }

        private static void sigdelset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            (mask)[(i - 1L) / 64L] &= 1L << (int)(((uint32(i) - 1L) & 63L));
        }

        private static void sigfillset(ptr<array<ulong>> _addr_mask)
        {
            ref array<ulong> mask = ref _addr_mask.val;

            (mask)[0L] = ~uint64(0L);
            (mask)[1L] = ~uint64(0L);

        }
    }
}
