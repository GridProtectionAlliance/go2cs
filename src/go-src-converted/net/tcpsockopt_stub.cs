// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package net -- go2cs converted at 2022 March 13 05:30:09 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\tcpsockopt_stub.go
namespace go;

using syscall = syscall_package;
using time = time_package;

public static partial class net_package {

private static error setNoDelay(ptr<netFD> _addr_fd, bool noDelay) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(syscall.ENOPROTOOPT)!;
}

private static error setKeepAlivePeriod(ptr<netFD> _addr_fd, time.Duration d) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(syscall.ENOPROTOOPT)!;
}

} // end net_package
