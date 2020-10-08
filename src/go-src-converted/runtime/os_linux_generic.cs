// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !mips
// +build !mipsle
// +build !mips64
// +build !mips64le
// +build !s390x
// +build !ppc64
// +build linux

// package runtime -- go2cs converted at 2020 October 08 03:21:58 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_generic.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _SS_DISABLE = (long)2L;
        private static readonly long _NSIG = (long)65L;
        private static readonly long _SI_USER = (long)0L;
        private static readonly long _SIG_BLOCK = (long)0L;
        private static readonly long _SIG_UNBLOCK = (long)1L;
        private static readonly long _SIG_SETMASK = (long)2L;


        // It's hard to tease out exactly how big a Sigset is, but
        // rt_sigprocmask crashes if we get it wrong, so if binaries
        // are running, this is right.
        private partial struct sigset // : array<uint>
        {
        }

        private static sigset sigset_all = new sigset(^uint32(0),^uint32(0));

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaddset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            (mask)[(i - 1L) / 32L] |= 1L << (int)(((uint32(i) - 1L) & 31L));
        }

        private static void sigdelset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            (mask)[(i - 1L) / 32L] &= 1L << (int)(((uint32(i) - 1L) & 31L));
        }

        private static void sigfillset(ptr<ulong> _addr_mask)
        {
            ref ulong mask = ref _addr_mask.val;

            mask = ~uint64(0L);
        }
    }
}
