// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build freebsd,amd64

// package syscall -- go2cs converted at 2020 August 29 08:37:33 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\route_freebsd_64bit.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static ref RouteMessage parseRouteMessage(this ref anyMessage any, slice<byte> b)
        {
            var p = (RouteMessage.Value)(@unsafe.Pointer(any));
            return ref new RouteMessage(Header:p.Header,Data:b[rsaAlignOf(int(unsafe.Offsetof(p.Header.Rmx))+SizeofRtMetrics):any.Msglen]);
        }

        private static ref InterfaceMessage parseInterfaceMessage(this ref anyMessage any, slice<byte> b)
        {
            var p = (InterfaceMessage.Value)(@unsafe.Pointer(any));
            return ref new InterfaceMessage(Header:p.Header,Data:b[int(unsafe.Offsetof(p.Header.Data))+int(p.Header.Data.Datalen):any.Msglen]);
        }
    }
}
