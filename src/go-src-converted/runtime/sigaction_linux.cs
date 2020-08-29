// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64

// package runtime -- go2cs converted at 2020 August 29 08:19:58 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\sigaction_linux.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // rt_sigaction calls the rt_sigaction system call. It is implemented in assembly.
        //go:noescape
        private static int rt_sigaction(System.UIntPtr sig, ref sigactiont @new, ref sigactiont old, System.UIntPtr size)
;
    }
}
