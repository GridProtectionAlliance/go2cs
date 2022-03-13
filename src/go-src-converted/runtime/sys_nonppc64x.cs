// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !ppc64 && !ppc64le
// +build !ppc64,!ppc64le

// package runtime -- go2cs converted at 2022 March 13 05:27:14 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sys_nonppc64x.go
namespace go;

public static partial class runtime_package {

private static void prepGoExitFrame(System.UIntPtr sp) {
}

} // end runtime_package
