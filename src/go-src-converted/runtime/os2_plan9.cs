// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Plan 9-specific system calls

// package runtime -- go2cs converted at 2020 August 29 08:18:41 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os2_plan9.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // open
        private static readonly long _OREAD = 0L;
        private static readonly long _OWRITE = 1L;
        private static readonly long _ORDWR = 2L;
        private static readonly long _OEXEC = 3L;
        private static readonly long _OTRUNC = 16L;
        private static readonly long _OCEXEC = 32L;
        private static readonly long _ORCLOSE = 64L;
        private static readonly ulong _OEXCL = 0x1000UL;

        // rfork
        private static readonly long _RFNAMEG = 1L << (int)(0L);
        private static readonly long _RFENVG = 1L << (int)(1L);
        private static readonly long _RFFDG = 1L << (int)(2L);
        private static readonly long _RFNOTEG = 1L << (int)(3L);
        private static readonly long _RFPROC = 1L << (int)(4L);
        private static readonly long _RFMEM = 1L << (int)(5L);
        private static readonly long _RFNOWAIT = 1L << (int)(6L);
        private static readonly long _RFCNAMEG = 1L << (int)(10L);
        private static readonly long _RFCENVG = 1L << (int)(11L);
        private static readonly long _RFCFDG = 1L << (int)(12L);
        private static readonly long _RFREND = 1L << (int)(13L);
        private static readonly long _RFNOMNT = 1L << (int)(14L);

        // notify
        private static readonly long _NCONT = 0L;
        private static readonly long _NDFLT = 1L;

        private partial struct uinptr // : _Plink
        {
        }

        private partial struct tos
        {
            public ulong cyclefreq; // cycle clock frequency if there is one, 0 otherwise
            public long kcycles; // cycles spent in kernel
            public long pcycles; // cycles spent in process (kernel + user)
            public uint pid; // might as well put the pid here
            public uint clock; // top of stack is here
        }

        private static readonly long _NSIG = 14L; // number of signals in sigtable array
        private static readonly long _ERRMAX = 128L; // max length of note string

        // Notes in runtime·sigtab that are handled by runtime·sigpanic.
        private static readonly long _SIGRFAULT = 2L;
        private static readonly long _SIGWFAULT = 3L;
        private static readonly long _SIGINTDIV = 4L;
        private static readonly long _SIGFLOAT = 5L;
        private static readonly long _SIGTRAP = 6L;
        private static readonly long _SIGPROF = 0L; // dummy value defined for badsignal
        private static readonly long _SIGQUIT = 0L; // dummy value defined for sighandler
    }
}
