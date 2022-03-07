// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build arm64 && !linux && !freebsd && !android && (!darwin || ios)
// +build arm64
// +build !linux
// +build !freebsd
// +build !android
// +build !darwin ios

// package cpu -- go2cs converted at 2022 March 06 22:29:51 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu_arm64_other.go


namespace go.@internal;

public static partial class cpu_package {

private static void osInit() { 
    // Other operating systems do not support reading HWCap from auxiliary vector,
    // reading privileged aarch64 system registers or sysctl in user space to detect
    // CPU features at runtime.
}

} // end cpu_package
