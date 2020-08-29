// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !mips64,!mips64le

// Declarations for routines that are implemented in noasm.go.

// package runtime -- go2cs converted at 2020 August 29 08:21:02 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\stubs_asm.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static long cmpstring(@string s1, @string s2)
;
    }
}
