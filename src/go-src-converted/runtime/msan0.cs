// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !msan

// Dummy MSan support API, used when not built with -msan.

// package runtime -- go2cs converted at 2020 October 09 04:47:09 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\msan0.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly var msanenabled = false;

        // Because msanenabled is false, none of these functions should be called.



        // Because msanenabled is false, none of these functions should be called.

        private static void msanread(unsafe.Pointer addr, System.UIntPtr sz)
        {
            throw("msan");
        }
        private static void msanwrite(unsafe.Pointer addr, System.UIntPtr sz)
        {
            throw("msan");
        }
        private static void msanmalloc(unsafe.Pointer addr, System.UIntPtr sz)
        {
            throw("msan");
        }
        private static void msanfree(unsafe.Pointer addr, System.UIntPtr sz)
        {
            throw("msan");
        }
    }
}
