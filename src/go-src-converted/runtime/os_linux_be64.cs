// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The standard GNU/Linux sigset type on big-endian 64-bit machines.

// +build linux
// +build ppc64 s390x

// package runtime -- go2cs converted at 2020 October 08 03:21:58 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_be64.go

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


        private partial struct sigset // : ulong
        {
        }

        private static var sigset_all = sigset(~uint64(0L));

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaddset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            if (i > 64L)
            {
                throw("unexpected signal greater than 64");
            }

            mask |= 1L << (int)((uint(i) - 1L));

        }

        private static void sigdelset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            if (i > 64L)
            {
                throw("unexpected signal greater than 64");
            }

            mask &= 1L << (int)((uint(i) - 1L));

        }

        private static void sigfillset(ptr<ulong> _addr_mask)
        {
            ref ulong mask = ref _addr_mask.val;

            mask = ~uint64(0L);
        }
    }
}
