// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build mips64 mips64le

// package runtime -- go2cs converted at 2020 August 29 08:18:55 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_mips64x.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static uint randomNumber = default;

        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {

            if (tag == _AT_RANDOM) 
                // sysargs filled in startupRandomData, but that
                // pointer may not be word aligned, so we must treat
                // it as a byte array.
                randomNumber = uint32(startupRandomData[4L]) | uint32(startupRandomData[5L]) << (int)(8L) | uint32(startupRandomData[6L]) << (int)(16L) | uint32(startupRandomData[7L]) << (int)(24L);
                    }

        //go:nosplit
        private static long cputicks()
        { 
            // Currently cputicks() is used in blocking profiler and to seed fastrand().
            // nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
            // randomNumber provides better seeding of fastrand.
            return nanotime() + int64(randomNumber);
        }

        private static readonly long _SS_DISABLE = 2L;
        private static readonly long _NSIG = 129L;
        private static readonly long _SI_USER = 0L;
        private static readonly long _SIG_BLOCK = 1L;
        private static readonly long _SIG_UNBLOCK = 2L;
        private static readonly long _SIG_SETMASK = 3L;
        private static readonly long _RLIMIT_AS = 6L;

        private partial struct sigset // : array<ulong>
        {
        }

        private partial struct rlimit
        {
            public System.UIntPtr rlim_cur;
            public System.UIntPtr rlim_max;
        }

        private static sigset sigset_all = new sigset(^uint64(0),^uint64(0));

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaddset(ref sigset mask, long i)
        {
            (mask.Value)[(i - 1L) / 64L] |= 1L << (int)(((uint32(i) - 1L) & 63L));
        }

        private static void sigdelset(ref sigset mask, long i)
        {
            (mask.Value)[(i - 1L) / 64L] &= 1L << (int)(((uint32(i) - 1L) & 63L));
        }

        private static void sigfillset(ref array<ulong> mask)
        {
            (mask.Value)[0L] = ~uint64(0L);
            (mask.Value)[1L] = ~uint64(0L);
        }
    }
}
