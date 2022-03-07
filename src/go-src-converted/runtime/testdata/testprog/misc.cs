// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:26:04 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\misc.go
using runtime = go.runtime_package;

namespace go;

public static partial class main_package {

private static void init() {
    register("NumGoroutine", NumGoroutine);
}

public static void NumGoroutine() {
    println(runtime.NumGoroutine());
}

} // end main_package
