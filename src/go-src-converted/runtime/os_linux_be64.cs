// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The standard GNU/Linux sigset type on big-endian 64-bit machines.

// +build ppc64 s390x

// package runtime -- go2cs converted at 2020 August 29 08:18:54 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_be64.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _SS_DISABLE = 2L;
        private static readonly long _NSIG = 65L;
        private static readonly long _SI_USER = 0L;
        private static readonly long _SIG_BLOCK = 0L;
        private static readonly long _SIG_UNBLOCK = 1L;
        private static readonly long _SIG_SETMASK = 2L;
        private static readonly long _RLIMIT_AS = 9L;

        private partial struct sigset // : ulong
        {
        }

        private partial struct rlimit
        {
            public System.UIntPtr rlim_cur;
            public System.UIntPtr rlim_max;
        }

        private static var sigset_all = sigset(~uint64(0L));

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaddset(ref sigset mask, long i)
        {
            if (i > 64L)
            {
                throw("unexpected signal greater than 64");
            }
            mask.Value |= 1L << (int)((uint(i) - 1L));
        }

        private static void sigdelset(ref sigset mask, long i)
        {
            if (i > 64L)
            {
                throw("unexpected signal greater than 64");
            }
            mask.Value &= 1L << (int)((uint(i) - 1L));
        }

        private static void sigfillset(ref ulong mask)
        {
            mask.Value = ~uint64(0L);
        }
    }
}
