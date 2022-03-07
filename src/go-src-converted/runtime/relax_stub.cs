// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !windows
// +build !windows

// package runtime -- go2cs converted at 2022 March 06 22:11:15 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\relax_stub.go


namespace go;

public static partial class runtime_package {

    // osRelaxMinNS is the number of nanoseconds of idleness to tolerate
    // without performing an osRelax. Since osRelax may reduce the
    // precision of timers, this should be enough larger than the relaxed
    // timer precision to keep the timer error acceptable.
private static readonly nint osRelaxMinNS = 0;

// osRelax is called by the scheduler when transitioning to and from
// all Ps being idle.


// osRelax is called by the scheduler when transitioning to and from
// all Ps being idle.
private static void osRelax(bool relax) {
}

} // end runtime_package
