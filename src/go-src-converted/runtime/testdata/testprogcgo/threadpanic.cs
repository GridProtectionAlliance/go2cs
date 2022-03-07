// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9

// package main -- go2cs converted at 2022 March 06 22:26:18 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\threadpanic.go
// void start(void);
using C = go.C_package;

namespace go;

public static partial class main_package {

private static void init() {
    register("CgoExternalThreadPanic", CgoExternalThreadPanic);
}

public static void CgoExternalThreadPanic() {
    C.start();
}

//export gopanic
private static void gopanic() => func((_, panic, _) => {
    panic("BOOM");
});

} // end main_package
