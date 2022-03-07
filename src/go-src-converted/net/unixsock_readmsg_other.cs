// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (js && wasm) || windows
// +build js,wasm windows

// package net -- go2cs converted at 2022 March 06 22:16:56 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\unixsock_readmsg_other.go


namespace go;

public static partial class net_package {

private static readonly nint readMsgFlags = 0;



private static void setReadMsgCloseOnExec(slice<byte> oob) {
}

} // end net_package
