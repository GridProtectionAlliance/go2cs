// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:19:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_illumos_amd64.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong _RCTL_LOCAL_DENY = (ulong)0x2UL;

        private static readonly ulong _RCTL_LOCAL_MAXIMAL = (ulong)0x80000000UL;

        private static readonly ulong _RCTL_FIRST = (ulong)0x0UL;
        private static readonly ulong _RCTL_NEXT = (ulong)0x1UL;

    }
}
