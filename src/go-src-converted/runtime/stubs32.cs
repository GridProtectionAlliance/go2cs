// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build 386 arm amd64p32 mips mipsle

// package runtime -- go2cs converted at 2020 August 29 08:21:01 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\stubs32.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Declarations for runtime services implemented in C or assembly that
        // are only present on 32 bit systems.
        private static void call16(unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
    }
}
