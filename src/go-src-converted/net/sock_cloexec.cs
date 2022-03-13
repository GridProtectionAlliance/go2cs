// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements sysSocket for platforms that provide a fast path for
// setting SetNonblock and CloseOnExec.

//go:build dragonfly || freebsd || illumos || linux || netbsd || openbsd
// +build dragonfly freebsd illumos linux netbsd openbsd

// package net -- go2cs converted at 2022 March 13 05:30:07 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sock_cloexec.go
namespace go;

using poll = @internal.poll_package;
using os = os_package;
using syscall = syscall_package;


// Wrapper around the socket system call that marks the returned file
// descriptor as nonblocking and close-on-exec.

public static partial class net_package {

private static (nint, error) sysSocket(nint family, nint sotype, nint proto) {
    nint _p0 = default;
    error _p0 = default!;

    var (s, err) = socketFunc(family, sotype | syscall.SOCK_NONBLOCK | syscall.SOCK_CLOEXEC, proto); 
    // On Linux the SOCK_NONBLOCK and SOCK_CLOEXEC flags were
    // introduced in 2.6.27 kernel and on FreeBSD both flags were
    // introduced in 10 kernel. If we get an EINVAL error on Linux
    // or EPROTONOSUPPORT error on FreeBSD, fall back to using
    // socket without them.

    if (err == null) 
        return (s, error.As(null!)!);
    else if (err == syscall.EPROTONOSUPPORT || err == syscall.EINVAL)     else 
        return (-1, error.As(os.NewSyscallError("socket", err))!);
    // See ../syscall/exec_unix.go for description of ForkLock.
    syscall.ForkLock.RLock();
    s, err = socketFunc(family, sotype, proto);
    if (err == null) {
        syscall.CloseOnExec(s);
    }
    syscall.ForkLock.RUnlock();
    if (err != null) {
        return (-1, error.As(os.NewSyscallError("socket", err))!);
    }
    err = syscall.SetNonblock(s, true);

    if (err != null) {
        poll.CloseFunc(s);
        return (-1, error.As(os.NewSyscallError("setnonblock", err))!);
    }
    return (s, error.As(null!)!);
}

} // end net_package
