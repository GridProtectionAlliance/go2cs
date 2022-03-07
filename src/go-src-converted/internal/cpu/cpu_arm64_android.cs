// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build arm64
// +build arm64

// package cpu -- go2cs converted at 2022 March 06 22:29:51 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu_arm64_android.go


namespace go.@internal;

public static partial class cpu_package {

private static void osInit() {
    hwcapInit("android");
}

} // end cpu_package
