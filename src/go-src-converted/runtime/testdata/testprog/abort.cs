// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:42 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\abort.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    { // for go:linkname
        private static void init()
        {
            register("Abort", Abort);
        }

        //go:linkname runtimeAbort runtime.abort
        private static void runtimeAbort()
;

        public static void Abort() => func((defer, panic, recover) =>
        {
            defer(() =>
            {>>MARKER:FUNCTION_runtimeAbort_BLOCK_PREFIX<<
                recover();
                panic("BAD: recovered from abort");
            }());
            runtimeAbort();
            println("BAD: after abort");

        });
    }
}
