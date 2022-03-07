// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !linux && (mips64 || mips64le)
// +build !linux
// +build mips64 mips64le

// package cpu -- go2cs converted at 2022 March 06 23:38:20 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_other_mips64x.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

private static void archInit() {
    Initialized = true;
}

} // end cpu_package
