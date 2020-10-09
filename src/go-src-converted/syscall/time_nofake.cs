// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !faketime

// package syscall -- go2cs converted at 2020 October 09 05:02:03 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\time_nofake.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly var faketime = false;



        private static long faketimeWrite(long fd, slice<byte> p) => func((_, panic, __) =>
        { 
            // This should never be called since faketime is false.
            panic("not implemented");

        });
    }
}
