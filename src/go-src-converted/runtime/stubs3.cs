// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9
// +build !solaris
// +build !freebsd
// +build !darwin
// +build !aix

// package runtime -- go2cs converted at 2020 October 08 03:23:47 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\stubs3.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static long nanotime1()
;
    }
}
