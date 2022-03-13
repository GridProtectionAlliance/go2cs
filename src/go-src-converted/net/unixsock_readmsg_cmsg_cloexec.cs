// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || linux || netbsd || openbsd
// +build dragonfly linux netbsd openbsd

// package net -- go2cs converted at 2022 March 13 05:30:14 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\unixsock_readmsg_cmsg_cloexec.go
namespace go;

using syscall = syscall_package;

public static partial class net_package {

private static readonly var readMsgFlags = syscall.MSG_CMSG_CLOEXEC;



private static void setReadMsgCloseOnExec(slice<byte> oob) {
}

} // end net_package
