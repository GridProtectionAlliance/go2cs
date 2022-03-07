// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:26:17 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\sigpanic.go
// This program will crash.
// We want to test unwinding from sigpanic into C code (without a C symbolizer).

/*
#cgo CFLAGS: -O0

char *pnil;

static int f1(void) {
    *pnil = 0;
    return 0;
}
*/
using C = go.C_package;

namespace go;

public static partial class main_package {

private static void init() {
    register("TracebackSigpanic", TracebackSigpanic);
}

public static void TracebackSigpanic() {
    C.f1();
}

} // end main_package
