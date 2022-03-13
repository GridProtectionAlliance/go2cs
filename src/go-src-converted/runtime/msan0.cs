// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !msan
// +build !msan

// Dummy MSan support API, used when not built with -msan.

// package runtime -- go2cs converted at 2022 March 13 05:25:56 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\msan0.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class runtime_package {

private static readonly var msanenabled = false;

// Because msanenabled is false, none of these functions should be called.



// Because msanenabled is false, none of these functions should be called.

private static void msanread(unsafe.Pointer addr, System.UIntPtr sz) {
    throw("msan");
}
private static void msanwrite(unsafe.Pointer addr, System.UIntPtr sz) {
    throw("msan");
}
private static void msanmalloc(unsafe.Pointer addr, System.UIntPtr sz) {
    throw("msan");
}
private static void msanfree(unsafe.Pointer addr, System.UIntPtr sz) {
    throw("msan");
}
private static void msanmove(unsafe.Pointer dst, unsafe.Pointer src, System.UIntPtr sz) {
    throw("msan");
}

} // end runtime_package
