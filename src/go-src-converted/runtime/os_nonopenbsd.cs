// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !openbsd

// package runtime -- go2cs converted at 2020 October 08 03:22:04 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_nonopenbsd.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // osStackAlloc performs OS-specific initialization before s is used
        // as stack memory.
        private static void osStackAlloc(ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;

        }

        // osStackFree undoes the effect of osStackAlloc before s is returned
        // to the heap.
        private static void osStackFree(ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;

        }
    }
}
