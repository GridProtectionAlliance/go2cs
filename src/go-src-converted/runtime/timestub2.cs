// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !darwin
// +build !windows
// +build !freebsd
// +build !aix
// +build !solaris

// package runtime -- go2cs converted at 2020 October 09 04:48:58 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\timestub2.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static (long, int) walltime1()
;
    }
}
