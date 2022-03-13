// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !msan
// +build !msan

// package syscall -- go2cs converted at 2022 March 13 05:40:31 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\msan0.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class syscall_package {

private static readonly var msanenabled = false;



private static void msanRead(unsafe.Pointer addr, nint len) {
}

private static void msanWrite(unsafe.Pointer addr, nint len) {
}

} // end syscall_package
