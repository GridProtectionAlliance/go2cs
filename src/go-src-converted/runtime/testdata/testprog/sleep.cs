// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:26:07 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\sleep.go
using time = go.time_package;

namespace go;

public static partial class main_package {

    // for golang.org/issue/27250
private static void init() {
    register("After1", After1);
}

public static void After1() {
    time.After(1 * time.Second).Receive();
}

} // end main_package
