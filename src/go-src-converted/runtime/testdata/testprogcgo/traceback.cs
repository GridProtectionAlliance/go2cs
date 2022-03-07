// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:26:19 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\traceback.go
// This program will crash.
// We want the stack trace to include the C functions.
// We use a fake traceback, and a symbolizer that dumps a string we recognize.

/*
#cgo CFLAGS: -g -O0

// Defined in traceback_c.c.
extern int crashInGo;
int tracebackF1(void);
void cgoTraceback(void* parg);
void cgoSymbolizer(void* parg);
*/
using C = go.C_package;// This program will crash.
// We want the stack trace to include the C functions.
// We use a fake traceback, and a symbolizer that dumps a string we recognize.

/*
#cgo CFLAGS: -g -O0

// Defined in traceback_c.c.
extern int crashInGo;
int tracebackF1(void);
void cgoTraceback(void* parg);
void cgoSymbolizer(void* parg);
*/


using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class main_package {

private static void init() {
    register("CrashTraceback", CrashTraceback);
    register("CrashTracebackGo", CrashTracebackGo);
}

public static void CrashTraceback() {
    runtime.SetCgoTraceback(0, @unsafe.Pointer(C.cgoTraceback), null, @unsafe.Pointer(C.cgoSymbolizer));
    C.tracebackF1();
}

public static void CrashTracebackGo() {
    C.crashInGo = 1;
    CrashTraceback();
}

//export h1
private static void h1() {
    h2();
}

private static void h2() {
    h3();
}

private static void h3() {
    ptr<nint> x;
    x.val = 0;
}

} // end main_package
