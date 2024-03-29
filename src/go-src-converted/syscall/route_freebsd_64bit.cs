// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (freebsd && amd64) || (freebsd && arm64)
// +build freebsd,amd64 freebsd,arm64

// package syscall -- go2cs converted at 2022 March 13 05:40:32 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\route_freebsd_64bit.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class syscall_package {

private static ptr<RouteMessage> parseRouteMessage(this ptr<anyMessage> _addr_any, slice<byte> b) {
    ref anyMessage any = ref _addr_any.val;

    var p = (RouteMessage.val)(@unsafe.Pointer(any));
    return addr(new RouteMessage(Header:p.Header,Data:b[rsaAlignOf(int(unsafe.Offsetof(p.Header.Rmx))+SizeofRtMetrics):any.Msglen]));
}

private static ptr<InterfaceMessage> parseInterfaceMessage(this ptr<anyMessage> _addr_any, slice<byte> b) {
    ref anyMessage any = ref _addr_any.val;

    var p = (InterfaceMessage.val)(@unsafe.Pointer(any));
    return addr(new InterfaceMessage(Header:p.Header,Data:b[int(unsafe.Offsetof(p.Header.Data))+int(p.Header.Data.Datalen):any.Msglen]));
}

} // end syscall_package
