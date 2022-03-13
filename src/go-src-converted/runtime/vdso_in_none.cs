// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (linux && !386 && !amd64 && !arm && !arm64 && !mips64 && !mips64le && !ppc64 && !ppc64le) || !linux
// +build linux,!386,!amd64,!arm,!arm64,!mips64,!mips64le,!ppc64,!ppc64le !linux

// package runtime -- go2cs converted at 2022 March 13 05:27:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\vdso_in_none.go
namespace go;

public static partial class runtime_package {

// A dummy version of inVDSOPage for targets that don't use a VDSO.

private static bool inVDSOPage(System.UIntPtr pc) {
    return false;
}

} // end runtime_package
