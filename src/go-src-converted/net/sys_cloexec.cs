// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements sysSocket for platforms that do not provide a fast path
// for setting SetNonblock and CloseOnExec.

//go:build aix || darwin || (solaris && !illumos)
// +build aix darwin solaris,!illumos

// package net -- go2cs converted at 2022 March 06 22:16:43 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sys_cloexec.go
using poll = go.@internal.poll_package;
using os = go.os_package;
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

    // Wrapper around the socket system call that marks the returned file
    // descriptor as nonblocking and close-on-exec.
private static (nint, error) sysSocket(nint family, nint sotype, nint proto) {
    nint _p0 = default;
    error _p0 = default!;
 
    // See ../syscall/exec_unix.go for description of ForkLock.
    syscall.ForkLock.RLock();
    var (s, err) = socketFunc(family, sotype, proto);
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
