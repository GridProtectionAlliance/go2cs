// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Declarations for operating systems implementing time.now
// indirectly, in terms of walltime and nanotime assembly.

// +build !darwin !amd64,!386
// +build !windows

// package runtime -- go2cs converted at 2020 August 29 08:21:17 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\timestub.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    { // for go:linkname
        private static (long, int) walltime()
;

        //go:linkname time_now time.now
        private static (long, int, long) time_now()
        {
            sec, nsec = walltime();
            return (sec, nsec, nanotime() - startNano);
        }
    }
}
