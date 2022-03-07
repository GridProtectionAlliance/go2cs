// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || (js && wasm) || solaris
// +build aix js,wasm solaris

// package net -- go2cs converted at 2022 March 06 22:16:41 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sock_stub.go
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

private static nint maxListenerBacklog() { 
    // TODO: Implement this
    // NOTE: Never return a number bigger than 1<<16 - 1. See issue 5030.
    return syscall.SOMAXCONN;

}

} // end net_package
