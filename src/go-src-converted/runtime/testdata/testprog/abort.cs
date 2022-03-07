// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:25:57 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\abort.go
using _@unsafe_ = go.@unsafe_package;
using System;


namespace go;

public static partial class main_package {
 // for go:linkname
private static void init() {
    register("Abort", Abort);
}

//go:linkname runtimeAbort runtime.abort
private static void runtimeAbort();

public static void Abort() => func((defer, panic, recover) => {
    defer(() => {>>MARKER:FUNCTION_runtimeAbort_BLOCK_PREFIX<<
        recover();
        panic("BAD: recovered from abort");
    }());
    runtimeAbort();
    println("BAD: after abort");
});

} // end main_package
