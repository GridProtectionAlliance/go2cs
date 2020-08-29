// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !windows

// package runtime -- go2cs converted at 2020 August 29 08:19:45 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\relax_stub.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // osRelaxMinNS is the number of nanoseconds of idleness to tolerate
        // without performing an osRelax. Since osRelax may reduce the
        // precision of timers, this should be enough larger than the relaxed
        // timer precision to keep the timer error acceptable.
        private static readonly long osRelaxMinNS = 0L;

        // osRelax is called by the scheduler when transitioning to and from
        // all Ps being idle.


        // osRelax is called by the scheduler when transitioning to and from
        // all Ps being idle.
        private static void osRelax(bool relax)
        {
        }
    }
}
