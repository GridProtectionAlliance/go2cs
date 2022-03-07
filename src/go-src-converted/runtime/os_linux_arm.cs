// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:29 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_linux_arm.go
using cpu = go.@internal.cpu_package;

namespace go;

public static partial class runtime_package {

private static readonly nint _HWCAP_VFP = 1 << 6; // introduced in at least 2.6.11
private static readonly nint _HWCAP_VFPv3 = 1 << 13; // introduced in 2.6.30

private static void checkgoarm() { 
    // On Android, /proc/self/auxv might be unreadable and hwcap won't
    // reflect the CPU capabilities. Assume that every Android arm device
    // has the necessary floating point hardware available.
    if (GOOS == "android") {
        return ;
    }
    if (goarm > 5 && cpu.HWCap & _HWCAP_VFP == 0) {
        print("runtime: this CPU has no floating point hardware, so it cannot run\n");
        print("this GOARM=", goarm, " binary. Recompile using GOARM=5.\n");
        exit(1);
    }
    if (goarm > 6 && cpu.HWCap & _HWCAP_VFPv3 == 0) {
        print("runtime: this CPU has no VFPv3 floating point hardware, so it cannot run\n");
        print("this GOARM=", goarm, " binary. Recompile using GOARM=5 or GOARM=6.\n");
        exit(1);
    }
}

private static void archauxv(System.UIntPtr tag, System.UIntPtr val) {

    if (tag == _AT_HWCAP) 
        cpu.HWCap = uint(val);
    else if (tag == _AT_HWCAP2) 
        cpu.HWCap2 = uint(val);
    
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
