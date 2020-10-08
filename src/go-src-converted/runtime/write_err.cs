// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !android

// package runtime -- go2cs converted at 2020 October 08 03:24:25 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\write_err.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void writeErr(slice<byte> b)
        {
            write(2L, @unsafe.Pointer(_addr_b[0L]), int32(len(b)));
        }
    }
}
