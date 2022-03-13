// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements accept for platforms that do not provide a fast path for
// setting SetNonblock and CloseOnExec.

//go:build aix || darwin || (js && wasm) || (solaris && !illumos)
// +build aix darwin js,wasm solaris,!illumos

// package poll -- go2cs converted at 2022 March 13 05:27:53 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\sys_cloexec.go
namespace go.@internal;

using syscall = syscall_package;


// Wrapper around the accept system call that marks the returned file
// descriptor as nonblocking and close-on-exec.

public static partial class poll_package {

private static (nint, syscall.Sockaddr, @string, error) accept(nint s) {
    nint _p0 = default;
    syscall.Sockaddr _p0 = default;
    @string _p0 = default;
    error _p0 = default!;
 
    // See ../syscall/exec_unix.go for description of ForkLock.
    // It is probably okay to hold the lock across syscall.Accept
    // because we have put fd.sysfd into non-blocking mode.
    // However, a call to the File method will put it back into
    // blocking mode. We can't take that risk, so no use of ForkLock here.
    var (ns, sa, err) = AcceptFunc(s);
    if (err == null) {
        syscall.CloseOnExec(ns);
    }
    if (err != null) {
        return (-1, null, "accept", error.As(err)!);
    }
    err = syscall.SetNonblock(ns, true);

    if (err != null) {
        CloseFunc(ns);
        return (-1, null, "setnonblock", error.As(err)!);
    }
    return (ns, sa, "", error.As(null!)!);
}

} // end poll_package
