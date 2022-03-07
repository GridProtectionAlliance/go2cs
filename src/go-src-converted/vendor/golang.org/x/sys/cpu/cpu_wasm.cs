// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build wasm
// +build wasm

// package cpu -- go2cs converted at 2022 March 06 23:38:21 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_wasm.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

    // We're compiling the cpu package for an unknown (software-abstracted) CPU.
    // Make CacheLinePad an empty struct and hope that the usual struct alignment
    // rules are good enough.
private static readonly nint cacheLineSize = 0;



private static void initOptions() {
}

private static void archInit() {
}

} // end cpu_package
