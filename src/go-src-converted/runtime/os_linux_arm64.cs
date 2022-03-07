// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build arm64
// +build arm64

// package runtime -- go2cs converted at 2022 March 06 22:10:30 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_linux_arm64.go
using cpu = go.@internal.cpu_package;

namespace go;

public static partial class runtime_package {

private static void archauxv(System.UIntPtr tag, System.UIntPtr val) {

    if (tag == _AT_HWCAP) 
        cpu.HWCap = uint(val);
    
}

private static void osArchInit() {
}

//go:nosplit
private static long cputicks() { 
    // Currently cputicks() is used in blocking profiler and to seed fastrand().
    // nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
    return nanotime();

}

} // end runtime_package
