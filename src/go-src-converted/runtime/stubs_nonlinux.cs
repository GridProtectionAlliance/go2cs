// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !linux
// +build !linux

// package runtime -- go2cs converted at 2022 March 13 05:27:11 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\stubs_nonlinux.go
namespace go;

public static partial class runtime_package {

// sbrk0 returns the current process brk, or 0 if not implemented.
private static System.UIntPtr sbrk0() {
    return 0;
}

} // end runtime_package
