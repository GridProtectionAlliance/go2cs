// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:33 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\sigpanic.go
namespace go;
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



} // end main_package
