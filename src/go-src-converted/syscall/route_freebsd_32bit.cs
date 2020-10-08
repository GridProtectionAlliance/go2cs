// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build freebsd,386 freebsd,arm

// package syscall -- go2cs converted at 2020 October 08 03:26:56 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\route_freebsd_32bit.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static ptr<RouteMessage> parseRouteMessage(this ptr<anyMessage> _addr_any, slice<byte> b)
        {
            ref anyMessage any = ref _addr_any.val;

            var p = (RouteMessage.val)(@unsafe.Pointer(any));
            var off = int(@unsafe.Offsetof(p.Header.Rmx)) + SizeofRtMetrics;
            if (freebsdConfArch == "amd64")
            {
                off += SizeofRtMetrics; // rt_metrics on amd64 is simply doubled
            }
            return addr(new RouteMessage(Header:p.Header,Data:b[rsaAlignOf(off):any.Msglen]));

        }

        private static ptr<InterfaceMessage> parseInterfaceMessage(this ptr<anyMessage> _addr_any, slice<byte> b)
        {
            ref anyMessage any = ref _addr_any.val;

            var p = (InterfaceMessage.val)(@unsafe.Pointer(any)); 
            // FreeBSD 10 and beyond have a restructured mbuf
            // packet header view.
            // See https://svnweb.freebsd.org/base?view=revision&revision=254804.
            if (supportsABI(1000000L))
            {
                var m = (ifMsghdr.val)(@unsafe.Pointer(any));
                p.Header.Data.Hwassist = uint32(m.Data.Hwassist);
                p.Header.Data.Epoch = m.Data.Epoch;
                p.Header.Data.Lastchange = m.Data.Lastchange;
                return addr(new InterfaceMessage(Header:p.Header,Data:b[int(unsafe.Offsetof(p.Header.Data))+int(p.Header.Data.Datalen):any.Msglen]));
            }

            return addr(new InterfaceMessage(Header:p.Header,Data:b[int(unsafe.Offsetof(p.Header.Data))+int(p.Header.Data.Datalen):any.Msglen]));

        }
    }
}
