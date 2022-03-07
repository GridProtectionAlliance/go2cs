// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && !arm && !arm64 && !mips64 && !mips64le && !ppc64 && !ppc64le && !s390x
// +build linux,!arm,!arm64,!mips64,!mips64le,!ppc64,!ppc64le,!s390x

// package cpu -- go2cs converted at 2022 March 06 23:38:19 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_linux_noinit.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

private static void doinit() {
}

} // end cpu_package
