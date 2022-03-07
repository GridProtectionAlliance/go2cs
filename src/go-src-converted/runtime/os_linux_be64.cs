// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The standard Linux sigset type on big-endian 64-bit machines.

//go:build linux && (ppc64 || s390x)
// +build linux
// +build ppc64 s390x

// package runtime -- go2cs converted at 2022 March 06 22:10:30 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_linux_be64.go


namespace go;

public static partial class runtime_package {

private static readonly nint _SS_DISABLE = 2;
private static readonly nint _NSIG = 65;
private static readonly nint _SI_USER = 0;
private static readonly nint _SIG_BLOCK = 0;
private static readonly nint _SIG_UNBLOCK = 1;
private static readonly nint _SIG_SETMASK = 2;


private partial struct sigset { // : ulong
}

private static var sigset_all = sigset(~uint64(0));

//go:nosplit
//go:nowritebarrierrec
private static void sigaddset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    if (i > 64) {
        throw("unexpected signal greater than 64");
    }
    mask |= 1 << (int)((uint(i) - 1));

}

private static void sigdelset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    if (i > 64) {
        throw("unexpected signal greater than 64");
    }
    mask &= 1 << (int)((uint(i) - 1));

}

//go:nosplit
private static void sigfillset(ptr<ulong> _addr_mask) {
    ref ulong mask = ref _addr_mask.val;

    mask = ~uint64(0);
}

} // end runtime_package
