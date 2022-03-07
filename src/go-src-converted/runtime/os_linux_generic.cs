// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !mips && !mipsle && !mips64 && !mips64le && !s390x && !ppc64 && linux
// +build !mips,!mipsle,!mips64,!mips64le,!s390x,!ppc64,linux

// package runtime -- go2cs converted at 2022 March 06 22:10:30 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_linux_generic.go


namespace go;

public static partial class runtime_package {

private static readonly nint _SS_DISABLE = 2;
private static readonly nint _NSIG = 65;
private static readonly nint _SI_USER = 0;
private static readonly nint _SIG_BLOCK = 0;
private static readonly nint _SIG_UNBLOCK = 1;
private static readonly nint _SIG_SETMASK = 2;


// It's hard to tease out exactly how big a Sigset is, but
// rt_sigprocmask crashes if we get it wrong, so if binaries
// are running, this is right.
private partial struct sigset { // : array<uint>
}

private static sigset sigset_all = new sigset(^uint32(0),^uint32(0));

//go:nosplit
//go:nowritebarrierrec
private static void sigaddset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    (mask)[(i - 1) / 32] |= 1 << (int)(((uint32(i) - 1) & 31));
}

private static void sigdelset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    (mask)[(i - 1) / 32] &= 1 << (int)(((uint32(i) - 1) & 31));
}

//go:nosplit
private static void sigfillset(ptr<ulong> _addr_mask) {
    ref ulong mask = ref _addr_mask.val;

    mask = ~uint64(0);
}

} // end runtime_package
