// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:41 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os2_solaris.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _SS_DISABLE = 2L;
        private static readonly long _SIG_UNBLOCK = 2L;
        private static readonly long _SIG_SETMASK = 3L;
        private static readonly long _NSIG = 73L; /* number of signals in sigtable array */
        private static readonly long _SI_USER = 0L;
        private static readonly long _RLIMIT_AS = 10L;
    }
}
