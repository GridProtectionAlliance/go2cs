// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build msan

// package syscall -- go2cs converted at 2020 August 29 08:37:15 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\msan.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly var msanenabled = true;



        private static void msanRead(unsafe.Pointer addr, long len)
        {
            runtime.MSanRead(addr, len);
        }

        private static void msanWrite(unsafe.Pointer addr, long len)
        {
            runtime.MSanWrite(addr, len);
        }
    }
}
