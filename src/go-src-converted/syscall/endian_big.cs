// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// +build ppc64 s390x mips mips64

// package syscall -- go2cs converted at 2020 October 09 05:01:10 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\endian_big.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly var isBigEndian = true;

    }
}
