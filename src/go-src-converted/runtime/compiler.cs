// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:16:39 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\compiler.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Compiler is the name of the compiler toolchain that built the
        // running binary. Known toolchains are:
        //
        //    gc      Also known as cmd/compile.
        //    gccgo   The gccgo front end, part of the GCC compiler suite.
        //
        public static readonly @string Compiler = "gc";

    }
}
