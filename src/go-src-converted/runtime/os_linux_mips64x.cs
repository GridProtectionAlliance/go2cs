// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && (mips64 || mips64le)
// +build linux
// +build mips64 mips64le

// package runtime -- go2cs converted at 2022 March 06 22:10:30 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_linux_mips64x.go
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

private static readonly nint _SS_DISABLE = 2;
private static readonly nint _NSIG = 129;
private static readonly nint _SI_USER = 0;
private static readonly nint _SIG_BLOCK = 1;
private static readonly nint _SIG_UNBLOCK = 2;
private static readonly nint _SIG_SETMASK = 3;


private partial struct sigset { // : array<ulong>
}

private static sigset sigset_all = new sigset(^uint64(0),^uint64(0));

//go:nosplit
//go:nowritebarrierrec
private static void sigaddset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    (mask)[(i - 1) / 64] |= 1 << (int)(((uint32(i) - 1) & 63));
}

private static void sigdelset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    (mask)[(i - 1) / 64] &= 1 << (int)(((uint32(i) - 1) & 63));
}

//go:nosplit
private static void sigfillset(ptr<array<ulong>> _addr_mask) {
    ref array<ulong> mask = ref _addr_mask.val;

    ((mask)[0], (mask)[1]) = (~uint64(0), ~uint64(0));
}

} // end runtime_package
