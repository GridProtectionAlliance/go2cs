// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !goexperiment.staticlockranking

// package runtime -- go2cs converted at 2020 October 08 03:19:57 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\lockrank_off.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // // lockRankStruct is embedded in mutex, but is empty when staticklockranking is
        // disabled (the default)
        private partial struct lockRankStruct
        {
        }

        private static void lockInit(ptr<mutex> _addr_l, lockRank rank)
        {
            ref mutex l = ref _addr_l.val;

        }

        private static lockRank getLockRank(ptr<mutex> _addr_l)
        {
            ref mutex l = ref _addr_l.val;

            return 0L;
        }

        // The following functions may be called in nosplit context.
        // Nosplit is not strictly required for lockWithRank, unlockWithRank
        // and lockWithRankMayAcquire, but these nosplit annotations must
        // be kept consistent with the equivalent functions in lockrank_on.go.

        //go:nosplit
        private static void lockWithRank(ptr<mutex> _addr_l, lockRank rank)
        {
            ref mutex l = ref _addr_l.val;

            lock2(l);
        }

        //go:nosplit
        private static void acquireLockRank(lockRank rank)
        {
        }

        //go:nosplit
        private static void unlockWithRank(ptr<mutex> _addr_l)
        {
            ref mutex l = ref _addr_l.val;

            unlock2(l);
        }

        //go:nosplit
        private static void releaseLockRank(lockRank rank)
        {
        }

        //go:nosplit
        private static void lockWithRankMayAcquire(ptr<mutex> _addr_l, lockRank rank)
        {
            ref mutex l = ref _addr_l.val;

        }
    }
}
