// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:56 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\dropm_stub.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    { // for go:linkname

        // Defined in the runtime package.
        //go:linkname runtime_getm_for_test runtime.getm
        private static System.UIntPtr runtime_getm_for_test()
;
    }
}
