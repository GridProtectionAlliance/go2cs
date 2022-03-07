// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || freebsd || solaris
// +build aix darwin freebsd solaris

// package net -- go2cs converted at 2022 March 06 22:16:55 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\unixsock_readmsg_cloexec.go
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

private static readonly nint readMsgFlags = 0;



private static void setReadMsgCloseOnExec(slice<byte> oob) {
    var (scms, err) = syscall.ParseSocketControlMessage(oob);
    if (err != null) {
        return ;
    }
    foreach (var (_, scm) in scms) {
        if (scm.Header.Level == syscall.SOL_SOCKET && scm.Header.Type == syscall.SCM_RIGHTS) {
            var (fds, err) = syscall.ParseUnixRights(_addr_scm);
            if (err != null) {
                continue;
            }
            foreach (var (_, fd) in fds) {
                syscall.CloseOnExec(fd);
            }
        }
    }
}

} // end net_package
