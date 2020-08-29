// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build freebsd,386 freebsd,arm

// package syscall -- go2cs converted at 2020 August 29 08:37:33 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\route_freebsd_32bit.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static ref RouteMessage parseRouteMessage(this ref anyMessage any, slice<byte> b)
        {
            var p = (RouteMessage.Value)(@unsafe.Pointer(any));
            var off = int(@unsafe.Offsetof(p.Header.Rmx)) + SizeofRtMetrics;
            if (freebsdConfArch == "amd64")
            {
                off += SizeofRtMetrics; // rt_metrics on amd64 is simply doubled
            }
            return ref new RouteMessage(Header:p.Header,Data:b[rsaAlignOf(off):any.Msglen]);
        }

        private static ref InterfaceMessage parseInterfaceMessage(this ref anyMessage any, slice<byte> b)
        {
            var p = (InterfaceMessage.Value)(@unsafe.Pointer(any)); 
            // FreeBSD 10 and beyond have a restructured mbuf
            // packet header view.
            // See http://svnweb.freebsd.org/base?view=revision&revision=254804.
            if (freebsdVersion >= 1000000L)
            {
                var m = (ifMsghdr.Value)(@unsafe.Pointer(any));
                p.Header.Data.Hwassist = uint32(m.Data.Hwassist);
                p.Header.Data.Epoch = m.Data.Epoch;
                p.Header.Data.Lastchange = m.Data.Lastchange;
                return ref new InterfaceMessage(Header:p.Header,Data:b[int(unsafe.Offsetof(p.Header.Data))+int(p.Header.Data.Datalen):any.Msglen]);
            }
            return ref new InterfaceMessage(Header:p.Header,Data:b[int(unsafe.Offsetof(p.Header.Data))+int(p.Header.Data.Datalen):any.Msglen]);
        }
    }
}
