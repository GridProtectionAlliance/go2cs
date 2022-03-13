// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:34 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\traceback.go
namespace go;
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



} // end main_package
