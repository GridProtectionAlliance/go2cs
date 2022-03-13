// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && !arm && !arm64 && !mips && !mipsle && !mips64 && !mips64le && !s390x && !ppc64 && !ppc64le
// +build linux,!arm,!arm64,!mips,!mipsle,!mips64,!mips64le,!s390x,!ppc64,!ppc64le

// package runtime -- go2cs converted at 2022 March 13 05:26:05 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_linux_noauxv.go
namespace go;

public static partial class runtime_package {

private static void archauxv(System.UIntPtr tag, System.UIntPtr val) {
}

} // end runtime_package
