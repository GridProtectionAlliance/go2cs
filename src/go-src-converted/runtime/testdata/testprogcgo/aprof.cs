// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:28 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\aprof.go
namespace go;
// Test that SIGPROF received in C code does not crash the process
// looking for the C code's func pointer.

// The test fails when the function is the first C function.
// The exported functions are the first C functions, so we use that.

// extern void CallGoNop();



} // end main_package
