// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// +build 386 amd64 amd64p32 arm arm64 ppc64le mips64le mipsle

// package syscall -- go2cs converted at 2020 August 29 08:36:46 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\endian_little.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly var isBigEndian = false;

    }
}
